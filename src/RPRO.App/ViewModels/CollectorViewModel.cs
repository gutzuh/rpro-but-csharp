using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RPRO.Core.Models;
using RPRO.Services.Collectors;
using System.Collections.ObjectModel;

namespace RPRO.App.ViewModels;

public partial class CollectorViewModel : ObservableObject
{
    private readonly CollectorService _collectorService;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private string _statusText = "Parado";

    [ObservableProperty]
    private DateTime? _lastRun;

    [ObservableProperty]
    private DateTime? _nextRun;

    [ObservableProperty]
    private int _filesProcessed;

    [ObservableProperty]
    private int _totalRowsSaved;

    [ObservableProperty]
    private string? _lastError;

    // Configuração Ração
    [ObservableProperty]
    private string _racaoIhmIp = "192.168.5.252";

    [ObservableProperty]
    private string _racaoIhmUser = "anonymous";

    [ObservableProperty]
    private string _racaoIhmPassword = "";

    [ObservableProperty]
    private string _racaoIhmPath = "/InternalStorage/data/";

    [ObservableProperty]
    private int _racaoIntervalo = 60;

    // Configuração Amendoim
    [ObservableProperty]
    private string _amendoimIhm1Ip = "192.168.5.253";

    [ObservableProperty]
    private string _amendoimIhm1User = "anonymous";

    [ObservableProperty]
    private string _amendoimIhm1Password = "";

    [ObservableProperty]
    private bool _amendoimDuasIhms = false;

    [ObservableProperty]
    private string _amendoimIhm2Ip = "";

    [ObservableProperty]
    private string _amendoimIhm2User = "anonymous";

    [ObservableProperty]
    private string _amendoimIhm2Password = "";

    [ObservableProperty]
    private int _amendoimIntervalo = 60;

    // Tipo de coletor ativo
    [ObservableProperty]
    private string _tipoColetorAtivo = "racao"; // "racao" ou "amendoim"

    // Resultados recentes
    public ObservableCollection<ProcessResult> RecentResults { get; } = new();

    // Teste de conexão
    [ObservableProperty]
    private bool _isTesting;

    [ObservableProperty]
    private string _testResult = "";

    public CollectorViewModel(CollectorService collectorService)
    {
        _collectorService = collectorService;
        
        _collectorService.OnStatusChanged += (s, status) =>
        {
            IsRunning = status.IsRunning;
            StatusText = status.IsRunning ? "Rodando" : "Parado";
            LastRun = status.LastRun;
            NextRun = status.NextRun;
            FilesProcessed = status.FilesProcessed;
            TotalRowsSaved = status.TotalRowsSaved;
            LastError = status.LastError;
        };

        _collectorService.OnFileProcessed += (s, result) =>
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                RecentResults.Insert(0, result);
                while (RecentResults.Count > 20)
                    RecentResults.RemoveAt(RecentResults.Count - 1);
            });
        };
    }

    [RelayCommand]
    private async Task StartCollectorAsync()
    {
        if (TipoColetorAtivo == "racao")
        {
            var config = new CollectorRacaoConfig
            {
                IHM = new IHMConfig
                {
                    Ip = RacaoIhmIp,
                    User = RacaoIhmUser,
                    Password = RacaoIhmPassword,
                    CaminhoRemoto = RacaoIhmPath
                },
                IntervaloSegundos = RacaoIntervalo
            };
            
            await _collectorService.StartRacaoCollectorAsync(config);
        }
        else
        {
            var config = new CollectorAmendoimConfig
            {
                IHM1 = new IHMConfig
                {
                    Ip = AmendoimIhm1Ip,
                    User = AmendoimIhm1User,
                    Password = AmendoimIhm1Password
                },
                DuasIHMs = AmendoimDuasIhms,
                IHM2 = AmendoimDuasIhms ? new IHMConfig
                {
                    Ip = AmendoimIhm2Ip,
                    User = AmendoimIhm2User,
                    Password = AmendoimIhm2Password
                } : null,
                IntervaloSegundos = AmendoimIntervalo
            };
            
            await _collectorService.StartAmendoimCollectorAsync(config);
        }
    }

    [RelayCommand]
    private async Task StopCollectorAsync()
    {
        await _collectorService.StopAsync();
    }

    [RelayCommand]
    private async Task RunManualCollectionAsync()
    {
        if (TipoColetorAtivo == "racao")
        {
            var config = new CollectorRacaoConfig
            {
                IHM = new IHMConfig
                {
                    Ip = RacaoIhmIp,
                    User = RacaoIhmUser,
                    Password = RacaoIhmPassword,
                    CaminhoRemoto = RacaoIhmPath
                }
            };
            
            await _collectorService.RunManualRacaoCollectionAsync(config);
        }
        else
        {
            var config = new CollectorAmendoimConfig
            {
                IHM1 = new IHMConfig
                {
                    Ip = AmendoimIhm1Ip,
                    User = AmendoimIhm1User,
                    Password = AmendoimIhm1Password
                },
                DuasIHMs = AmendoimDuasIhms,
                IHM2 = AmendoimDuasIhms ? new IHMConfig
                {
                    Ip = AmendoimIhm2Ip,
                    User = AmendoimIhm2User,
                    Password = AmendoimIhm2Password
                } : null
            };
            
            await _collectorService.RunManualAmendoimCollectionAsync(config);
        }
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        IsTesting = true;
        TestResult = "Testando conexão...";

        try
        {
            var config = TipoColetorAtivo == "racao"
                ? new IHMConfig { Ip = RacaoIhmIp, User = RacaoIhmUser, Password = RacaoIhmPassword, CaminhoRemoto = RacaoIhmPath }
                : new IHMConfig { Ip = AmendoimIhm1Ip, User = AmendoimIhm1User, Password = AmendoimIhm1Password };

            using var ihm = new IHMService(config, "Test");
            var (success, message) = await ihm.TestConnectionAsync();
            
            TestResult = success ? $"✅ {message}" : $"❌ {message}";
        }
        catch (Exception ex)
        {
            TestResult = $"❌ Erro: {ex.Message}";
        }
        finally
        {
            IsTesting = false;
        }
    }
}