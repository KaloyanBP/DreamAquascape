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
            migrationBuilder.DropIndex(
                name: "IX_ContestCategories_Name",
                table: "ContestCategories");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ContestCategories",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

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
                type: "nvarchar(450)",
                maxLength: 450,
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

            migrationBuilder.CreateIndex(
                name: "IX_ContestCategories_DeletedAt",
                table: "ContestCategories",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ContestCategories_IsDeleted",
                table: "ContestCategories",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ContestCategories_Name",
                table: "ContestCategories",
                column: "Name",
                unique: true,
                filter: "IsDeleted = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContestCategories_DeletedAt",
                table: "ContestCategories");

            migrationBuilder.DropIndex(
                name: "IX_ContestCategories_IsDeleted",
                table: "ContestCategories");

            migrationBuilder.DropIndex(
                name: "IX_ContestCategories_Name",
                table: "ContestCategories");

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

            migrationBuilder.CreateIndex(
                name: "IX_ContestCategories_Name",
                table: "ContestCategories",
                column: "Name",
                unique: true);
        }
    }
}
