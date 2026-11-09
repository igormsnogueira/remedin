namespace Remedin.Application.Catalog.Ingestion;

/// <summary>A bula de um medicamento no Bulário Eletrônico da ANVISA.</summary>
/// <param name="PublishedOn">Data da versão atual da bula.</param>
public sealed record Leaflet(string RegistrationNumber, DateOnly PublishedOn);

/// <summary>
/// Uma publicação da lista de bulas dos dados abertos da ANVISA, já lida.
/// </summary>
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
    /// <summary>Troca a lista inteira numa transação, como o catálogo.</summary>
    Task ReplaceAllAsync(IReadOnlyList<Leaflet> leaflets, CancellationToken cancellationToken);
}
