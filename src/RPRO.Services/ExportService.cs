using RPRO.Core.Entities;
using System.Text;

namespace RPRO.Services;

public class ExportService
{
    /// <summary>
    /// Exporta dados de ração para CSV
    /// </summary>
    public async Task<string> ExportRacaoToCsvAsync(
        IEnumerable<Relatorio> dados,
        Dictionary<int, string>? labels = null)
    {
        var sb = new StringBuilder();

        // Cabeçalho
        var headers = new List<string> { "Dia", "Hora", "Fórmula", "Código", "Número" };
        for (int i = 1; i <= 40; i++)
        {
            var label = labels?.TryGetValue(i, out var nome) == true ? nome : $"Prod_{i}";
            headers.Add(label);
        }
        headers.Add("Total");
        sb.AppendLine(string.Join(";", headers));

        // Dados
        foreach (var r in dados)
        {
            var row = new List<string>
            {
                r.Dia ?? "",
                r.Hora ?? "",
                $"\"{r.Nome?.Replace("\"", "\"\"")}\"",
                r.Form1.ToString(),
                r.Form2.ToString()
            };

            var produtos = r.GetProdutosArray();
            foreach (var p in produtos)
            {
                row.Add(p.ToString("F2"));
            }
            row.Add(r.TotalProdutos.ToString("F2"));

            sb.AppendLine(string.Join(";", row));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Exporta dados de amendoim para CSV
    /// </summary>
    public async Task<string> ExportAmendoimToCsvAsync(IEnumerable<Amendoim> dados)
    {
        var sb = new StringBuilder();

        // Cabeçalho
        sb.AppendLine("Tipo;Dia;Hora;Código Produto;Nome Produto;Peso (kg);Balança");

        // Dados
        foreach (var a in dados)
        {
            var row = new List<string>
            {
                a.Tipo,
                a.Dia,
                a.Hora,
                a.CodigoProduto,
                $"\"{a.NomeProduto?.Replace("\"", "\"\"")}\"",
                a.Peso.ToString("F3"),
                a.Balanca ?? ""
            };

            sb.AppendLine(string.Join(";", row));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Salva conteúdo em arquivo
    /// </summary>
    public async Task SaveToFileAsync(string content, string filePath, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        await File.WriteAllTextAsync(filePath, content, encoding);
    }
}