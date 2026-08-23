using Microsoft.EntityFrameworkCore;
using Remedin.Application.Catalog.Ingestion;
using Remedin.Domain.Ingestion;

namespace Remedin.Infrastructure.Persistence;

public sealed class IngestionJournal(RemedinDbContext context) : IIngestionJournal
{
    public Task<string?> LastSuccessfulContentHashAsync(
        string source,
        CancellationToken cancellationToken) =>
        context.IngestionRuns
            .Where(run => run.Source == source && run.Outcome == IngestionOutcome.Succeeded)
            .OrderByDescending(run => run.StartedAt)
            .Select(run => run.ContentHash)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task RecordAsync(IngestionRun run, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(run);

        context.IngestionRuns.Add(run);
        await context.SaveChangesAsync(cancellationToken);
    }
}
