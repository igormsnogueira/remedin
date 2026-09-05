using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Remedin.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Guarda a forma canônica do princípio ativo, usada para achar
    /// medicamentos equivalentes (ADR 0010).
    ///
    /// A coluna nasce vazia e é preenchida pela carga de preço. Calcular a
    /// chave aqui em SQL duplicaria a regra que já existe no domínio, e duas
    /// implementações da mesma regra divergem com o tempo.
    /// </summary>
    public partial class SubstanceKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE medicines ADD COLUMN substance_key text;");

            // Só medicamento com preço entra na comparação, e o índice parcial
            // reflete isso: o catálogo tem 32 mil registros e menos de 9 mil
            // participam.
            migrationBuilder.Sql("""
                CREATE INDEX ix_medicines_substance_key
                ON medicines (substance_key)
                WHERE substance_key IS NOT NULL AND has_price;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_medicines_substance_key;");
            migrationBuilder.Sql("ALTER TABLE medicines DROP COLUMN IF EXISTS substance_key;");
        }
    }
}
