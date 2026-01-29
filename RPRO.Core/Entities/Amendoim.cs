public class Amendoim
{
    public int Id { get; set; }
    public string Tipo { get; set; } = "entrada"; // entrada | saida
    public string Dia { get; set; } = "";
    public string Hora { get; set; } = "";
    public string CodigoProduto { get; set; } = "";
    public string NomeProduto { get; set; } = "";
    public decimal Peso { get; set; }
    public string? Balanca { get; set; }
    public DateTime CreatedAt { get; set; }
}