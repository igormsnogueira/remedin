using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Remedin.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Tira o limite de tamanho dos campos de texto livre.
    ///
    /// Limitar não valida nada — a origem escreve o que quiser — e transforma
    /// variação normal do dado em carga quebrada: a CMED concatena os
    /// princípios ativos de uma associação e passa de mil caracteres. Código e
    /// identificador mantêm limite, porque a forma deles é definida.
    ///
    /// A coluna de busca é gerada a partir destes campos, e o PostgreSQL não
    /// permite alterar o tipo de coluna usada por coluna gerada. Por isso a
    /// migration derruba o índice de busca, altera, e recria.
    /// </summary>
    public partial class UnboundedTextFields : Migration
    {
        private const string SearchVector = """
            ALTER TABLE medicines
            ADD COLUMN search_vector tsvector
            GENERATED ALWAYS AS (
                setweight(to_tsvector('portuguese', immutable_unaccent(coalesce(name, ''))), 'A') ||
                setweight(to_tsvector('portuguese', immutable_unaccent(coalesce(active_ingredient, ''))), 'B') ||
                setweight(to_tsvector('portuguese', immutable_unaccent(coalesce(therapeutic_class_name, ''))), 'C') ||
                setweight(to_tsvector('portuguese', immutable_unaccent(coalesce(manufacturer, ''))), 'D')
            ) STORED;
            """;

        private const string SearchIndexes = """
            CREATE INDEX ix_medicines_search_vector
            ON medicines USING gin (search_vector);

            CREATE INDEX ix_medicines_name_trgm
            ON medicines USING gin (immutable_unaccent(name) gin_trgm_ops);

            CREATE INDEX ix_medicines_active_ingredient_trgm
            ON medicines USING gin (immutable_unaccent(coalesce(active_ingredient, '')) gin_trgm_ops);
            """;

        private const string DropSearch = """
            DROP INDEX IF EXISTS ix_medicines_active_ingredient_trgm;
            DROP INDEX IF EXISTS ix_medicines_name_trgm;
            DROP INDEX IF EXISTS ix_medicines_search_vector;
            ALTER TABLE medicines DROP COLUMN IF EXISTS search_vector;
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(DropSearch);

            migrationBuilder.Sql("""
                ALTER TABLE presentations ALTER COLUMN description TYPE text;
                ALTER TABLE medicines ALTER COLUMN name TYPE text;
                ALTER TABLE medicines ALTER COLUMN active_ingredient TYPE text;
                ALTER TABLE medicines ALTER COLUMN manufacturer TYPE text;
                ALTER TABLE medicines ALTER COLUMN therapeutic_class_name TYPE text;
                ALTER TABLE medicines ALTER COLUMN prescription_band TYPE text;
                """);

            migrationBuilder.Sql(SearchVector);
            migrationBuilder.Sql(SearchIndexes);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(DropSearch);

            // Voltar pode truncar: os valores que motivaram esta migration não
            // cabem no tamanho anterior.
            migrationBuilder.Sql("""
                ALTER TABLE presentations ALTER COLUMN description TYPE character varying(500);
                ALTER TABLE medicines ALTER COLUMN name TYPE character varying(300);
                ALTER TABLE medicines ALTER COLUMN active_ingredient TYPE character varying(1000);
                ALTER TABLE medicines ALTER COLUMN manufacturer TYPE character varying(300);
                ALTER TABLE medicines ALTER COLUMN therapeutic_class_name TYPE character varying(300);
                ALTER TABLE medicines ALTER COLUMN prescription_band TYPE character varying(50);
                """);

            migrationBuilder.Sql(SearchVector);
            migrationBuilder.Sql(SearchIndexes);
        }
    }
}
