namespace RPRO.Services;

using System.Security.Cryptography;
using System.Text;
using RPRO.Core.Entities;
using RPRO.Core.Interfaces;

public class AuthService
{
    private readonly IUserRepository? _userRepository;
    private readonly List<User> _testUsers;

    public AuthService(IUserRepository? userRepository = null)
    {
        _userRepository = userRepository;
        
        _testUsers = new List<User>
        {
            new User 
            { 
                Id = 1,
                Username = "admin", 
                DisplayName = "Administrador",
                Email = "admin@cortez.com",
                PasswordHash = HashPassword("admin123"),
                IsAdmin = true,
                UserType = "admin",
                Ativo = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new User 
            { 
                Id = 2,
                Username = "user", 
                DisplayName = "Usuário",
                Email = "user@cortez.com",
                PasswordHash = HashPassword("user123"),
                IsAdmin = false,
                UserType = "user",
                Ativo = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new User 
            { 
                Id = 3,
                Username = "teste", 
                DisplayName = "Teste",
                Email = "teste@cortez.com",
                PasswordHash = HashPassword("teste"),
                IsAdmin = false,
                UserType = "operator",
                Ativo = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }
        };
    }

    public async Task<(bool Success, string Message, User? User)> AuthenticateAsync(string username, string password)
    {
        try
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return (false, "Usuário e senha são obrigatórios", null);
            }

            // Tentar banco de dados primeiro se disponível
            if (_userRepository != null)
            {
                try
                {
                    var user = await _userRepository.AuthenticateAsync(username, password);
                    if (user != null && user.Ativo)
                    {
                        return (true, "Login realizado com sucesso", user);
                    }
                }
                catch
                {
                    // Se falhar, usar usuários de teste como fallback
                }
            }

            // Fallback para usuários de teste
            var testUser = _testUsers.FirstOrDefault(u => u.Username == username);
            if (testUser != null && VerifyPassword(password, testUser.PasswordHash) && testUser.Ativo)
            {
                return (true, "Login realizado com sucesso", testUser);
            }

            return (false, "Usuário ou senha inválidos", null);
        }
        catch (Exception ex)
        {
            return (false, $"Erro ao autenticar: {ex.Message}", null);
        }
    }

    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    private bool VerifyPassword(string password, string hash)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput.Equals(hash);
    }
}
