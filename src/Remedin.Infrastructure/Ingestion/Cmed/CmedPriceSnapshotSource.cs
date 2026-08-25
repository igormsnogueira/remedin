using System.Security.Cryptography;
using Remedin.Application.Catalog.Ingestion;
using Remedin.Domain.Medicines;

namespace Remedin.Infrastructure.Ingestion.Cmed;

/// <summary>
/// Baixa e interpreta a lista de preços vigente da CMED, agrupando as
/// apresentações por número de registro.
/// </summary>
public sealed class CmedPriceSnapshotSource(HttpClient http, CmedPriceReader reader) : IPriceSnapshotSource
{
    /// <summary>
    /// A extensão é minúscula. A descrição oficial do conjunto escreve
    /// `.CSV`, e essa URL devolve 404 porque o servidor diferencia caixa.
    /// </summary>
    public static readonly Uri FileUrl =
        new("https://dados.anvisa.gov.br/dados/TA_PRECO_MEDICAMENTO.csv");

    public async Task<PriceSnapshot> FetchAsync(CancellationToken cancellationToken)
    {
        using var response = await http.GetAsync(FileUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var content = new MemoryStream();
        await response.Content.CopyToAsync(content, cancellationToken);
        content.Position = 0;

        var result = reader.Read(content);

        content.Position = 0;
        var hash = Convert.ToHexStringLower(await SHA256.HashDataAsync(content, cancellationToken));

        return new PriceSnapshot(GroupByRegistration(result.Rows), result.RowsRead, result.Rejected.Count, hash);
    }

    private static List<PricedMedicine> GroupByRegistration(IEnumerable<PriceRow> rows) =>
    [
        .. rows
            .GroupBy(row => row.Registration)
            .Select(group => new PricedMedicine(group.Key, ToPresentations(group)))
    ];

    private static List<Presentation> ToPresentations(IEnumerable<PriceRow> rows) =>
    [
        .. rows
            // A lista tem um caso de código repetido dentro do mesmo registro.
            // Manter o primeiro evita quebrar a invariante do agregado por
            // causa de um defeito da origem.
            .DistinctBy(row => row.GgremCode)
            .Select(row => Presentation.Create(
                row.GgremCode,
                row.Presentation,
                row.HospitalOnly,
                row.SoldRecently,
                row.Prices.Select(ToPrice)))
    ];

    private static Price ToPrice(PriceQuote quote) =>
        Price.Create(quote.Kind, quote.Aliquot, quote.FreeTradeZone, quote.Value);
}
