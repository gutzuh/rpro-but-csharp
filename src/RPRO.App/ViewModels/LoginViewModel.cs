using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RPRO.Core.Entities;
using RPRO.Services;

namespace RPRO.App.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly AuthService _authService;
    
    public event EventHandler<User>? LoginSuccess;

    [ObservableProperty]
    private string _username = "";

    [ObservableProperty]
    private string _password = "";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = "";

    [ObservableProperty]
    private bool _lembrarUsuario;

    public LoginViewModel(AuthService authService)
    {
        _authService = authService;
        LoadSavedUser();
    }

    private void LoadSavedUser()
    {
        // Sem salvamento de usuário por enquanto
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Preencha usuário e senha";
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = "";

            var (success, message, user) = await _authService.AuthenticateAsync(Username, Password);

            if (success && user != null)
            {
                LoginSuccess?.Invoke(this, user);
            }
            else
            {
                ErrorMessage = message;
                Password = "";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao conectar: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ExitApp()
    {
        App.Current.Shutdown();
    }
}