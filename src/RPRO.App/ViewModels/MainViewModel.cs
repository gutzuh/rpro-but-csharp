using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RPRO.Core.Entities;
using System.Collections.ObjectModel;
using System.Linq;

namespace RPRO.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private User? _currentUser;

    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private bool _isMenuOpen = true;

    [ObservableProperty]
    private MenuItem? _selectedMenuItem;

    public ObservableCollection<MenuItem> MenuItems { get; } = new();

    private readonly IServiceProvider _services;

    // Evento para notificar o MainWindow sobre logout
    public event EventHandler? LogoutRequested;

    public MainViewModel(IServiceProvider services)
    {
        _services = services;
    }

    private void LoadMenuItems()
    {
        MenuItems.Clear();
        
        // Adicionar TEST no início
        MenuItems.Add(new MenuItem { Title = "🧪 TEST", Icon = "Bug", ViewType = typeof(TestViewModel) });
        
        if (CurrentUser?.UserType == "amendoim")
        {
            MenuItems.Add(new MenuItem { Title = "Dashboard Amendoim", Icon = "Peanut", ViewType = typeof(DashboardAmendoimViewModel) });
            MenuItems.Add(new MenuItem { Title = "Dashboard Ração", Icon = "ViewDashboard", ViewType = typeof(DashboardRacaoViewModel) });
        }
        else
        {
            MenuItems.Add(new MenuItem { Title = "Dashboard Ração", Icon = "ViewDashboard", ViewType = typeof(DashboardRacaoViewModel) });
            MenuItems.Add(new MenuItem { Title = "Dashboard Amendoim", Icon = "Peanut", ViewType = typeof(DashboardAmendoimViewModel) });
        }
        
        MenuItems.Add(new MenuItem { Title = "Relatórios", Icon = "FileDocument", ViewType = typeof(RelatorioViewModel) });
        MenuItems.Add(new MenuItem { Title = "Coletor", Icon = "CloudDownload", ViewType = typeof(CollectorViewModel) });
        MenuItems.Add(new MenuItem { Title = "Configurações", Icon = "Cog", ViewType = typeof(ConfiguracoesViewModel) });
    }

    partial void OnSelectedMenuItemChanged(MenuItem? value)
    {
        if (value?.ViewType != null)
        {
            NavigateTo(value.ViewType);
        }
    }

    [RelayCommand]
    private void ToggleMenu()
    {
        IsMenuOpen = !IsMenuOpen;
    }

    [RelayCommand]
    private void Logout()
    {
        CurrentUser = null;
        CurrentView = null;
        MenuItems.Clear();
        LogoutRequested?.Invoke(this, EventArgs.Empty);
    }

    public void NavigateTo(Type viewModelType)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"NavigateTo chamado para: {viewModelType.Name}");
            App.LogError($"NavigateTo: {viewModelType.Name}");
            
            var viewModel = _services.GetService(viewModelType);
            if (viewModel != null)
            {
                CurrentView = viewModel;
                System.Diagnostics.Debug.WriteLine($"Navegou para: {viewModelType.Name}");
                App.LogError($"Navegou com sucesso para: {viewModelType.Name}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"ERRO: ViewModel {viewModelType.Name} é null!");
                App.LogError($"ERRO: ViewModel {viewModelType.Name} retornou null do DI!");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERRO na navegação: {ex.Message}");
            App.LogError($"ERRO na navegação: {ex}");
        }
    }

    public void SetUser(User user)
    {
        CurrentUser = user;
        System.Diagnostics.Debug.WriteLine($"User definido: {user.DisplayName}");
        App.LogError($"SetUser: {user.DisplayName}, Type: {user.UserType}");
        
        LoadMenuItems();
        System.Diagnostics.Debug.WriteLine($"MenuItems carregados: {MenuItems.Count}");
        App.LogError($"MenuItems carregados: {MenuItems.Count}");
        
        // Selecionar primeiro menu útil (pular o item de teste) ou o primeiro disponível
        var defaultMenu = MenuItems.FirstOrDefault(item => item.ViewType != typeof(TestViewModel))
                          ?? MenuItems.FirstOrDefault();

        if (defaultMenu != null)
        {
            App.LogError($"Selecionando primeiro menu: {defaultMenu.Title}");
            SelectedMenuItem = defaultMenu;
            System.Diagnostics.Debug.WriteLine($"Primeiro menu item selecionado: {defaultMenu.Title}");
        }
        else
        {
            App.LogError("ERRO: MenuItems está vazio!");
        }
    }
}

public class MenuItem
{
    public string Title { get; set; } = "";
    public string Icon { get; set; } = "";
    public Type? ViewType { get; set; }
}