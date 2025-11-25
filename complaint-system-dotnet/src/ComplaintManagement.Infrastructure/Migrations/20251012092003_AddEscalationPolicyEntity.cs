using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEscalationPolicyEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EscalationPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EnableAutoEscalation = table.Column<bool>(type: "bit", nullable: false),
                    RequireManualApproval = table.Column<bool>(type: "bit", nullable: false),
                    DefaultEscalationMatrixId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MinimumSeverityForAutoEscalation = table.Column<int>(type: "int", nullable: true),
                    MaxAutoEscalationLevels = table.Column<int>(type: "int", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_EscalationPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EscalationPolicies_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EscalationPolicies_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EscalationPolicies_ComplaintCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ComplaintCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EscalationPolicies_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EscalationPolicies_EscalationMatrices_DefaultEscalationMatrixId",
                        column: x => x.DefaultEscalationMatrixId,
                        principalTable: "EscalationMatrices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EscalationPolicies_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EscalationPolicies_BranchId",
                table: "EscalationPolicies",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationPolicies_CategoryId",
                table: "EscalationPolicies",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationPolicies_CompanyId_BranchId_IsActive",
                table: "EscalationPolicies",
                columns: new[] { "CompanyId", "BranchId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_EscalationPolicies_CompanyId_CategoryId_IsActive",
                table: "EscalationPolicies",
                columns: new[] { "CompanyId", "CategoryId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_EscalationPolicies_CompanyId_DepartmentId_IsActive",
                table: "EscalationPolicies",
                columns: new[] { "CompanyId", "DepartmentId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_EscalationPolicies_CompanyId_IsActive",
                table: "EscalationPolicies",
                columns: new[] { "CompanyId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_EscalationPolicies_CompanyId_SectionId_IsActive",
                table: "EscalationPolicies",
                columns: new[] { "CompanyId", "SectionId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_EscalationPolicies_DefaultEscalationMatrixId",
                table: "EscalationPolicies",
                column: "DefaultEscalationMatrixId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationPolicies_DepartmentId",
                table: "EscalationPolicies",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationPolicies_FullHierarchy",
                table: "EscalationPolicies",
                columns: new[] { "CompanyId", "BranchId", "DepartmentId", "SectionId", "CategoryId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_EscalationPolicies_SectionId",
                table: "EscalationPolicies",
                column: "SectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EscalationPolicies");
        }
    }
}
