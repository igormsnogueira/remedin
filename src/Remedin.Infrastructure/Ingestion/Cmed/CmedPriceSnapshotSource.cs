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
            .Select(group => new PricedMedicine(
                group.Key,
                ToClinicalInformation(group.First()),
                ToPresentations(group)))
    ];

    /// <summary>
    /// A informação clínica se repete em todas as apresentações do mesmo
    /// registro, então a primeira linha basta.
    /// </summary>
    private static ClinicalInformation ToClinicalInformation(PriceRow row) =>
        new(row.Substance, row.TherapeuticClassCode, row.TherapeuticClassName, row.PrescriptionBand);

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
                row.Prices.Where(IsServed).Select(ToPrice)))
    ];

    /// <summary>
    /// Mantém só o preço que alguma tela consegue mostrar.
    ///
    /// A CMED publica 14 alíquotas e as variantes de área de livre comércio, o
    /// que dá 52 colunas de preço por linha. O catálogo consulta por estado, e
    /// os 27 estados usam quatro alíquotas; a zona franca exigiria perguntar o
    /// município, e nenhuma tela faz isso. O resto é dado que entra no banco e
    /// nenhuma consulta alcança (ADR 0012).
    ///
    /// O descarte é aqui, na leitura, e não na gravação: montar 1,2 milhão de
    /// objetos para jogar 85% fora custa memória que o processo de carga não
    /// precisa gastar.
    /// </summary>
    private static bool IsServed(PriceQuote quote) =>
        !quote.FreeTradeZone && IcmsRates.Serves(quote.Aliquot);

    private static Price ToPrice(PriceQuote quote) =>
        Price.Create(quote.Kind, quote.Aliquot, quote.FreeTradeZone, quote.Value);
}
