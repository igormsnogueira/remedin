using Npgsql;
using NpgsqlTypes;
using Remedin.Application.Catalog.Search;
using Remedin.Domain.Medicines;

namespace Remedin.Infrastructure.Persistence;

/// <summary>
/// Lista os medicamentos com o mesmo princípio ativo, do mais barato ao mais
/// caro, com o preço do estado consultado.
/// </summary>
public sealed class PostgresMedicineAlternatives(RemedinDbContext context) : IMedicineAlternatives
{
    private const string Sql = """
        WITH target AS (
            SELECT registration_number, active_ingredient, substance_key
            FROM medicines
            WHERE registration_number = @registration
        )
        SELECT t.active_ingredient,
               m.registration_number,
               m.name,
               m.manufacturer,
               cheapest.description,
               cheapest.amount
        FROM target t
        JOIN medicines m ON m.substance_key = t.substance_key AND m.has_price
        -- Uma linha por medicamento: a apresentação de balcão mais barata,
        -- que é o que a comparação precisa mostrar.
        JOIN LATERAL (
            SELECT p.description, pr.amount
            FROM presentations p
            JOIN prices pr USING (registration_number, ggrem_code)
            WHERE p.registration_number = m.registration_number
              AND NOT p.hospital_only
              AND pr.kind = 'Consumer'
              AND pr.icms_rate = @rate
              AND NOT pr.free_trade_zone
            ORDER BY pr.amount ASC
            LIMIT 1
        ) AS cheapest ON true
        WHERE t.substance_key IS NOT NULL
        ORDER BY cheapest.amount ASC, m.name ASC
        LIMIT @limit;
        """;

    /// <summary>
    /// Um princípio ativo comum tem dezenas de fabricantes. Passar disso vira
    /// lista que ninguém lê, e o que importa está no começo.
    /// </summary>
    private const int MaximumAlternatives = 30;

    public async Task<AlternativesResult?> FindAsync(
        string registrationNumber,
        string state,
        CancellationToken cancellationToken)
    {
        if (!RegistrationNumber.TryParse(registrationNumber, out var registration))
        {
            return null;
        }

        var rate = IcmsRates.For(state);
        var connection = await PostgresMedicineSearch.OpenAsync(context, cancellationToken);

        await using var command = new NpgsqlCommand(Sql, connection);
        command.Parameters.Add(
            new NpgsqlParameter("registration", NpgsqlDbType.Char) { Value = registration.Value });
        command.Parameters.Add(new NpgsqlParameter("rate", NpgsqlDbType.Numeric) { Value = rate });
        command.Parameters.Add(
            new NpgsqlParameter("limit", NpgsqlDbType.Integer) { Value = MaximumAlternatives });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        string? activeIngredient = null;
        var alternatives = new List<MedicineAlternative>();

        while (await reader.ReadAsync(cancellationToken))
        {
            activeIngredient ??= reader.IsDBNull(0) ? null : reader.GetString(0);

            var number = reader.GetString(1);

            alternatives.Add(new MedicineAlternative(
                RegistrationNumber: number,
                Name: reader.GetString(2),
                Manufacturer: reader.IsDBNull(3) ? null : reader.GetString(3),
                Presentation: reader.GetString(4),
                ConsumerPrice: reader.GetDecimal(5),
                IsCurrent: number == registration.Value));
        }

        return new AlternativesResult(
            registration.Value, activeIngredient, state.ToUpperInvariant(), rate, alternatives);
    }
}
