using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Context;

#nullable disable

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260610165000_AddEffectiveMapAreas")]
    public partial class AddEffectiveMapAreas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_map_areas_map_areas_base_area_id",
                table: "user_map_areas");

            migrationBuilder.DropIndex(
                name: "IX_user_map_areas_base_area_id",
                table: "user_map_areas");

            migrationBuilder.AddColumn<string>(
                name: "base_area_key",
                table: "map_areas",
                type: "varchar",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE map_areas
                SET base_area_key = concat(coalesce(city_id, 'unknown'), ':', lower(risk), ':legacy:', id)
                WHERE base_area_key IS NULL
                """);

            migrationBuilder.AlterColumn<string>(
                name: "base_area_key",
                table: "map_areas",
                type: "varchar",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "base_area_key",
                table: "user_map_areas",
                type: "varchar",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE user_map_areas AS user_area
                SET base_area_key = map_area.base_area_key
                FROM map_areas AS map_area
                WHERE user_area.base_area_id = map_area.id
                """);

            migrationBuilder.AlterColumn<Polygon>(
                name: "geom",
                table: "user_map_areas",
                type: "geometry(Polygon, 4326)",
                nullable: true,
                oldClrType: typeof(Polygon),
                oldType: "geometry(Polygon, 4326)");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "user_map_areas",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now() at time zone 'utc'");

            migrationBuilder.DropColumn(
                name: "base_area_id",
                table: "user_map_areas");

            migrationBuilder.CreateIndex(
                name: "IX_map_areas_base_area_key",
                table: "map_areas",
                column: "base_area_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_map_areas_base_area_key",
                table: "user_map_areas",
                column: "base_area_key");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_map_areas_base_area_key",
                table: "map_areas");

            migrationBuilder.DropIndex(
                name: "IX_user_map_areas_base_area_key",
                table: "user_map_areas");

            migrationBuilder.AddColumn<Guid>(
                name: "base_area_id",
                table: "user_map_areas",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE user_map_areas AS user_area
                SET base_area_id = map_area.id
                FROM map_areas AS map_area
                WHERE user_area.base_area_key = map_area.base_area_key
                """);

            migrationBuilder.Sql("DELETE FROM user_map_areas WHERE geom IS NULL");

            migrationBuilder.AlterColumn<Polygon>(
                name: "geom",
                table: "user_map_areas",
                type: "geometry(Polygon, 4326)",
                nullable: false,
                oldClrType: typeof(Polygon),
                oldType: "geometry(Polygon, 4326)",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "user_map_areas");

            migrationBuilder.DropColumn(
                name: "base_area_key",
                table: "user_map_areas");

            migrationBuilder.DropColumn(
                name: "base_area_key",
                table: "map_areas");

            migrationBuilder.CreateIndex(
                name: "IX_user_map_areas_base_area_id",
                table: "user_map_areas",
                column: "base_area_id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_map_areas_map_areas_base_area_id",
                table: "user_map_areas",
                column: "base_area_id",
                principalTable: "map_areas",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
