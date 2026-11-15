using Npgsql;
using NpgsqlTypes;
using Remedin.Application.Catalog.Search;
using Remedin.Domain.Medicines;

namespace Remedin.Infrastructure.Persistence;

public sealed class PostgresMedicinePurposes(RemedinDbContext context) : IMedicinePurposes
{
    private const string Eligible = "m.status = 'Active' AND m.has_price AND m.therapeutic_class_code IS NOT NULL";

    private const string CountSql = $"""
        SELECT upper(left(m.therapeutic_class_code, 1)) AS group_code, count(*) AS total
        FROM medicines m
        WHERE {Eligible}
        GROUP BY 1;
        """;

    private const string MedicinesSql = $"""
        WITH chosen AS (
            SELECT m.registration_number, m.name, m.active_ingredient, m.manufacturer,
                   m.therapeutic_class_code, m.therapeutic_class_name
            FROM medicines m
            WHERE {Eligible}
              AND upper(left(m.therapeutic_class_code, 1)) = @group
        )
        SELECT c.registration_number, c.name, c.active_ingredient, c.manufacturer,
               c.therapeutic_class_code, c.therapeutic_class_name, cheapest.amount,
               count(*) OVER () AS total
        FROM chosen c
        LEFT JOIN LATERAL (
            SELECT min(pr.amount) AS amount
            FROM presentations p
            JOIN prices pr USING (registration_number, ggrem_code)
            WHERE p.registration_number = c.registration_number
              AND NOT p.hospital_only
              AND NOT p.institutional
              AND pr.kind = 'Consumer'
              AND pr.icms_rate = @rate
              AND NOT pr.free_trade_zone
        ) AS cheapest ON true
        ORDER BY cheapest.amount ASC NULLS LAST, c.name ASC
        LIMIT @limit OFFSET @offset;
        """;

    public async Task<IReadOnlyList<PurposeSummary>> ListAsync(CancellationToken cancellationToken)
    {
        var connection = await PostgresMedicineSearch.OpenAsync(context, cancellationToken);

        await using var command = new NpgsqlCommand(CountSql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var totals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        while (await reader.ReadAsync(cancellationToken))
        {
            totals[reader.GetString(0)] = (int)reader.GetInt64(1);
        }

        return
        [
            .. TherapeuticCategories.Groups
                .Where(group => totals.ContainsKey(group.Code))
                .Select(group => new PurposeSummary(group.Code, group.Label, totals[group.Code]))
        ];
    }

    public async Task<PurposeMedicines?> FindAsync(
        string code,
        string state,
        Page page,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(page);

        var group = TherapeuticCategories.Groups
            .FirstOrDefault(category => string.Equals(category.Code, code, StringComparison.OrdinalIgnoreCase));

        if (group is null)
        {
            return null;
        }

        var rate = IcmsRates.For(state);
        var connection = await PostgresMedicineSearch.OpenAsync(context, cancellationToken);

        await using var command = new NpgsqlCommand(MedicinesSql, connection);
        command.Parameters.Add(new NpgsqlParameter("group", NpgsqlDbType.Text) { Value = group.Code });
        command.Parameters.Add(new NpgsqlParameter("rate", NpgsqlDbType.Numeric) { Value = rate });
        command.Parameters.Add(new NpgsqlParameter("limit", NpgsqlDbType.Integer) { Value = page.Size });
        command.Parameters.Add(new NpgsqlParameter("offset", NpgsqlDbType.Integer) { Value = page.Offset });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var medicines = new List<MedicineSummary>();
        var total = 0;

        while (await reader.ReadAsync(cancellationToken))
        {
            medicines.Add(new MedicineSummary(
                RegistrationNumber: reader.GetString(0),
                Name: reader.GetString(1),
                ActiveIngredient: reader.IsDBNull(2) ? null : reader.GetString(2),
                Manufacturer: reader.IsDBNull(3) ? null : reader.GetString(3),
                TherapeuticClassCode: reader.IsDBNull(4) ? null : reader.GetString(4),
                TherapeuticClass: reader.IsDBNull(5) ? null : reader.GetString(5),
                IsActive: true,
                CheapestConsumerPrice: reader.IsDBNull(6) ? null : reader.GetDecimal(6)));

            total = (int)reader.GetInt64(7);
        }

        return new PurposeMedicines(
            group.Code,
            group.Label,
            state.ToUpperInvariant(),
            rate,
            PagedResults<MedicineSummary>.Of(medicines, total, page));
    }
}
