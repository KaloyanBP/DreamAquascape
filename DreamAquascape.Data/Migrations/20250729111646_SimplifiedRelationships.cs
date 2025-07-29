using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamAquascape.Data.Migrations
{
    /// <inheritdoc />
    public partial class SimplifiedRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Votes_ContestEntries_ContestEntryId",
                table: "Votes");

            migrationBuilder.DropForeignKey(
                name: "FK_Votes_Contests_ContestId",
                table: "Votes");

            migrationBuilder.DropTable(
                name: "UserContestParticipations");

            migrationBuilder.DropIndex(
                name: "IX_Votes_ContestId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Votes_UserId",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "ContestId",
                table: "Votes");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Votes",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                comment: "Foreign key to the referenced AspNetUser.",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_Vote_User_Entry_Unique",
                table: "Votes",
                columns: new[] { "UserId", "ContestEntryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Votes_VotedAt",
                table: "Votes",
                column: "VotedAt");

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_ContestEntries_ContestEntryId",
                table: "Votes",
                column: "ContestEntryId",
                principalTable: "ContestEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Votes_ContestEntries_ContestEntryId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Vote_User_Entry_Unique",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Votes_VotedAt",
                table: "Votes");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Votes",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldComment: "Foreign key to the referenced AspNetUser.");

            migrationBuilder.AddColumn<int>(
                name: "ContestId",
                table: "Votes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "UserContestParticipations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContestId = table.Column<int>(type: "int", nullable: false),
                    SubmittedEntryId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VotedForEntryId = table.Column<int>(type: "int", nullable: true),
                    EntrySubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HasSubmittedEntry = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    HasVoted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ParticipationDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    VotedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserContestParticipations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserContestParticipations_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserContestParticipations_ContestEntries_SubmittedEntryId",
                        column: x => x.SubmittedEntryId,
                        principalTable: "ContestEntries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserContestParticipations_ContestEntries_VotedForEntryId",
                        column: x => x.VotedForEntryId,
                        principalTable: "ContestEntries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserContestParticipations_Contests_ContestId",
                        column: x => x.ContestId,
                        principalTable: "Contests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tracks all user activities in contests in a central place");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_ContestId",
                table: "Votes",
                column: "ContestId");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_UserId",
                table: "Votes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserContestParticipations_ContestId",
                table: "UserContestParticipations",
                column: "ContestId");

            migrationBuilder.CreateIndex(
                name: "IX_UserContestParticipations_SubmittedEntryId",
                table: "UserContestParticipations",
                column: "SubmittedEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserContestParticipations_UserId",
                table: "UserContestParticipations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserContestParticipations_VotedForEntryId",
                table: "UserContestParticipations",
                column: "VotedForEntryId");

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
    }
}
