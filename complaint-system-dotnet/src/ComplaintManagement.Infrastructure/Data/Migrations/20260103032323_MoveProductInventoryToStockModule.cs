using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MoveProductInventoryToStockModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultWarehouse",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "LeadTimeDays",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "QuantityInStock",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "QuantityReserved",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ReorderLevel",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ReorderQuantity",
                table: "Products");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId1",
                table: "StockItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DefaultLocationId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DefaultStockCategoryId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 3, 23, 20, 630, DateTimeKind.Utc).AddTicks(8173), new DateTime(2026, 1, 3, 3, 23, 20, 630, DateTimeKind.Utc).AddTicks(8277) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 3, 23, 20, 630, DateTimeKind.Utc).AddTicks(8480), new DateTime(2026, 1, 3, 3, 23, 20, 630, DateTimeKind.Utc).AddTicks(8480) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 3, 23, 20, 630, DateTimeKind.Utc).AddTicks(8483), new DateTime(2026, 1, 3, 3, 23, 20, 630, DateTimeKind.Utc).AddTicks(8483) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 3, 23, 20, 630, DateTimeKind.Utc).AddTicks(8485), new DateTime(2026, 1, 3, 3, 23, 20, 630, DateTimeKind.Utc).AddTicks(8486) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 3, 23, 20, 630, DateTimeKind.Utc).AddTicks(8488), new DateTime(2026, 1, 3, 3, 23, 20, 630, DateTimeKind.Utc).AddTicks(8488) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7767), new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7769) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7775), new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7775) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7777), new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7778) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7780), new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7780) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7782), new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7783) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7785), new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7785) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7787), new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7788) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7790), new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7790) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7792), new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7793) });

            migrationBuilder.CreateIndex(
                name: "IX_StockItems_ProductId1",
                table: "StockItems",
                column: "ProductId1");

            migrationBuilder.CreateIndex(
                name: "IX_Products_DefaultLocationId",
                table: "Products",
                column: "DefaultLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_DefaultStockCategoryId",
                table: "Products",
                column: "DefaultStockCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_StockCategories_DefaultStockCategoryId",
                table: "Products",
                column: "DefaultStockCategoryId",
                principalTable: "StockCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_StockLocations_DefaultLocationId",
                table: "Products",
                column: "DefaultLocationId",
                principalTable: "StockLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_StockItems_Products_ProductId1",
                table: "StockItems",
                column: "ProductId1",
                principalTable: "Products",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_StockCategories_DefaultStockCategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_StockLocations_DefaultLocationId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_StockItems_Products_ProductId1",
                table: "StockItems");

            migrationBuilder.DropIndex(
                name: "IX_StockItems_ProductId1",
                table: "StockItems");

            migrationBuilder.DropIndex(
                name: "IX_Products_DefaultLocationId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_DefaultStockCategoryId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductId1",
                table: "StockItems");

            migrationBuilder.DropColumn(
                name: "DefaultLocationId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DefaultStockCategoryId",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "DefaultWarehouse",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LeadTimeDays",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuantityInStock",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuantityReserved",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReorderLevel",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReorderQuantity",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 4, 38, 25, 603, DateTimeKind.Utc).AddTicks(2308), new DateTime(2026, 1, 2, 4, 38, 25, 603, DateTimeKind.Utc).AddTicks(2520) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 4, 38, 25, 603, DateTimeKind.Utc).AddTicks(2908), new DateTime(2026, 1, 2, 4, 38, 25, 603, DateTimeKind.Utc).AddTicks(2909) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 4, 38, 25, 603, DateTimeKind.Utc).AddTicks(2914), new DateTime(2026, 1, 2, 4, 38, 25, 603, DateTimeKind.Utc).AddTicks(2915) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 4, 38, 25, 603, DateTimeKind.Utc).AddTicks(2920), new DateTime(2026, 1, 2, 4, 38, 25, 603, DateTimeKind.Utc).AddTicks(2920) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 4, 38, 25, 603, DateTimeKind.Utc).AddTicks(2925), new DateTime(2026, 1, 2, 4, 38, 25, 603, DateTimeKind.Utc).AddTicks(2926) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 4, 38, 25, 606, DateTimeKind.Utc).AddTicks(8766), new DateTime(2026, 1, 2, 4, 38, 25, 606, DateTimeKind.Utc).AddTicks(8781) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 4, 38, 25, 606, DateTimeKind.Utc).AddTicks(8796), new DateTime(2026, 1, 2, 4, 38, 25, 606, DateTimeKind.Utc).AddTicks(8796) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 4, 38, 25, 606, DateTimeKind.Utc).AddTicks(8801), new DateTime(2026, 1, 2, 4, 38, 25, 606, DateTimeKind.Utc).AddTicks(8801) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 4, 38, 25, 606, DateTimeKind.Utc).AddTicks(8805), new DateTime(2026, 1, 2, 4, 38, 25, 606, DateTimeKind.Utc).AddTicks(8805) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 4, 38, 25, 606, DateTimeKind.Utc).AddTicks(8809), new DateTime(2026, 1, 2, 4, 38, 25, 606, DateTimeKind.Utc).AddTicks(8810) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 4, 38, 25, 606, DateTimeKind.Utc).AddTicks(8945), new DateTime(2026, 1, 2, 4, 38, 25, 606, DateTimeKind.Utc).AddTicks(8946) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 4, 38, 25, 606, DateTimeKind.Utc).AddTicks(8950), new DateTime(2026, 1, 2, 4, 38, 25, 606, DateTimeKind.Utc).AddTicks(8951) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 4, 38, 25, 606, DateTimeKind.Utc).AddTicks(8955), new DateTime(2026, 1, 2, 4, 38, 25, 606, DateTimeKind.Utc).AddTicks(8955) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 4, 38, 25, 606, DateTimeKind.Utc).AddTicks(8959), new DateTime(2026, 1, 2, 4, 38, 25, 606, DateTimeKind.Utc).AddTicks(8960) });
        }
    }
}
