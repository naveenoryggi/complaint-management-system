using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEscalationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EscalationMatrices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    EnableAutoEscalation = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SendEmailNotifications = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_EscalationMatrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EscalationMatrices_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EscalationMatrices_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EscalationMatrices_ComplaintCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ComplaintCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EscalationMatrices_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EscalationLevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EscalationMatrixId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TriggerAfterHours = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    AssignmentStrategy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssignToUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssignToRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AssignToUserIds = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SendNotification = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EmailTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NotifyPreviousHandler = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EscalationMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_EscalationLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EscalationLevels_EscalationMatrices_EscalationMatrixId",
                        column: x => x.EscalationMatrixId,
                        principalTable: "EscalationMatrices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EscalationLevels_Users_AssignToUserId",
                        column: x => x.AssignToUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EscalationHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComplaintId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EscalationLevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EscalationMatrixId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    FromUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ToUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EscalatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EscalatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IsAutoEscalation = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AssignmentStrategy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SlaHoursAtEscalation = table.Column<int>(type: "int", nullable: true),
                    HoursOverdue = table.Column<int>(type: "int", nullable: true),
                    EmailSent = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EmailSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
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
                    table.PrimaryKey("PK_EscalationHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EscalationHistories_Complaints_ComplaintId",
                        column: x => x.ComplaintId,
                        principalTable: "Complaints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EscalationHistories_EscalationLevels_EscalationLevelId",
                        column: x => x.EscalationLevelId,
                        principalTable: "EscalationLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EscalationHistories_EscalationMatrices_EscalationMatrixId",
                        column: x => x.EscalationMatrixId,
                        principalTable: "EscalationMatrices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EscalationHistories_Users_EscalatedBy",
                        column: x => x.EscalatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EscalationHistories_Users_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EscalationHistories_Users_ToUserId",
                        column: x => x.ToUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EscalationHistories_ComplaintId",
                table: "EscalationHistories",
                column: "ComplaintId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationHistories_ComplaintId_EscalatedAt",
                table: "EscalationHistories",
                columns: new[] { "ComplaintId", "EscalatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EscalationHistories_ComplaintId_Level",
                table: "EscalationHistories",
                columns: new[] { "ComplaintId", "Level" });

            migrationBuilder.CreateIndex(
                name: "IX_EscalationHistories_EscalatedAt",
                table: "EscalationHistories",
                column: "EscalatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationHistories_EscalatedBy",
                table: "EscalationHistories",
                column: "EscalatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationHistories_EscalationLevelId",
                table: "EscalationHistories",
                column: "EscalationLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationHistories_EscalationMatrixId",
                table: "EscalationHistories",
                column: "EscalationMatrixId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationHistories_FromUserId",
                table: "EscalationHistories",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationHistories_Status",
                table: "EscalationHistories",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationHistories_ToUserId",
                table: "EscalationHistories",
                column: "ToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationLevels_AssignToUserId",
                table: "EscalationLevels",
                column: "AssignToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationLevels_EscalationMatrixId",
                table: "EscalationLevels",
                column: "EscalationMatrixId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationLevels_EscalationMatrixId_Level",
                table: "EscalationLevels",
                columns: new[] { "EscalationMatrixId", "Level" });

            migrationBuilder.CreateIndex(
                name: "IX_EscalationLevels_IsActive",
                table: "EscalationLevels",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationMatrices_BranchId",
                table: "EscalationMatrices",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationMatrices_CategoryId",
                table: "EscalationMatrices",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationMatrices_CompanyId",
                table: "EscalationMatrices",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationMatrices_CompanyId_IsActive_Priority",
                table: "EscalationMatrices",
                columns: new[] { "CompanyId", "IsActive", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_EscalationMatrices_DepartmentId",
                table: "EscalationMatrices",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationMatrices_IsActive",
                table: "EscalationMatrices",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EscalationHistories");

            migrationBuilder.DropTable(
                name: "EscalationLevels");

            migrationBuilder.DropTable(
                name: "EscalationMatrices");
        }
    }
}
