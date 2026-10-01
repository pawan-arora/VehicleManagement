using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CreditWorks.VehicleManagement.Modules.Categories.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "categories");

            migrationBuilder.CreateTable(
                name: "CategoryIcon",
                schema: "categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Svg = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryIcon", x => x.Id);
                    table.CheckConstraint("CK_CategoryIcon_Name_NotBlank", "LEN(TRIM([Name])) > 0");
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                schema: "categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1001, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MinWeightKg = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    MaxWeightKg = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    IconId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.CheckConstraint("CK_Categories_MaxWeightKg_AboveMin", "[MaxWeightKg] IS NULL OR [MaxWeightKg] > [MinWeightKg]");
                    table.CheckConstraint("CK_Categories_MinWeightKg_NotNegative", "[MinWeightKg] >= 0");
                    table.CheckConstraint("CK_Categories_Name_NotBlank", "LEN(TRIM([Name])) > 0");
                    table.ForeignKey(
                        name: "FK_Categories_CategoryIcon_IconId",
                        column: x => x.IconId,
                        principalSchema: "categories",
                        principalTable: "CategoryIcon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "categories",
                table: "CategoryIcon",
                columns: new[] { "Id", "Name", "Svg" },
                values: new object[,]
                {
                    { 1, "Bicycle", "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"24\" height=\"24\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"#64748b\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><circle cx=\"18.5\" cy=\"17.5\" r=\"3.5\"/><circle cx=\"5.5\" cy=\"17.5\" r=\"3.5\"/><circle cx=\"15\" cy=\"5\" r=\"1\"/><path d=\"M12 17.5V14l-3-3 4-3 2 3h2\"/></svg>" },
                    { 2, "Motorcycle", "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"24\" height=\"24\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"#64748b\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path d=\"m18 14-1-3\"/><path d=\"m3 9 6 2a2 2 0 0 1 2-2h2a2 2 0 0 1 1.99 1.81\"/><path d=\"M8 17h3a1 1 0 0 0 1-1 6 6 0 0 1 6-6 1 1 0 0 0 1-1v-.75A5 5 0 0 0 17 5\"/><circle cx=\"19\" cy=\"17\" r=\"3\"/><circle cx=\"5\" cy=\"17\" r=\"3\"/></svg>" },
                    { 3, "Car", "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"24\" height=\"24\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"#64748b\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path d=\"M19 17h2c.6 0 1-.4 1-1v-3c0-.9-.7-1.7-1.5-1.9C18.7 10.6 16 10 16 10s-1.3-1.4-2.2-2.3c-.5-.4-1.1-.7-1.8-.7H5c-.6 0-1.1.4-1.4.9l-1.4 2.9A3.7 3.7 0 0 0 2 12v4c0 .6.4 1 1 1h2\"/><circle cx=\"7\" cy=\"17\" r=\"2\"/><path d=\"M9 17h6\"/><circle cx=\"17\" cy=\"17\" r=\"2\"/></svg>" },
                    { 4, "Van", "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"24\" height=\"24\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"#64748b\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path d=\"M13 6v5a1 1 0 0 0 1 1h6.102a1 1 0 0 1 .712.298l.898.91a1 1 0 0 1 .288.702V17a1 1 0 0 1-1 1h-3\"/><path d=\"M5 18H3a1 1 0 0 1-1-1V8a2 2 0 0 1 2-2h12c1.1 0 2.1.8 2.4 1.8l1.176 4.2\"/><path d=\"M9 18h5\"/><circle cx=\"16\" cy=\"18\" r=\"2\"/><circle cx=\"7\" cy=\"18\" r=\"2\"/></svg>" },
                    { 5, "Truck", "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"24\" height=\"24\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"#64748b\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path d=\"M14 18V6a2 2 0 0 0-2-2H4a2 2 0 0 0-2 2v11a1 1 0 0 0 1 1h2\"/><path d=\"M15 18H9\"/><path d=\"M19 18h2a1 1 0 0 0 1-1v-3.65a1 1 0 0 0-.22-.624l-3.48-4.35A1 1 0 0 0 17.52 8H14\"/><circle cx=\"17\" cy=\"18\" r=\"2\"/><circle cx=\"7\" cy=\"18\" r=\"2\"/></svg>" },
                    { 6, "Bus", "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"24\" height=\"24\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"#64748b\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path d=\"M8 6v6\"/><path d=\"M15 6v6\"/><path d=\"M2 12h19.6\"/><path d=\"M18 18h3s.5-1.7.8-2.8c.1-.4.2-.8.2-1.2 0-.4-.1-.8-.2-1.2l-1.4-5C20.1 6.8 19.1 6 18 6H4a2 2 0 0 0-2 2v10h3\"/><circle cx=\"7\" cy=\"18\" r=\"2\"/><path d=\"M9 18h5\"/><circle cx=\"16\" cy=\"18\" r=\"2\"/></svg>" },
                    { 7, "Tractor", "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"24\" height=\"24\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"#64748b\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path d=\"m10 11 11 .9a1 1 0 0 1 .8 1.1l-.665 4.158a1 1 0 0 1-.988.842H20\"/><path d=\"M16 18h-5\"/><path d=\"M18 5a1 1 0 0 0-1 1v5.573\"/><path d=\"M3 4h8.129a1 1 0 0 1 .99.863L13 11.246\"/><path d=\"M4 11V4\"/><path d=\"M7 15h.01\"/><path d=\"M8 10.1V4\"/><circle cx=\"18\" cy=\"18\" r=\"2\"/><circle cx=\"7\" cy=\"15\" r=\"5\"/></svg>" }
                });

            migrationBuilder.InsertData(
                schema: "categories",
                table: "Categories",
                columns: new[] { "Id", "IconId", "MaxWeightKg", "MinWeightKg", "Name" },
                values: new object[,]
                {
                    { 1001, 2, 500m, 0m, "Light" },
                    { 1002, 3, 2500m, 500m, "Medium" },
                    { 1003, 5, null, 2500m, "Heavy" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_IconId",
                schema: "categories",
                table: "Categories",
                column: "IconId");

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

            migrationBuilder.CreateIndex(
                name: "IX_CategoryIcon_Name",
                schema: "categories",
                table: "CategoryIcon",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Categories",
                schema: "categories");

            migrationBuilder.DropTable(
                name: "CategoryIcon",
                schema: "categories");
        }
    }
}
