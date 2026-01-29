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
        
        // Carregar usuário salvo
        LoadSavedUser();
    }

    private void LoadSavedUser()
    {
        var savedUser = Properties.Settings.Default.SavedUsername;
        if (!string.IsNullOrEmpty(savedUser))
        {
            Username = savedUser;
            LembrarUsuario = true;
        }
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

            var user = await _authService.LoginAsync(Username, Password);

            if (user != null)
            {
                // Salvar usuário se marcado
                if (LembrarUsuario)
                {
                    Properties.Settings.Default.SavedUsername = Username;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    Properties.Settings.Default.SavedUsername = "";
                    Properties.Settings.Default.Save();
                }

                LoginSuccess?.Invoke(this, user);
            }
            else
            {
                ErrorMessage = "Usuário ou senha incorretos";
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