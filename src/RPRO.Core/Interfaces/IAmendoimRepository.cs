using RPRO.Core.Entities;

namespace RPRO.Core.Interfaces;

public interface IAmendoimRepository
{
    Task<IEnumerable<Amendoim>> GetAllAsync();
    Task<Amendoim?> GetByIdAsync(int id);
    Task<(IEnumerable<Amendoim> Items, int Total)> GetPaginatedAsync(
        int page,
        int pageSize,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        string? tipo = null,
        string? codigoProduto = null);
    Task<int> InsertAsync(Amendoim amendoim);
    Task<int> InsertManyAsync(IEnumerable<Amendoim> amendoins);
    Task<bool> ExistsAsync(string tipo, string dia, string hora, string codigoProduto, decimal peso);
    
    // Métricas
    Task<(decimal PesoEntrada, decimal PesoSaida)> GetMetricasRendimentoAsync(
        DateTime? dataInicio = null, 
        DateTime? dataFim = null);
    Task<IEnumerable<(string Dia, decimal Entrada, decimal Saida)>> GetFluxoPorDiaAsync(
        DateTime? dataInicio = null, 
        DateTime? dataFim = null);
}