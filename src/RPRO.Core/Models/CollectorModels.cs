namespace RPRO.Core.Models;

/// <summary>
/// Configuração de conexão com IHM
/// </summary>
public class IHMConfig
{
    public string Ip { get; set; } = "";
    public string User { get; set; } = "anonymous";
    public string Password { get; set; } = "";
    public string CaminhoRemoto { get; set; } = "/InternalStorage/data/";
    public bool Ativo { get; set; } = true;
}

/// <summary>
/// Configuração do coletor de ração
/// </summary>
public class CollectorRacaoConfig
{
    public IHMConfig IHM { get; set; } = new();
    public int IntervaloSegundos { get; set; } = 60;
    public bool AutoStart { get; set; } = false;
}

/// <summary>
/// Configuração do coletor de amendoim
/// </summary>
public class CollectorAmendoimConfig
{
    public IHMConfig IHM1 { get; set; } = new(); // Entrada (Balanças 1,2)
    public IHMConfig? IHM2 { get; set; } = null;  // Saída (Balança 3) - opcional
    public bool DuasIHMs { get; set; } = false;
    public int IntervaloSegundos { get; set; } = 60;
    public bool AutoStart { get; set; } = false;
}

/// <summary>
/// Arquivo coletado do IHM
/// </summary>
public class CollectedFile
{
    public string Name { get; set; } = "";
    public string LocalPath { get; set; } = "";
    public string RemotePath { get; set; } = "";
    public DateTime ModifiedTime { get; set; }
    public long Size { get; set; }
    public string Hash { get; set; } = "";
    public string SourceIhm { get; set; } = "";
}

/// <summary>
/// Resultado do processamento de arquivo
/// </summary>
public class ProcessResult
{
    public bool Success { get; set; }
    public string FileName { get; set; } = "";
    public int RowsProcessed { get; set; }
    public int RowsSaved { get; set; }
    public int RowsDuplicated { get; set; }
    public int Errors { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// Status do coletor
/// </summary>
public class CollectorStatus
{
    public bool IsRunning { get; set; }
    public DateTime? LastRun { get; set; }
    public DateTime? NextRun { get; set; }
    public int FilesProcessed { get; set; }
    public int TotalRowsSaved { get; set; }
    public string? LastError { get; set; }
    public List<ProcessResult> RecentResults { get; set; } = new();
}

/// <summary>
/// Linha parseada de arquivo CSV
/// </summary>
public class ParsedRow
{
    public string? Date { get; set; }
    public string? Time { get; set; }
    public string? Label { get; set; }
    public int? Form1 { get; set; }
    public int? Form2 { get; set; }
    public decimal[] Values { get; set; } = Array.Empty<decimal>();
    public string? RawLine { get; set; }
}

/// <summary>
/// Linha parseada de arquivo CSV de amendoim
/// </summary>
public class ParsedAmendoimRow
{
    public string Dia { get; set; } = "";
    public string Hora { get; set; } = "";
    public string CodigoProduto { get; set; } = "";
    public string CodigoCaixa { get; set; } = "";
    public string NomeProduto { get; set; } = "";
    public decimal Peso { get; set; }
    public string? Balanca { get; set; }
    public string? RawLine { get; set; }
}