using System.Security.Cryptography;
using Remedin.Application.Catalog.Ingestion;
using Remedin.Domain.Medicines;

namespace Remedin.Infrastructure.Ingestion.Cmed;

public sealed class CmedPriceSnapshotSource(HttpClient http, CmedPriceReader reader) : IPriceSnapshotSource
{
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
            .Select(group => new PricedMedicine(
                group.Key,
                ToClinicalInformation(group.First()),
                ToPresentations(group)))
    ];

    private static ClinicalInformation ToClinicalInformation(PriceRow row) =>
        new(row.Substance, row.TherapeuticClassCode, row.TherapeuticClassName, row.PrescriptionBand);

    private static List<Presentation> ToPresentations(IEnumerable<PriceRow> rows) =>
    [
        .. rows
            .DistinctBy(row => row.GgremCode)
            .Select(row => Presentation.Create(
                row.GgremCode,
                row.Presentation,
                row.HospitalOnly,
                row.SoldRecently,
                row.Prices.Where(IsServed).Select(ToPrice)))
    ];

    private static bool IsServed(PriceQuote quote) =>
        !quote.FreeTradeZone && IcmsRates.Serves(quote.Aliquot);

    private static Price ToPrice(PriceQuote quote) =>
        Price.Create(quote.Kind, quote.Aliquot, quote.FreeTradeZone, quote.Value);
}
