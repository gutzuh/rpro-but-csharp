using RPRO.Data;
using System;
using System.Threading.Tasks;

namespace RPRO.App;

/// <summary>
/// Classe para inserir dados de teste no banco de dados
/// </summary>
public static class SeedTestData
{
    public static async Task SeedAsync(DatabaseContext db)
    {
        try
        {
            // Verificar se já há dados
            var count = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM relatorio");
            if (count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"[Seed] Já existem {count} registros em relatorio");
                return;
            }

            System.Diagnostics.Debug.WriteLine("[Seed] Inserindo dados de teste...");

            // Inserir dados de teste para Ração
            var random = new Random();
            var formulas = new[] { "Ração Crescimento", "Ração Engorda", "Ração Inicial", "Ração Postura", "Ração Gestação" };

            for (int dia = 0; dia < 30; dia++)
            {
                var data = DateTime.Today.AddDays(-dia);
                var diaStr = data.ToString("dd/MM/yyyy");

                for (int batida = 0; batida < random.Next(5, 15); batida++)
                {
                    var hora = new TimeSpan(random.Next(6, 22), random.Next(0, 60), random.Next(0, 60));
                    var formula = formulas[random.Next(formulas.Length)];
                    var form1 = random.Next(1, 50);
                    var form2 = random.Next(1, 50);

                    // Produtos (40 produtos possíveis)
                    var prods = new int[40];
                    for (int i = 0; i < random.Next(5, 15); i++)
                    {
                        prods[random.Next(40)] = random.Next(10, 500);
                    }

                    await db.ExecuteAsync(@"
                        INSERT INTO relatorio (id, Dia, Hora, Nome, Form1, Form2, 
                            Prod_1, Prod_2, Prod_3, Prod_4, Prod_5, Prod_6, Prod_7, Prod_8, Prod_9, Prod_10,
                            Prod_11, Prod_12, Prod_13, Prod_14, Prod_15, Prod_16, Prod_17, Prod_18, Prod_19, Prod_20,
                            Prod_21, Prod_22, Prod_23, Prod_24, Prod_25, Prod_26, Prod_27, Prod_28, Prod_29, Prod_30,
                            Prod_31, Prod_32, Prod_33, Prod_34, Prod_35, Prod_36, Prod_37, Prod_38, Prod_39, Prod_40)
                        VALUES (@id, @dia, @hora, @nome, @form1, @form2,
                            @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10,
                            @p11, @p12, @p13, @p14, @p15, @p16, @p17, @p18, @p19, @p20,
                            @p21, @p22, @p23, @p24, @p25, @p26, @p27, @p28, @p29, @p30,
                            @p31, @p32, @p33, @p34, @p35, @p36, @p37, @p38, @p39, @p40)",
                        new
                        {
                            id = Guid.NewGuid().ToString(),
                            dia = diaStr,
                            hora = hora,
                            nome = formula,
                            form1 = form1,
                            form2 = form2,
                            p1 = prods[0], p2 = prods[1], p3 = prods[2], p4 = prods[3], p5 = prods[4],
                            p6 = prods[5], p7 = prods[6], p8 = prods[7], p9 = prods[8], p10 = prods[9],
                            p11 = prods[10], p12 = prods[11], p13 = prods[12], p14 = prods[13], p15 = prods[14],
                            p16 = prods[15], p17 = prods[16], p18 = prods[17], p19 = prods[18], p20 = prods[19],
                            p21 = prods[20], p22 = prods[21], p23 = prods[22], p24 = prods[23], p25 = prods[24],
                            p26 = prods[25], p27 = prods[26], p28 = prods[27], p29 = prods[28], p30 = prods[29],
                            p31 = prods[30], p32 = prods[31], p33 = prods[32], p34 = prods[33], p35 = prods[34],
                            p36 = prods[35], p37 = prods[36], p38 = prods[37], p39 = prods[38], p40 = prods[39]
                        });
                }
            }

            // Inserir dados de teste para Amendoim
            var produtos = new[] { "Amendoim Cru", "Amendoim Torrado", "Amendoim Descascado", "Pasta de Amendoim" };
            
            for (int dia = 0; dia < 30; dia++)
            {
                var data = DateTime.Today.AddDays(-dia);
                var diaStr = data.ToString("dd/MM/yyyy");

                for (int registro = 0; registro < random.Next(10, 30); registro++)
                {
                    var hora = $"{random.Next(6, 22):D2}:{random.Next(0, 60):D2}:{random.Next(0, 60):D2}";
                    var tipo = random.Next(2) == 0 ? "entrada" : "saida";
                    var produto = produtos[random.Next(produtos.Length)];
                    var peso = Math.Round(random.NextDouble() * 500 + 50, 3);

                    await db.ExecuteAsync(@"
                        INSERT IGNORE INTO amendoim (tipo, dia, hora, codigoProduto, nomeProduto, peso, balanca)
                        VALUES (@tipo, @dia, @hora, @codigo, @nome, @peso, @balanca)",
                        new
                        {
                            tipo = tipo,
                            dia = diaStr,
                            hora = hora,
                            codigo = $"PROD{random.Next(1, 100):D3}",
                            nome = produto,
                            peso = peso,
                            balanca = random.Next(2) == 0 ? "BAL1" : "BAL2"
                        });
                }
            }

            System.Diagnostics.Debug.WriteLine("[Seed] Dados de teste inseridos com sucesso!");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Seed] Erro: {ex.Message}");
        }
    }
}
