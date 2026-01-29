namespace RPRO.Core.DTOs;

public class DashboardAmendoimDto
{
    public decimal PesoEntrada { get; set; }
    public decimal PesoSaida { get; set; }
    public decimal Perda => PesoEntrada - PesoSaida;
    public decimal PerdaPercentual => PesoEntrada > 0 ? (Perda / PesoEntrada) * 100 : 0;
    public decimal RendimentoPercentual => PesoEntrada > 0 ? (PesoSaida / PesoEntrada) * 100 : 0;
    
    public List<FluxoHorarioItem> EntradaSaidaPorHorario { get; set; } = new();
    public List<RendimentoDiaItem> RendimentoPorDia { get; set; } = new();
    public List<FluxoSemanalItem> FluxoSemanal { get; set; } = new();
    public List<EficienciaTurnoItem> EficienciaPorTurno { get; set; } = new();
    public List<PerdaAcumuladaItem> PerdaAcumulada { get; set; } = new();
}

public class FluxoHorarioItem
{
    public int Hora { get; set; }
    public decimal Entrada { get; set; }
    public decimal Saida { get; set; }
}

public class RendimentoDiaItem
{
    public string Dia { get; set; } = "";
    public decimal Entrada { get; set; }
    public decimal Saida { get; set; }
    public decimal Rendimento => Entrada > 0 ? (Saida / Entrada) * 100 : 0;
}

public class FluxoSemanalItem
{
    public string DiaSemana { get; set; } = "";
    public decimal Entrada { get; set; }
    public decimal Saida { get; set; }
}

public class EficienciaTurnoItem
{
    public string Turno { get; set; } = "";
    public decimal Entrada { get; set; }
    public decimal Saida { get; set; }
    public decimal Rendimento => Entrada > 0 ? (Saida / Entrada) * 100 : 0;
}

public class PerdaAcumuladaItem
{
    public string Dia { get; set; } = "";
    public decimal PerdaDiaria { get; set; }
    public decimal PerdaAcumulada { get; set; }
}