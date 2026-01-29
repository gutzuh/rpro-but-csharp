namespace RPRO.Core.DTOs;

public class DashboardRacaoDto
{
    public int TotalBatidas { get; set; }
    public int FormulasUnicas { get; set; }
    public decimal TotalKg { get; set; }
    public List<ChartItem> PorFormula { get; set; } = new();
    public List<ChartItem> PorDia { get; set; } = new();
    public List<ChartItem> PorProduto { get; set; } = new();
    public DateTime PrimeiraData { get; set; }
    public DateTime UltimaData { get; set; }

    public class ChartItem
    {
        public string Label { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public int Value { get; set; }
        public decimal Valor { get; set; }
        public decimal Quantidade { get; set; }
    }
}
