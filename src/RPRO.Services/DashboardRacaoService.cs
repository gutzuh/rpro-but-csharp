using RPRO.Core.DTOs;
using RPRO.Core.Interfaces;

namespace RPRO.Services;

public class DashboardRacaoService
{
    private readonly IRelatorioRepository _relatorioRepo;
    private readonly IMateriaPrimaRepository _materiaPrimaRepo;

    public DashboardRacaoService(
        IRelatorioRepository relatorioRepo,
        IMateriaPrimaRepository materiaPrimaRepo)
    {
        _relatorioRepo = relatorioRepo;
        _materiaPrimaRepo = materiaPrimaRepo;
    }

    /// <summary>
    /// Obtém dados consolidados para o dashboard de ração
    /// </summary>
    public async Task<DashboardRacaoDto> GetDashboardDataAsync(DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        // Buscar todos os relatórios do período
        var (relatorios, total) = await _relatorioRepo.GetPaginatedAsync(
            page: 1,
            pageSize: 10000, // Pegar todos para cálculos
            dataInicio: dataInicio,
            dataFim: dataFim);

        var listaRelatorios = relatorios.ToList();
        var materiasPrimas = (await _materiaPrimaRepo.GetAllAsync()).ToDictionary(m => m.Num);

        var dashboard = new DashboardRacaoDto
        {
            TotalBatidas = listaRelatorios.Count,
            FormulasUnicas = listaRelatorios.Select(r => r.Nome).Distinct().Count()
        };

        // Calcular total produzido
        decimal totalKg = 0;
        var porFormula = new Dictionary<string, (decimal Valor, int Qtd)>();
        var porDia = new Dictionary<string, decimal>();
        var porProduto = new Dictionary<int, decimal>();

        foreach (var rel in listaRelatorios)
        {
            var produtos = rel.GetProdutosArray();
            decimal totalRelatorio = 0;

            for (int i = 0; i < produtos.Length; i++)
            {
                var prodNum = i + 1;
                var valor = produtos[i];

                // Converter para kg se necessário
                if (materiasPrimas.TryGetValue(prodNum, out var mp))
                {
                    if (mp.Medida == 0) // gramas
                        valor = valor / 1000;

                    if (!mp.IgnorarCalculos && mp.Ativo)
                    {
                        totalRelatorio += valor;

                        // Acumular por produto
                        if (!porProduto.ContainsKey(prodNum))
                            porProduto[prodNum] = 0;
                        porProduto[prodNum] += valor;
                    }
                }
                else
                {
                    totalRelatorio += valor;
                }
            }

            totalKg += totalRelatorio;

            // Agrupar por fórmula
            var nomeFormula = rel.Nome ?? "Sem Nome";
            if (!porFormula.ContainsKey(nomeFormula))
                porFormula[nomeFormula] = (0, 0);
            var atual = porFormula[nomeFormula];
            porFormula[nomeFormula] = (atual.Valor + totalRelatorio, atual.Qtd + 1);

            // Agrupar por dia
            var dia = rel.Dia ?? "Sem Data";
            if (!porDia.ContainsKey(dia))
                porDia[dia] = 0;
            porDia[dia] += totalRelatorio;
        }

        dashboard.TotalKg = totalKg;

        // Top 10 fórmulas
        dashboard.PorFormula = porFormula
            .OrderByDescending(x => x.Value.Valor)
            .Take(10)
            .Select(x => new ChartItem
            {
                Nome = x.Key,
                Valor = x.Value.Valor,
                Quantidade = x.Value.Qtd
            })
            .ToList();

        // Últimos 7 dias
        dashboard.PorDia = porDia
            .OrderByDescending(x => x.Key)
            .Take(7)
            .Reverse()
            .Select(x => new ChartItem
            {
                Nome = x.Key,
                Valor = x.Value
            })
            .ToList();

        // Top 10 produtos
        dashboard.PorProduto = porProduto
            .OrderByDescending(x => x.Value)
            .Take(10)
            .Select(x => new ChartItem
            {
                Nome = materiasPrimas.TryGetValue(x.Key, out var mp) ? mp.Produto : $"Produto {x.Key}",
                Valor = x.Value
            })
            .ToList();

        // Datas
        if (listaRelatorios.Any())
        {
            var datas = listaRelatorios
                .Where(r => !string.IsNullOrEmpty(r.Dia))
                .Select(r => ParseData(r.Dia!))
                .Where(d => d.HasValue)
                .Select(d => d!.Value)
                .ToList();

            if (datas.Any())
            {
                dashboard.PrimeiraData = datas.Min();
                dashboard.UltimaData = datas.Max();
            }
        }

        return dashboard;
    }

    /// <summary>
    /// Obtém dados para gráfico semanal
    /// </summary>
    public async Task<List<ChartItem>> GetProducaoSemanalAsync(DateTime? weekStart = null)
    {
        var inicio = weekStart ?? DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
        var fim = inicio.AddDays(6);

        var (relatorios, _) = await _relatorioRepo.GetPaginatedAsync(
            page: 1,
            pageSize: 10000,
            dataInicio: inicio,
            dataFim: fim);

        var diasSemana = new[] { "Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb" };
        var porDiaSemana = new decimal[7];

        foreach (var rel in relatorios)
        {
            var data = ParseData(rel.Dia);
            if (data.HasValue)
            {
                var diaSemana = (int)data.Value.DayOfWeek;
                porDiaSemana[diaSemana] += rel.TotalProdutos;
            }
        }

        return diasSemana.Select((nome, i) => new ChartItem
        {
            Nome = nome,
            Valor = porDiaSemana[i]
        }).ToList();
    }

    private DateTime? ParseData(string? dataStr)
    {
        if (string.IsNullOrEmpty(dataStr)) return null;

        // Tentar formato DD/MM/YYYY
        if (DateTime.TryParseExact(dataStr, "dd/MM/yyyy",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var data1))
            return data1;

        // Tentar formato YYYY-MM-DD
        if (DateTime.TryParseExact(dataStr, "yyyy-MM-dd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var data2))
            return data2;

        return null;
    }
}