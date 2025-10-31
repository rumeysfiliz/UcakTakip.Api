using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UcakTakip.Api.Migrations
{
    /// <inheritdoc />
    public partial class koordinatsistemi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DestinationLat",
                table: "UcusPlanlari",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DestinationLng",
                table: "UcusPlanlari",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "OriginLat",
                table: "UcusPlanlari",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "OriginLng",
                table: "UcusPlanlari",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DestinationLat",
                table: "UcusPlanlari");

            migrationBuilder.DropColumn(
                name: "DestinationLng",
                table: "UcusPlanlari");

            migrationBuilder.DropColumn(
                name: "OriginLat",
                table: "UcusPlanlari");

            migrationBuilder.DropColumn(
                name: "OriginLng",
                table: "UcusPlanlari");
        }
    }
}
