namespace Remedin.Domain.Ingestion;

public enum IngestionOutcome
{
    Running = 0,
    Succeeded = 1,
    Skipped = 2,
    Failed = 3,
}

public sealed class IngestionRun
{
    private IngestionRun(Guid id, string source, DateTimeOffset startedAt)
    {
        Id = id;
        Source = source;
        StartedAt = startedAt;
        Outcome = IngestionOutcome.Running;
    }

    public Guid Id { get; }

    public string Source { get; }

    public DateTimeOffset StartedAt { get; }

    public DateTimeOffset? FinishedAt { get; private set; }

    public IngestionOutcome Outcome { get; private set; }

    public string? ContentHash { get; private set; }

    public int RowsRead { get; private set; }

    public int Accepted { get; private set; }

    public int Rejected { get; private set; }

    public int Duplicates { get; private set; }

    public string? Detail { get; private set; }

    public static IngestionRun Start(string source, DateTimeOffset startedAt)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            throw new ArgumentException("Execução exige a origem.", nameof(source));
        }

        return new IngestionRun(Guid.NewGuid(), source.Trim(), startedAt);
    }

    public void Succeed(
        string contentHash,
        int rowsRead,
        int accepted,
        int rejected,
        int duplicates,
        DateTimeOffset finishedAt)
    {
        EnsureRunning();

        ContentHash = contentHash;
        RowsRead = rowsRead;
        Accepted = accepted;
        Rejected = rejected;
        Duplicates = duplicates;
        Finish(IngestionOutcome.Succeeded, finishedAt, detail: null);
    }

    public void Skip(string contentHash, DateTimeOffset finishedAt)
    {
        EnsureRunning();

        ContentHash = contentHash;
        Finish(IngestionOutcome.Skipped, finishedAt, "arquivo idêntico ao da carga anterior");
    }

    public void Fail(string reason, DateTimeOffset finishedAt)
    {
        EnsureRunning();

        Finish(IngestionOutcome.Failed, finishedAt, reason);
    }

    private void Finish(IngestionOutcome outcome, DateTimeOffset finishedAt, string? detail)
    {
        if (finishedAt < StartedAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(finishedAt), finishedAt, "Término não pode ser anterior ao início.");
        }

        Outcome = outcome;
        FinishedAt = finishedAt;
        Detail = detail;
    }

    private void EnsureRunning()
    {
        if (Outcome != IngestionOutcome.Running)
        {
            throw new InvalidOperationException($"Execução já foi encerrada como {Outcome}.");
        }
    }
}
