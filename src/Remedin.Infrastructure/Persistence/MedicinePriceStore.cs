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
        await UpdateClinicalInformationAsync(connection, medicines, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    /// <summary>
    /// Completa no catálogo os campos clínicos que a lista de preço traz e a
    /// base de registro não tem, ou tem pior (ADR 0009).
    ///
    /// Via tabela temporária: são cerca de 9 mil medicamentos, e uma atualização
    /// por linha custaria 9 mil idas ao banco.
    /// </summary>
    private static async Task UpdateClinicalInformationAsync(
        NpgsqlConnection connection,
        IReadOnlyList<PricedMedicine> medicines,
        CancellationToken cancellationToken)
    {
        await using (var create = new NpgsqlCommand(
            """
            CREATE TEMP TABLE clinical_information (
                registration_number    char(9) PRIMARY KEY,
                active_ingredient      text,
                therapeutic_class_code text,
                therapeutic_class_name text,
                prescription_band      text
            ) ON COMMIT DROP;
            """,
            connection))
        {
            await create.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var writer = await connection.BeginBinaryImportAsync(
            """
            COPY clinical_information (registration_number, active_ingredient,
                therapeutic_class_code, therapeutic_class_name, prescription_band)
            FROM STDIN (FORMAT BINARY)
            """,
            cancellationToken))
        {
            foreach (var medicine in medicines)
            {
                await writer.StartRowAsync(cancellationToken);
                await writer.WriteAsync(medicine.Registration.Value, NpgsqlDbType.Char, cancellationToken);
                await WriteNullableAsync(writer, medicine.Clinical.ActiveIngredient, cancellationToken);
                await WriteNullableAsync(writer, medicine.Clinical.TherapeuticClassCode, cancellationToken);
                await WriteNullableAsync(writer, medicine.Clinical.TherapeuticClassName, cancellationToken);
                await WriteNullableAsync(writer, medicine.Clinical.PrescriptionBand, cancellationToken);
            }

            await writer.CompleteAsync(cancellationToken);
        }

        // coalesce preserva o valor da ANVISA quando a CMED não publica o
        // campo: os dois recortes não coincidem, e sobrescrever com nulo
        // apagaria informação boa.
        await using (var update = new NpgsqlCommand(
            """
            UPDATE medicines m
            SET active_ingredient      = coalesce(c.active_ingredient, m.active_ingredient),
                therapeutic_class_code = c.therapeutic_class_code,
                therapeutic_class_name = coalesce(c.therapeutic_class_name, m.therapeutic_class_name),
                prescription_band      = c.prescription_band
            FROM clinical_information c
            WHERE m.registration_number = c.registration_number;
            """,
            connection))
        {
            await update.ExecuteNonQueryAsync(cancellationToken);
        }

        // A marca de preço precisa refletir a carga inteira, inclusive quem
        // saiu da lista e deixou de ter preço.
        await using var markPriced = new NpgsqlCommand(
            """
            UPDATE medicines m
            SET has_price = EXISTS (
                SELECT 1 FROM clinical_information c
                WHERE c.registration_number = m.registration_number
            )
            WHERE m.has_price <> EXISTS (
                SELECT 1 FROM clinical_information c
                WHERE c.registration_number = m.registration_number
            );
            """,
            connection);

        await markPriced.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task WriteNullableAsync(
        NpgsqlBinaryImporter writer,
        string? value,
        CancellationToken cancellationToken)
    {
        if (value is null)
        {
            await writer.WriteNullAsync(cancellationToken);
        }
        else
        {
            await writer.WriteAsync(value, NpgsqlDbType.Text, cancellationToken);
        }
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
