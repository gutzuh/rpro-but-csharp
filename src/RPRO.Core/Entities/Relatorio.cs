namespace RPRO.Core.Entities;

public class Relatorio
{
    public Guid Id { get; set; }
    public string? Dia { get; set; }
    public string? Hora { get; set; }
    public string? Nome { get; set; }
    public int Form1 { get; set; } // Código Fórmula
    public int Form2 { get; set; } // Número Fórmula
    
    // Produtos 1-40
    public decimal Prod_1 { get; set; }
    public decimal Prod_2 { get; set; }
    public decimal Prod_3 { get; set; }
    public decimal Prod_4 { get; set; }
    public decimal Prod_5 { get; set; }
    public decimal Prod_6 { get; set; }
    public decimal Prod_7 { get; set; }
    public decimal Prod_8 { get; set; }
    public decimal Prod_9 { get; set; }
    public decimal Prod_10 { get; set; }
    public decimal Prod_11 { get; set; }
    public decimal Prod_12 { get; set; }
    public decimal Prod_13 { get; set; }
    public decimal Prod_14 { get; set; }
    public decimal Prod_15 { get; set; }
    public decimal Prod_16 { get; set; }
    public decimal Prod_17 { get; set; }
    public decimal Prod_18 { get; set; }
    public decimal Prod_19 { get; set; }
    public decimal Prod_20 { get; set; }
    public decimal Prod_21 { get; set; }
    public decimal Prod_22 { get; set; }
    public decimal Prod_23 { get; set; }
    public decimal Prod_24 { get; set; }
    public decimal Prod_25 { get; set; }
    public decimal Prod_26 { get; set; }
    public decimal Prod_27 { get; set; }
    public decimal Prod_28 { get; set; }
    public decimal Prod_29 { get; set; }
    public decimal Prod_30 { get; set; }
    public decimal Prod_31 { get; set; }
    public decimal Prod_32 { get; set; }
    public decimal Prod_33 { get; set; }
    public decimal Prod_34 { get; set; }
    public decimal Prod_35 { get; set; }
    public decimal Prod_36 { get; set; }
    public decimal Prod_37 { get; set; }
    public decimal Prod_38 { get; set; }
    public decimal Prod_39 { get; set; }
    public decimal Prod_40 { get; set; }

    /// <summary>
    /// Retorna a soma total de todos os produtos
    /// </summary>
    public decimal TotalProdutos => 
        Prod_1 + Prod_2 + Prod_3 + Prod_4 + Prod_5 + 
        Prod_6 + Prod_7 + Prod_8 + Prod_9 + Prod_10 +
        Prod_11 + Prod_12 + Prod_13 + Prod_14 + Prod_15 +
        Prod_16 + Prod_17 + Prod_18 + Prod_19 + Prod_20 +
        Prod_21 + Prod_22 + Prod_23 + Prod_24 + Prod_25 +
        Prod_26 + Prod_27 + Prod_28 + Prod_29 + Prod_30 +
        Prod_31 + Prod_32 + Prod_33 + Prod_34 + Prod_35 +
        Prod_36 + Prod_37 + Prod_38 + Prod_39 + Prod_40;

    /// <summary>
    /// Retorna os valores dos produtos como array
    /// </summary>
    public decimal[] GetProdutosArray() => new[]
    {
        Prod_1, Prod_2, Prod_3, Prod_4, Prod_5,
        Prod_6, Prod_7, Prod_8, Prod_9, Prod_10,
        Prod_11, Prod_12, Prod_13, Prod_14, Prod_15,
        Prod_16, Prod_17, Prod_18, Prod_19, Prod_20,
        Prod_21, Prod_22, Prod_23, Prod_24, Prod_25,
        Prod_26, Prod_27, Prod_28, Prod_29, Prod_30,
        Prod_31, Prod_32, Prod_33, Prod_34, Prod_35,
        Prod_36, Prod_37, Prod_38, Prod_39, Prod_40
    };
}