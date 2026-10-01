using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Remedin.Infrastructure.Persistence.Migrations;

/// <summary>
/// Põe a finalidade em linguagem comum no índice de busca.
///
/// A classe terapêutica já era indexada, mas com o texto técnico da fonte:
/// "ANTIRREUMÁTICOS NÃO ESTEROIDAIS PUROS" não é encontrado por ninguém que
/// digite "dor".
///
/// Peso B, o mesmo do princípio ativo. É o único campo escrito na língua de
/// quem procura: acerto nele diz "este remédio serve para isso", sinal mais
/// forte que coincidir com o nome do fabricante (D) ou com o jargão da classe
/// (C). Não disputa o topo com nome exato nem com nome que começa pelo termo.
/// </summary>
public partial class PurposeLabel : Migration
{
    private const string WeightedVector = """
        setweight(to_tsvector('portuguese', immutable_unaccent(coalesce(name, ''))), 'A') ||
        setweight(to_tsvector('portuguese', immutable_unaccent(coalesce(active_ingredient, ''))), 'B') ||
        setweight(to_tsvector('portuguese', immutable_unaccent(coalesce(purpose_label, ''))), 'B') ||
        setweight(to_tsvector('portuguese', immutable_unaccent(coalesce(therapeutic_class_name, ''))), 'C') ||
        setweight(to_tsvector('portuguese', immutable_unaccent(coalesce(manufacturer, ''))), 'D')
        """;

    private const string PreviousVector = """
        setweight(to_tsvector('portuguese', immutable_unaccent(coalesce(name, ''))), 'A') ||
        setweight(to_tsvector('portuguese', immutable_unaccent(coalesce(active_ingredient, ''))), 'B') ||
        setweight(to_tsvector('portuguese', immutable_unaccent(coalesce(therapeutic_class_name, ''))), 'C') ||
        setweight(to_tsvector('portuguese', immutable_unaccent(coalesce(manufacturer, ''))), 'D')
        """;

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // A coluna gerada depende de purpose_label, então ela some antes de a
        // coluna nova existir e volta depois.
        migrationBuilder.Sql("DROP INDEX IF EXISTS ix_medicines_search_vector;");
        migrationBuilder.Sql("ALTER TABLE medicines DROP COLUMN IF EXISTS search_vector;");

        migrationBuilder.Sql("ALTER TABLE medicines ADD COLUMN purpose_label text;");

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
        migrationBuilder.Sql("ALTER TABLE medicines DROP COLUMN IF EXISTS purpose_label;");

        migrationBuilder.Sql($"""
            ALTER TABLE medicines
            ADD COLUMN search_vector tsvector
            GENERATED ALWAYS AS ({PreviousVector}) STORED;
            """);

        migrationBuilder.Sql("""
            CREATE INDEX ix_medicines_search_vector
            ON medicines USING gin (search_vector);
            """);
    }
}
