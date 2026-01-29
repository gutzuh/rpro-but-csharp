using FluentFTP;

public class IHMCollectorService : ICollectorService
{
    private readonly string _host;
    private readonly string _user;
    private readonly string _password;
    private readonly string _remotePath;
    
    public IHMCollectorService(IHMConfig config)
    {
        _host = config.Ip;
        _user = config.User;
        _password = config.Password;
        _remotePath = config.CaminhoRemoto;
    }
    
    public async Task<List<CollectedFile>> CollectFilesAsync()
    {
        var collectedFiles = new List<CollectedFile>();
        
        using var ftp = new AsyncFtpClient(_host, _user, _password);
        await ftp.Connect();
        
        var files = await ftp.GetListing(_remotePath);
        
        foreach (var file in files.Where(f => f.Name.EndsWith(".csv")))
        {
            var localPath = Path.Combine(Path.GetTempPath(), file.Name);
            await ftp.DownloadFile(localPath, file.FullName);
            
            collectedFiles.Add(new CollectedFile
            {
                Name = file.Name,
                LocalPath = localPath,
                ModifiedTime = file.Modified
            });
        }
        
        await ftp.Disconnect();
        return collectedFiles;
    }
}