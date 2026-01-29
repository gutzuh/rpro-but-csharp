namespace RPRO.Core.Entities;

public class MateriaPrima
{
    public int Num { get; set; }
    public string Produto { get; set; } = string.Empty;
    public int Medida { get; set; }
    public bool Ativo { get; set; }
    public bool IgnorarCalculos { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
