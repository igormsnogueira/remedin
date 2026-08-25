using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using Remedin.Application.Catalog.Ingestion;
using Remedin.Domain.Medicines;

namespace Remedin.Infrastructure.Persistence;

/// <summary>
/// Grava as apresentações e seus preços.
///
/// São cerca de 25 mil apresentações e 1,3 milhão de preços por carga. Pelo
/// rastreamento de mudanças do EF isso levaria minutos e ocuparia memória à
/// toa; a importação em bloco do PostgreSQL resolve em segundos. É a exceção
/// que a carga em lote justifica — o resto do sistema continua no EF.
/// </summary>
public sealed class MedicinePriceStore(RemedinDbContext context) : IMedicinePriceStore
{
    public async Task ReplaceAllAsync(
        IReadOnlyList<PricedMedicine> medicines,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(medicines);

        var connection = (NpgsqlConnection)context.Database.GetDbConnection();

        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        // Os preços saem junto pela cascata declarada na migration.
        await using (var clear = new NpgsqlCommand("DELETE FROM presentations;", connection, transaction))
        {
            await clear.ExecuteNonQueryAsync(cancellationToken);
        }

        await CopyPresentationsAsync(connection, medicines, cancellationToken);
        await CopyPricesAsync(connection, medicines, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    private static async Task CopyPresentationsAsync(
        NpgsqlConnection connection,
        IReadOnlyList<PricedMedicine> medicines,
        CancellationToken cancellationToken)
    {
        await using var writer = await connection.BeginBinaryImportAsync(
            """
            COPY presentations (registration_number, ggrem_code, description, hospital_only, sold_recently)
            FROM STDIN (FORMAT BINARY)
            """,
            cancellationToken);

        foreach (var medicine in medicines)
        {
            foreach (var presentation in medicine.Presentations)
            {
                await writer.StartRowAsync(cancellationToken);
                await writer.WriteAsync(medicine.Registration.Value, NpgsqlDbType.Char, cancellationToken);
                await writer.WriteAsync(presentation.GgremCode, NpgsqlDbType.Varchar, cancellationToken);
                await writer.WriteAsync(presentation.Description, NpgsqlDbType.Varchar, cancellationToken);
                await writer.WriteAsync(presentation.HospitalOnly, NpgsqlDbType.Boolean, cancellationToken);
                await writer.WriteAsync(presentation.SoldRecently, NpgsqlDbType.Boolean, cancellationToken);
            }
        }

        await writer.CompleteAsync(cancellationToken);
    }

    private static async Task CopyPricesAsync(
        NpgsqlConnection connection,
        IReadOnlyList<PricedMedicine> medicines,
        CancellationToken cancellationToken)
    {
        await using var writer = await connection.BeginBinaryImportAsync(
            """
            COPY prices (registration_number, ggrem_code, kind, icms_rate, free_trade_zone, amount)
            FROM STDIN (FORMAT BINARY)
            """,
            cancellationToken);

        foreach (var medicine in medicines)
        {
            foreach (var presentation in medicine.Presentations)
            {
                foreach (var price in presentation.Prices)
                {
                    await writer.StartRowAsync(cancellationToken);
                    await writer.WriteAsync(medicine.Registration.Value, NpgsqlDbType.Char, cancellationToken);
                    await writer.WriteAsync(presentation.GgremCode, NpgsqlDbType.Varchar, cancellationToken);
                    await writer.WriteAsync(price.Kind.ToString(), NpgsqlDbType.Varchar, cancellationToken);

                    if (price.IcmsRate is null)
                    {
                        await writer.WriteNullAsync(cancellationToken);
                    }
                    else
                    {
                        await writer.WriteAsync(price.IcmsRate.Value, NpgsqlDbType.Numeric, cancellationToken);
                    }

                    await writer.WriteAsync(price.FreeTradeZone, NpgsqlDbType.Boolean, cancellationToken);
                    await writer.WriteAsync(price.Amount, NpgsqlDbType.Numeric, cancellationToken);
                }
            }
        }

        await writer.CompleteAsync(cancellationToken);
    }
}
