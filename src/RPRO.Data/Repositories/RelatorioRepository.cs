using Dapper;
using RPRO.Core.Entities;
using RPRO.Core.Interfaces;

namespace RPRO.Data.Repositories;

public class RelatorioRepository : IRelatorioRepository
{
    private readonly DatabaseContext _db;

    public RelatorioRepository(DatabaseContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Relatorio>> GetAllAsync()
    {
        return await _db.QueryAsync<Relatorio>(
            "SELECT * FROM relatorio ORDER BY Dia DESC, Hora DESC");
    }

    public async Task<Relatorio?> GetByIdAsync(Guid id)
    {
        return await _db.QueryFirstOrDefaultAsync<Relatorio>(
            "SELECT * FROM relatorio WHERE id = @Id", 
            new { Id = id.ToString() });
    }

    public async Task<(IEnumerable<Relatorio> Items, int Total)> GetPaginatedAsync(
        int page,
        int pageSize,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        string? formula = null,
        int? codigo = null,
        int? numero = null)
    {
        var parameters = new DynamicParameters();
        var whereClause = "WHERE 1=1";

        if (dataInicio.HasValue)
        {
            whereClause += " AND STR_TO_DATE(Dia, '%d/%m/%Y') >= @DataInicio";
            parameters.Add("DataInicio", dataInicio.Value.Date);
        }

        if (dataFim.HasValue)
        {
            whereClause += " AND STR_TO_DATE(Dia, '%d/%m/%Y') <= @DataFim";
            parameters.Add("DataFim", dataFim.Value.Date);
        }

        if (!string.IsNullOrEmpty(formula))
        {
            whereClause += " AND Nome LIKE @Formula";
            parameters.Add("Formula", $"%{formula}%");
        }

        if (codigo.HasValue)
        {
            whereClause += " AND Form1 = @Codigo";
            parameters.Add("Codigo", codigo.Value);
        }

        if (numero.HasValue)
        {
            whereClause += " AND Form2 = @Numero";
            parameters.Add("Numero", numero.Value);
        }

        // Count total
        var countSql = $"SELECT COUNT(*) FROM relatorio {whereClause}";
        var total = await _db.ExecuteScalarAsync<int>(countSql, parameters);

        // Get page
        var offset = (page - 1) * pageSize;
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", pageSize);

        var sql = $@"
            SELECT * FROM relatorio 
            {whereClause}
            ORDER BY Dia DESC, Hora DESC
            LIMIT @Offset, @PageSize";

        var items = await _db.QueryAsync<Relatorio>(sql, parameters);

        return (items, total);
    }

    public async Task<int> InsertAsync(Relatorio relatorio)
    {
        if (relatorio.Id == Guid.Empty)
            relatorio.Id = Guid.NewGuid();

        var sql = @"
            INSERT INTO relatorio (id, Dia, Hora, Nome, Form1, Form2,
                Prod_1, Prod_2, Prod_3, Prod_4, Prod_5, Prod_6, Prod_7, Prod_8, Prod_9, Prod_10,
                Prod_11, Prod_12, Prod_13, Prod_14, Prod_15, Prod_16, Prod_17, Prod_18, Prod_19, Prod_20,
                Prod_21, Prod_22, Prod_23, Prod_24, Prod_25, Prod_26, Prod_27, Prod_28, Prod_29, Prod_30,
                Prod_31, Prod_32, Prod_33, Prod_34, Prod_35, Prod_36, Prod_37, Prod_38, Prod_39, Prod_40)
            VALUES (@Id, @Dia, @Hora, @Nome, @Form1, @Form2,
                @Prod_1, @Prod_2, @Prod_3, @Prod_4, @Prod_5, @Prod_6, @Prod_7, @Prod_8, @Prod_9, @Prod_10,
                @Prod_11, @Prod_12, @Prod_13, @Prod_14, @Prod_15, @Prod_16, @Prod_17, @Prod_18, @Prod_19, @Prod_20,
                @Prod_21, @Prod_22, @Prod_23, @Prod_24, @Prod_25, @Prod_26, @Prod_27, @Prod_28, @Prod_29, @Prod_30,
                @Prod_31, @Prod_32, @Prod_33, @Prod_34, @Prod_35, @Prod_36, @Prod_37, @Prod_38, @Prod_39, @Prod_40)";

        return await _db.ExecuteAsync(sql, new
        {
            Id = relatorio.Id.ToString(),
            relatorio.Dia,
            relatorio.Hora,
            relatorio.Nome,
            relatorio.Form1,
            relatorio.Form2,
            relatorio.Prod_1, relatorio.Prod_2, relatorio.Prod_3, relatorio.Prod_4, relatorio.Prod_5,
            relatorio.Prod_6, relatorio.Prod_7, relatorio.Prod_8, relatorio.Prod_9, relatorio.Prod_10,
            relatorio.Prod_11, relatorio.Prod_12, relatorio.Prod_13, relatorio.Prod_14, relatorio.Prod_15,
            relatorio.Prod_16, relatorio.Prod_17, relatorio.Prod_18, relatorio.Prod_19, relatorio.Prod_20,
            relatorio.Prod_21, relatorio.Prod_22, relatorio.Prod_23, relatorio.Prod_24, relatorio.Prod_25,
            relatorio.Prod_26, relatorio.Prod_27, relatorio.Prod_28, relatorio.Prod_29, relatorio.Prod_30,
            relatorio.Prod_31, relatorio.Prod_32, relatorio.Prod_33, relatorio.Prod_34, relatorio.Prod_35,
            relatorio.Prod_36, relatorio.Prod_37, relatorio.Prod_38, relatorio.Prod_39, relatorio.Prod_40
        });
    }

    public async Task<int> InsertManyAsync(IEnumerable<Relatorio> relatorios)
    {
        var count = 0;
        foreach (var rel in relatorios)
        {
            count += await InsertAsync(rel);
        }
        return count;
    }

    public async Task<int> UpdateAsync(Relatorio relatorio)
    {
        var sql = @"
            UPDATE relatorio SET 
                Dia = @Dia, Hora = @Hora, Nome = @Nome, Form1 = @Form1, Form2 = @Form2,
                Prod_1 = @Prod_1, Prod_2 = @Prod_2, Prod_3 = @Prod_3, Prod_4 = @Prod_4, Prod_5 = @Prod_5,
                Prod_6 = @Prod_6, Prod_7 = @Prod_7, Prod_8 = @Prod_8, Prod_9 = @Prod_9, Prod_10 = @Prod_10
            WHERE id = @Id";

        return await _db.ExecuteAsync(sql, relatorio);
    }

    public async Task<int> DeleteAsync(Guid id)
    {
        return await _db.ExecuteAsync(
            "DELETE FROM relatorio WHERE id = @Id", 
            new { Id = id.ToString() });
    }

    public async Task<int> GetCountAsync()
    {
        return await _db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM relatorio");
    }
}