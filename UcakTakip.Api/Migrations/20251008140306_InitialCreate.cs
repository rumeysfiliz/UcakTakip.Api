using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UcakTakip.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UcusPlanlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Origin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Destination = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UcusPlanlari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UcakKonumlari",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UcusPlaniId = table.Column<int>(type: "int", nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Altitude = table.Column<double>(type: "float", nullable: true),
                    Heading = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UcakKonumlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UcakKonumlari_UcusPlanlari_UcusPlaniId",
                        column: x => x.UcusPlaniId,
                        principalTable: "UcusPlanlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UcakKonumlari_UcusPlaniId_TimestampUtc",
                table: "UcakKonumlari",
                columns: new[] { "UcusPlaniId", "TimestampUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_UcusPlanlari_Code",
                table: "UcusPlanlari",
                column: "Code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UcakKonumlari");

            migrationBuilder.DropTable(
                name: "UcusPlanlari");
        }
    }
}
