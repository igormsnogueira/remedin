using Npgsql;
using NpgsqlTypes;
using Remedin.Application.Catalog.Ingestion;

namespace Remedin.Infrastructure.Persistence;

/// <summary>
/// A lista de bulas, numa tabela própria.
///
/// Fora da tabela de medicamentos de propósito: o catálogo é regravado a cada
/// carga da base de registro, e uma coluna ali seria apagada junto. Também
/// não tem chave estrangeira, para as duas cargas não dependerem da ordem.
/// </summary>
public sealed class PostgresLeaflets(RemedinDbContext context) : ILeafletStore
{
    public async Task ReplaceAllAsync(IReadOnlyList<Leaflet> leaflets, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(leaflets);

        var connection = await PostgresMedicineSearch.OpenAsync(context, cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using (var clear = new NpgsqlCommand("TRUNCATE leaflets;", connection, transaction))
        {
            await clear.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var writer = await connection.BeginBinaryImportAsync(
            "COPY leaflets (registration_number, published_on) FROM STDIN (FORMAT BINARY)",
            cancellationToken))
        {
            foreach (var leaflet in leaflets)
            {
                await writer.StartRowAsync(cancellationToken);
                await writer.WriteAsync(leaflet.RegistrationNumber, NpgsqlDbType.Char, cancellationToken);
                await writer.WriteAsync(leaflet.PublishedOn, NpgsqlDbType.Date, cancellationToken);
            }

            await writer.CompleteAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }
}
