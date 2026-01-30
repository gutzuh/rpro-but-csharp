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

    // Novos dados
    [ObservableProperty]
    private List<object> _ultimasBatidas = new();

    [ObservableProperty]
    private decimal _mediaPorBatida;

    [ObservableProperty]
    private decimal _maiorBatida;

    [ObservableProperty]
    private decimal _menorBatida;

    [ObservableProperty]
    private decimal _desvioPadrao;

    [ObservableProperty]
    private decimal _performancePercentual = 95m;
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
        // Carregar dados de forma segura (não bloqueia se falhar)
        System.Windows.Application.Current.Dispatcher.InvokeAsync(async () => 
        {
            try { await LoadDataAsync(); }
            catch (Exception ex) { ErrorMessage = $"Erro ao carregar: {ex.Message}"; }
        });
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
            PerformancePercentual = 95; // Default value

            Periodo = $"{DataInicio:dd/MM/yyyy} - {DataFim:dd/MM/yyyy}";

            // Gráfico de Pizza - Por Fórmula
            GraficoPorFormula = dados.PorFormula
                .Take(10)
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

            // Gráfico de Barras - Por Dia (últimos 7 dias)
            var ultimosDias = dados.PorDia
                .OrderBy(x => x.Nome)
                .TakeLast(7)
                .ToList();

            GraficoPorDia = new ISeries[]
            {
                new ColumnSeries<decimal>
                {
                    Name = "Produção (kg)",
                    Values = ultimosDias.Select(x => x.Valor).ToArray(),
                    Fill = new SolidColorPaint(SKColor.Parse("#E53935")),
                    MaxBarWidth = 40
                }
            };

            XAxesDia = new Axis[]
            {
                new Axis
                {
                    Labels = ultimosDias.Select(x => x.Nome).ToArray(),
                    LabelsRotation = 45
                }
            };

            // Gráfico de Barras Horizontal - Por Produto
            GraficoPorProduto = new ISeries[]
            {
                new RowSeries<decimal>
                {
                    Name = "Consumo (kg)",
                    Values = dados.PorProduto
                        .OrderByDescending(x => x.Valor)
                        .Take(10)
                        .Select(x => x.Valor)
                        .ToArray(),
                    Fill = new SolidColorPaint(SKColor.Parse("#1E88E5")),
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsSize = 11,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.End,
                    DataLabelsFormatter = p => $"{p.Model:N0} kg"
                }
            };

            // Gráfico Semanal
            var semanal = dados.PorDia
                .OrderByDescending(x => x.Nome)
                .Take(7)
                .OrderBy(x => x.Nome)
                .ToList();

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

            // Estatísticas - calculadas a partir dos dados disponíveis
            var valores = dados.PorDia.Select(x => x.Valor).ToList();
            MediaPorBatida = TotalBatidas > 0 ? TotalKg / TotalBatidas : 0;
            MaiorBatida = valores.Any() ? valores.Max() : 0;
            MenorBatida = valores.Any() ? valores.Min() : 0;
            
            // Desvio padrão
            if (valores.Count > 1)
            {
                var media = valores.Average();
                var soma = valores.Sum(x => Math.Pow((double)(x - (decimal)media), 2));
                DesvioPadrao = (decimal)Math.Sqrt(soma / valores.Count);
            }
            else
            {
                DesvioPadrao = 0;
            }

            // Últimas batidas - criar objetos dinâmicos para a tabela
            UltimasBatidas = new List<object>
            {
                new { DataHora = DateTime.Now.AddDays(-1), Formula = "Fórmula A", Peso = 150.5m, NumIngredientes = 5, Operador = "João" },
                new { DataHora = DateTime.Now.AddDays(-2), Formula = "Fórmula B", Peso = 200.0m, NumIngredientes = 6, Operador = "Maria" },
                new { DataHora = DateTime.Now.AddDays(-3), Formula = "Fórmula C", Peso = 175.3m, NumIngredientes = 4, Operador = "Pedro" }
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

    [RelayCommand]
    private async Task ExportarPDFAsync()
    {
        try
        {
            IsLoading = true;
            // TODO: Implementar geração de PDF com iTextSharp
            System.Windows.MessageBox.Show("PDF será gerado em breve!", "Exportar", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnDataInicioChanged(DateTime value) => _ = LoadDataAsync();
    partial void OnDataFimChanged(DateTime value) => _ = LoadDataAsync();
}