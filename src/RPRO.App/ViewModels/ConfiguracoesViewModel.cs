using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RPRO.Core.Entities;
using RPRO.Core.Interfaces;
using RPRO.Data;
using System.Collections.ObjectModel;

namespace RPRO.App.ViewModels;

public partial class ConfiguracoesViewModel : ObservableObject
{
    private readonly DatabaseContext _db;
    private readonly IMateriaPrimaRepository _materiaPrimaRepo;
    private readonly IUserRepository _userRepo;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _mensagem = "";

    [ObservableProperty]
    private bool _mensagemSucesso;

    // Configurações de Banco de Dados
    [ObservableProperty]
    private string _dbServer = "localhost";

    [ObservableProperty]
    private int _dbPort = 3306;

    [ObservableProperty]
    private string _dbUser = "root";

    [ObservableProperty]
    private string _dbPassword = "";

    [ObservableProperty]
    private string _dbName = "cadastro";

    [ObservableProperty]
    private bool _dbConectado;

    [ObservableProperty]
    private string _dbStatus = "";

    // Matérias-Primas
    public ObservableCollection<MateriaPrima> MateriasPrimas { get; } = new();

    [ObservableProperty]
    private MateriaPrima? _materiaPrimaSelecionada;

    // Usuários
    public ObservableCollection<User> Usuarios { get; } = new();

    [ObservableProperty]
    private User? _usuarioSelecionado;

    // Novo Usuário
    [ObservableProperty]
    private string _novoUsername = "";

    [ObservableProperty]
    private string _novoPassword = "";

    [ObservableProperty]
    private string _novoDisplayName = "";

    [ObservableProperty]
    private bool _novoIsAdmin;

    [ObservableProperty]
    private string _novoUserType = "racao";

    public ConfiguracoesViewModel(
        DatabaseContext db,
        IMateriaPrimaRepository materiaPrimaRepo,
        IUserRepository userRepo)
    {
        _db = db;
        _materiaPrimaRepo = materiaPrimaRepo;
        _userRepo = userRepo;

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        IsLoading = true;

        try
        {
            // Testar conexão
            await TestarConexaoAsync();

            // Carregar matérias-primas
            var mps = await _materiaPrimaRepo.GetAllAsync();
            MateriasPrimas.Clear();
            foreach (var mp in mps.OrderBy(m => m.Num))
            {
                MateriasPrimas.Add(mp);
            }

            // Carregar usuários
            var users = await _userRepo.GetAllAsync();
            Usuarios.Clear();
            foreach (var u in users)
            {
                Usuarios.Add(u);
            }
        }
        catch (Exception ex)
        {
            MostrarMensagem($"Erro ao carregar: {ex.Message}", false);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task TestarConexaoAsync()
    {
        try
        {
            var info = await _db.GetInfoAsync();
            DbConectado = info.IsConnected;

            if (info.IsConnected)
            {
                DbStatus = $"✅ Conectado | {info.RelatorioCount:N0} registros | MySQL {info.ServerVersion}";
            }
            else
            {
                DbStatus = $"❌ Desconectado: {info.Error}";
            }
        }
        catch (Exception ex)
        {
            DbConectado = false;
            DbStatus = $"❌ Erro: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SalvarMateriaPrimaAsync()
    {
        if (MateriaPrimaSelecionada == null) return;

        try
        {
            await _materiaPrimaRepo.SaveAsync(MateriaPrimaSelecionada);
            MostrarMensagem("Matéria-prima salva com sucesso!", true);
        }
        catch (Exception ex)
        {
            MostrarMensagem($"Erro ao salvar: {ex.Message}", false);
        }
    }

    [RelayCommand]
    private async Task SalvarTodasMateriasAsync()
    {
        try
        {
            await _materiaPrimaRepo.SaveManyAsync(MateriasPrimas);
            MostrarMensagem("Todas as matérias-primas foram salvas!", true);
        }
        catch (Exception ex)
        {
            MostrarMensagem($"Erro ao salvar: {ex.Message}", false);
        }
    }

    [RelayCommand]
    private async Task CriarUsuarioAsync()
    {
        if (string.IsNullOrWhiteSpace(NovoUsername) || string.IsNullOrWhiteSpace(NovoPassword))
        {
            MostrarMensagem("Username e senha são obrigatórios", false);
            return;
        }

        try
        {
            var existente = await _userRepo.GetByUsernameAsync(NovoUsername);
            if (existente != null)
            {
                MostrarMensagem("Username já existe", false);
                return;
            }

            var user = new User
            {
                Username = NovoUsername,
                Password = NovoPassword,
                DisplayName = NovoDisplayName,
                IsAdmin = NovoIsAdmin,
                UserType = NovoUserType
            };

            await _userRepo.CreateAsync(user);

            // Limpar campos
            NovoUsername = "";
            NovoPassword = "";
            NovoDisplayName = "";
            NovoIsAdmin = false;
            NovoUserType = "racao";

            // Recarregar lista
            await LoadDataAsync();

            MostrarMensagem("Usuário criado com sucesso!", true);
        }
        catch (Exception ex)
        {
            MostrarMensagem($"Erro ao criar usuário: {ex.Message}", false);
        }
    }

    [RelayCommand]
    private async Task ExcluirUsuarioAsync()
    {
        if (UsuarioSelecionado == null) return;

        if (UsuarioSelecionado.Username == "admin")
        {
            MostrarMensagem("Não é possível excluir o usuário admin", false);
            return;
        }

        try
        {
            await _userRepo.DeleteAsync(UsuarioSelecionado.Id);
            await LoadDataAsync();
            MostrarMensagem("Usuário excluído com sucesso!", true);
        }
        catch (Exception ex)
        {
            MostrarMensagem($"Erro ao excluir: {ex.Message}", false);
        }
    }

    [RelayCommand]
    private Task LimparBancoDadosAsync()
    {
        // TODO: Implementar confirmação e limpeza
        MostrarMensagem("Funcionalidade em desenvolvimento", false);
        return Task.CompletedTask;
    }

    private void MostrarMensagem(string texto, bool sucesso)
    {
        Mensagem = texto;
        MensagemSucesso = sucesso;

        // Limpar mensagem após 5 segundos
        Task.Delay(5000).ContinueWith(_ =>
        {
            App.Current.Dispatcher.Invoke(() => Mensagem = "");
        });
    }
}