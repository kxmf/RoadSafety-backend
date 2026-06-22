using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddMapAreas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .Annotation("Npgsql:PostgresExtension:postgis", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateTable(
                name: "map_areas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    osm_id = table.Column<long>(type: "bigint", nullable: true),
                    risk = table.Column<string>(type: "varchar", nullable: false),
                    geom = table.Column<Polygon>(type: "geometry(Polygon, 4326)", nullable: false),
                    city_id = table.Column<string>(type: "varchar", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_map_areas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_map_areas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    child_id = table.Column<Guid>(type: "uuid", nullable: true),
                    base_area_id = table.Column<Guid>(type: "uuid", nullable: true),
                    risk = table.Column<string>(type: "varchar", nullable: false),
                    geom = table.Column<Polygon>(type: "geometry(Polygon, 4326)", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_map_areas", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_map_areas_families_family_id",
                        column: x => x.family_id,
                        principalTable: "families",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_map_areas_map_areas_base_area_id",
                        column: x => x.base_area_id,
                        principalTable: "map_areas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_user_map_areas_users_child_id",
                        column: x => x.child_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_map_areas_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_map_areas_city_id",
                table: "map_areas",
                column: "city_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_areas_risk",
                table: "map_areas",
                column: "risk");

            migrationBuilder.CreateIndex(
                name: "map_areas_geom_idx",
                table: "map_areas",
                column: "geom")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_user_map_areas_base_area_id",
                table: "user_map_areas",
                column: "base_area_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_map_areas_child_id",
                table: "user_map_areas",
                column: "child_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_map_areas_created_by_user_id",
                table: "user_map_areas",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_map_areas_family_id_child_id",
                table: "user_map_areas",
                columns: new[] { "family_id", "child_id" });

            migrationBuilder.CreateIndex(
                name: "user_areas_geom_idx",
                table: "user_map_areas",
                column: "geom")
                .Annotation("Npgsql:IndexMethod", "gist");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_map_areas");

            migrationBuilder.DropTable(
                name: "map_areas");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");
        }
    }
}
