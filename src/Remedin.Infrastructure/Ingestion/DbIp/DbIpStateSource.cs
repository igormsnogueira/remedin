using System.Globalization;
using System.IO.Compression;
using System.Net;
using Microsoft.Extensions.Logging;
using Remedin.Application.Location;
using Remedin.Domain.Medicines;

namespace Remedin.Infrastructure.Ingestion.DbIp;

// Base IP to City Lite (DB-IP), licença CC BY 4.0: o rodapé do site precisa do link para db-ip.com.
public sealed class DbIpStateSource(HttpClient http, ILogger<DbIpStateSource> logger) : IIpStateSource
{
    private const string Country = "BR";

    private const int MinimumBrazilianRanges = 10_000;

    public static Uri UrlFor(string release) =>
        new($"https://download.db-ip.com/free/dbip-city-lite-{release}.csv.gz");

    public async Task<IpStateSnapshot> FetchAsync(DateOnly month, CancellationToken cancellationToken)
    {
        foreach (var candidate in new[] { month, month.AddMonths(-1) })
        {
            var release = candidate.ToString("yyyy-MM", CultureInfo.InvariantCulture);

            using var response = await http.GetAsync(
                UrlFor(release), HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogInformation("Geolocalização {Release} ainda não publicada.", release);
                continue;
            }

            response.EnsureSuccessStatusCode();

            await using var compressed = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var csv = new GZipStream(compressed, CompressionMode.Decompress);

            return await ReadAsync(release, csv, MinimumBrazilianRanges, cancellationToken);
        }

        throw new InvalidOperationException(
            "Nenhuma publicação da geolocalização encontrada para este mês nem para o anterior.");
    }

    public static async Task<IpStateSnapshot> ReadAsync(
        string release,
        Stream csv,
        int minimumRows,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(csv);

        var ranges = new List<IpStateRange>();
        var rowsRead = 0;
        var unmapped = 0;

        // São 7 milhões de faixas: lido em fluxo, só as do Brasil ficam em memória.
        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            var fields = DbIpCsv.ReadFields(line, count: 5);

            if (fields.Count < 5 || fields[3] != Country)
            {
                continue;
            }

            rowsRead++;

            if (!BrazilianStates.TryFindByName(fields[4], out var state)
                || !IPAddress.TryParse(fields[0], out var start)
                || !IPAddress.TryParse(fields[1], out var end))
            {
                unmapped++;
                continue;
            }

            AppendMerging(ranges, new IpStateRange(start, end, state));
        }

        if (rowsRead < minimumRows)
        {
            throw new InvalidDataException(
                $"Só {rowsRead} faixas do Brasil na publicação {release}. " +
                "O arquivo provavelmente veio truncado ou mudou de formato.");
        }

        return new IpStateSnapshot(release, ranges, rowsRead, unmapped);
    }

    private static void AppendMerging(List<IpStateRange> ranges, IpStateRange next)
    {
        if (ranges.Count > 0)
        {
            var last = ranges[^1];

            if (last.State == next.State && IpMath.IsNext(last.End, next.Start))
            {
                ranges[^1] = last with { End = next.End };
                return;
            }
        }

        ranges.Add(next);
    }
}
