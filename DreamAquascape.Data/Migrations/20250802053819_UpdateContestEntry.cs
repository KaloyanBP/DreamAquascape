using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamAquascape.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateContestEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "EntryImages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ContestEntries",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "EntryImages");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ContestEntries");
        }
    }
}
