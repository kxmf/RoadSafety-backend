using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackingAndNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "child_locations",
                columns: table => new
                {
                    child_id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    location = table.Column<Point>(type: "geometry(Point, 4326)", nullable: false),
                    accuracy_meters = table.Column<double>(type: "double precision", nullable: true),
                    current_risk = table.Column<string>(type: "varchar", nullable: false),
                    matched_user_area_id = table.Column<Guid>(type: "uuid", nullable: true),
                    matched_base_area_key = table.Column<string>(type: "varchar", nullable: true),
                    recorded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_child_locations", x => x.child_id);
                    table.ForeignKey(
                        name: "FK_child_locations_families_family_id",
                        column: x => x.family_id,
                        principalTable: "families",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_child_locations_users_child_id",
                        column: x => x.child_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "child_risk_states",
                columns: table => new
                {
                    child_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_risk = table.Column<string>(type: "varchar", nullable: false),
                    entered_red_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_red_notification_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_child_risk_states", x => x.child_id);
                    table.ForeignKey(
                        name: "FK_child_risk_states_users_child_id",
                        column: x => x.child_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "child_stats",
                columns: table => new
                {
                    child_id = table.Column<Guid>(type: "uuid", nullable: false),
                    total_score = table.Column<int>(type: "integer", nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_child_stats", x => x.child_id);
                    table.ForeignKey(
                        name: "FK_child_stats_users_child_id",
                        column: x => x.child_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    child_id = table.Column<Guid>(type: "uuid", nullable: true),
                    type = table.Column<string>(type: "varchar", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    body = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    risk = table.Column<string>(type: "varchar", nullable: true),
                    location = table.Column<Point>(type: "geometry(Point, 4326)", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_notifications_users_child_id",
                        column: x => x.child_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_notifications_users_recipient_user_id",
                        column: x => x.recipient_user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "child_locations_location_idx",
                table: "child_locations",
                column: "location")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_child_locations_family_id",
                table: "child_locations",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_child_id",
                table: "notifications",
                column: "child_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_recipient_user_id_read_at_created_at",
                table: "notifications",
                columns: new[] { "recipient_user_id", "read_at", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "child_locations");

            migrationBuilder.DropTable(
                name: "child_risk_states");

            migrationBuilder.DropTable(
                name: "child_stats");

            migrationBuilder.DropTable(
                name: "notifications");
        }
    }
}
