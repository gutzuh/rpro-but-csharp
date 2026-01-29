using RPRO.Core.Entities;
using RPRO.Core.Interfaces;

namespace RPRO.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DatabaseContext _db;

    public UserRepository(DatabaseContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _db.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM user WHERE username = @Username",
            new { Username = username });
    }

    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        return await _db.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM user WHERE username = @Username AND password = @Password",
            new { Username = username, Password = password });
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _db.QueryAsync<User>("SELECT * FROM user ORDER BY username");
    }

    public async Task<int> CreateAsync(User user)
    {
        var sql = @"
            INSERT INTO user (username, password, isAdmin, displayName, photoPath, userType)
            VALUES (@Username, @Password, @IsAdmin, @DisplayName, @PhotoPath, @UserType)";

        return await _db.ExecuteAsync(sql, user);
    }

    public async Task<int> UpdateAsync(User user)
    {
        var sql = @"
            UPDATE user SET 
                password = @Password,
                isAdmin = @IsAdmin,
                displayName = @DisplayName,
                photoPath = @PhotoPath,
                userType = @UserType
            WHERE id = @Id";

        return await _db.ExecuteAsync(sql, user);
    }

    public async Task<int> DeleteAsync(int id)
    {
        return await _db.ExecuteAsync("DELETE FROM user WHERE id = @Id", new { Id = id });
    }
}