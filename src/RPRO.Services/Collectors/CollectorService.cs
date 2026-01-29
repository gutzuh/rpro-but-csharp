using RPRO.Core.Entities;
using RPRO.Core.Interfaces;
using RPRO.Core.Models;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace RPRO.Services.Collectors;

public class CollectorService : IDisposable
{
    private readonly IRelatorioRepository _relatorioRepo;
    private readonly IAmendoimRepository _amendoimRepo;
    private readonly FileParserService _parserService;
    
    private CancellationTokenSource? _cts;
    private Task? _runningTask;
    
    private readonly ConcurrentDictionary<string, DateTime> _processedFiles = new();
    private readonly HashSet<string> _processedHashes = new();
    private readonly List<ProcessResult> _recentResults = new();
    private const int MaxRecentResults = 50;

    public CollectorStatus Status { get; private set; } = new();
    
    public event EventHandler<ProcessResult>? OnFileProcessed;
    public event EventHandler<string>? OnError;
    public event EventHandler<CollectorStatus>? OnStatusChanged;

    public CollectorService(
        IRelatorioRepository relatorioRepo,
        IAmendoimRepository amendoimRepo)
    {
        _relatorioRepo = relatorioRepo;
        _amendoimRepo = amendoimRepo;
        _parserService = new FileParserService();
    }

    /// <summary>
    /// Inicia o coletor de ração
    /// </summary>
    public async Task StartRacaoCollectorAsync(CollectorRacaoConfig config)
    {
        if (Status.IsRunning)
        {
            Console.WriteLine("[Collector] Já está rodando");
            return;
        }

        _cts = new CancellationTokenSource();
        Status.IsRunning = true;
        UpdateStatus();

        _runningTask = Task.Run(async () =>
        {
            Console.WriteLine("[Collector Ração] Iniciado");
            
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    await RunRacaoCollectionCycleAsync(config);
                    
                    Status.NextRun = DateTime.Now.AddSeconds(config.IntervaloSegundos);
                    UpdateStatus();
                    
                    await Task.Delay(TimeSpan.FromSeconds(config.IntervaloSegundos), _cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Status.LastError = ex.Message;
                    OnError?.Invoke(this, ex.Message);
                    Console.WriteLine($"[Collector Ração] Erro: {ex.Message}");
                    
                    await Task.Delay(TimeSpan.FromSeconds(30), _cts.Token);
                }
            }
            
            Status.IsRunning = false;
            UpdateStatus();
            Console.WriteLine("[Collector Ração] Parado");
        });
    }

    /// <summary>
    /// Inicia o coletor de amendoim
    /// </summary>
    public async Task StartAmendoimCollectorAsync(CollectorAmendoimConfig config)
    {
        if (Status.IsRunning)
        {
            Console.WriteLine("[Collector] Já está rodando");
            return;
        }

        _cts = new CancellationTokenSource();
        Status.IsRunning = true;
        UpdateStatus();

        _runningTask = Task.Run(async () =>
        {
            Console.WriteLine("[Collector Amendoim] Iniciado");
            
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    await RunAmendoimCollectionCycleAsync(config);
                    
                    Status.NextRun = DateTime.Now.AddSeconds(config.IntervaloSegundos);
                    UpdateStatus();
                    
                    await Task.Delay(TimeSpan.FromSeconds(config.IntervaloSegundos), _cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Status.LastError = ex.Message;
                    OnError?.Invoke(this, ex.Message);
                    Console.WriteLine($"[Collector Amendoim] Erro: {ex.Message}");
                    
                    await Task.Delay(TimeSpan.FromSeconds(30), _cts.Token);
                }
            }
            
            Status.IsRunning = false;
            UpdateStatus();
            Console.WriteLine("[Collector Amendoim] Parado");
        });
    }

    /// <summary>
    /// Para o coletor
    /// </summary>
    public async Task StopAsync()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            
            if (_runningTask != null)
            {
                try
                {
                    await _runningTask.WaitAsync(TimeSpan.FromSeconds(10));
                }
                catch (TimeoutException)
                {
                    Console.WriteLine("[Collector] Timeout ao parar");
                }
            }
            
            _cts.Dispose();
            _cts = null;
        }
        
        Status.IsRunning = false;
        UpdateStatus();
    }

    /// <summary>
    /// Executa um ciclo de coleta manual (ração)
    /// </summary>
    public async Task<List<ProcessResult>> RunManualRacaoCollectionAsync(CollectorRacaoConfig config)
    {
        return await RunRacaoCollectionCycleAsync(config);
    }

    /// <summary>
    /// Executa um ciclo de coleta manual (amendoim)
    /// </summary>
    public async Task<List<ProcessResult>> RunManualAmendoimCollectionAsync(CollectorAmendoimConfig config)
    {
        return await RunAmendoimCollectionCycleAsync(config);
    }

    private async Task<List<ProcessResult>> RunRacaoCollectionCycleAsync(CollectorRacaoConfig config)
    {
        var results = new List<ProcessResult>();
        var sw = Stopwatch.StartNew();
        
        Status.LastRun = DateTime.Now;
        
        using var ihmService = new IHMService(config.IHM, "IHM_Racao");
        var files = await ihmService.FindAndDownloadNewFilesAsync(_processedHashes);
        
        Console.WriteLine($"[Collector Ração] {files.Count} arquivo(s) para processar");

        foreach (var file in files)
        {
            var result = await ProcessRacaoFileAsync(file);
            results.Add(result);
            AddRecentResult(result);
            
            if (result.Success)
            {
                _processedHashes.Add(file.Hash);
                Status.FilesProcessed++;
                Status.TotalRowsSaved += result.RowsSaved;
            }
            
            OnFileProcessed?.Invoke(this, result);
            
            // Limpar arquivo temporário
            if (File.Exists(file.LocalPath))
                File.Delete(file.LocalPath);
        }

        sw.Stop();
        Console.WriteLine($"[Collector Ração] Ciclo concluído em {sw.ElapsedMilliseconds}ms");
        
        UpdateStatus();
        return results;
    }

    private async Task<List<ProcessResult>> RunAmendoimCollectionCycleAsync(CollectorAmendoimConfig config)
    {
        var results = new List<ProcessResult>();
        var sw = Stopwatch.StartNew();
        
        Status.LastRun = DateTime.Now;

        // Coletar da IHM1 (entrada - balanças 1,2)
        using var ihm1 = new IHMService(config.IHM1, "IHM1_Entrada");
        var filesIhm1 = await ihm1.FindAndDownloadNewFilesAsync(_processedHashes);
        
        foreach (var file in filesIhm1)
        {
            var result = await ProcessAmendoimFileAsync(file, "entrada");
            results.Add(result);
            AddRecentResult(result);
            
            if (result.Success)
            {
                _processedHashes.Add(file.Hash);
                Status.FilesProcessed++;
                Status.TotalRowsSaved += result.RowsSaved;
            }
            
            OnFileProcessed?.Invoke(this, result);
            
            if (File.Exists(file.LocalPath))
                File.Delete(file.LocalPath);
        }

        // Coletar da IHM2 se configurado (saída - balança 3)
        if (config.DuasIHMs && config.IHM2 != null)
        {
            using var ihm2 = new IHMService(config.IHM2, "IHM2_Saida");
            var filesIhm2 = await ihm2.FindAndDownloadNewFilesAsync(_processedHashes);
            
            foreach (var file in filesIhm2)
            {
                var result = await ProcessAmendoimFileAsync(file, "saida");
                results.Add(result);
                AddRecentResult(result);
                
                if (result.Success)
                {
                    _processedHashes.Add(file.Hash);
                    Status.FilesProcessed++;
                    Status.TotalRowsSaved += result.RowsSaved;
                }
                
                OnFileProcessed?.Invoke(this, result);
                
                if (File.Exists(file.LocalPath))
                    File.Delete(file.LocalPath);
            }
        }

        sw.Stop();
        Console.WriteLine($"[Collector Amendoim] Ciclo concluído em {sw.ElapsedMilliseconds}ms");
        
        UpdateStatus();
        return results;
    }

    private async Task<ProcessResult> ProcessRacaoFileAsync(CollectedFile file)
    {
        var sw = Stopwatch.StartNew();
        var result = new ProcessResult { FileName = file.Name };

        try
        {
            Console.WriteLine($"[Processor] Processando: {file.Name}");
            
            var rows = _parserService.ParseRelatorioFile(file.LocalPath);
            result.RowsProcessed = rows.Count;

            var relatorios = rows.Select(r => new Relatorio
            {
                Id = Guid.NewGuid(),
                Dia = r.Date,
                Hora = r.Time,
                Nome = r.Label,
                Form1 = r.Form1 ?? 0,
                Form2 = r.Form2 ?? 0,
                Prod_1 = r.Values.Length > 0 ? r.Values[0] : 0,
                Prod_2 = r.Values.Length > 1 ? r.Values[1] : 0,
                Prod_3 = r.Values.Length > 2 ? r.Values[2] : 0,
                Prod_4 = r.Values.Length > 3 ? r.Values[3] : 0,
                Prod_5 = r.Values.Length > 4 ? r.Values[4] : 0,
                Prod_6 = r.Values.Length > 5 ? r.Values[5] : 0,
                Prod_7 = r.Values.Length > 6 ? r.Values[6] : 0,
                Prod_8 = r.Values.Length > 7 ? r.Values[7] : 0,
                Prod_9 = r.Values.Length > 8 ? r.Values[8] : 0,
                Prod_10 = r.Values.Length > 9 ? r.Values[9] : 0,
                // ... continuar para os outros produtos
            }).ToList();

            result.RowsSaved = await _relatorioRepo.InsertManyAsync(relatorios);
            result.Success = true;
            
            Console.WriteLine($"[Processor] {file.Name}: {result.RowsSaved}/{result.RowsProcessed} linhas salvas");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.Errors = 1;
            Console.WriteLine($"[Processor] Erro em {file.Name}: {ex.Message}");
        }

        sw.Stop();
        result.Duration = sw.Elapsed;
        return result;
    }

    private async Task<ProcessResult> ProcessAmendoimFileAsync(CollectedFile file, string tipoDefault)
    {
        var sw = Stopwatch.StartNew();
        var result = new ProcessResult { FileName = file.Name };

        try
        {
            Console.WriteLine($"[Processor] Processando amendoim: {file.Name} (tipo: {tipoDefault})");
            
            var rows = _parserService.ParseAmendoimFile(file.LocalPath, tipoDefault);
            result.RowsProcessed = rows.Count;

            var amendoins = rows.Select(r => new Amendoim
            {
                Tipo = DeterminarTipoByBalanca(r.Balanca, tipoDefault),
                Dia = r.Dia,
                Hora = r.Hora,
                CodigoProduto = r.CodigoProduto,
                CodigoCaixa = r.CodigoCaixa,
                NomeProduto = r.NomeProduto,
                Peso = r.Peso,
                Balanca = r.Balanca,
                CreatedAt = DateTime.Now
            }).ToList();

            result.RowsSaved = await _amendoimRepo.InsertManyAsync(amendoins);
            result.RowsDuplicated = result.RowsProcessed - result.RowsSaved;
            result.Success = true;
            
            Console.WriteLine($"[Processor] {file.Name}: {result.RowsSaved}/{result.RowsProcessed} linhas salvas ({result.RowsDuplicated} duplicadas)");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.Errors = 1;
            Console.WriteLine($"[Processor] Erro em {file.Name}: {ex.Message}");
        }

        sw.Stop();
        result.Duration = sw.Elapsed;
        return result;
    }

    private string DeterminarTipoByBalanca(string? balanca, string defaultTipo)
    {
        if (string.IsNullOrEmpty(balanca))
            return defaultTipo;

        // Balanças 1,2 = entrada, Balança 3 = saída
        if (balanca == "1" || balanca == "2")
            return "entrada";
        
        if (balanca == "3")
            return "saida";

        return defaultTipo;
    }

    private void AddRecentResult(ProcessResult result)
    {
        _recentResults.Insert(0, result);
        
        while (_recentResults.Count > MaxRecentResults)
            _recentResults.RemoveAt(_recentResults.Count - 1);
        
        Status.RecentResults = _recentResults.Take(10).ToList();
    }

    private void UpdateStatus()
    {
        OnStatusChanged?.Invoke(this, Status);
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}