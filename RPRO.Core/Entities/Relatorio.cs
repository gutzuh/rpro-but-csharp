public class Relatorio
{
    public Guid Id { get; set; }
    public string? Dia { get; set; }
    public TimeSpan? Hora { get; set; }
    public string? Nome { get; set; }
    public int Form1 { get; set; } // Código Fórmula
    public int Form2 { get; set; } // Número Fórmula
    
    // Produtos 1-40 (simplificado com array)
    public int[] Produtos { get; set; } = new int[40];
}