using RPRO.Core.Models;
using System.Globalization;
using System.Text;

namespace RPRO.Services.Collectors;

public class FileParserService
{
    /// <summary>
    /// Detecta o encoding do arquivo
    /// </summary>
    public Encoding DetectEncoding(string filePath)
    {
        var bytes = File.ReadAllBytes(filePath);
        
        // Detectar BOM
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return Encoding.UTF8;
        
        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            return Encoding.Unicode;
        
        // Tentar detectar caracteres especiais (ISO-8859-1 vs UTF-8)
        try
        {
            var utf8 = Encoding.UTF8.GetString(bytes);
            if (utf8.Contains('�'))
                return Encoding.GetEncoding("ISO-8859-1");
            return Encoding.UTF8;
        }
        catch
        {
            return Encoding.GetEncoding("ISO-8859-1");
        }
    }

    /// <summary>
    /// Detecta o separador usado no CSV
    /// </summary>
    public char DetectSeparator(string firstLine)
    {
        var separators = new[] { ';', ',', '\t' };
        var counts = separators.Select(s => (Separator: s, Count: firstLine.Count(c => c == s))).ToList();
        return counts.OrderByDescending(x => x.Count).First().Separator;
    }

    /// <summary>
    /// Parse arquivo CSV de relatório (ração)
    /// </summary>
    public List<ParsedRow> ParseRelatorioFile(string filePath)
    {
        var rows = new List<ParsedRow>();
        var encoding = DetectEncoding(filePath);
        var lines = File.ReadAllLines(filePath, encoding);

        if (lines.Length == 0)
            return rows;

        var separator = DetectSeparator(lines[0]);
        var startIndex = 0;

        // Pular cabeçalho se existir
        if (lines[0].Contains("Dia") || lines[0].Contains("Data") || lines[0].Contains("Nome"))
            startIndex = 1;

        for (var i = startIndex; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            try
            {
                var parsed = ParseRelatorioLine(line, separator);
                if (parsed != null)
                {
                    parsed.RawLine = line;
                    rows.Add(parsed);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Parser] Erro na linha {i + 1}: {ex.Message}");
            }
        }

        return rows;
    }

    private ParsedRow? ParseRelatorioLine(string line, char separator)
    {
        var parts = line.Split(separator);
        
        if (parts.Length < 6)
            return null;

        var row = new ParsedRow();

        // Colunas esperadas: Dia, Hora, Nome, Form1, Form2, Prod1, Prod2, ...
        row.Date = parts[0].Trim();
        row.Time = parts[1].Trim();
        row.Label = parts.Length > 2 ? parts[2].Trim() : null;
        row.Form1 = TryParseInt(parts.Length > 3 ? parts[3] : null);
        row.Form2 = TryParseInt(parts.Length > 4 ? parts[4] : null);

        // Valores dos produtos (a partir da coluna 5)
        var values = new List<decimal>();
        for (var i = 5; i < parts.Length && i < 45; i++) // Máximo 40 produtos
        {
            values.Add(TryParseDecimal(parts[i]));
        }
        row.Values = values.ToArray();

        return row;
    }

    /// <summary>
    /// Parse arquivo CSV de amendoim
    /// </summary>
    public List<ParsedAmendoimRow> ParseAmendoimFile(string filePath, string tipo = "entrada")
    {
        var rows = new List<ParsedAmendoimRow>();
        var encoding = DetectEncoding(filePath);
        var lines = File.ReadAllLines(filePath, encoding);

        if (lines.Length == 0)
            return rows;

        var separator = DetectSeparator(lines[0]);
        var startIndex = 0;

        // Pular cabeçalho se existir
        if (lines[0].Contains("Dia") || lines[0].Contains("Data") || lines[0].Contains("Hora"))
            startIndex = 1;

        for (var i = startIndex; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            try
            {
                var parsed = ParseAmendoimLine(line, separator);
                if (parsed != null)
                {
                    parsed.RawLine = line;
                    rows.Add(parsed);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Parser] Erro na linha {i + 1}: {ex.Message}");
            }
        }

        return rows;
    }

    private ParsedAmendoimRow? ParseAmendoimLine(string line, char separator)
    {
        var parts = line.Split(separator);
        
        // Formato esperado: Dia, Hora, ?, ?, Código Produto, Nome Produto, ?, ?, Peso, ?, Balanca
        if (parts.Length < 9)
            return null;

        var row = new ParsedAmendoimRow
        {
            Dia = parts[0].Trim(),
            Hora = parts[1].Trim(),
            CodigoProduto = parts.Length > 4 ? parts[4].Trim() : "",
            NomeProduto = parts.Length > 5 ? parts[5].Trim() : "",
            Peso = TryParseDecimal(parts.Length > 8 ? parts[8] : "0"),
            Balanca = parts.Length > 10 ? parts[10].Trim() : null
        };

        // Determinar tipo baseado na balança (1,2 = entrada, 3 = saída)
        // Isso será tratado no collector

        return row;
    }

    /// <summary>
    /// Detecta o tipo de arquivo (racao ou amendoim)
    /// </summary>
    public string DetectFileType(string filePath)
    {
        var encoding = DetectEncoding(filePath);
        var lines = File.ReadAllLines(filePath, encoding).Take(5).ToArray();
        
        if (lines.Length == 0)
            return "unknown";

        var content = string.Join(" ", lines).ToLowerInvariant();

        // Detectar por conteúdo
        if (content.Contains("amendoim") || content.Contains("balanca") || content.Contains("caixa"))
            return "amendoim";

        if (content.Contains("formula") || content.Contains("prod_") || content.Contains("batida"))
            return "racao";

        // Detectar por estrutura de colunas
        var separator = DetectSeparator(lines[0]);
        var columns = lines[0].Split(separator).Length;

        // Amendoim tem ~11 colunas, Ração tem 45+
        if (columns > 20)
            return "racao";
        
        return "amendoim";
    }

    private int? TryParseInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        
        value = value.Trim().Replace("\"", "");
        
        if (int.TryParse(value, out var result))
            return result;
        
        return null;
    }

    private decimal TryParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;
        
        value = value.Trim().Replace("\"", "").Replace(",", ".");
        
        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;
        
        return 0;
    }
}