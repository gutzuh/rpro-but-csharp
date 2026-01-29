using Microsoft.Extensions.DependencyInjection;
using RPRO.Core.Interfaces;
using RPRO.Data;
using RPRO.Data.Repositories;
using RPRO.Services;
using RPRO.Services.Collectors;
using RPRO.App.ViewModels;
using System.Windows;

namespace RPRO.App;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        // Executar migrations
        var migration = Services.GetRequiredService<DatabaseMigration>();
        await migration.MigrateAsync();

        // Mostrar janela principal
        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
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