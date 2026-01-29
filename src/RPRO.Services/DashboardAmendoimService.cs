using RPRO.Core.DTOs;
using RPRO.Core.Interfaces;

namespace RPRO.Services;

public class DashboardAmendoimService
{
    private readonly IAmendoimRepository _amendoimRepo;

    public DashboardAmendoimService(IAmendoimRepository amendoimRepo)
    {
        _amendoimRepo = amendoimRepo;
    }

    /// <summary>
    /// Obtém dados consolidados para o dashboard de amendoim
    /// </summary>
    public async Task<DashboardAmendoimDto> GetDashboardDataAsync(DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        var dashboard = new DashboardAmendoimDto();

        // Métricas de rendimento
        var (pesoEntrada, pesoSaida) = await _amendoimRepo.GetMetricasRendimentoAsync(dataInicio, dataFim);
        dashboard.PesoEntrada = pesoEntrada;
        dashboard.PesoSaida = pesoSaida;

        // Buscar todos os registros para análises
        var (registros, _) = await _amendoimRepo.GetPaginatedAsync(
            page: 1,
            pageSize: 50000,
            dataInicio: dataInicio,
            dataFim: dataFim);

        var lista = registros.ToList();

        // Entrada/Saída por Horário
        dashboard.EntradaSaidaPorHorario = CalcularFluxoPorHorario(lista);

        // Rendimento por Dia
        dashboard.RendimentoPorDia = await CalcularRendimentoPorDiaAsync(dataInicio, dataFim);

        // Fluxo Semanal
        dashboard.FluxoSemanal = CalcularFluxoSemanal(lista);

        // Eficiência por Turno
        dashboard.EficienciaPorTurno = CalcularEficienciaPorTurno(lista);

        // Perda Acumulada
        dashboard.PerdaAcumulada = CalcularPerdaAcumulada(lista);

        return dashboard;
    }

    private List<FluxoHorarioItem> CalcularFluxoPorHorario(List<Core.Entities.Amendoim> registros)
    {
        var porHora = new Dictionary<int, (decimal Entrada, decimal Saida)>();

        foreach (var reg in registros)
        {
            if (int.TryParse(reg.Hora?.Split(':')[0], out var hora))
            {
                if (!porHora.ContainsKey(hora))
                    porHora[hora] = (0, 0);

                var atual = porHora[hora];
                if (reg.IsEntrada)
                    porHora[hora] = (atual.Entrada + reg.Peso, atual.Saida);
                else
                    porHora[hora] = (atual.Entrada, atual.Saida + reg.Peso);
            }
        }

        return Enumerable.Range(0, 24)
            .Select(h => new FluxoHorarioItem
            {
                Hora = h,
                Entrada = porHora.TryGetValue(h, out var val) ? val.Entrada : 0,
                Saida = porHora.TryGetValue(h, out val) ? val.Saida : 0
            })
            .Where(x => x.Entrada > 0 || x.Saida > 0)
            .ToList();
    }

    private async Task<List<RendimentoDiaItem>> CalcularRendimentoPorDiaAsync(DateTime? dataInicio, DateTime? dataFim)
    {
        var fluxoPorDia = await _amendoimRepo.GetFluxoPorDiaAsync(dataInicio, dataFim);

        return fluxoPorDia
            .Select(x => new RendimentoDiaItem
            {
                Dia = x.Dia,
                Entrada = x.Entrada,
                Saida = x.Saida
            })
            .Take(30)
            .ToList();
    }

    private List<FluxoSemanalItem> CalcularFluxoSemanal(List<Core.Entities.Amendoim> registros)
    {
        var diasSemana = new[] { "Domingo", "Segunda", "Terça", "Quarta", "Quinta", "Sexta", "Sábado" };
        var porDiaSemana = new Dictionary<int, (decimal Entrada, decimal Saida)>();

        foreach (var reg in registros)
        {
            var data = ParseData(reg.Dia);
            if (data.HasValue)
            {
                var diaSemana = (int)data.Value.DayOfWeek;
                if (!porDiaSemana.ContainsKey(diaSemana))
                    porDiaSemana[diaSemana] = (0, 0);

                var atual = porDiaSemana[diaSemana];
                if (reg.IsEntrada)
                    porDiaSemana[diaSemana] = (atual.Entrada + reg.Peso, atual.Saida);
                else
                    porDiaSemana[diaSemana] = (atual.Entrada, atual.Saida + reg.Peso);
            }
        }

        return Enumerable.Range(0, 7)
            .Select(i => new FluxoSemanalItem
            {
                DiaSemana = diasSemana[i],
                Entrada = porDiaSemana.TryGetValue(i, out var val) ? val.Entrada : 0,
                Saida = porDiaSemana.TryGetValue(i, out val) ? val.Saida : 0
            })
            .ToList();
    }

    private List<EficienciaTurnoItem> CalcularEficienciaPorTurno(List<Core.Entities.Amendoim> registros)
    {
        var turnos = new Dictionary<string, (decimal Entrada, decimal Saida)>
        {
            ["Manhã (06-12)"] = (0, 0),
            ["Tarde (12-18)"] = (0, 0),
            ["Noite (18-00)"] = (0, 0),
            ["Madrugada (00-06)"] = (0, 0)
        };

        foreach (var reg in registros)
        {
            if (int.TryParse(reg.Hora?.Split(':')[0], out var hora))
            {
                var turno = hora switch
                {
                    >= 6 and < 12 => "Manhã (06-12)",
                    >= 12 and < 18 => "Tarde (12-18)",
                    >= 18 => "Noite (18-00)",
                    _ => "Madrugada (00-06)"
                };

                var atual = turnos[turno];
                if (reg.IsEntrada)
                    turnos[turno] = (atual.Entrada + reg.Peso, atual.Saida);
                else
                    turnos[turno] = (atual.Entrada, atual.Saida + reg.Peso);
            }
        }

        return turnos.Select(t => new EficienciaTurnoItem
        {
            Turno = t.Key,
            Entrada = t.Value.Entrada,
            Saida = t.Value.Saida
        }).ToList();
    }

    private List<PerdaAcumuladaItem> CalcularPerdaAcumulada(List<Core.Entities.Amendoim> registros)
    {
        var porDia = new Dictionary<string, (decimal Entrada, decimal Saida)>();

        foreach (var reg in registros)
        {
            if (!porDia.ContainsKey(reg.Dia))
                porDia[reg.Dia] = (0, 0);

            var atual = porDia[reg.Dia];
            if (reg.IsEntrada)
                porDia[reg.Dia] = (atual.Entrada + reg.Peso, atual.Saida);
            else
                porDia[reg.Dia] = (atual.Entrada, atual.Saida + reg.Peso);
        }

        var resultado = new List<PerdaAcumuladaItem>();
        decimal acumulada = 0;

        foreach (var dia in porDia.OrderBy(x => ParseData(x.Key)))
        {
            var perdaDiaria = dia.Value.Entrada - dia.Value.Saida;
            acumulada += perdaDiaria;

            resultado.Add(new PerdaAcumuladaItem
            {
                Dia = dia.Key,
                PerdaDiaria = perdaDiaria,
                PerdaAcumulada = acumulada
            });
        }

        return resultado.TakeLast(30).ToList();
    }

    private DateTime? ParseData(string? dataStr)
    {
        if (string.IsNullOrEmpty(dataStr)) return null;

        // Formato DD-MM-YY
        if (DateTime.TryParseExact(dataStr, "dd-MM-yy",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var data1))
            return data1;

        // Formato DD/MM/YYYY
        if (DateTime.TryParseExact(dataStr, "dd/MM/yyyy",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var data2))
            return data2;

        return null;
    }
}