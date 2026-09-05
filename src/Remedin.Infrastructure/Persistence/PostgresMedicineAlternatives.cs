using Npgsql;
using NpgsqlTypes;
using Remedin.Application.Catalog.Search;
using Remedin.Domain.Medicines;

namespace Remedin.Infrastructure.Persistence;

/// <summary>
/// Lista os medicamentos com o mesmo princípio ativo, do mais barato por
/// comprimido ao mais caro, com o preço do estado consultado.
/// </summary>
public sealed class PostgresMedicineAlternatives(RemedinDbContext context) : IMedicineAlternatives
{
    /// <summary>
    /// Preço por comprimido quando a quantidade é conhecida, e pela caixa
    /// quando não é. Comparar caixas premia a embalagem pequena, que custa
    /// menos e rende menos.
    /// </summary>
    private const string PricePerUnit = "pr.amount / coalesce(nullif(p.unit_count, 0), 1)";

    /// <summary>A apresentação de balcão mais barata por comprimido.</summary>
    private const string CheapestPresentation = $"""
        SELECT p.description, p.dosage_mg, p.unit_count, pr.amount,
               {PricePerUnit} AS price_per_unit
        FROM presentations p
        JOIN prices pr USING (registration_number, ggrem_code)
        WHERE p.registration_number = source.registration_number
          AND NOT p.hospital_only
          AND pr.kind = 'Consumer'
          AND pr.icms_rate = @rate
          AND NOT pr.free_trade_zone
        ORDER BY price_per_unit ASC, pr.amount ASC
        LIMIT 1
        """;

    private const string Sql = $"""
        WITH target AS (
            SELECT source.registration_number, source.active_ingredient, source.substance_key,
                   chosen.dosage_mg
            FROM medicines source
            LEFT JOIN LATERAL ({CheapestPresentation}) AS chosen ON true
            WHERE source.registration_number = @registration
        )
        SELECT t.active_ingredient,
               source.registration_number,
               source.name,
               source.manufacturer,
               chosen.description,
               chosen.amount,
               chosen.dosage_mg,
               chosen.unit_count
        FROM target t
        JOIN medicines source
          ON source.substance_key = t.substance_key AND source.has_price
        JOIN LATERAL ({CheapestPresentation}) AS chosen ON true
        WHERE t.substance_key IS NOT NULL
        -- Mesma dosagem primeiro: 10 MG e 40 MG do mesmo princípio ativo não
        -- são alternativas um do outro, e apareciam misturados.
        ORDER BY (chosen.dosage_mg IS NOT DISTINCT FROM t.dosage_mg) DESC,
                 chosen.price_per_unit ASC,
                 source.name ASC
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
                DosageInMilligrams: reader.IsDBNull(6) ? null : reader.GetDecimal(6),
                UnitCount: reader.IsDBNull(7) ? null : reader.GetInt32(7),
                IsCurrent: number == registration.Value));
        }

        return new AlternativesResult(
            registration.Value, activeIngredient, state.ToUpperInvariant(), rate, alternatives);
    }
}
