using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Remedin.Application.Catalog.Ingestion;
using Remedin.Domain.Medicines;

namespace Remedin.Infrastructure.Ingestion.Anvisa;

/// <summary>
/// Lê a lista de bulas dos dados abertos da ANVISA
/// (CONSULTAS/DOCUMENTOS/TA_CONSULTA_BULA_PRODUTO.CSV).
///
/// O arquivo não tem cabeçalho. As posições foram conferidas na publicação de
/// outubro de 2026, contra fichas conhecidas: MECLIN 101180165, bula de
/// 09/12/2025; omeprazol Cristália 102980354, de 24/07/2026.
/// </summary>
public sealed class AnvisaLeafletReader
{
    /// <summary>Latin1, como a base de registro. Lido como UTF-8, o acento quebra sem erro.</summary>
    private static readonly Encoding FileEncoding = Encoding.Latin1;

    /// <summary>O arquivo real tem 1,6 MB; abaixo disso veio truncado.</summary>
    public const long MinimumBytes = 500_000;

    private const int RegistrationNumberField = 2;
    private const int PublishedAtField = 9;

    /// <summary>
    /// A data vem no formato americano, mês antes do dia: "06/18/2026" é 18 de
    /// junho. Lida como dia/mês, metade das datas daria erro e a outra metade
    /// sairia trocada sem aviso.
    /// </summary>
    private const string DateFormat = "MM/dd/yyyy HH:mm:ss";

    public LeafletSnapshot Read(Stream content, string contentHash)
    {
        ArgumentNullException.ThrowIfNull(content);

        SourceFile.EnsureLooksLikeCsv(content, MinimumBytes);

        return ReadRows(content, contentHash);
    }

    /// <summary>Sem a validação de arquivo. Serve para ler amostra em teste.</summary>
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

            // Hoje é uma linha por registro. Se vier mais de uma, vale a bula
            // mais recente, que é a que o Bulário mostra.
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
