using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamAquascape.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateContestConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Votes_VotedAt",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "VotedAt",
                table: "Votes");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Contests",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "ModifiedBy",
                table: "ContestCategories",
                newName: "UpdatedBy");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Votes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Votes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Contests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Contests",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Contests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Contests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ContestEntries",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "NULL");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ContestEntries",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ContestEntries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ContestEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "ContestEntries",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "ContestEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "ContestCategories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Votes_CreatedAt",
                table: "Votes",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Contests_DeletedAt",
                table: "Contests",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Contests_IsDeleted",
                table: "Contests",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ContestEntries_DeletedAt",
                table: "ContestEntries",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ContestEntries_IsDeleted",
                table: "ContestEntries",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Votes_CreatedAt",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Contests_DeletedAt",
                table: "Contests");

            migrationBuilder.DropIndex(
                name: "IX_Contests_IsDeleted",
                table: "Contests");

            migrationBuilder.DropIndex(
                name: "IX_ContestEntries_DeletedAt",
                table: "ContestEntries");

            migrationBuilder.DropIndex(
                name: "IX_ContestEntries_IsDeleted",
                table: "ContestEntries");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Contests");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Contests");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Contests");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Contests");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ContestEntries");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ContestEntries");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ContestEntries");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "ContestEntries");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ContestEntries");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Contests",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "ContestCategories",
                newName: "ModifiedBy");

            migrationBuilder.AddColumn<DateTime>(
                name: "VotedAt",
                table: "Votes",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ContestEntries",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "NULL",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "ContestCategories",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_VotedAt",
                table: "Votes",
                column: "VotedAt");
        }
    }
}
