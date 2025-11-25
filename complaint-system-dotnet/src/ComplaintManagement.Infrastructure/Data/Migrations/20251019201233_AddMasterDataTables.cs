using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMasterDataTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PriorityMasterId",
                table: "Complaints",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StatusMasterId",
                table: "Complaints",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ComplaintPriorityMasters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    ColorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SlaResponseHours = table.Column<int>(type: "int", nullable: true),
                    SlaResolutionHours = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplaintPriorityMasters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintPriorityMasters_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComplaintStatusMasters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ColorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    IsFinal = table.Column<bool>(type: "bit", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplaintStatusMasters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintStatusMasters_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomFieldDefinition",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FieldName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FieldKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FieldType = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSearchable = table.Column<bool>(type: "bit", nullable: false),
                    DefaultValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidationRules = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Options = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Placeholder = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HelpText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Section = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsVisibleToComplainant = table.Column<bool>(type: "bit", nullable: false),
                    IsVisibleToHandler = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomFieldDefinition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomFieldDefinition_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomFieldValue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomFieldDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumericValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DateValue = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BooleanValue = table.Column<bool>(type: "bit", nullable: true),
                    JsonValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ComplaintId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomFieldValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomFieldValue_Complaints_ComplaintId",
                        column: x => x.ComplaintId,
                        principalTable: "Complaints",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CustomFieldValue_CustomFieldDefinition_CustomFieldDefinitionId",
                        column: x => x.CustomFieldDefinitionId,
                        principalTable: "CustomFieldDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ComplaintPriorityMasters",
                columns: new[] { "Id", "Code", "ColorCode", "CompanyId", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "IconClass", "IsActive", "IsDeleted", "IsSystem", "Level", "Name", "SlaResolutionHours", "SlaResponseHours", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), "LOW", "#4CAF50", null, new DateTime(2025, 10, 19, 20, 12, 31, 794, DateTimeKind.Utc).AddTicks(6863), null, null, null, "Low priority - No immediate action required", 1, "bi-arrow-down-circle", true, false, true, 1, "Low", 168, 72, new DateTime(2025, 10, 19, 20, 12, 31, 794, DateTimeKind.Utc).AddTicks(7075), null },
                    { new Guid("20000000-0000-0000-0000-000000000002"), "NORMAL", "#2196F3", null, new DateTime(2025, 10, 19, 20, 12, 31, 794, DateTimeKind.Utc).AddTicks(7505), null, null, null, "Normal priority - Standard processing time", 2, "bi-dash-circle", true, false, true, 3, "Normal", 120, 48, new DateTime(2025, 10, 19, 20, 12, 31, 794, DateTimeKind.Utc).AddTicks(7507), null },
                    { new Guid("20000000-0000-0000-0000-000000000003"), "HIGH", "#FF9800", null, new DateTime(2025, 10, 19, 20, 12, 31, 794, DateTimeKind.Utc).AddTicks(7514), null, null, null, "High priority - Requires expedited attention", 3, "bi-exclamation-circle", true, false, true, 5, "High", 72, 24, new DateTime(2025, 10, 19, 20, 12, 31, 794, DateTimeKind.Utc).AddTicks(7515), null },
                    { new Guid("20000000-0000-0000-0000-000000000004"), "CRITICAL", "#F44336", null, new DateTime(2025, 10, 19, 20, 12, 31, 794, DateTimeKind.Utc).AddTicks(7521), null, null, null, "Critical priority - Requires immediate attention", 4, "bi-exclamation-triangle", true, false, true, 8, "Critical", 24, 4, new DateTime(2025, 10, 19, 20, 12, 31, 794, DateTimeKind.Utc).AddTicks(7522), null },
                    { new Guid("20000000-0000-0000-0000-000000000005"), "URGENT", "#9C27B0", null, new DateTime(2025, 10, 19, 20, 12, 31, 794, DateTimeKind.Utc).AddTicks(7529), null, null, null, "Urgent priority - Highest priority level", 5, "bi-lightning", true, false, true, 10, "Urgent", 8, 1, new DateTime(2025, 10, 19, 20, 12, 31, 794, DateTimeKind.Utc).AddTicks(7530), null }
                });

            migrationBuilder.InsertData(
                table: "ComplaintStatusMasters",
                columns: new[] { "Id", "Code", "ColorCode", "CompanyId", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "IconClass", "IsActive", "IsDeleted", "IsFinal", "IsSystem", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), "SUBMITTED", "#9E9E9E", null, new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8139), null, null, null, "Complaint has been submitted but not yet reviewed", 1, "bi-inbox", true, false, false, true, "Submitted", new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8144), null },
                    { new Guid("10000000-0000-0000-0000-000000000002"), "UNDER_REVIEW", "#2196F3", null, new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8164), null, null, null, "Complaint is being reviewed by the assigned handler", 2, "bi-eye", true, false, false, true, "Under Review", new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8165), null },
                    { new Guid("10000000-0000-0000-0000-000000000003"), "IN_PROGRESS", "#FF9800", null, new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8170), null, null, null, "Complaint is currently being investigated", 3, "bi-gear", true, false, false, true, "In Progress", new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8171), null },
                    { new Guid("10000000-0000-0000-0000-000000000004"), "ESCALATED", "#FF5722", null, new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8176), null, null, null, "Complaint has been escalated to a higher level", 4, "bi-arrow-up-circle", true, false, false, true, "Escalated", new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8176), null },
                    { new Guid("10000000-0000-0000-0000-000000000005"), "PENDING_INFO", "#FFC107", null, new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8183), null, null, null, "Complaint is awaiting information from the complainant", 5, "bi-question-circle", true, false, false, true, "Pending Info", new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8184), null },
                    { new Guid("10000000-0000-0000-0000-000000000006"), "RESOLVED", "#4CAF50", null, new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8190), null, null, null, "Complaint has been resolved", 6, "bi-check-circle", true, false, false, true, "Resolved", new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8191), null },
                    { new Guid("10000000-0000-0000-0000-000000000007"), "CLOSED", "#607D8B", null, new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8195), null, null, null, "Complaint has been closed (final state)", 7, "bi-lock", true, false, true, true, "Closed", new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8196), null },
                    { new Guid("10000000-0000-0000-0000-000000000008"), "REJECTED", "#F44336", null, new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8202), null, null, null, "Complaint has been rejected/dismissed", 8, "bi-x-circle", true, false, true, true, "Rejected", new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8203), null },
                    { new Guid("10000000-0000-0000-0000-000000000009"), "REOPENED", "#E91E63", null, new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8208), null, null, null, "Complaint has been reopened after closure", 9, "bi-arrow-repeat", true, false, false, true, "Reopened", new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8209), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_PriorityMasterId",
                table: "Complaints",
                column: "PriorityMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_StatusMasterId",
                table: "Complaints",
                column: "StatusMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintPriorityMasters_Code",
                table: "ComplaintPriorityMasters",
                column: "Code",
                unique: true,
                filter: "[CompanyId] IS NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintPriorityMasters_Code_CompanyId",
                table: "ComplaintPriorityMasters",
                columns: new[] { "Code", "CompanyId" },
                unique: true,
                filter: "[CompanyId] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintPriorityMasters_CompanyId",
                table: "ComplaintPriorityMasters",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintStatusMasters_Code",
                table: "ComplaintStatusMasters",
                column: "Code",
                unique: true,
                filter: "[CompanyId] IS NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintStatusMasters_Code_CompanyId",
                table: "ComplaintStatusMasters",
                columns: new[] { "Code", "CompanyId" },
                unique: true,
                filter: "[CompanyId] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintStatusMasters_CompanyId",
                table: "ComplaintStatusMasters",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldDefinition_CompanyId",
                table: "CustomFieldDefinition",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldValue_ComplaintId",
                table: "CustomFieldValue",
                column: "ComplaintId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldValue_CustomFieldDefinitionId",
                table: "CustomFieldValue",
                column: "CustomFieldDefinitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_ComplaintPriorityMasters_PriorityMasterId",
                table: "Complaints",
                column: "PriorityMasterId",
                principalTable: "ComplaintPriorityMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_ComplaintStatusMasters_StatusMasterId",
                table: "Complaints",
                column: "StatusMasterId",
                principalTable: "ComplaintStatusMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_ComplaintPriorityMasters_PriorityMasterId",
                table: "Complaints");

            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_ComplaintStatusMasters_StatusMasterId",
                table: "Complaints");

            migrationBuilder.DropTable(
                name: "ComplaintPriorityMasters");

            migrationBuilder.DropTable(
                name: "ComplaintStatusMasters");

            migrationBuilder.DropTable(
                name: "CustomFieldValue");

            migrationBuilder.DropTable(
                name: "CustomFieldDefinition");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_PriorityMasterId",
                table: "Complaints");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_StatusMasterId",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "PriorityMasterId",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "StatusMasterId",
                table: "Complaints");
        }
    }
}
