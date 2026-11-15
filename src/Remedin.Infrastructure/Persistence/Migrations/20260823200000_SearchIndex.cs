using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Remedin.Infrastructure.Persistence.Migrations;

public partial class SearchIndex : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE OR REPLACE FUNCTION immutable_unaccent(text)
            RETURNS text
            LANGUAGE sql IMMUTABLE STRICT PARALLEL SAFE
            AS $$ SELECT public.unaccent('public.unaccent'::regdictionary, $1) $$;
            """);

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

        migrationBuilder.Sql("""
            CREATE INDEX ix_medicines_name_trgm
            ON medicines USING gin (immutable_unaccent(name) gin_trgm_ops);
            """);

        migrationBuilder.Sql("""
            CREATE INDEX ix_medicines_active_ingredient_trgm
            ON medicines USING gin (immutable_unaccent(coalesce(active_ingredient, '')) gin_trgm_ops);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP INDEX IF EXISTS ix_medicines_active_ingredient_trgm;");
        migrationBuilder.Sql("DROP INDEX IF EXISTS ix_medicines_name_trgm;");
        migrationBuilder.Sql("DROP INDEX IF EXISTS ix_medicines_search_vector;");
        migrationBuilder.Sql("ALTER TABLE medicines DROP COLUMN IF EXISTS search_vector;");
        migrationBuilder.Sql("DROP FUNCTION IF EXISTS immutable_unaccent(text);");
    }
}
