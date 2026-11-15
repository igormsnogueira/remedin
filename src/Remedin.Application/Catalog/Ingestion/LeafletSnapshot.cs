namespace Remedin.Application.Catalog.Ingestion;

public sealed record Leaflet(string RegistrationNumber, DateOnly PublishedOn);

public sealed record LeafletSnapshot(
    IReadOnlyList<Leaflet> Leaflets,
    int RowsRead,
    int Rejected,
    int Duplicates,
    string ContentHash);

public interface ILeafletSource
{
    Task<LeafletSnapshot> FetchAsync(CancellationToken cancellationToken);
}

public interface ILeafletStore
{
    Task ReplaceAllAsync(IReadOnlyList<Leaflet> leaflets, CancellationToken cancellationToken);
}
