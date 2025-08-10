using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamAquascape.Data.Migrations
{
    /// <inheritdoc />
    public partial class ContestCategoryModelUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ContestCategories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ContestCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ContestCategories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "ContestCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ContestCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "ContestCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ContestCategories",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ContestCategories");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ContestCategories");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ContestCategories");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "ContestCategories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ContestCategories");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "ContestCategories");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ContestCategories");
        }
    }
}
