namespace RPRO.Core.Interfaces;

using RPRO.Core.Entities;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> AuthenticateAsync(string username, string password);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> DeleteAsync(int id);
}
