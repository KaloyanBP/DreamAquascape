using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamAquascape.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedVoteToSupportSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vote_User_Entry_Unique",
                table: "Votes");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Votes",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Votes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Votes",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Votes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Votes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vote_User_Entry_Unique",
                table: "Votes",
                columns: new[] { "UserId", "ContestEntryId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_DeletedAt",
                table: "Votes",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_IsDeleted",
                table: "Votes",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vote_User_Entry_Unique",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Votes_DeletedAt",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Votes_IsDeleted",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Votes");

            migrationBuilder.CreateIndex(
                name: "IX_Vote_User_Entry_Unique",
                table: "Votes",
                columns: new[] { "UserId", "ContestEntryId" },
                unique: true);
        }
    }
}
