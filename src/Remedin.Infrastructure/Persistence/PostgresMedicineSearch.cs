using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using Remedin.Application.Catalog.Search;
using Remedin.Domain.Medicines;

namespace Remedin.Infrastructure.Persistence;

/// <summary>
/// Busca por nome comercial, princípio ativo, fabricante e classe terapêutica.
///
/// Combina duas estratégias porque o público erra a grafia e nem sempre sabe
/// o nome do princípio ativo:
///
///   busca textual  encontra palavra inteira, com radical em português, de
///                  modo que "analgésicos" acha "analgésico"
///   trigrama       encontra grafia aproximada, tolerando erro de digitação
///
/// Os pesos da relevância estão na ADR 0008.
///
/// SQL direto em vez de LINQ: as duas estratégias combinadas não têm tradução
/// natural, e a consulta é o coração do produto — vale poder lê-la inteira.
/// </summary>
public sealed class PostgresMedicineSearch(RemedinDbContext context) : IMedicineSearch
{
    /// <summary>Abaixo disso o trigrama devolve o catálogo inteiro.</summary>
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
                   -- Quem digita "dipirona" procura primeiro o produto que se
                   -- chama assim, não os trezentos que têm esse princípio
                   -- ativo. Daí a escada: nome exato, nome que começa com o
                   -- termo, relevância textual com peso por campo, e por
                   -- último a semelhança que socorre erro de digitação.
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
            SELECT *
            FROM scored
            -- Registro ativo e com preço vem antes: para o mesmo grau de
            -- relevância, produto fora do mercado ou sem preço publicado é
            -- resposta pior (ADR 0007). A marca de preço entra aqui, e não
            -- depois, porque reordenar após o corte não traz de volta quem
            -- ficou de fora.
            ORDER BY (status = 'Active') DESC, score DESC, has_price DESC, name ASC
            LIMIT @limit
        )
        SELECT r.registration_number, r.name, r.active_ingredient, r.manufacturer,
               r.therapeutic_class_code, r.therapeutic_class_name, r.status, cheapest.amount,
               r.has_price
        FROM ranked r
        -- O preço é buscado só para as linhas que vão sair, e não para as
        -- milhares que a busca casou. Sem isso, procurar um princípio ativo
        -- comum varreria centenas de milhares de preços à toa.
        LEFT JOIN LATERAL (
            SELECT min(pr.amount) AS amount
            FROM presentations p
            JOIN prices pr USING (registration_number, ggrem_code)
            WHERE p.registration_number = r.registration_number
              AND NOT p.hospital_only
              AND pr.kind = 'Consumer'
              AND pr.icms_rate = @rate
              AND NOT pr.free_trade_zone
        ) AS cheapest ON true
        -- O preço só desempata no fim, entre os que já foram escolhidos: vários
        -- laboratórios publicam o mesmo genérico com o mesmo nome, e mostrar o
        -- mais caro primeiro contraria o motivo de o site existir.
        ORDER BY (r.status = 'Active') DESC, r.score DESC, r.has_price DESC,
                 r.name ASC, cheapest.amount ASC NULLS LAST;
        """;

    public async Task<IReadOnlyList<MedicineSummary>> SearchAsync(
        string term,
        string state,
        int limit,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Trim().Length < MinimumTermLength)
        {
            return [];
        }

        var rate = IcmsRates.For(state);
        var connection = await OpenAsync(context, cancellationToken);

        await using var command = new NpgsqlCommand(Sql, connection);
        command.Parameters.Add(new NpgsqlParameter("term", NpgsqlDbType.Text) { Value = term.Trim() });
        command.Parameters.Add(new NpgsqlParameter("limit", NpgsqlDbType.Integer) { Value = limit });
        command.Parameters.Add(new NpgsqlParameter("rate", NpgsqlDbType.Numeric) { Value = rate });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var results = new List<MedicineSummary>();

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
        }

        return results;
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
