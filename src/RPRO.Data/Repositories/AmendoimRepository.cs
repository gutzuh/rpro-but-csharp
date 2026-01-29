using Dapper;
using RPRO.Core.Entities;
using RPRO.Core.Interfaces;

namespace RPRO.Data.Repositories;

public class AmendoimRepository : IAmendoimRepository
{
    private readonly DatabaseContext _db;

    public AmendoimRepository(DatabaseContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Amendoim>> GetAllAsync()
    {
        return await _db.QueryAsync<Amendoim>(
            "SELECT * FROM amendoim ORDER BY dia DESC, hora DESC");
    }

    public async Task<Amendoim?> GetByIdAsync(int id)
    {
        return await _db.QueryFirstOrDefaultAsync<Amendoim>(
            "SELECT * FROM amendoim WHERE id = @Id",
            new { Id = id });
    }

    public async Task<(IEnumerable<Amendoim> Items, int Total)> GetPaginatedAsync(
        int page,
        int pageSize,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        string? tipo = null,
        string? codigoProduto = null)
    {
        var parameters = new DynamicParameters();
        var whereClause = "WHERE 1=1";

        if (dataInicio.HasValue)
        {
            whereClause += " AND STR_TO_DATE(dia, '%d-%m-%y') >= @DataInicio";
            parameters.Add("DataInicio", dataInicio.Value.Date);
        }

        if (dataFim.HasValue)
        {
            whereClause += " AND STR_TO_DATE(dia, '%d-%m-%y') <= @DataFim";
            parameters.Add("DataFim", dataFim.Value.Date);
        }

        if (!string.IsNullOrEmpty(tipo))
        {
            whereClause += " AND tipo = @Tipo";
            parameters.Add("Tipo", tipo);
        }

        if (!string.IsNullOrEmpty(codigoProduto))
        {
            whereClause += " AND codigoProduto = @CodigoProduto";
            parameters.Add("CodigoProduto", codigoProduto);
        }

        var countSql = $"SELECT COUNT(*) FROM amendoim {whereClause}";
        var total = await _db.ExecuteScalarAsync<int>(countSql, parameters);

        var offset = (page - 1) * pageSize;
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", pageSize);

        var sql = $@"
            SELECT * FROM amendoim 
            {whereClause}
            ORDER BY dia DESC, hora DESC
            LIMIT @Offset, @PageSize";

        var items = await _db.QueryAsync<Amendoim>(sql, parameters);

        return (items, total);
    }

    public async Task<int> InsertAsync(Amendoim amendoim)
    {
        var sql = @"
            INSERT INTO amendoim (tipo, dia, hora, codigoProduto, codigoCaixa, nomeProduto, peso, balanca, createdAt)
            VALUES (@Tipo, @Dia, @Hora, @CodigoProduto, @CodigoCaixa, @NomeProduto, @Peso, @Balanca, @CreatedAt)";

        return await _db.ExecuteAsync(sql, amendoim);
    }

    public async Task<int> InsertManyAsync(IEnumerable<Amendoim> amendoins)
    {
        var count = 0;
        foreach (var a in amendoins)
        {
            try
            {
                if (!await ExistsAsync(a.Tipo, a.Dia, a.Hora, a.CodigoProduto, a.Peso))
                {
                    count += await InsertAsync(a);
                }
            }
            catch
            {
                // Ignora duplicados
            }
        }
        return count;
    }

    public async Task<bool> ExistsAsync(string tipo, string dia, string hora, string codigoProduto, decimal peso)
    {
        var count = await _db.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*) FROM amendoim 
            WHERE tipo = @Tipo AND dia = @Dia AND hora = @Hora 
            AND codigoProduto = @CodigoProduto AND peso = @Peso",
            new { Tipo = tipo, Dia = dia, Hora = hora, CodigoProduto = codigoProduto, Peso = peso });

        return count > 0;
    }

    public async Task<(decimal PesoEntrada, decimal PesoSaida)> GetMetricasRendimentoAsync(
        DateTime? dataInicio = null,
        DateTime? dataFim = null)
    {
        var parameters = new DynamicParameters();
        var whereClause = "WHERE 1=1";

        if (dataInicio.HasValue)
        {
            whereClause += " AND STR_TO_DATE(dia, '%d-%m-%y') >= @DataInicio";
            parameters.Add("DataInicio", dataInicio.Value.Date);
        }

        if (dataFim.HasValue)
        {
            whereClause += " AND STR_TO_DATE(dia, '%d-%m-%y') <= @DataFim";
            parameters.Add("DataFim", dataFim.Value.Date);
        }

        var sql = $@"
            SELECT 
                COALESCE(SUM(CASE WHEN tipo = 'entrada' THEN peso ELSE 0 END), 0) as PesoEntrada,
                COALESCE(SUM(CASE WHEN tipo = 'saida' THEN peso ELSE 0 END), 0) as PesoSaida
            FROM amendoim {whereClause}";

        var result = await _db.QueryFirstOrDefaultAsync<(decimal PesoEntrada, decimal PesoSaida)>(sql, parameters);
        return result;
    }

    public async Task<IEnumerable<(string Dia, decimal Entrada, decimal Saida)>> GetFluxoPorDiaAsync(
        DateTime? dataInicio = null,
        DateTime? dataFim = null)
    {
        var parameters = new DynamicParameters();
        var whereClause = "WHERE 1=1";

        if (dataInicio.HasValue)
        {
            whereClause += " AND STR_TO_DATE(dia, '%d-%m-%y') >= @DataInicio";
            parameters.Add("DataInicio", dataInicio.Value.Date);
        }

        if (dataFim.HasValue)
        {
            whereClause += " AND STR_TO_DATE(dia, '%d-%m-%y') <= @DataFim";
            parameters.Add("DataFim", dataFim.Value.Date);
        }

        var sql = $@"
            SELECT 
                dia as Dia,
                COALESCE(SUM(CASE WHEN tipo = 'entrada' THEN peso ELSE 0 END), 0) as Entrada,
                COALESCE(SUM(CASE WHEN tipo = 'saida' THEN peso ELSE 0 END), 0) as Saida
            FROM amendoim {whereClause}
            GROUP BY dia
            ORDER BY STR_TO_DATE(dia, '%d-%m-%y') DESC";

        return await _db.QueryAsync<(string Dia, decimal Entrada, decimal Saida)>(sql, parameters);
    }
}