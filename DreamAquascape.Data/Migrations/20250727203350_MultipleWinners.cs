using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamAquascape.Data.Migrations
{
    /// <inheritdoc />
    public partial class MultipleWinners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contests_ContestEntries_WinnerEntryId",
                table: "Contests");

            migrationBuilder.DropForeignKey(
                name: "FK_ContestsCategories_ContestCategories_CategoryId",
                table: "ContestsCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ContestsCategories_Contests_ContestId",
                table: "ContestsCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_EntryImages_ContestEntries_ContestEntryId",
                table: "EntryImages");

            migrationBuilder.DropForeignKey(
                name: "FK_UserContestParticipations_Contests_ContestId",
                table: "UserContestParticipations");

            migrationBuilder.DropForeignKey(
                name: "FK_Votes_ContestEntries_ContestEntryId",
                table: "Votes");

            migrationBuilder.DropForeignKey(
                name: "FK_Votes_Contests_ContestId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Contests_WinnerEntryId",
                table: "Contests");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Contests",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SubmittedAt",
                table: "ContestEntries",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.CreateTable(
                name: "ContestWinners",
                columns: table => new
                {
                    ContestId = table.Column<int>(type: "int", nullable: false),
                    ContestEntryId = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    WonAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AwardTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContestWinners", x => new { x.ContestId, x.ContestEntryId, x.Position });
                    table.ForeignKey(
                        name: "FK_ContestWinners_ContestEntries_ContestEntryId",
                        column: x => x.ContestEntryId,
                        principalTable: "ContestEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContestWinners_Contests_ContestId",
                        column: x => x.ContestId,
                        principalTable: "Contests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContestWinner_Contest_Position_Unique",
                table: "ContestWinners",
                columns: new[] { "ContestId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContestWinner_Entry_Unique",
                table: "ContestWinners",
                column: "ContestEntryId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ContestsCategories_ContestCategories_CategoryId",
                table: "ContestsCategories",
                column: "CategoryId",
                principalTable: "ContestCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContestsCategories_Contests_ContestId",
                table: "ContestsCategories",
                column: "ContestId",
                principalTable: "Contests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EntryImages_ContestEntries_ContestEntryId",
                table: "EntryImages",
                column: "ContestEntryId",
                principalTable: "ContestEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserContestParticipations_Contests_ContestId",
                table: "UserContestParticipations",
                column: "ContestId",
                principalTable: "Contests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_ContestEntries_ContestEntryId",
                table: "Votes",
                column: "ContestEntryId",
                principalTable: "ContestEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_Contests_ContestId",
                table: "Votes",
                column: "ContestId",
                principalTable: "Contests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContestsCategories_ContestCategories_CategoryId",
                table: "ContestsCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ContestsCategories_Contests_ContestId",
                table: "ContestsCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_EntryImages_ContestEntries_ContestEntryId",
                table: "EntryImages");

            migrationBuilder.DropForeignKey(
                name: "FK_UserContestParticipations_Contests_ContestId",
                table: "UserContestParticipations");

            migrationBuilder.DropForeignKey(
                name: "FK_Votes_ContestEntries_ContestEntryId",
                table: "Votes");

            migrationBuilder.DropForeignKey(
                name: "FK_Votes_Contests_ContestId",
                table: "Votes");

            migrationBuilder.DropTable(
                name: "ContestWinners");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Contests",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SubmittedAt",
                table: "ContestEntries",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.CreateIndex(
                name: "IX_Contests_WinnerEntryId",
                table: "Contests",
                column: "WinnerEntryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contests_ContestEntries_WinnerEntryId",
                table: "Contests",
                column: "WinnerEntryId",
                principalTable: "ContestEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ContestsCategories_ContestCategories_CategoryId",
                table: "ContestsCategories",
                column: "CategoryId",
                principalTable: "ContestCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ContestsCategories_Contests_ContestId",
                table: "ContestsCategories",
                column: "ContestId",
                principalTable: "Contests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EntryImages_ContestEntries_ContestEntryId",
                table: "EntryImages",
                column: "ContestEntryId",
                principalTable: "ContestEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserContestParticipations_Contests_ContestId",
                table: "UserContestParticipations",
                column: "ContestId",
                principalTable: "Contests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_ContestEntries_ContestEntryId",
                table: "Votes",
                column: "ContestEntryId",
                principalTable: "ContestEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_Contests_ContestId",
                table: "Votes",
                column: "ContestId",
                principalTable: "Contests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
