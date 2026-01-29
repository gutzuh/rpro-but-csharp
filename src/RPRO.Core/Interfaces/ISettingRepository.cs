using RPRO.Core.Entities;

namespace RPRO.Core.Interfaces;

public interface ISettingRepository
{
    Task<string?> GetAsync(string key);
    Task SetAsync(string key, string value);
    Task<Dictionary<string, string>> GetAllAsync();
    Task SetManyAsync(Dictionary<string, string> settings);
}