using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventManagementSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ForthMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Events_StartDateTime",
                table: "Events");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDateTime1",
                table: "Events",
                type: "datetime2",
                nullable: false,
                computedColumnSql: "[EndDateTime]");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDateTime1",
                table: "Events",
                type: "datetime2",
                nullable: false,
                computedColumnSql: "[StartDateTime]");

            migrationBuilder.CreateIndex(
                name: "IX_Events_StartDateTime",
                table: "Events",
                column: "StartDateTime1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Events_StartDateTime",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "EndDateTime1",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "StartDateTime1",
                table: "Events");

            migrationBuilder.CreateIndex(
                name: "IX_Events_StartDateTime",
                table: "Events",
                column: "StartDateTime");
        }
    }
}
