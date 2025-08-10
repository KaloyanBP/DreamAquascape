using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamAquascape.Data.Migrations
{
    /// <inheritdoc />
    public partial class ModifyCategoryNameUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContestCategories_Name",
                table: "ContestCategories");

            migrationBuilder.CreateIndex(
                name: "IX_ContestCategories_Name_IsDeleted",
                table: "ContestCategories",
                columns: new[] { "Name", "IsDeleted" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContestCategories_Name_IsDeleted",
                table: "ContestCategories");

            migrationBuilder.CreateIndex(
                name: "IX_ContestCategories_Name",
                table: "ContestCategories",
                column: "Name",
                unique: true);
        }
    }
}
