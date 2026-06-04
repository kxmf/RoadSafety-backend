using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserFamilyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FamilyId",
                table: "users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FamilyId",
                table: "users",
                type: "uuid",
                nullable: true);
        }
    }
}
