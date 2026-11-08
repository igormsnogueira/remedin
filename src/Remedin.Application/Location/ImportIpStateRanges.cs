using System.Globalization;
using Microsoft.Extensions.Logging;
using Remedin.Application.Catalog.Ingestion;
using Remedin.Domain.Ingestion;

namespace Remedin.Application.Location;

/// <summary>
/// Carrega a base de geolocalização por IP, para o site abrir com o estado de
/// quem acessa já escolhido.
///
/// A publicação é mensal, e a carga roda toda semana com as outras. Por isso a
/// verificação de "já carregado" acontece antes do download: são mais de 100
/// MB, e baixar quatro vezes por mês para descartar três seria desperdício.
/// </summary>
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
                    // A deste mês ainda não saiu, e a anterior já está no banco.
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

    /// <summary>"2026-11": o mesmo formato do nome do arquivo publicado.</summary>
    public static string ReleaseOf(DateOnly month) =>
        month.ToString("yyyy-MM", CultureInfo.InvariantCulture);
}
