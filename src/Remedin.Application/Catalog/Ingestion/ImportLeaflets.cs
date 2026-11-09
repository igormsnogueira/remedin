using Microsoft.Extensions.Logging;
using Remedin.Domain.Ingestion;

namespace Remedin.Application.Catalog.Ingestion;

/// <summary>
/// Carrega quais medicamentos têm bula no Bulário Eletrônico, e de quando.
///
/// O PDF não vem junto: o download no Bulário passa por uma verificação
/// contra robôs, e contorná-la seria burlar uma proteção posta de propósito.
/// O que a lista traz é o bastante para a ficha mostrar a data e levar ao
/// Bulário já filtrado pelo registro.
/// </summary>
public sealed class ImportLeaflets(
    ILeafletSource source,
    ILeafletStore store,
    IIngestionJournal journal,
    TimeProvider clock,
    ILogger<ImportLeaflets> logger)
{
    public const string SourceName = "anvisa-bulas";

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
                logger.LogInformation("Bulas: a origem publicou o mesmo arquivo da carga anterior.");
            }
            else
            {
                await store.ReplaceAllAsync(snapshot.Leaflets, cancellationToken);

                run.Succeed(
                    snapshot.ContentHash,
                    snapshot.RowsRead,
                    snapshot.Leaflets.Count,
                    snapshot.Rejected,
                    snapshot.Duplicates,
                    clock.GetUtcNow());

                logger.LogInformation(
                    "Bulas: {Read} linhas lidas, {Accepted} medicamentos com bula, " +
                    "{Rejected} recusadas, {Duplicates} repetidas.",
                    snapshot.RowsRead, snapshot.Leaflets.Count, snapshot.Rejected, snapshot.Duplicates);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            run.Fail(exception.Message, clock.GetUtcNow());
            await journal.RecordAsync(run, CancellationToken.None);

            logger.LogError(exception, "Carga das bulas falhou.");
            throw;
        }

        await journal.RecordAsync(run, cancellationToken);
        return run;
    }
}
