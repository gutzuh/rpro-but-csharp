using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

public partial class DashboardRacaoViewModel : ObservableObject
{
    private readonly DashboardRacaoService _service;
    
    [ObservableProperty]
    private ISeries[] _producaoPorFormula = Array.Empty<ISeries>();
    
    [ObservableProperty]
    private ISeries[] _producaoSemanal = Array.Empty<ISeries>();
    
    [ObservableProperty]
    private decimal _totalProduzido;
    
    [ObservableProperty]
    private int _totalBatidas;
    
    public async Task LoadDataAsync(DateTime inicio, DateTime fim)
    {
        var dados = await _service.GetDashboardDataAsync(inicio, fim);
        
        TotalProduzido = dados.TotalKg;
        TotalBatidas = dados.TotalBatidas;
        
        // Gráfico de Pizza - Produção por Fórmula
        ProducaoPorFormula = dados.PorFormula
            .Select(x => new PieSeries<decimal>
            {
                Name = x.Nome,
                Values = new[] { x.Valor }
            })
            .ToArray();
        
        // Gráfico de Barras - Produção Semanal
        ProducaoSemanal = new ISeries[]
        {
            new ColumnSeries<decimal>
            {
                Name = "Produção (kg)",
                Values = dados.PorDia.Select(x => x.Valor).ToArray()
            }
        };
    }
}