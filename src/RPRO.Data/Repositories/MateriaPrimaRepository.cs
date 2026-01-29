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

    public async Task<List<MateriaPrima>> GetAllAsync()
    {
        // Garantir que existam produtos padrão
        await EnsureDefaultProductsAsync();

        var result = await _db.QueryAsync<MateriaPrima>(
            "SELECT * FROM materia_prima ORDER BY num");
        return result.ToList();
    }

    public async Task<MateriaPrima?> GetByNumAsync(int num)
    {
        return await _db.QueryFirstOrDefaultAsync<MateriaPrima>(
            "SELECT * FROM materia_prima WHERE num = @Num",
            new { Num = num });
    }

    public async Task SaveAsync(MateriaPrima materiaPrima)
    {
        var existing = await GetByNumAsync(materiaPrima.Num);

        if (existing != null)
        {
            // Update
            await _db.ExecuteAsync(@"
                UPDATE materia_prima 
                SET produto = @Produto, medida = @Medida, ativo = @Ativo, ignorarCalculos = @IgnorarCalculos
                WHERE num = @Num",
                materiaPrima);
        }
        else
        {
            // Insert
            await _db.ExecuteAsync(@"
                INSERT INTO materia_prima (num, produto, medida, ativo, ignorarCalculos, createdAt, updatedAt)
                VALUES (@Num, @Produto, @Medida, @Ativo, @IgnorarCalculos, @CreatedAt, @UpdatedAt)",
                new
                {
                    materiaPrima.Num,
                    materiaPrima.Produto,
                    materiaPrima.Medida,
                    materiaPrima.Ativo,
                    materiaPrima.IgnorarCalculos,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
        }
    }

    public async Task SaveManyAsync(IEnumerable<MateriaPrima> materiasPrimas)
    {
        foreach (var mp in materiasPrimas)
        {
            await SaveAsync(mp);
        }
    }

    private async Task EnsureDefaultProductsAsync()
    {
        const int DEFAULT_COUNT = 40;

        var existingCount = await _db.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM materia_prima");

        if (existingCount >= DEFAULT_COUNT)
            return;

        var defaultProducts = GenerateDefaultProducts();
        await SaveManyAsync(defaultProducts);
    }

    private IEnumerable<MateriaPrima> GenerateDefaultProducts()
    {
        return new List<MateriaPrima>();
    }
}