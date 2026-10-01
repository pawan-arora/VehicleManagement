using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreditWorks.VehicleManagement.Modules.Categories.Migrations
{
    /// <inheritdoc />
    public partial class StoreIconKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_CategoryIcon_Name_NotBlank",
                schema: "categories",
                table: "CategoryIcon");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "categories",
                table: "CategoryIcon",
                newName: "Key");

            migrationBuilder.RenameIndex(
                name: "IX_CategoryIcon_Name",
                schema: "categories",
                table: "CategoryIcon",
                newName: "IX_CategoryIcon_Key");

            migrationBuilder.UpdateData(
                schema: "categories",
                table: "CategoryIcon",
                keyColumn: "Id",
                keyValue: 1,
                column: "Key",
                value: "bicycle");

            migrationBuilder.UpdateData(
                schema: "categories",
                table: "CategoryIcon",
                keyColumn: "Id",
                keyValue: 2,
                column: "Key",
                value: "motorcycle");

            migrationBuilder.UpdateData(
                schema: "categories",
                table: "CategoryIcon",
                keyColumn: "Id",
                keyValue: 3,
                column: "Key",
                value: "car");

            migrationBuilder.UpdateData(
                schema: "categories",
                table: "CategoryIcon",
                keyColumn: "Id",
                keyValue: 4,
                column: "Key",
                value: "van");

            migrationBuilder.UpdateData(
                schema: "categories",
                table: "CategoryIcon",
                keyColumn: "Id",
                keyValue: 5,
                column: "Key",
                value: "truck");

            migrationBuilder.UpdateData(
                schema: "categories",
                table: "CategoryIcon",
                keyColumn: "Id",
                keyValue: 6,
                column: "Key",
                value: "bus");

            migrationBuilder.UpdateData(
                schema: "categories",
                table: "CategoryIcon",
                keyColumn: "Id",
                keyValue: 7,
                column: "Key",
                value: "tractor");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CategoryIcon_Key_NotBlank",
                schema: "categories",
                table: "CategoryIcon",
                sql: "LEN(TRIM([Key])) > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_CategoryIcon_Key_NotBlank",
                schema: "categories",
                table: "CategoryIcon");

            migrationBuilder.RenameColumn(
                name: "Key",
                schema: "categories",
                table: "CategoryIcon",
                newName: "Name");

            migrationBuilder.RenameIndex(
                name: "IX_CategoryIcon_Key",
                schema: "categories",
                table: "CategoryIcon",
                newName: "IX_CategoryIcon_Name");

            migrationBuilder.UpdateData(
                schema: "categories",
                table: "CategoryIcon",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Bicycle");

            migrationBuilder.UpdateData(
                schema: "categories",
                table: "CategoryIcon",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Motorcycle");

            migrationBuilder.UpdateData(
                schema: "categories",
                table: "CategoryIcon",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Car");

            migrationBuilder.UpdateData(
                schema: "categories",
                table: "CategoryIcon",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Van");

            migrationBuilder.UpdateData(
                schema: "categories",
                table: "CategoryIcon",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Truck");

            migrationBuilder.UpdateData(
                schema: "categories",
                table: "CategoryIcon",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "Bus");

            migrationBuilder.UpdateData(
                schema: "categories",
                table: "CategoryIcon",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "Tractor");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CategoryIcon_Name_NotBlank",
                schema: "categories",
                table: "CategoryIcon",
                sql: "LEN(TRIM([Name])) > 0");
        }
    }
}
