using Microsoft.Extensions.Logging;
using Remedin.Domain.Ingestion;

namespace Remedin.Application.Catalog.Ingestion;

public sealed class ImportPriceList(
    IPriceSnapshotSource source,
    IMedicineCatalog catalog,
    IMedicinePriceStore prices,
    IIngestionJournal journal,
    TimeProvider clock,
    ILogger<ImportPriceList> logger)
{
    public const string SourceName = "cmed-preco";

    public const decimal MinimumCoverage = 0.95m;

    public const decimal ExpectedCoverage = 0.99m;

    public async Task<IngestionRun> ExecuteAsync(
        RepeatedPublication whenRepeated = RepeatedPublication.Skip,
        CancellationToken cancellationToken = default)
    {
        var run = IngestionRun.Start(SourceName, clock.GetUtcNow());

        try
        {
            var snapshot = await source.FetchAsync(cancellationToken);
            var lastHash = await journal.LastSuccessfulContentHashAsync(SourceName, cancellationToken);
            var repeated = lastHash == snapshot.ContentHash;

            if (repeated && whenRepeated == RepeatedPublication.Skip)
            {
                run.Skip(snapshot.ContentHash, clock.GetUtcNow());
                logger.LogInformation("Carga de preço ignorada: mesma publicação da anterior.");
            }
            else
            {
                if (repeated)
                {
                    logger.LogInformation("Reprocessando a mesma publicação de preço, por pedido explícito.");
                }

                await LoadAsync(run, snapshot, cancellationToken);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            run.Fail(exception.Message, clock.GetUtcNow());
            await journal.RecordAsync(run, CancellationToken.None);

            logger.LogError(exception, "Carga da lista de preços falhou.");
            throw;
        }

        await journal.RecordAsync(run, cancellationToken);
        return run;
    }

    private async Task LoadAsync(IngestionRun run, PriceSnapshot snapshot, CancellationToken cancellationToken)
    {
        var registered = await catalog.RegistrationNumbersAsync(cancellationToken);

        var matched = snapshot.Medicines
            .Where(medicine => registered.Contains(medicine.Registration.Value))
            .ToList();

        var matchedRows = matched.Sum(medicine => medicine.Presentations.Count);
        var orphanRows = snapshot.RowsRead - snapshot.Rejected - matchedRows;
        var coverage = Coverage(matchedRows, snapshot.RowsRead - snapshot.Rejected);

        if (coverage < MinimumCoverage)
        {
            throw new InvalidOperationException(
                $"Cobertura do cruzamento em {coverage:P2}, abaixo do mínimo de {MinimumCoverage:P0}. " +
                "Carga rejeitada e dados anteriores mantidos.");
        }

        if (coverage < ExpectedCoverage)
        {
            logger.LogWarning(
                "Cobertura do cruzamento em {Coverage:P2}, abaixo do esperado de {Expected:P0}. " +
                "{Orphans} linhas de preço sem medicamento correspondente.",
                coverage, ExpectedCoverage, orphanRows);
        }

        await prices.ReplaceAllAsync(matched, cancellationToken);

        run.Succeed(
            snapshot.ContentHash,
            snapshot.RowsRead,
            accepted: matchedRows,
            rejected: snapshot.Rejected + orphanRows,
            duplicates: 0,
            clock.GetUtcNow());

        logger.LogInformation(
            "Carga de preço concluída: {Read} linhas lidas, {Matched} apresentações em " +
            "{Medicines} medicamentos, cobertura de {Coverage:P2}.",
            snapshot.RowsRead, matchedRows, matched.Count, coverage);
    }

    private static decimal Coverage(int matched, int total) =>
        total == 0 ? 0 : (decimal)matched / total;
}
