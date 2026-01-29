using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using RPRO.Core.DTOs;
using RPRO.Services;
using SkiaSharp;

namespace RPRO.App.ViewModels;

public partial class DashboardRacaoViewModel : ObservableObject
{
    private readonly DashboardRacaoService _service;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = "";

    // Métricas
    [ObservableProperty]
    private decimal _totalKg;

    [ObservableProperty]
    private int _totalBatidas;

    [ObservableProperty]
    private int _formulasUnicas;

    [ObservableProperty]
    private string _periodo = "";

    // Filtros
    [ObservableProperty]
    private DateTime _dataInicio = DateTime.Today.AddDays(-30);

    [ObservableProperty]
    private DateTime _dataFim = DateTime.Today;

    // Gráficos
    [ObservableProperty]
    private ISeries[] _graficoPorFormula = Array.Empty<ISeries>();

    [ObservableProperty]
    private ISeries[] _graficoPorDia = Array.Empty<ISeries>();

    [ObservableProperty]
    private ISeries[] _graficoPorProduto = Array.Empty<ISeries>();

    [ObservableProperty]
    private ISeries[] _graficoSemanal = Array.Empty<ISeries>();

    [ObservableProperty]
    private Axis[] _xAxesDia = Array.Empty<Axis>();

    [ObservableProperty]
    private Axis[] _xAxesSemanal = Array.Empty<Axis>();

    // Cores
    private static readonly SKColor[] _cores = new[]
    {
        SKColor.Parse("#E53935"), // Vermelho
        SKColor.Parse("#1E88E5"), // Azul
        SKColor.Parse("#43A047"), // Verde
        SKColor.Parse("#FB8C00"), // Laranja
        SKColor.Parse("#8E24AA"), // Roxo
        SKColor.Parse("#00ACC1"), // Ciano
        SKColor.Parse("#FFB300"), // Amarelo
        SKColor.Parse("#6D4C41"), // Marrom
        SKColor.Parse("#546E7A"), // Cinza
        SKColor.Parse("#D81B60"), // Rosa
    };

    public DashboardRacaoViewModel(DashboardRacaoService service)
    {
        _service = service;
        _ = LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = "";

            var dados = await _service.GetDashboardDataAsync(DataInicio, DataFim);

            // Atualizar métricas
            TotalKg = dados.TotalKg;
            TotalBatidas = dados.TotalBatidas;
            FormulasUnicas = dados.FormulasUnicas;

            if (dados.PrimeiraData.HasValue && dados.UltimaData.HasValue)
            {
                Periodo = $"{dados.PrimeiraData:dd/MM/yyyy} - {dados.UltimaData:dd/MM/yyyy}";
            }

            // Gráfico de Pizza - Por Fórmula
            GraficoPorFormula = dados.PorFormula
                .Select((item, index) => new PieSeries<decimal>
                {
                    Name = item.Nome,
                    Values = new[] { item.Valor },
                    Fill = new SolidColorPaint(_cores[index % _cores.Length]),
                    DataLabelsSize = 12,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsFormatter = p => $"{p.Model:N0} kg"
                })
                .Cast<ISeries>()
                .ToArray();

            // Gráfico de Barras - Por Dia
            GraficoPorDia = new ISeries[]
            {
                new ColumnSeries<decimal>
                {
                    Name = "Produção (kg)",
                    Values = dados.PorDia.Select(x => x.Valor).ToArray(),
                    Fill = new SolidColorPaint(SKColor.Parse("#E53935")),
                    MaxBarWidth = 40
                }
            };

            XAxesDia = new Axis[]
            {
                new Axis
                {
                    Labels = dados.PorDia.Select(x => x.Nome).ToArray(),
                    LabelsRotation = 45
                }
            };

            // Gráfico de Barras Horizontal - Por Produto
            GraficoPorProduto = new ISeries[]
            {
                new RowSeries<decimal>
                {
                    Name = "Consumo (kg)",
                    Values = dados.PorProduto.Select(x => x.Valor).ToArray(),
                    Fill = new SolidColorPaint(SKColor.Parse("#1E88E5")),
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsSize = 11,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.End,
                    DataLabelsFormatter = p => $"{p.Model:N0} kg"
                }
            };

            // Gráfico Semanal
            var semanal = await _service.GetProducaoSemanalAsync();
            GraficoSemanal = new ISeries[]
            {
                new ColumnSeries<decimal>
                {
                    Name = "Produção",
                    Values = semanal.Select(x => x.Valor).ToArray(),
                    Fill = new SolidColorPaint(SKColor.Parse("#43A047"))
                }
            };

            XAxesSemanal = new Axis[]
            {
                new Axis
                {
                    Labels = semanal.Select(x => x.Nome).ToArray()
                }
            };
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

    partial void OnDataInicioChanged(DateTime value) => _ = LoadDataAsync();
    partial void OnDataFimChanged(DateTime value) => _ = LoadDataAsync();
}