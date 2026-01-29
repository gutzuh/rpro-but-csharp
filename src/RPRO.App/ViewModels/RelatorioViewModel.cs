using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RPRO.Core.DTOs;
using RPRO.Core.Entities;
using RPRO.Core.Interfaces;
using System.Collections.ObjectModel;

namespace RPRO.App.ViewModels;

public partial class RelatorioViewModel : ObservableObject
{
    private readonly IRelatorioRepository _relatorioRepo;
    private readonly IAmendoimRepository _amendoimRepo;
    private readonly IMateriaPrimaRepository _materiaPrimaRepo;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = "";

    // Tipo de visualização
    [ObservableProperty]
    private string _tipoVisualizacao = "racao"; // "racao" ou "amendoim"

    // Filtros
    [ObservableProperty]
    private DateTime _dataInicio = DateTime.Today.AddDays(-30);

    [ObservableProperty]
    private DateTime _dataFim = DateTime.Today;

    [ObservableProperty]
    private string _filtroFormula = "";

    [ObservableProperty]
    private int? _filtroCodigo;

    [ObservableProperty]
    private int? _filtroNumero;

    [ObservableProperty]
    private string _filtroTipoAmendoim = ""; // "", "entrada", "saida"

    // Paginação
    [ObservableProperty]
    private int _paginaAtual = 1;

    [ObservableProperty]
    private int _itensPorPagina = 50;

    [ObservableProperty]
    private int _totalItens;

    [ObservableProperty]
    private int _totalPaginas;

    // Dados
    public ObservableCollection<RelatorioRow> DadosRacao { get; } = new();
    public ObservableCollection<AmendoimRow> DadosAmendoim { get; } = new();

    // Matérias-primas para labels
    private Dictionary<int, string> _materiaPrimaLabels = new();

    // Colunas visíveis (para ração)
    public ObservableCollection<ColunaConfig> ColunasVisiveis { get; } = new();

    public RelatorioViewModel(
        IRelatorioRepository relatorioRepo,
        IAmendoimRepository amendoimRepo,
        IMateriaPrimaRepository materiaPrimaRepo)
    {
        _relatorioRepo = relatorioRepo;
        _amendoimRepo = amendoimRepo;
        _materiaPrimaRepo = materiaPrimaRepo;

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadMateriaPrimasAsync();
        await LoadDataAsync();
    }

    private async Task LoadMateriaPrimasAsync()
    {
        var mps = await _materiaPrimaRepo.GetAllAsync();
        _materiaPrimaLabels = mps.ToDictionary(m => m.Num, m => m.Produto);

        // Configurar colunas visíveis
        ColunasVisiveis.Clear();
        ColunasVisiveis.Add(new ColunaConfig { Nome = "Dia", Visivel = true, Largura = 100 });
        ColunasVisiveis.Add(new ColunaConfig { Nome = "Hora", Visivel = true, Largura = 80 });
        ColunasVisiveis.Add(new ColunaConfig { Nome = "Fórmula", Visivel = true, Largura = 200 });
        ColunasVisiveis.Add(new ColunaConfig { Nome = "Código", Visivel = true, Largura = 80 });
        ColunasVisiveis.Add(new ColunaConfig { Nome = "Número", Visivel = true, Largura = 80 });

        for (int i = 1; i <= 40; i++)
        {
            var label = _materiaPrimaLabels.TryGetValue(i, out var nome) ? nome : $"Prod {i}";
            ColunasVisiveis.Add(new ColunaConfig { Nome = label, Visivel = i <= 10, Largura = 100 });
        }
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = "";

            if (TipoVisualizacao == "racao")
            {
                await LoadRacaoDataAsync();
            }
            else
            {
                await LoadAmendoimDataAsync();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao carregar dados: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadRacaoDataAsync()
    {
        var (items, total) = await _relatorioRepo.GetPaginatedAsync(
            PaginaAtual,
            ItensPorPagina,
            DataInicio,
            DataFim,
            string.IsNullOrEmpty(FiltroFormula) ? null : FiltroFormula,
            FiltroCodigo,
            FiltroNumero);

        TotalItens = total;
        TotalPaginas = (int)Math.Ceiling((double)total / ItensPorPagina);

        DadosRacao.Clear();
        foreach (var item in items)
        {
            DadosRacao.Add(new RelatorioRow
            {
                Id = item.Id,
                Dia = item.Dia ?? "",
                Hora = item.Hora ?? "",
                Formula = item.Nome ?? "",
                Codigo = item.Form1,
                Numero = item.Form2,
                Produtos = item.GetProdutosArray(),
                Total = item.TotalProdutos
            });
        }
    }

    private async Task LoadAmendoimDataAsync()
    {
        var (items, total) = await _amendoimRepo.GetPaginatedAsync(
            PaginaAtual,
            ItensPorPagina,
            DataInicio,
            DataFim,
            string.IsNullOrEmpty(FiltroTipoAmendoim) ? null : FiltroTipoAmendoim);

        TotalItens = total;
        TotalPaginas = (int)Math.Ceiling((double)total / ItensPorPagina);

        DadosAmendoim.Clear();
        foreach (var item in items)
        {
            DadosAmendoim.Add(new AmendoimRow
            {
                Id = item.Id,
                Tipo = item.Tipo,
                Dia = item.Dia,
                Hora = item.Hora,
                CodigoProduto = item.CodigoProduto,
                NomeProduto = item.NomeProduto,
                Peso = item.Peso,
                Balanca = item.Balanca ?? ""
            });
        }
    }

    [RelayCommand]
    private async Task PrimeiraPaginaAsync()
    {
        PaginaAtual = 1;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task PaginaAnteriorAsync()
    {
        if (PaginaAtual > 1)
        {
            PaginaAtual--;
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task ProximaPaginaAsync()
    {
        if (PaginaAtual < TotalPaginas)
        {
            PaginaAtual++;
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task UltimaPaginaAsync()
    {
        PaginaAtual = TotalPaginas;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task AplicarFiltrosAsync()
    {
        PaginaAtual = 1;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task LimparFiltrosAsync()
    {
        DataInicio = DateTime.Today.AddDays(-30);
        DataFim = DateTime.Today;
        FiltroFormula = "";
        FiltroCodigo = null;
        FiltroNumero = null;
        FiltroTipoAmendoim = "";
        PaginaAtual = 1;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task ExportarExcelAsync()
    {
        // TODO: Implementar exportação Excel
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ExportarPdfAsync()
    {
        // TODO: Implementar exportação PDF
        await Task.CompletedTask;
    }

    partial void OnTipoVisualizacaoChanged(string value)
    {
        PaginaAtual = 1;
        _ = LoadDataAsync();
    }
}

// Classes auxiliares
public class RelatorioRow
{
    public Guid Id { get; set; }
    public string Dia { get; set; } = "";
    public string Hora { get; set; } = "";
    public string Formula { get; set; } = "";
    public int Codigo { get; set; }
    public int Numero { get; set; }
    public decimal[] Produtos { get; set; } = Array.Empty<decimal>();
    public decimal Total { get; set; }
}

public class AmendoimRow
{
    public int Id { get; set; }
    public string Tipo { get; set; } = "";
    public string Dia { get; set; } = "";
    public string Hora { get; set; } = "";
    public string CodigoProduto { get; set; } = "";
    public string NomeProduto { get; set; } = "";
    public decimal Peso { get; set; }
    public string Balanca { get; set; } = "";

    public string TipoFormatado => Tipo == "entrada" ? "📥 Entrada" : "📤 Saída";
    public string PesoFormatado => $"{Peso:N3} kg";
}

public class ColunaConfig
{
    public string Nome { get; set; } = "";
    public bool Visivel { get; set; } = true;
    public double Largura { get; set; } = 100;
}