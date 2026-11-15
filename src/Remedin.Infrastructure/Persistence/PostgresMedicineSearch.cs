using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using Remedin.Application.Catalog.Search;
using Remedin.Domain.Medicines;

namespace Remedin.Infrastructure.Persistence;

public sealed class PostgresMedicineSearch(RemedinDbContext context) : IMedicineSearch
{
    private const int MinimumTermLength = 3;

    private const string Sql = """
        WITH input AS (
            SELECT immutable_unaccent(lower(@term)) AS plain,
                   websearch_to_tsquery('portuguese', immutable_unaccent(@term)) AS query
        ),
        scored AS (
            SELECT m.registration_number,
                   m.name,
                   m.active_ingredient,
                   m.manufacturer,
                   m.therapeutic_class_code,
                   m.therapeutic_class_name,
                   m.status,
                   m.has_price,
                   ( CASE WHEN immutable_unaccent(lower(m.name)) = i.plain THEN 10 ELSE 0 END
                   + CASE WHEN immutable_unaccent(lower(m.name)) LIKE i.plain || '%' THEN 3 ELSE 0 END
                   + ts_rank(m.search_vector, i.query) * 4
                   + similarity(immutable_unaccent(m.name), i.plain)
                   + similarity(immutable_unaccent(coalesce(m.active_ingredient, '')), i.plain) * 0.5
                   ) AS score
            FROM medicines m, input i
            WHERE m.search_vector @@ i.query
               OR immutable_unaccent(m.name) % i.plain
               OR immutable_unaccent(coalesce(m.active_ingredient, '')) % i.plain
        ),
        ranked AS (
            SELECT *, count(*) OVER () AS total
            FROM scored
            ORDER BY (status = 'Active') DESC, score DESC, has_price DESC, name ASC
            LIMIT @limit OFFSET @offset
        )
        SELECT r.registration_number, r.name, r.active_ingredient, r.manufacturer,
               r.therapeutic_class_code, r.therapeutic_class_name, r.status, cheapest.amount,
               r.total
        FROM ranked r
        LEFT JOIN LATERAL (
            SELECT min(pr.amount) AS amount
            FROM presentations p
            JOIN prices pr USING (registration_number, ggrem_code)
            WHERE p.registration_number = r.registration_number
              AND NOT p.hospital_only
              AND NOT p.institutional
              AND pr.kind = 'Consumer'
              AND pr.icms_rate = @rate
              AND NOT pr.free_trade_zone
        ) AS cheapest ON true
        ORDER BY (r.status = 'Active') DESC, r.score DESC, r.has_price DESC,
                 r.name ASC, cheapest.amount ASC NULLS LAST;
        """;

    public async Task<PagedResults<MedicineSummary>> SearchAsync(
        string term,
        string state,
        Page page,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(page);

        if (string.IsNullOrWhiteSpace(term) || term.Trim().Length < MinimumTermLength)
        {
            return PagedResults<MedicineSummary>.Of([], 0, page);
        }

        var rate = IcmsRates.For(state);
        var connection = await OpenAsync(context, cancellationToken);

        await using var command = new NpgsqlCommand(Sql, connection);
        command.Parameters.Add(new NpgsqlParameter("term", NpgsqlDbType.Text) { Value = term.Trim() });
        command.Parameters.Add(new NpgsqlParameter("limit", NpgsqlDbType.Integer) { Value = page.Size });
        command.Parameters.Add(new NpgsqlParameter("offset", NpgsqlDbType.Integer) { Value = page.Offset });
        command.Parameters.Add(new NpgsqlParameter("rate", NpgsqlDbType.Numeric) { Value = rate });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var results = new List<MedicineSummary>();

        var total = 0;

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new MedicineSummary(
                RegistrationNumber: reader.GetString(0),
                Name: reader.GetString(1),
                ActiveIngredient: reader.IsDBNull(2) ? null : reader.GetString(2),
                Manufacturer: reader.IsDBNull(3) ? null : reader.GetString(3),
                TherapeuticClassCode: reader.IsDBNull(4) ? null : reader.GetString(4),
                TherapeuticClass: reader.IsDBNull(5) ? null : reader.GetString(5),
                IsActive: reader.GetString(6) == "Active",
                CheapestConsumerPrice: reader.IsDBNull(7) ? null : reader.GetDecimal(7)));

            total = (int)reader.GetInt64(8);
        }

        return PagedResults<MedicineSummary>.Of(results, total, page);
    }

    internal static async Task<NpgsqlConnection> OpenAsync(
        RemedinDbContext context,
        CancellationToken cancellationToken)
    {
        var connection = (NpgsqlConnection)context.Database.GetDbConnection();

        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        return connection;
    }
}
