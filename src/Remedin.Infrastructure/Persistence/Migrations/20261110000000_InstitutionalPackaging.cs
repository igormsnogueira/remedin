using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Remedin.Infrastructure.Persistence.Migrations;

/// <summary>
/// Marca as embalagens de compra institucional (hospital, prefeitura, rede de
/// farmácia), que saem da escolha e da comparação de preço de balcão.
///
/// Começa falsa para todas. A regra fica no domínio
/// (Packaging.IsInstitutional), e o valor certo entra na próxima carga de
/// preço com reprocessamento; repetir a regra aqui em SQL criaria uma segunda
/// versão dela.
///
/// Como dosage_mg e unit_count, a coluna fica fora do modelo do EF: é gravada
/// pelo COPY da carga de preço e lida só por SQL direto. Por isso não há
/// Designer nem mudança no snapshot.
/// </summary>
[DbContext(typeof(RemedinDbContext))]
[Migration("20261110000000_InstitutionalPackaging")]
public partial class InstitutionalPackaging : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "ALTER TABLE presentations ADD COLUMN institutional boolean NOT NULL DEFAULT false;");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE presentations DROP COLUMN IF EXISTS institutional;");
    }
}
