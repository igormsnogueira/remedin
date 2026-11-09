using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Remedin.Infrastructure.Persistence.Migrations;

/// <summary>
/// Quais medicamentos têm bula no Bulário Eletrônico, e de quando.
///
/// Fora do modelo do EF, como as faixas de IP: escrita por COPY e lida por
/// SQL direto. Por isso não tem Designer nem muda o snapshot.
/// </summary>
[DbContext(typeof(RemedinDbContext))]
[Migration("20261109000000_Leaflets")]
public partial class Leaflets : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE leaflets (
                registration_number char(9) PRIMARY KEY,
                published_on        date    NOT NULL
            );
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP TABLE IF EXISTS leaflets;");
    }
}
