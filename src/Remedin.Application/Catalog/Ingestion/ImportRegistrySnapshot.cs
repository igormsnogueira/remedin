using Microsoft.Extensions.Logging;
using Remedin.Domain.Ingestion;

namespace Remedin.Application.Catalog.Ingestion;

/// <summary>
/// Carrega a publicação mais recente da base de registro no catálogo.
///
/// Comando: passa pelo domínio, valida invariantes e escreve. Roda pelo
/// agendador, não por requisição de usuário.
/// </summary>
public sealed class ImportRegistrySnapshot(
    IRegistrySnapshotSource source,
    IMedicineCatalog catalog,
    IIngestionJournal journal,
    TimeProvider clock,
    ILogger<ImportRegistrySnapshot> logger)
{
    public const string SourceName = "anvisa-registro";

    public async Task<IngestionRun> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var run = IngestionRun.Start(SourceName, clock.GetUtcNow());

        try
        {
            var snapshot = await source.FetchAsync(cancellationToken);
            var lastHash = await journal.LastSuccessfulContentHashAsync(SourceName, cancellationToken);

            if (lastHash == snapshot.ContentHash)
            {
                run.Skip(snapshot.ContentHash, clock.GetUtcNow());
                logger.LogInformation("Carga ignorada: a origem publicou o mesmo arquivo da anterior.");
            }
            else
            {
                await catalog.ReplaceAllAsync(snapshot.Medicines, cancellationToken);

                run.Succeed(
                    snapshot.ContentHash,
                    snapshot.RowsRead,
                    snapshot.Medicines.Count,
                    snapshot.Rejected,
                    snapshot.Duplicates,
                    clock.GetUtcNow());

                logger.LogInformation(
                    "Carga concluída: {Read} linhas lidas, {Accepted} no catálogo, " +
                    "{Rejected} fora de escopo, {Duplicates} duplicadas.",
                    snapshot.RowsRead, snapshot.Medicines.Count, snapshot.Rejected, snapshot.Duplicates);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // A execução é registrada como falha antes de propagar: sem isso,
            // uma carga que quebrou de madrugada não deixa rastro nenhum.
            run.Fail(exception.Message, clock.GetUtcNow());
            await journal.RecordAsync(run, CancellationToken.None);

            logger.LogError(exception, "Carga da base de registro falhou.");
            throw;
        }

        await journal.RecordAsync(run, cancellationToken);
        return run;
    }
}
