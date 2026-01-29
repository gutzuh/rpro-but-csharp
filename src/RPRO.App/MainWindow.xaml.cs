using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using RPRO.App.ViewModels;
using RPRO.App.Views;
using RPRO.Core.Entities;

namespace RPRO.App;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private User? _currentUser;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        // Esconder janela principal até login
        Visibility = Visibility.Hidden;

        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // TODO: Descomentar para ativar login
        // ShowLogin();
        
        // Teste temporário - carregar direto sem login
        var testUser = new User
        {
            Id = 1,
            Username = "admin",
            DisplayName = "Administrador",
            Email = "admin@cortez.com",
            IsAdmin = true,
            UserType = "admin",
            Ativo = true
        };
        
        _currentUser = testUser;
        _viewModel.SetUser(testUser);
        Visibility = Visibility.Visible;
    }

    private void ShowLogin()
    {
        var loginVm = App.Services.GetRequiredService<LoginViewModel>();
        var loginView = new LoginView(loginVm);

        loginVm.LoginSuccess += (s, user) =>
        {
            _currentUser = user;
            _viewModel.SetUser(user);
            
            loginView.Close();
            Visibility = Visibility.Visible;
            Activate();
        };

        loginView.ShowDialog();

        // Se fechou sem login, fechar app
        if (_currentUser == null)
        {
            Application.Current.Shutdown();
        }
    }

    public void Logout()
    {
        _currentUser = null;
        Visibility = Visibility.Hidden;
        ShowLogin();
    }
}