using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Remedin.Application.Catalog.Ingestion;
using Remedin.Domain.Medicines;

namespace Remedin.Infrastructure.Ingestion.Anvisa;

public sealed class AnvisaLeafletReader
{
    // Latin-1 e sem cabeçalho: as posições foram conferidas na publicação de outubro de 2026.
    private static readonly Encoding FileEncoding = Encoding.Latin1;

    public const long MinimumBytes = 500_000;

    private const int RegistrationNumberField = 2;
    private const int PublishedAtField = 9;

    // A fonte usa mês/dia/ano: "06/18/2026" é 18 de junho.
    private const string DateFormat = "MM/dd/yyyy HH:mm:ss";

    public LeafletSnapshot Read(Stream content, string contentHash)
    {
        ArgumentNullException.ThrowIfNull(content);

        SourceFile.EnsureLooksLikeCsv(content, MinimumBytes);

        return ReadRows(content, contentHash);
    }

    public LeafletSnapshot ReadRows(Stream content, string contentHash)
    {
        ArgumentNullException.ThrowIfNull(content);

        var leaflets = new Dictionary<string, Leaflet>();
        var rowsRead = 0;
        var rejected = 0;
        var duplicates = 0;

        using var reader = new StreamReader(content, FileEncoding, leaveOpen: true);
        using var csv = new CsvReader(reader, Configuration());

        while (csv.Read())
        {
            rowsRead++;

            if (!RegistrationNumber.TryParse(csv.GetField(RegistrationNumberField), out var registration)
                || !DateTime.TryParseExact(
                    csv.GetField(PublishedAtField),
                    DateFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var publishedAt))
            {
                rejected++;
                continue;
            }

            var leaflet = new Leaflet(registration.Value, DateOnly.FromDateTime(publishedAt));

            if (leaflets.TryGetValue(leaflet.RegistrationNumber, out var existing))
            {
                duplicates++;

                if (existing.PublishedOn >= leaflet.PublishedOn)
                {
                    continue;
                }
            }

            leaflets[leaflet.RegistrationNumber] = leaflet;
        }

        return new LeafletSnapshot([.. leaflets.Values], rowsRead, rejected, duplicates, contentHash);
    }

    private static CsvConfiguration Configuration() =>
        new(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HasHeaderRecord = false,
            TrimOptions = TrimOptions.Trim,
            BadDataFound = null,
            MissingFieldFound = null,
        };
}
