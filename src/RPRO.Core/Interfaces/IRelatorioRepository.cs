using RPRO.Core.Entities;

namespace RPRO.Core.Interfaces;

public interface IRelatorioRepository
{
    Task<IEnumerable<Relatorio>> GetAllAsync();
    Task<Relatorio?> GetByIdAsync(Guid id);
    Task<(IEnumerable<Relatorio> Items, int Total)> GetPaginatedAsync(
        int page, 
        int pageSize, 
        DateTime? dataInicio = null, 
        DateTime? dataFim = null, 
        string? formula = null,
        int? codigo = null,
        int? numero = null);
    Task<int> InsertAsync(Relatorio relatorio);
    Task<int> InsertManyAsync(IEnumerable<Relatorio> relatorios);
    Task<int> UpdateAsync(Relatorio relatorio);
    Task<int> DeleteAsync(Guid id);
    Task<int> GetCountAsync();
}