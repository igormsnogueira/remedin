using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Remedin.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Guarda a dosagem e a quantidade lidas da descrição da apresentação.
    ///
    /// Das 25.691 apresentações, 14.548 têm os dois valores legíveis com
    /// segurança. Nas demais as colunas ficam nulas, e a comparação por
    /// unidade não é oferecida (ADR 0010).
    ///
    /// Preenchidas pela carga: a leitura é regra do domínio, e reescrevê-la em
    /// SQL criaria duas versões da mesma coisa.
    /// </summary>
    public partial class PresentationPackaging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE presentations
                ADD COLUMN dosage_mg numeric(12,4),
                ADD COLUMN unit_count integer;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE presentations
                DROP COLUMN IF EXISTS dosage_mg,
                DROP COLUMN IF EXISTS unit_count;
                """);
        }
    }
}
