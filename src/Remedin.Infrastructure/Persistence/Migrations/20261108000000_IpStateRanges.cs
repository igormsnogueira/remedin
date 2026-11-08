using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Remedin.Infrastructure.Persistence.Migrations;

/// <summary>
/// Faixas de IP por estado, para estimar o estado de quem abre o site.
///
/// A tabela fica fora do modelo do EF, como as de preço em lote: é escrita por
/// COPY e lida por SQL direto, e não tem entidade. Por isso esta migration não
/// tem arquivo Designer nem muda o snapshot do modelo; os atributos que o EF
/// usa para encontrá-la ficam aqui.
/// </summary>
[DbContext(typeof(RemedinDbContext))]
[Migration("20261108000000_IpStateRanges")]
public partial class IpStateRanges : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // A chave primária sobre o início da faixa é também o índice que a
        // consulta usa ("última faixa que começa antes do endereço").
        migrationBuilder.Sql("""
            CREATE TABLE ip_state_ranges (
                range_start inet    PRIMARY KEY,
                range_end   inet    NOT NULL,
                state       char(2) NOT NULL
            );
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP TABLE IF EXISTS ip_state_ranges;");
    }
}
