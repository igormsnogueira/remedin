using System.Net;
using Npgsql;
using NpgsqlTypes;
using Remedin.Application.Location;

namespace Remedin.Infrastructure.Persistence;

/// <summary>
/// As faixas de IP por estado, no PostgreSQL.
///
/// O tipo inet do PostgreSQL compara endereços IPv4 e IPv6 do jeito certo, e
/// um índice comum sobre o início da faixa basta: a consulta pega a última
/// faixa que começa antes do endereço e confere se ele cabe nela.
/// </summary>
public sealed class PostgresIpStates(RemedinDbContext context) : IIpStateStore, IStateLocator
{
    public async Task ReplaceAllAsync(
        IReadOnlyList<IpStateRange> ranges,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ranges);

        var connection = await PostgresMedicineSearch.OpenAsync(context, cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using (var clear = new NpgsqlCommand("TRUNCATE ip_state_ranges;", connection, transaction))
        {
            await clear.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var writer = await connection.BeginBinaryImportAsync(
            "COPY ip_state_ranges (range_start, range_end, state) FROM STDIN (FORMAT BINARY)",
            cancellationToken))
        {
            foreach (var range in ranges)
            {
                await writer.StartRowAsync(cancellationToken);
                await writer.WriteAsync(range.Start, NpgsqlDbType.Inet, cancellationToken);
                await writer.WriteAsync(range.End, NpgsqlDbType.Inet, cancellationToken);
                await writer.WriteAsync(range.State, NpgsqlDbType.Char, cancellationToken);
            }

            await writer.CompleteAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<string?> FindStateAsync(IPAddress address, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(address);

        // Endereço IPv4 que chega escrito como IPv6 (::ffff:1.2.3.4) não
        // encontraria as faixas IPv4.
        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        var connection = await PostgresMedicineSearch.OpenAsync(context, cancellationToken);

        await using var command = new NpgsqlCommand(
            """
            SELECT state
            FROM (
                SELECT state, range_end
                FROM ip_state_ranges
                WHERE range_start <= @address
                ORDER BY range_start DESC
                LIMIT 1
            ) AS candidate
            WHERE candidate.range_end >= @address;
            """,
            connection);

        command.Parameters.Add(new NpgsqlParameter("address", NpgsqlDbType.Inet) { Value = address });

        return await command.ExecuteScalarAsync(cancellationToken) as string;
    }
}
