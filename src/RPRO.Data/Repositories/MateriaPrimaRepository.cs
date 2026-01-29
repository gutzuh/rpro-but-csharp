using Dapper;
using RPRO.Core.Entities;
using RPRO.Core.Interfaces;

namespace RPRO.Data.Repositories;

public class MateriaPrimaRepository : IMateriaPrimaRepository
{
    private readonly DatabaseContext _db;

    public MateriaPrimaRepository(DatabaseContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<MateriaPrima>> GetAllAsync()
    {
        // Garantir que existam produtos padrão
        await EnsureDefaultProductsAsync();

        return await _db.QueryAsync<MateriaPrima>(
            "SELECT * FROM materia_prima ORDER BY num");
    }

    public async Task<MateriaPrima?> GetByNumAsync(int num)
    {
        return await _db.QueryFirstOrDefaultAsync<MateriaPrima>(
            "SELECT * FROM materia_prima WHERE num = @Num",
            new { Num = num });
    }

    public async Task<int> SaveAsync(MateriaPrima materiaPrima)
    {
        var existing = await GetByNumAsync(materiaPrima.Num);

        if (existing != null)
        {
            // Update
            return await _db.ExecuteAsync(@"
                UPDATE materia_prima 
                SET produto = @Produto, medida = @Medida, ativo = @Ativo, ignorarCalculos = @IgnorarCalculos
                WHERE num = @Num",
                materiaPrima);
        }
        else
        {
            // Insert
            if (materiaPrima.Id == Guid.Empty)
                materiaPrima.Id = Guid.NewGuid();

            return await _db.ExecuteAsync(@"
                INSERT INTO materia_prima (id, num, produto, medida, ativo, ignorarCalculos)
                VALUES (@Id, @Num, @Produto, @Medida, @Ativo, @IgnorarCalculos)",
                new
                {
                    Id = materiaPrima.Id.ToString(),
                    materiaPrima.Num,
                    materiaPrima.Produto,
                    materiaPrima.Medida,
                    materiaPrima.Ativo,
                    materiaPrima.IgnorarCalculos
                });
        }
    }

    public async Task<int> SaveManyAsync(IEnumerable<MateriaPrima> materiasPrimas)
    {
        var count = 0;
        foreach (var mp in materiasPrimas)
        {
            count += await SaveAsync(mp);
        }
        return count;
    }

    private async Task EnsureDefaultProductsAsync()
    {
        const int DEFAULT_COUNT = 40;

        var existingCount = await _db.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM materia_prima");

        if (existingCount >= DEFAULT_