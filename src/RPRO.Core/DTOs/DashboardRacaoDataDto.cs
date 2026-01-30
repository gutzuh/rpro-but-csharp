namespace RPRO.Core.DTOs;

public class DashboardRacaoDataDto
{
    public decimal TotalKg { get; set; }
    public int TotalBatidas { get; set; }
    public int FormulasUnicas { get; set; }
    public DateTime PrimeiraData { get; set; }
    public DateTime UltimaData { get; set; }
    
    // Estatísticas
    public decimal MaiorBatida { get; set; }
    public decimal MenorBatida { get; set; }
    public decimal DesvioPadrao { get; set; }
    public decimal MediaPorBatida => TotalBatidas > 0 ? TotalKg / TotalBatidas : 0;

    // Gráficos
    public List<GraficoItemDto> PorFormula { get; set; } = new();
    public List<GraficoItemDto> PorDia { get; set; } = new();
    public List<GraficoItemDto> PorProduto { get; set; } = new();
    public List<GraficoItemDto> PorSemana { get; set; } = new();

    // Dados da tabela
    public List<UltimaBatidaDto> UltimasBatidas { get; set; } = new();
}

public class GraficoItemDto
{
    public string Nome { get; set; } = "";
    public decimal Valor { get; set; }
}

public class UltimaBatidaDto
{
    public DateTime DataHora { get; set; }
    public string Formula { get; set; } = "";
    public decimal Peso { get; set; }
    public int NumIngredientes { get; set; }
    public string Operador { get; set; } = "";
}
