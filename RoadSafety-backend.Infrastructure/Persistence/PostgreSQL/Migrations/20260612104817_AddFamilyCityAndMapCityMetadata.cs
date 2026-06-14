using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddFamilyCityAndMapCityMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "city_id",
                table: "families",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "ekb");

            migrationBuilder.AlterColumn<string>(
                name: "city_id",
                table: "families",
                type: "varchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldDefaultValue: "ekb");

            migrationBuilder.CreateTable(
                name: "map_city_metadata",
                columns: table => new
                {
                    city_id = table.Column<string>(type: "varchar(50)", nullable: false),
                    generation_version = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    min_lon = table.Column<double>(type: "double precision", nullable: false),
                    min_lat = table.Column<double>(type: "double precision", nullable: false),
                    max_lon = table.Column<double>(type: "double precision", nullable: false),
                    max_lat = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_map_city_metadata", x => x.city_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "map_city_metadata");

            migrationBuilder.DropColumn(
                name: "city_id",
                table: "families");
        }
    }
}
