using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using Remedin.Application.Catalog.Search;

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
                   m.therapeutic_class_name,
                   m.status,
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
        )
        SELECT registration_number, name, active_ingredient, manufacturer,
               therapeutic_class_name, status
        FROM scored
        -- Registro ativo vem antes: produto fora do mercado é resposta pior
        -- para o mesmo grau de relevância.
        ORDER BY (status = 'Active') DESC, score DESC, name ASC
        LIMIT @limit;
        """;

    public async Task<IReadOnlyList<MedicineSummary>> SearchAsync(
        string term,
        int limit,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Trim().Length < MinimumTermLength)
        {
            return [];
        }

        var connection = (NpgsqlConnection)context.Database.GetDbConnection();

        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = new NpgsqlCommand(Sql, connection);
        command.Parameters.Add(new NpgsqlParameter("term", NpgsqlDbType.Text) { Value = term.Trim() });
        command.Parameters.Add(new NpgsqlParameter("limit", NpgsqlDbType.Integer) { Value = limit });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var results = new List<MedicineSummary>();

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new MedicineSummary(
                RegistrationNumber: reader.GetString(0),
                Name: reader.GetString(1),
                ActiveIngredient: reader.IsDBNull(2) ? null : reader.GetString(2),
                Manufacturer: reader.IsDBNull(3) ? null : reader.GetString(3),
                TherapeuticClass: reader.IsDBNull(4) ? null : reader.GetString(4),
                IsActive: reader.GetString(5) == "Active"));
        }

        return results;
    }
}
