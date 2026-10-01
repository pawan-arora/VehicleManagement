using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreditWorks.VehicleManagement.Modules.Categories.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCrossRowUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_MinWeightKg",
                schema: "categories",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Name",
                schema: "categories",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "UX_Categories_SingleOpenEnded",
                schema: "categories",
                table: "Categories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Categories_MinWeightKg",
                schema: "categories",
                table: "Categories",
                column: "MinWeightKg",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                schema: "categories",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Categories_SingleOpenEnded",
                schema: "categories",
                table: "Categories",
                column: "MaxWeightKg",
                unique: true,
                filter: "[MaxWeightKg] IS NULL");
        }
    }
}
