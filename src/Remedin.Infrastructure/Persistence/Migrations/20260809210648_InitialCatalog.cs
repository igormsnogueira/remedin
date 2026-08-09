using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Remedin.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,");

            migrationBuilder.CreateTable(
                name: "medicines",
                columns: table => new
                {
                    registration_number = table.Column<string>(type: "character(9)", fixedLength: true, maxLength: 9, nullable: false),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    active_ingredient = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    manufacturer = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    therapeutic_class_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    therapeutic_class_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    prescription_band = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medicines", x => x.registration_number);
                });

            migrationBuilder.CreateTable(
                name: "presentations",
                columns: table => new
                {
                    ggrem_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    registration_number = table.Column<string>(type: "character(9)", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    consumer_price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    factory_price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    hospital_only = table.Column<bool>(type: "boolean", nullable: false),
                    sold_recently = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_presentations", x => new { x.registration_number, x.ggrem_code });
                    table.ForeignKey(
                        name: "FK_presentations_medicines_registration_number",
                        column: x => x.registration_number,
                        principalTable: "medicines",
                        principalColumn: "registration_number",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "presentations");

            migrationBuilder.DropTable(
                name: "medicines");
        }
    }
}
