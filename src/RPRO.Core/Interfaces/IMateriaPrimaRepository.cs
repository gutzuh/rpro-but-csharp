namespace RPRO.Core.Interfaces;

using RPRO.Core.Entities;

public interface IMateriaPrimaRepository
{
    Task<List<MateriaPrima>> GetAllAsync();
    Task<MateriaPrima?> GetByNumAsync(int num);
    Task SaveAsync(MateriaPrima materiaPrima);
    Task SaveManyAsync(IEnumerable<MateriaPrima> materiasPrimas);
}
