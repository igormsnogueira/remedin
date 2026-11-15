using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Remedin.Infrastructure.Persistence.Migrations;

public partial class SearchRanking : Migration
{
    private const string WeightedVector = """
        setweight(to_tsvector('portuguese', immutable_unaccent(coalesce(name, ''))), 'A') ||
        setweight(to_tsvector('portuguese', immutable_unaccent(coalesce(active_ingredient, ''))), 'B') ||
        setweight(to_tsvector('portuguese', immutable_unaccent(coalesce(therapeutic_class_name, ''))), 'C') ||
        setweight(to_tsvector('portuguese', immutable_unaccent(coalesce(manufacturer, ''))), 'D')
        """;

    protected override void Up(MigrationBuilder migrationBuilder)
    {
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
