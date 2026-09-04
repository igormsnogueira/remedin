using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using Remedin.Application.Catalog.Ingestion;
using Remedin.Domain.Medicines;

namespace Remedin.Infrastructure.Persistence;

/// <summary>
/// Grava o catálogo de medicamentos.
///
/// A carga atualiza em cima do que já existe, em vez de apagar e reinserir.
/// Apagar levaria junto, pela cascata, as apresentações e os preços — que são
/// da outra carga e não seriam recriados se a lista de preço não tivesse
/// mudado naquele mês.
/// </summary>
public sealed class MedicineCatalog(RemedinDbContext context) : IMedicineCatalog
{
    private const string CreateStaging = """
        CREATE TEMP TABLE incoming_medicines (
            registration_number    char(9) PRIMARY KEY,
            name                   text NOT NULL,
            active_ingredient      text,
            manufacturer           text,
            therapeutic_class_name text,
            status                 text NOT NULL
        ) ON COMMIT DROP;
        """;

    private const string CopyStaging = """
        COPY incoming_medicines (registration_number, name, active_ingredient,
            manufacturer, therapeutic_class_name, status)
        FROM STDIN (FORMAT BINARY)
        """;

    /// <summary>
    /// Nome, fabricante e situação vêm sempre da ANVISA. Princípio ativo e
    /// classe terapêutica só entram se ainda estiverem vazios, porque a lista
    /// de preço traz versão melhor deles (ADR 0009). Tarja e código da classe
    /// não aparecem aqui: são exclusivos da CMED e sobrevivem à carga.
    /// </summary>
    private const string Upsert = """
        INSERT INTO medicines (registration_number, name, active_ingredient, manufacturer,
                               therapeutic_class_name, status)
        SELECT registration_number, name, active_ingredient, manufacturer,
               therapeutic_class_name, status
        FROM incoming_medicines
        ON CONFLICT (registration_number) DO UPDATE
        SET name                   = EXCLUDED.name,
            manufacturer           = EXCLUDED.manufacturer,
            status                 = EXCLUDED.status,
            active_ingredient      = coalesce(medicines.active_ingredient, EXCLUDED.active_ingredient),
            therapeutic_class_name = coalesce(medicines.therapeutic_class_name, EXCLUDED.therapeutic_class_name);
        """;

    /// <summary>Registro que saiu da publicação sai do catálogo.</summary>
    private const string DeleteMissing = """
        DELETE FROM medicines m
        WHERE NOT EXISTS (
            SELECT 1 FROM incoming_medicines i
            WHERE i.registration_number = m.registration_number
        );
        """;

    public async Task ReplaceAllAsync(
        IReadOnlyList<Medicine> medicines,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(medicines);

        var connection = (NpgsqlConnection)context.Database.GetDbConnection();

        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        // Tudo numa transação: quem consulta continua vendo o catálogo
        // anterior até o commit.
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await ExecuteAsync(connection, CreateStaging, cancellationToken);
        await CopyAsync(connection, medicines, cancellationToken);
        await ExecuteAsync(connection, Upsert, cancellationToken);
        await ExecuteAsync(connection, DeleteMissing, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlySet<string>> RegistrationNumbersAsync(CancellationToken cancellationToken)
    {
        var numbers = await context.Medicines
            .AsNoTracking()
            .Select(medicine => medicine.RegistrationNumber.Value)
            .ToListAsync(cancellationToken);

        return numbers.ToHashSet(StringComparer.Ordinal);
    }

    private static async Task CopyAsync(
        NpgsqlConnection connection,
        IReadOnlyList<Medicine> medicines,
        CancellationToken cancellationToken)
    {
        await using var writer = await connection.BeginBinaryImportAsync(CopyStaging, cancellationToken);

        foreach (var medicine in medicines)
        {
            await writer.StartRowAsync(cancellationToken);
            await writer.WriteAsync(medicine.RegistrationNumber.Value, NpgsqlDbType.Char, cancellationToken);
            await writer.WriteAsync(medicine.Name, NpgsqlDbType.Text, cancellationToken);
            await WriteNullableAsync(writer, medicine.ActiveIngredient, cancellationToken);
            await WriteNullableAsync(writer, medicine.Manufacturer, cancellationToken);
            await WriteNullableAsync(writer, medicine.TherapeuticClassName, cancellationToken);
            await writer.WriteAsync(medicine.Status.ToString(), NpgsqlDbType.Text, cancellationToken);
        }

        await writer.CompleteAsync(cancellationToken);
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

    private static async Task ExecuteAsync(
        NpgsqlConnection connection,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
