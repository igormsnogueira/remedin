using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Remedin.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Marca no medicamento se ele tem preço publicado.
    ///
    /// O dado já existe nas apresentações, mas a busca precisa dele antes de
    /// cortar os resultados: com o corte primeiro, os vinte escolhidos podem
    /// ser todos sem preço, e reordenar depois não traz de volta os que
    /// ficaram de fora. Junção com preço antes do corte varreria centenas de
    /// milhares de linhas a cada busca.
    ///
    /// É denormalização deliberada, mantida pela carga de preço. A ADR 0007 já
    /// previa que os estados do funil virassem coluna calculada na carga.
    /// </summary>
    public partial class MedicineHasPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE medicines
                ADD COLUMN has_price boolean NOT NULL DEFAULT false;
                """);

            // Preenche a partir do que já está carregado, para a coluna nascer
            // correta sem depender da próxima carga.
            migrationBuilder.Sql("""
                UPDATE medicines m
                SET has_price = EXISTS (
                    SELECT 1 FROM presentations p
                    WHERE p.registration_number = m.registration_number
                );
                """);

            migrationBuilder.Sql("""
                CREATE INDEX ix_medicines_has_price ON medicines (has_price);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_medicines_has_price;");
            migrationBuilder.Sql("ALTER TABLE medicines DROP COLUMN IF EXISTS has_price;");
        }
    }
}
