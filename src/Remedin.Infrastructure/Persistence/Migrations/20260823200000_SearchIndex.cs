using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Remedin.Infrastructure.Persistence.Migrations;

/// <summary>
/// Índice de busca do catálogo.
///
/// Escrita à mão porque cria uma função e uma coluna gerada, que o EF não
/// representa no modelo. A alternativa seria manter isso num script de
/// inicialização do container, que não roda em produção.
/// </summary>
public partial class SearchIndex : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // unaccent vem marcada como STABLE, e o PostgreSQL só aceita função
        // IMMUTABLE dentro de índice. Sem este envelope dá para usar unaccent
        // numa consulta, mas cada busca varreria a tabela inteira.
        migrationBuilder.Sql("""
            CREATE OR REPLACE FUNCTION immutable_unaccent(text)
            RETURNS text
            LANGUAGE sql IMMUTABLE STRICT PARALLEL SAFE
            AS $$ SELECT public.unaccent('public.unaccent'::regdictionary, $1) $$;
            """);

        // Coluna gerada: o banco mantém o índice em dia sozinho, sem trigger
        // e sem passo extra na carga.
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

        // Trigrama sobre nome e princípio ativo separadamente: similaridade
        // sobre o texto concatenado inteiro dilui a nota e deixa de encontrar
        // erro de digitação.
        migrationBuilder.Sql("""
            CREATE INDEX ix_medicines_name_trgm
            ON medicines USING gin (immutable_unaccent(name) gin_trgm_ops);
            """);

        migrationBuilder.Sql("""
            CREATE INDEX ix_medicines_active_ingredient_trgm
            ON medicines USING gin (immutable_unaccent(coalesce(active_ingredient, '')) gin_trgm_ops);
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP INDEX IF EXISTS ix_medicines_active_ingredient_trgm;");
        migrationBuilder.Sql("DROP INDEX IF EXISTS ix_medicines_name_trgm;");
        migrationBuilder.Sql("DROP INDEX IF EXISTS ix_medicines_search_vector;");
        migrationBuilder.Sql("ALTER TABLE medicines DROP COLUMN IF EXISTS search_vector;");
        migrationBuilder.Sql("DROP FUNCTION IF EXISTS immutable_unaccent(text);");
    }
}
