using FluentFTP;
using RPRO.Core.Models;
using System.Security.Cryptography;

namespace RPRO.Services.Collectors;

public class IHMService : IDisposable
{
    private readonly IHMConfig _config;
    private AsyncFtpClient? _client;
    private readonly string _tempDir;
    private readonly string _identifier;

    public IHMService(IHMConfig config, string identifier = "IHM1")
    {
        _config = config;
        _identifier = identifier;
        _tempDir = Path.Combine(Path.GetTempPath(), "RPRO", $"ihm_{config.Ip.Replace(".", "_")}");
        
        if (!Directory.Exists(_tempDir))
            Directory.CreateDirectory(_tempDir);
    }

    /// <summary>
    /// Conecta ao servidor FTP
    /// </summary>
    public async Task<bool> ConnectAsync()
    {
        try
        {
            _client = new AsyncFtpClient(_config.Ip, _config.User, _config.Password);
            _client.Config.ConnectTimeout = 10000;
            _client.Config.ReadTimeout = 30000;
            _client.Config.DataConnectionConnectTimeout = 10000;
            
            await _client.Connect();
            Console.WriteLine($"[{_identifier}] Conectado ao FTP: {_config.Ip}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{_identifier}] Erro ao conectar FTP: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Desconecta do servidor FTP
    /// </summary>
    public async Task DisconnectAsync()
    {
        if (_client != null && _client.IsConnected)
        {
            await _client.Disconnect();
            Console.WriteLine($"[{_identifier}] Desconectado do FTP");
        }
    }

    /// <summary>
    /// Testa a conexão com o IHM
    /// </summary>
    public async Task<(bool Success, string Message)> TestConnectionAsync()
    {
        try
        {
            using var client = new AsyncFtpClient(_config.Ip, _config.User, _config.Password);
            client.Config.ConnectTimeout = 5000;
            
            await client.Connect();
            
            var exists = await client.DirectoryExists(_config.CaminhoRemoto);
            await client.Disconnect();
            
            if (exists)
                return (true, $"Conexão OK. Diretório '{_config.CaminhoRemoto}' encontrado.");
            else
                return (false, $"Conectado, mas diretório '{_config.CaminhoRemoto}' não encontrado.");
        }
        catch (Exception ex)
        {
            return (false, $"Erro: {ex.Message}");
        }
    }

    /// <summary>
    /// Lista arquivos CSV no diretório remoto
    /// </summary>
    public async Task<List<FtpListItem>> ListFilesAsync(string? pattern = "*.csv")
    {
        if (_client == null || !_client.IsConnected)
            await ConnectAsync();

        var items = await _client!.GetListing(_config.CaminhoRemoto);
        
        return items
            .Where(f => f.Type == FtpObjectType.File)
            .Where(f => pattern == null || f.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(f => f.Modified)
            .ToList();
    }

    /// <summary>
    /// Baixa um arquivo do FTP
    /// </summary>
    public async Task<CollectedFile?> DownloadFileAsync(string remotePath, string fileName)
    {
        if (_client == null || !_client.IsConnected)
            await ConnectAsync();

        var localPath = Path.Combine(_tempDir, $"{DateTime.Now:yyyyMMdd_HHmmss}_{fileName}");

        try
        {
            var status = await _client!.DownloadFile(localPath, remotePath);
            
            if (status == FtpStatus.Success)
            {
                var fileInfo = new FileInfo(localPath);
                var hash = await ComputeFileHashAsync(localPath);

                return new CollectedFile
                {
                    Name = fileName,
                    LocalPath = localPath,
                    RemotePath = remotePath,
                    ModifiedTime = fileInfo.LastWriteTime,
                    Size = fileInfo.Length,
                    Hash = hash,
                    SourceIhm = _identifier
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{_identifier}] Erro ao baixar {fileName}: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Encontra e baixa novos arquivos
    /// </summary>
    public async Task<List<CollectedFile>> FindAndDownloadNewFilesAsync(HashSet<string>? processedHashes = null)
    {
        var downloaded = new List<CollectedFile>();
        processedHashes ??= new HashSet<string>();

        try
        {
            if (!await ConnectAsync())
                return downloaded;

            var files = await ListFilesAsync();
            Console.WriteLine($"[{_identifier}] Encontrados {files.Count} arquivos CSV");

            foreach (var file in files)
            {
                var remotePath = $"{_config.CaminhoRemoto.TrimEnd('/')}/{file.Name}";
                var collected = await DownloadFileAsync(remotePath, file.Name);

                if (collected != null)
                {
                    // Verificar se já foi processado (pelo hash)
                    if (!processedHashes.Contains(collected.Hash))
                    {
                        downloaded.Add(collected);
                        Console.WriteLine($"[{_identifier}] Baixado: {file.Name} ({collected.Size} bytes)");
                    }
                    else
                    {
                        Console.WriteLine($"[{_identifier}] Ignorado (já processado): {file.Name}");
                        // Limpar arquivo temporário
                        if (File.Exists(collected.LocalPath))
                            File.Delete(collected.LocalPath);
                    }
                }
            }

            await DisconnectAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{_identifier}] Erro na coleta: {ex.Message}");
        }

        return downloaded;
    }

    private async Task<string> ComputeFileHashAsync(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = await sha256.ComputeHashAsync(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}