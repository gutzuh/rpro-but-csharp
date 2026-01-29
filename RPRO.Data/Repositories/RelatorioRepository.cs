public class RelatorioRepository : IRelatorioRepository
{
    private readonly DatabaseContext _db;
    
    public RelatorioRepository(DatabaseContext db) => _db = db;
    
    public async Task<(IEnumerable<Relatorio> Items, int Total)> GetPaginatedAsync(
        int page, int pageSize, DateTime? dataInicio, DateTime? dataFim, string? formula)
    {
        var sql = @"
            SELECT * FROM relatorio 
            WHERE 1=1
            @DataFilter
            @FormulaFilter
            ORDER BY Dia DESC, Hora DESC
            LIMIT @Offset, @PageSize";
        
        var countSql = "SELECT COUNT(*) FROM relatorio WHERE 1=1 @DataFilter @FormulaFilter";
        
        var parameters = new DynamicParameters();
        parameters.Add("Offset", (page - 1) * pageSize);
        parameters.Add("PageSize", pageSize);
        
        // Adicionar filtros dinamicamente...
        
        var items = await _db.QueryAsync<Relatorio>(sql, parameters);
        var total = await _db.QueryAsync<int>(countSql, parameters);
        
        return (items, total.FirstOrDefault());
    }
}