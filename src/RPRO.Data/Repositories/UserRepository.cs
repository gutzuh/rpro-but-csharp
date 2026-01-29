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

    public async Task<List<User>> GetAllAsync()
    {
        var result = await _db.QueryAsync<User>("SELECT * FROM user ORDER BY username");
        return result.ToList();
    }

    public async Task<User> CreateAsync(User user)
    {
        var sql = @"
            INSERT INTO user (username, password, isAdmin, displayName, photoPath, userType)
            VALUES (@Username, @Password, @IsAdmin, @DisplayName, @PhotoPath, @UserType)";

        await _db.ExecuteAsync(sql, user);
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        var sql = @"
            UPDATE user SET 
                password = @Password,
                isAdmin = @IsAdmin,
                displayName = @DisplayName,
                photoPath = @PhotoPath,
                userType = @UserType
            WHERE id = @Id";

        await _db.ExecuteAsync(sql, user);
        return user;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var result = await _db.ExecuteAsync("DELETE FROM user WHERE id = @Id", new { Id = id });
        return result > 0;
    }
}