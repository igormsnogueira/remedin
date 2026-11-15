using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Remedin.Infrastructure.Persistence.Migrations;

[DbContext(typeof(RemedinDbContext))]
[Migration("20261108000000_IpStateRanges")]
public partial class IpStateRanges : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE ip_state_ranges (
                range_start inet    PRIMARY KEY,
                range_end   inet    NOT NULL,
                state       char(2) NOT NULL
            );
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP TABLE IF EXISTS ip_state_ranges;");
    }
}
