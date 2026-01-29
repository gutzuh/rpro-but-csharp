using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RPRO.Core.Entities;
using System.Collections.ObjectModel;

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
        var viewModel = _services.GetService(viewModelType);
        CurrentView = viewModel;
    }

    public void SetUser(User user)
    {
        CurrentUser = user;
        LoadMenuItems();
        
        // Navegar para primeiro item
        if (MenuItems.Count > 0)
        {
            SelectedMenuItem = MenuItems[0];
        }
    }
}

public class MenuItem
{
    public string Title { get; set; } = "";
    public string Icon { get; set; } = "";
    public Type? ViewType { get; set; }
}