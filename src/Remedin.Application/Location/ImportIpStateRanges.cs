using System.Globalization;
using Microsoft.Extensions.Logging;
using Remedin.Application.Catalog.Ingestion;
using Remedin.Domain.Ingestion;

namespace Remedin.Application.Location;

public sealed class ImportIpStateRanges(
    IIpStateSource source,
    IIpStateStore store,
    IIngestionJournal journal,
    TimeProvider clock,
    ILogger<ImportIpStateRanges> logger)
{
    public const string SourceName = "dbip-estados";

    public async Task<IngestionRun> ExecuteAsync(
        RepeatedPublication whenRepeated = RepeatedPublication.Skip,
        CancellationToken cancellationToken = default)
    {
        var now = clock.GetUtcNow();
        var run = IngestionRun.Start(SourceName, now);
        var month = DateOnly.FromDateTime(now.UtcDateTime);
        var currentRelease = ReleaseOf(month);

        try
        {
            // A publicação é mensal e passa de 100 MB: confere se já foi carregada antes de baixar.
            var lastRelease = await journal.LastSuccessfulContentHashAsync(SourceName, cancellationToken);

            if (lastRelease == currentRelease && whenRepeated == RepeatedPublication.Skip)
            {
                run.Skip(currentRelease, clock.GetUtcNow());
                logger.LogInformation("Geolocalização: a publicação deste mês já está carregada.");
            }
            else
            {
                var snapshot = await source.FetchAsync(month, cancellationToken);

                if (snapshot.Release == lastRelease && whenRepeated == RepeatedPublication.Skip)
                {
                    run.Skip(snapshot.Release, clock.GetUtcNow());
                    logger.LogInformation(
                        "Geolocalização: publicação {Release} já carregada; a deste mês ainda não saiu.",
                        snapshot.Release);
                }
                else
                {
                    await store.ReplaceAllAsync(snapshot.Ranges, cancellationToken);

                    run.Succeed(
                        snapshot.Release,
                        snapshot.RowsRead,
                        snapshot.Ranges.Count,
                        snapshot.Unmapped,
                        duplicates: 0,
                        clock.GetUtcNow());

                    logger.LogInformation(
                        "Geolocalização {Release}: {Read} faixas do Brasil lidas, {Stored} gravadas " +
                        "depois de juntar as vizinhas, {Unmapped} sem estado reconhecido.",
                        snapshot.Release, snapshot.RowsRead, snapshot.Ranges.Count, snapshot.Unmapped);
                }
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            run.Fail(exception.Message, clock.GetUtcNow());
            await journal.RecordAsync(run, CancellationToken.None);

            logger.LogError(exception, "Carga da geolocalização falhou.");
            throw;
        }

        await journal.RecordAsync(run, cancellationToken);
        return run;
    }

    public static string ReleaseOf(DateOnly month) =>
        month.ToString("yyyy-MM", CultureInfo.InvariantCulture);
}
