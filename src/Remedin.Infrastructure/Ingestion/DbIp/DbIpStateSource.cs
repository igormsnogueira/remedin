using System.Globalization;
using System.IO.Compression;
using System.Net;
using Microsoft.Extensions.Logging;
using Remedin.Application.Location;
using Remedin.Domain.Medicines;

namespace Remedin.Infrastructure.Ingestion.DbIp;

/// <summary>
/// Baixa a base gratuita IP to City Lite, da DB-IP, e fica só com as faixas do
/// Brasil, já com a sigla do estado.
///
/// Licença Creative Commons BY 4.0: o uso é livre desde que o site mostre o
/// link para db-ip.com nas páginas que usam o resultado (o rodapé do front).
///
/// O arquivo compactado passa de 100 MB e descompactado passa de 600 MB, com
/// 7 milhões de faixas do mundo todo. Por isso é lido em fluxo, linha a linha,
/// sem nunca estar inteiro na memória; só as cerca de 1% brasileiras ficam.
/// </summary>
public sealed class DbIpStateSource(HttpClient http, ILogger<DbIpStateSource> logger) : IIpStateSource
{
    private const string Country = "BR";

    /// <summary>
    /// Abaixo disso o arquivo veio errado (truncado, ou com o país em outro
    /// campo). Melhor falhar a carga que trocar a tabela boa por uma vazia.
    /// </summary>
    private const int MinimumBrazilianRanges = 10_000;

    public static Uri UrlFor(string release) =>
        new($"https://download.db-ip.com/free/dbip-city-lite-{release}.csv.gz");

    public async Task<IpStateSnapshot> FetchAsync(DateOnly month, CancellationToken cancellationToken)
    {
        // A publicação sai no começo do mês, mas não no dia 1 à meia-noite.
        // Se a deste mês ainda não existe, vale a do mês anterior.
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

    /// <summary>
    /// Lê o CSV já descompactado. Público para ser testado sem rede, com um
    /// mínimo de faixas compatível com um arquivo de exemplo.
    /// </summary>
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

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            // ip_start,ip_end,continent,country,stateprov,city,latitude,longitude
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

    /// <summary>
    /// O arquivo vem em ordem de endereço, e faixas vizinhas do mesmo estado
    /// são comuns (cidades diferentes no mesmo estado). Juntar reduz a tabela
    /// a uma fração, sem mudar nenhuma resposta.
    /// </summary>
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
