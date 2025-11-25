using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryWorkflowTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WorkflowId",
                table: "ComplaintCategories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CategoryWorkflows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryWorkflows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryWorkflows_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CategoryWorkflows_ComplaintCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ComplaintCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CategoryWorkflowStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusMasterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsInitialStatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DefaultSLAHours = table.Column<int>(type: "int", nullable: true),
                    EscalationHours = table.Column<int>(type: "int", nullable: true),
                    RequiresApproval = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AllowedRoles = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryWorkflowStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryWorkflowStatuses_CategoryWorkflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "CategoryWorkflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryWorkflowStatuses_ComplaintStatusMasters_StatusMasterId",
                        column: x => x.StatusMasterId,
                        principalTable: "ComplaintStatusMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CategoryWorkflowTransitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransitionName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequiresComment = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RequiresApproval = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AllowedRoles = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsAutomatic = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AutoTransitionAfterHours = table.Column<int>(type: "int", nullable: true),
                    TransitionConditions = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ButtonColor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryWorkflowTransitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryWorkflowTransitions_CategoryWorkflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "CategoryWorkflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryWorkflowTransitions_ComplaintStatusMasters_FromStatusId",
                        column: x => x.FromStatusId,
                        principalTable: "ComplaintStatusMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CategoryWorkflowTransitions_ComplaintStatusMasters_ToStatusId",
                        column: x => x.ToStatusId,
                        principalTable: "ComplaintStatusMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 9, 11, 47, 52, DateTimeKind.Utc).AddTicks(4634), new DateTime(2025, 11, 2, 9, 11, 47, 52, DateTimeKind.Utc).AddTicks(4843) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 9, 11, 47, 52, DateTimeKind.Utc).AddTicks(5215), new DateTime(2025, 11, 2, 9, 11, 47, 52, DateTimeKind.Utc).AddTicks(5216) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 9, 11, 47, 52, DateTimeKind.Utc).AddTicks(5222), new DateTime(2025, 11, 2, 9, 11, 47, 52, DateTimeKind.Utc).AddTicks(5223) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 9, 11, 47, 52, DateTimeKind.Utc).AddTicks(5228), new DateTime(2025, 11, 2, 9, 11, 47, 52, DateTimeKind.Utc).AddTicks(5229) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 9, 11, 47, 52, DateTimeKind.Utc).AddTicks(5234), new DateTime(2025, 11, 2, 9, 11, 47, 52, DateTimeKind.Utc).AddTicks(5235) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(3931), new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(3938) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(3951), new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(3952) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(3956), new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(3957) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(4038), new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(4039) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(4043), new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(4044) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(4048), new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(4049) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(4053), new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(4054) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(4058), new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(4058) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(4062), new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(4063) });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryWorkflows_CategoryId",
                table: "CategoryWorkflows",
                column: "CategoryId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryWorkflows_CategoryId_IsDefault",
                table: "CategoryWorkflows",
                columns: new[] { "CategoryId", "IsDefault" },
                filter: "[IsDeleted] = 0 AND [IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryWorkflows_CompanyId",
                table: "CategoryWorkflows",
                column: "CompanyId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryWorkflowStatuses_StatusMasterId",
                table: "CategoryWorkflowStatuses",
                column: "StatusMasterId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryWorkflowStatuses_WorkflowId",
                table: "CategoryWorkflowStatuses",
                column: "WorkflowId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryWorkflowStatuses_WorkflowId_DisplayOrder",
                table: "CategoryWorkflowStatuses",
                columns: new[] { "WorkflowId", "DisplayOrder" },
                filter: "[IsDeleted] = 0 AND [IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryWorkflowStatuses_WorkflowId_StatusMasterId",
                table: "CategoryWorkflowStatuses",
                columns: new[] { "WorkflowId", "StatusMasterId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryWorkflowTransitions_FromStatusId",
                table: "CategoryWorkflowTransitions",
                column: "FromStatusId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryWorkflowTransitions_ToStatusId",
                table: "CategoryWorkflowTransitions",
                column: "ToStatusId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryWorkflowTransitions_WorkflowId",
                table: "CategoryWorkflowTransitions",
                column: "WorkflowId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryWorkflowTransitions_WorkflowId_FromStatusId",
                table: "CategoryWorkflowTransitions",
                columns: new[] { "WorkflowId", "FromStatusId" },
                filter: "[IsDeleted] = 0 AND [IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryWorkflowTransitions_WorkflowId_FromStatusId_ToStatusId",
                table: "CategoryWorkflowTransitions",
                columns: new[] { "WorkflowId", "FromStatusId", "ToStatusId" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoryWorkflowStatuses");

            migrationBuilder.DropTable(
                name: "CategoryWorkflowTransitions");

            migrationBuilder.DropTable(
                name: "CategoryWorkflows");

            migrationBuilder.DropColumn(
                name: "WorkflowId",
                table: "ComplaintCategories");

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 16, 58, 17, 249, DateTimeKind.Utc).AddTicks(4209), new DateTime(2025, 11, 1, 16, 58, 17, 249, DateTimeKind.Utc).AddTicks(4484) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 16, 58, 17, 249, DateTimeKind.Utc).AddTicks(4926), new DateTime(2025, 11, 1, 16, 58, 17, 249, DateTimeKind.Utc).AddTicks(4927) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 16, 58, 17, 249, DateTimeKind.Utc).AddTicks(4932), new DateTime(2025, 11, 1, 16, 58, 17, 249, DateTimeKind.Utc).AddTicks(4932) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 16, 58, 17, 249, DateTimeKind.Utc).AddTicks(4937), new DateTime(2025, 11, 1, 16, 58, 17, 249, DateTimeKind.Utc).AddTicks(4937) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 16, 58, 17, 249, DateTimeKind.Utc).AddTicks(4940), new DateTime(2025, 11, 1, 16, 58, 17, 249, DateTimeKind.Utc).AddTicks(4941) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(639), new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(647) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(656), new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(656) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(660), new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(661) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(664), new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(664) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(667), new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(667) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(670), new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(671) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(673), new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(674) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(676), new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(677) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(680), new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(680) });
        }
    }
}
