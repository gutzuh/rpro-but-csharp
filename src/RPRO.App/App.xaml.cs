using Microsoft.Extensions.DependencyInjection;
using RPRO.Core.Interfaces;
using RPRO.Data;
using RPRO.Data.Repositories;
using RPRO.Services;
using RPRO.Services.Collectors;
using RPRO.App.ViewModels;
using System.Windows;
using System.IO;

namespace RPRO.App;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    private static readonly string LogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "cortez_error.log");

    private static void LogError(string message)
    {
        try
        {
            File.AppendAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n\n");
        }
        catch { }
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        LogError("=== Iniciando aplicação ===");

        // Captura de exceções não tratadas
        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            LogError($"UnhandledException: {ex?.Message}\n{ex?.StackTrace}");
            MessageBox.Show($"Exceção não tratada: {ex?.Message}\n{ex?.StackTrace}", "Erro Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
        };

        DispatcherUnhandledException += (s, args) =>
        {
            LogError($"DispatcherUnhandledException: {args.Exception.Message}\n{args.Exception.StackTrace}");
            MessageBox.Show($"Exceção do Dispatcher: {args.Exception.Message}\n{args.Exception.StackTrace}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        try
        {
            LogError("Configurando serviços...");
            var services = new ServiceCollection();
            ConfigureServices(services);
            Services = services.BuildServiceProvider();
            LogError("Serviços configurados.");

            // Tentar migrations (não bloqueia se falhar)
            try
            {
                LogError("Executando migrations...");
                var migration = Services.GetRequiredService<DatabaseMigration>();
                await migration.MigrateAsync();
                LogError("Migrations concluídas.");
            }
            catch (Exception dbEx)
            {
                LogError($"Erro na migration: {dbEx.Message}\n{dbEx.StackTrace}");
                MessageBox.Show($"Aviso: Não foi possível conectar ao banco de dados.\n{dbEx.Message}\n\nO aplicativo continuará em modo offline.", "Aviso de Conexão", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // Mostrar janela principal
            LogError("Criando MainWindow...");
            var mainWindow = Services.GetRequiredService<MainWindow>();
            LogError("Mostrando MainWindow...");
            mainWindow.Show();
            LogError("MainWindow exibida.");
        }
        catch (Exception ex)
        {
            LogError($"Erro fatal: {ex.Message}\nInner: {ex.InnerException?.Message}\nStack: {ex.StackTrace}");
            MessageBox.Show($"Erro ao iniciar o aplicativo: {ex.Message}\n\nInner: {ex.InnerException?.Message}\n\nStack: {ex.StackTrace}", "Erro Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        var connectionString = "Server=localhost;Database=cadastro;User=root;Password=root;";
        
        // Database
        services.AddSingleton(new DatabaseContext(connectionString));
        services.AddSingleton<DatabaseMigration>();

        // Repositories
        services.AddScoped<IRelatorioRepository, RelatorioRepository>();
        services.AddScoped<IAmendoimRepository, AmendoimRepository>();
        services.AddScoped<IMateriaPrimaRepository, MateriaPrimaRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        // Services
        services.AddScoped<DashboardRacaoService>();
        services.AddScoped<DashboardAmendoimService>();
        services.AddScoped<AuthService>();
        services.AddSingleton<CollectorService>();
        services.AddTransient<FileParserService>();

        // ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<DashboardRacaoViewModel>();
        services.AddTransient<DashboardAmendoimViewModel>();
        services.AddTransient<RelatorioViewModel>();
        services.AddTransient<ConfiguracoesViewModel>();
        services.AddTransient<CollectorViewModel>();

        // Views
        services.AddTransient<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Parar o coletor ao fechar
        var collector = Services.GetService<CollectorService>();
        collector?.StopAsync().Wait(TimeSpan.FromSeconds(5));
        
        base.OnExit(e);
    }
}