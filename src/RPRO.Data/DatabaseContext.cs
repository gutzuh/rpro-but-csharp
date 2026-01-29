using MySqlConnector;
using Dapper;

namespace RPRO.Data;

public class DatabaseContext
{
    private readonly string _connectionString;

    public DatabaseContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public MySqlConnection CreateConnection()
    {
        return new MySqlConnection(_connectionString);
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        return await connection.QueryAsync<T>(sql, param);
    }

    public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        return await connection.QueryFirstOrDefaultAsync<T>(sql, param);
    }

    public async Task<T> QuerySingleAsync<T>(string sql, object? param = null)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        return await connection.QuerySingleAsync<T>(sql, param);
    }

    public async Task<int> ExecuteAsync(string sql, object? param = null)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        return await connection.ExecuteAsync(sql, param);
    }

    public async Task<T> ExecuteScalarAsync<T>(string sql, object? param = null)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        return await connection.ExecuteScalarAsync<T>(sql, param);
    }

    /// <summary>
    /// Testa a conexão com o banco de dados
    /// </summary>
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Obtém informações sobre a conexão
    /// </summary>
    public async Task<DatabaseInfo> GetInfoAsync()
    {
        try
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();
            
            var relatorioCount = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM relatorio");
            
            return new DatabaseInfo
            {
                IsConnected = true,
                RelatorioCount = relatorioCount,
                ServerVersion = connection.ServerVersion
            };
        }
        catch (Exception ex)
        {
            return new DatabaseInfo
            {
                IsConnected = false,
                Error = ex.Message
            };
        }
    }
}

public class DatabaseInfo
{
    public bool IsConnected { get; set; }
    public int RelatorioCount { get; set; }
    public string? ServerVersion { get; set; }
    public string? Error { get; set; }
}