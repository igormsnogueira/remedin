using Remedin.Application.Catalog.Ingestion;

namespace Remedin.Worker;

public sealed class IngestionSettings
{
    public const string Section = "Ingestion";

    public bool ReloadRepeated { get; init; }

    public RepeatedPublication WhenRepeated =>
        ReloadRepeated ? RepeatedPublication.Reload : RepeatedPublication.Skip;
}
