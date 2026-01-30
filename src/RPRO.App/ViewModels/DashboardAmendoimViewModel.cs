using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using RPRO.Core.DTOs;
using RPRO.Services;
using SkiaSharp;

namespace RPRO.App.ViewModels;

public partial class DashboardAmendoimViewModel : ObservableObject
{
    private readonly DashboardAmendoimService _service;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = "";

    // Métricas
    [ObservableProperty]
    private decimal _pesoEntrada;

    [ObservableProperty]
    private decimal _pesoSaida;

    [ObservableProperty]
    private decimal _perda;

    [ObservableProperty]
    private decimal _rendimentoPercentual;

    [ObservableProperty]
    private decimal _perdaPercentual;

    // Filtros
    [ObservableProperty]
    private DateTime _dataInicio = DateTime.Today.AddDays(-30);

    [ObservableProperty]
    private DateTime _dataFim = DateTime.Today;

    // Gráficos
    [ObservableProperty]
    private ISeries[] _graficoEntradaSaidaHorario = Array.Empty<ISeries>();

    [ObservableProperty]
    private ISeries[] _graficoRendimentoDia = Array.Empty<ISeries>();

    [ObservableProperty]
    private ISeries[] _graficoFluxoSemanal = Array.Empty<ISeries>();

    [ObservableProperty]
    private ISeries[] _graficoEficienciaTurno = Array.Empty<ISeries>();

    [ObservableProperty]
    private ISeries[] _graficoPerdaAcumulada = Array.Empty<ISeries>();

    // Eixos
    [ObservableProperty]
    private Axis[] _xAxesHorario = Array.Empty<Axis>();

    [ObservableProperty]
    private Axis[] _xAxesDia = Array.Empty<Axis>();

    [ObservableProperty]
    private Axis[] _xAxesSemanal = Array.Empty<Axis>();

    [ObservableProperty]
    private Axis[] _xAxesTurno = Array.Empty<Axis>();

    [ObservableProperty]
    private Axis[] _xAxesPerda = Array.Empty<Axis>();

    // Cores
    private static readonly SKColor CorEntrada = SKColor.Parse("#43A047"); // Verde
    private static readonly SKColor CorSaida = SKColor.Parse("#1E88E5");   // Azul
    private static readonly SKColor CorRendimento = SKColor.Parse("#FB8C00"); // Laranja
    private static readonly SKColor CorPerda = SKColor.Parse("#E53935");   // Vermelho

    public DashboardAmendoimViewModel(DashboardAmendoimService service)
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
            PesoEntrada = dados.PesoEntrada;
            PesoSaida = dados.PesoSaida;
            Perda = dados.Perda;
            RendimentoPercentual = dados.RendimentoPercentual;
            PerdaPercentual = dados.PerdaPercentual;

            // Gráfico Entrada/Saída por Horário
            GraficoEntradaSaidaHorario = new ISeries[]
            {
                new ColumnSeries<decimal>
                {
                    Name = "Entrada (kg)",
                    Values = dados.EntradaSaidaPorHorario.Select(x => x.Entrada).ToArray(),
                    Fill = new SolidColorPaint(CorEntrada),
                    MaxBarWidth = 20
                },
                new ColumnSeries<decimal>
                {
                    Name = "Saída (kg)",
                    Values = dados.EntradaSaidaPorHorario.Select(x => x.Saida).ToArray(),
                    Fill = new SolidColorPaint(CorSaida),
                    MaxBarWidth = 20
                }
            };

            XAxesHorario = new Axis[]
            {
                new Axis
                {
                    Labels = dados.EntradaSaidaPorHorario.Select(x => $"{x.Hora}h").ToArray()
                }
            };

            // Gráfico Rendimento por Dia (ComposedChart - Barras + Linha)
            GraficoRendimentoDia = new ISeries[]
            {
                new ColumnSeries<decimal>
                {
                    Name = "Entrada (kg)",
                    Values = dados.RendimentoPorDia.Select(x => x.Entrada).ToArray(),
                    Fill = new SolidColorPaint(CorEntrada),
                    MaxBarWidth = 15
                },
                new ColumnSeries<decimal>
                {
                    Name = "Saída (kg)",
                    Values = dados.RendimentoPorDia.Select(x => x.Saida).ToArray(),
                    Fill = new SolidColorPaint(CorSaida),
                    MaxBarWidth = 15
                },
                new LineSeries<decimal>
                {
                    Name = "Rendimento (%)",
                    Values = dados.RendimentoPorDia.Select(x => x.Rendimento).ToArray(),
                    Stroke = new SolidColorPaint(CorRendimento, 3),
                    GeometrySize = 8,
                    GeometryFill = new SolidColorPaint(CorRendimento),
                    GeometryStroke = new SolidColorPaint(SKColors.White, 2),
                    ScalesYAt = 1 // Eixo Y secundário
                }
            };

            XAxesDia = new Axis[]
            {
                new Axis
                {
                    Labels = dados.RendimentoPorDia.Select(x => x.Dia).ToArray(),
                    LabelsRotation = 45
                }
            };

            // Gráfico Fluxo Semanal
            GraficoFluxoSemanal = new ISeries[]
            {
                new ColumnSeries<decimal>
                {
                    Name = "Entrada",
                    Values = dados.FluxoSemanal.Select(x => x.Entrada).ToArray(),
                    Fill = new SolidColorPaint(CorEntrada)
                },
                new ColumnSeries<decimal>
                {
                    Name = "Saída",
                    Values = dados.FluxoSemanal.Select(x => x.Saida).ToArray(),
                    Fill = new SolidColorPaint(CorSaida)
                }
            };

            XAxesSemanal = new Axis[]
            {
                new Axis
                {
                    Labels = dados.FluxoSemanal.Select(x => x.DiaSemana.Substring(0, 3)).ToArray()
                }
            };

            // Gráfico Eficiência por Turno
            GraficoEficienciaTurno = new ISeries[]
            {
                new ColumnSeries<decimal>
                {
                    Name = "Entrada",
                    Values = dados.EficienciaPorTurno.Select(x => x.Entrada).ToArray(),
                    Fill = new SolidColorPaint(CorEntrada)
                },
                new ColumnSeries<decimal>
                {
                    Name = "Saída",
                    Values = dados.EficienciaPorTurno.Select(x => x.Saida).ToArray(),
                    Fill = new SolidColorPaint(CorSaida)
                }
            };

            XAxesTurno = new Axis[]
            {
                new Axis
                {
                    Labels = dados.EficienciaPorTurno.Select(x => x.Turno.Split('(')[0].Trim()).ToArray()
                }
            };

            // Gráfico Perda Acumulada
            GraficoPerdaAcumulada = new ISeries[]
            {
                new ColumnSeries<decimal>
                {
                    Name = "Perda Diária (kg)",
                    Values = dados.PerdaAcumulada.Select(x => x.PerdaDiaria).ToArray(),
                    Fill = new SolidColorPaint(CorPerda.WithAlpha(150)),
                    MaxBarWidth = 20
                },
                new LineSeries<decimal>
                {
                    Name = "Perda Acumulada (kg)",
                    Values = dados.PerdaAcumulada.Select(x => x.PerdaAcumulada).ToArray(),
                    Stroke = new SolidColorPaint(CorPerda, 3),
                    GeometrySize = 6,
                    GeometryFill = new SolidColorPaint(CorPerda),
                    Fill = null
                }
            };

            XAxesPerda = new Axis[]
            {
                new Axis
                {
                    Labels = dados.PerdaAcumulada.Select(x => x.Dia).ToArray(),
                    LabelsRotation = 45
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