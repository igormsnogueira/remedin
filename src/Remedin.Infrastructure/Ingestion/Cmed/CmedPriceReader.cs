using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using Remedin.Domain.Medicines;

namespace Remedin.Infrastructure.Ingestion.Cmed;

/// <summary>
/// Lê a lista de preços de medicamentos da CMED.
///
/// As regras vêm da análise da lista real, em docs/analise-dados-cmed.md.
/// </summary>
public sealed partial class CmedPriceReader
{
    private static readonly Encoding FileEncoding = Encoding.UTF8;

    /// <summary>A lista real tem 15,8 MB; abaixo disso veio truncada.</summary>
    public const long MinimumBytes = 5_000_000;

    /// <summary>
    /// O arquivo começa com dezenas de linhas de texto jurídico. Procurar o
    /// cabeçalho pela linha mais larga, em vez de pular um número fixo, é o
    /// que sobrevive a esse texto mudar de tamanho.
    /// </summary>
    private const int HeaderScanLines = 200;

    private const string RegistrationColumn = "REGISTRO";
    private const string GgremColumn = "CÓDIGO GGREM";
    private const string ProductColumn = "PRODUTO";
    private const string SubstanceColumn = "SUBSTÂNCIA";
    private const string LaboratoryColumn = "LABORATÓRIO";
    private const string TherapeuticClassColumn = "CLASSE TERAPÊUTICA";
    private const string PresentationColumn = "APRESENTAÇÃO";
    private const string PrescriptionBandColumn = "TARJA";
    private const string HospitalOnlyColumn = "RESTRIÇÃO HOSPITALAR";
    private const string SoldRecentlyColumn = "COMERCIALIZAÇÃO 2025";

    private static readonly string[] RequiredColumns =
        [RegistrationColumn, GgremColumn, ProductColumn, PresentationColumn];

    /// <summary>
    /// A CMED usa "-" e "- (*)" como preenchimento de campo sem valor. Sem
    /// converter para ausência, viram categoria fantasma nos filtros.
    /// </summary>
    private static readonly string[] Placeholders = ["-", "- (*)"];

    private static readonly CultureInfo BrazilianNumbers = CultureInfo.GetCultureInfo("pt-BR");

    public PriceReadResult Read(Stream content)
    {
        ArgumentNullException.ThrowIfNull(content);

        SourceFile.EnsureLooksLikeCsv(content, MinimumBytes);

        return ReadRows(content);
    }

    /// <summary>Sem a validação de arquivo. Serve para ler amostra em teste.</summary>
    public PriceReadResult ReadRows(Stream content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var headerLine = FindHeaderLine(content);

        content.Position = 0;
        using var reader = new StreamReader(content, FileEncoding, leaveOpen: true);
        using var csv = new CsvReader(reader, Configuration());

        for (var skipped = 0; skipped < headerLine; skipped++)
        {
            csv.Read();
        }

        csv.Read();
        csv.ReadHeader();
        EnsureRequiredColumns(csv);

        var priceColumns = PriceColumns(csv.HeaderRecord ?? []);
        var rows = new List<PriceRow>();
        var rejected = new List<RejectedPriceRow>();
        var rowsRead = 0;

        while (csv.Read())
        {
            rowsRead++;

            if (!RegistrationNumber.TryParse(Field(csv, RegistrationColumn), out var registration))
            {
                rejected.Add(new RejectedPriceRow(csv.Parser.Row, "sem número de registro válido"));
                continue;
            }

            var ggrem = Field(csv, GgremColumn);
            var product = Field(csv, ProductColumn);

            if (string.IsNullOrWhiteSpace(ggrem) || string.IsNullOrWhiteSpace(product))
            {
                rejected.Add(new RejectedPriceRow(csv.Parser.Row, "sem código da apresentação ou nome"));
                continue;
            }

            rows.Add(BuildRow(csv, registration, ggrem, product, priceColumns));
        }

        return new PriceReadResult(rows, rowsRead, rejected, headerLine);
    }

    private static PriceRow BuildRow(
        CsvReader csv,
        RegistrationNumber registration,
        string ggrem,
        string product,
        IReadOnlyList<PriceColumn> priceColumns)
    {
        var (classCode, className) = SplitTherapeuticClass(Field(csv, TherapeuticClassColumn));

        return new PriceRow(
            registration,
            ggrem,
            product,
            Field(csv, SubstanceColumn),
            Field(csv, LaboratoryColumn),
            classCode,
            className,
            Field(csv, PresentationColumn) ?? string.Empty,
            Field(csv, PrescriptionBandColumn),
            HospitalOnly: string.Equals(Field(csv, HospitalOnlyColumn), "Sim", StringComparison.OrdinalIgnoreCase),
            SoldRecently: string.Equals(Field(csv, SoldRecentlyColumn), "Sim", StringComparison.OrdinalIgnoreCase),
            Prices: ReadPrices(csv, priceColumns));
    }

    private static List<PriceQuote> ReadPrices(CsvReader csv, IReadOnlyList<PriceColumn> priceColumns)
    {
        var prices = new List<PriceQuote>(priceColumns.Count);

        foreach (var column in priceColumns)
        {
            var value = ParseMoney(Field(csv, column.Header));

            if (value is not null)
            {
                prices.Add(new PriceQuote(column.Kind, column.Aliquot, column.FreeTradeZone, value.Value));
            }
        }

        return prices;
    }

    /// <summary>
    /// A CMED escreve "1.234,56". Interpretar com cultura invariável devolve
    /// 123456, e o site mostraria preço cem vezes maior.
    /// </summary>
    private static decimal? ParseMoney(string? value) =>
        value is not null
        && decimal.TryParse(value, NumberStyles.Number, BrazilianNumbers, out var parsed)
            ? parsed
            : null;

    /// <summary>"D7B2 - CORTICOESTERÓIDES" vira código e descrição.</summary>
    private static (string? Code, string? Name) SplitTherapeuticClass(string? value)
    {
        if (value is null)
        {
            return (null, null);
        }

        var separator = value.IndexOf(" - ", StringComparison.Ordinal);

        return separator < 0
            ? (null, value)
            : (value[..separator].Trim(), value[(separator + 3)..].Trim());
    }

    private static int FindHeaderLine(Stream content)
    {
        content.Position = 0;

        using var reader = new StreamReader(content, FileEncoding, leaveOpen: true);
        using var parser = new CsvParser(reader, Configuration());

        var headerLine = 0;
        var widest = 0;

        for (var line = 0; line < HeaderScanLines && parser.Read(); line++)
        {
            var filled = parser.Record?.Count(field => !string.IsNullOrWhiteSpace(field)) ?? 0;

            if (filled > widest)
            {
                widest = filled;
                headerLine = line;
            }
        }

        return headerLine;
    }

    private static IReadOnlyList<PriceColumn> PriceColumns(IEnumerable<string> header) =>
        [.. header.Select(PriceColumn.TryParse).OfType<PriceColumn>()];

    private static CsvConfiguration Configuration() =>
        new(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            BadDataFound = null,
            MissingFieldFound = null,
            // O texto jurídico do começo tem linhas com contagem de colunas
            // diferente da do cabeçalho.
            IgnoreBlankLines = false,
            DetectDelimiter = false,
        };

    private static void EnsureRequiredColumns(CsvReader csv)
    {
        var header = csv.HeaderRecord ?? [];
        var missing = RequiredColumns.Except(header, StringComparer.OrdinalIgnoreCase).ToArray();

        if (missing.Length > 0)
        {
            throw new InvalidDataException(
                $"Colunas obrigatórias ausentes: {string.Join(", ", missing)}. " +
                "O layout da origem mudou.");
        }
    }

    private static string? Field(CsvReader csv, string column)
    {
        if (!csv.TryGetField<string>(column, out var value) || string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();

        return Placeholders.Contains(trimmed) ? null : trimmed;
    }

    /// <summary>
    /// Uma coluna de preço, identificada pelo próprio cabeçalho:
    /// "PF Sem Impostos", "PF 0%", "PMC 17,5 %", "PMC 22 %  ALC".
    /// </summary>
    private sealed partial record PriceColumn(string Header, PriceKind Kind, decimal? Aliquot, bool FreeTradeZone)
    {
        public static PriceColumn? TryParse(string header)
        {
            var match = Pattern().Match(header.Trim());

            if (!match.Success)
            {
                return null;
            }

            var kind = match.Groups[1].Value.Equals("PF", StringComparison.OrdinalIgnoreCase)
                ? PriceKind.Factory
                : PriceKind.Consumer;

            decimal? aliquot = match.Groups[3].Success
                ? decimal.Parse(match.Groups[3].Value, BrazilianNumbers)
                : null;

            return new PriceColumn(header, kind, aliquot, match.Groups[4].Success);
        }

        [GeneratedRegex(
            @"^(PF|PMC)\s+(?:(Sem\s+Impostos)|([\d,]+)\s*%)\s*(ALC)?$",
            RegexOptions.IgnoreCase)]
        private static partial Regex Pattern();
    }
}
