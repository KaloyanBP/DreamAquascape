using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamAquascape.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialContestDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContestCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContestCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContestEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContestId = table.Column<int>(type: "int", nullable: false),
                    ParticipantId = table.Column<string>(type: "nvarchar(450)", nullable: false, comment: "Foreign key to the referenced AspNetUser."),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContestEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContestEntries_AspNetUsers_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Contests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ImageFileUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    SubmissionStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmissionEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VotingStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VotingEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResultDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    WinnerEntryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contests_ContestEntries_WinnerEntryId",
                        column: x => x.WinnerEntryId,
                        principalTable: "ContestEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EntryImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContestEntryId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntryImages_ContestEntries_ContestEntryId",
                        column: x => x.ContestEntryId,
                        principalTable: "ContestEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContestsCategories",
                columns: table => new
                {
                    ContestId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContestsCategories", x => new { x.ContestId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_ContestsCategories_ContestCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ContestCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContestsCategories_Contests_ContestId",
                        column: x => x.ContestId,
                        principalTable: "Contests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prizes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContestId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NavigationUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    MonetaryValue = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    SponsorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prizes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prizes_Contests_ContestId",
                        column: x => x.ContestId,
                        principalTable: "Contests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserContestParticipations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContestId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ParticipationDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    HasSubmittedEntry = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    HasVoted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    VotedForEntryId = table.Column<int>(type: "int", nullable: true),
                    VotedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmittedEntryId = table.Column<int>(type: "int", nullable: true),
                    EntrySubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Tracks all user activities in contests in a central place");

            migrationBuilder.CreateTable(
                name: "Votes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContestId = table.Column<int>(type: "int", nullable: false),
                    ContestEntryId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VotedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Votes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Votes_ContestEntries_ContestEntryId",
                        column: x => x.ContestEntryId,
                        principalTable: "ContestEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Votes_Contests_ContestId",
                        column: x => x.ContestId,
                        principalTable: "Contests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContestCategories_Name",
                table: "ContestCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContestEntries_ContestId",
                table: "ContestEntries",
                column: "ContestId");

            migrationBuilder.CreateIndex(
                name: "IX_ContestEntries_ParticipantId",
                table: "ContestEntries",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_Contests_WinnerEntryId",
                table: "Contests",
                column: "WinnerEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_ContestsCategories_CategoryId",
                table: "ContestsCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryImages_ContestEntryId",
                table: "EntryImages",
                column: "ContestEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_Prizes_ContestId",
                table: "Prizes",
                column: "ContestId",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_Votes_ContestEntryId",
                table: "Votes",
                column: "ContestEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_ContestId",
                table: "Votes",
                column: "ContestId");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_UserId",
                table: "Votes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContestEntries_Contests_ContestId",
                table: "ContestEntries",
                column: "ContestId",
                principalTable: "Contests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContestEntries_Contests_ContestId",
                table: "ContestEntries");

            migrationBuilder.DropTable(
                name: "ContestsCategories");

            migrationBuilder.DropTable(
                name: "EntryImages");

            migrationBuilder.DropTable(
                name: "Prizes");

            migrationBuilder.DropTable(
                name: "UserContestParticipations");

            migrationBuilder.DropTable(
                name: "Votes");

            migrationBuilder.DropTable(
                name: "ContestCategories");

            migrationBuilder.DropTable(
                name: "Contests");

            migrationBuilder.DropTable(
                name: "ContestEntries");
        }
    }
}
