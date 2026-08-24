using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Remedin.Infrastructure.Persistence.Migrations;

/// <summary>
/// Dá peso diferente a cada campo da busca.
///
/// Com todos os campos valendo o mesmo, buscar "dipirona" devolvia os
/// medicamentos em ordem alfabética, porque centenas deles têm esse princípio
/// ativo e todos empatavam. O que a pessoa procura primeiro é o produto que
/// se chama assim.
/// </summary>
public partial class SearchRanking : Migration
{
    private const string WeightedVector = """
        setweight(to_tsvector('portuguese', immutable_unaccent(coalesce(name, ''))), 'A') ||
        setweight(to_tsvector('portuguese', immutable_unaccent(coalesce(active_ingredient, ''))), 'B') ||
        setweight(to_tsvector('portuguese', immutable_unaccent(coalesce(therapeutic_class_name, ''))), 'C') ||
        setweight(to_tsvector('portuguese', immutable_unaccent(coalesce(manufacturer, ''))), 'D')
        """;

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Coluna gerada não se altera no lugar: recriar é o caminho.
        migrationBuilder.Sql("DROP INDEX IF EXISTS ix_medicines_search_vector;");
        migrationBuilder.Sql("ALTER TABLE medicines DROP COLUMN IF EXISTS search_vector;");

        migrationBuilder.Sql($"""
            ALTER TABLE medicines
            ADD COLUMN search_vector tsvector
            GENERATED ALWAYS AS ({WeightedVector}) STORED;
            """);

        migrationBuilder.Sql("""
            CREATE INDEX ix_medicines_search_vector
            ON medicines USING gin (search_vector);
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP INDEX IF EXISTS ix_medicines_search_vector;");
        migrationBuilder.Sql("ALTER TABLE medicines DROP COLUMN IF EXISTS search_vector;");

        migrationBuilder.Sql("""
            ALTER TABLE medicines
            ADD COLUMN search_vector tsvector
            GENERATED ALWAYS AS (
                to_tsvector(
                    'portuguese',
                    immutable_unaccent(
                        coalesce(name, '') || ' ' ||
                        coalesce(active_ingredient, '') || ' ' ||
                        coalesce(manufacturer, '') || ' ' ||
                        coalesce(therapeutic_class_name, '')
                    )
                )
            ) STORED;
            """);

        migrationBuilder.Sql("""
            CREATE INDEX ix_medicines_search_vector
            ON medicines USING gin (search_vector);
            """);
    }
}
