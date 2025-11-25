using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase1_OrganizationalHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "EscalationLevels",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "EscalationLevels",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HrContactId",
                table: "EscalationLevels",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PrimaryContactId",
                table: "EscalationLevels",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResourcePoolAssignmentMethod",
                table: "EscalationLevels",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ResourcePoolId",
                table: "EscalationLevels",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SecondaryContactId",
                table: "EscalationLevels",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ResourcePoolId",
                table: "Complaints",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ResourcePools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PoolType = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_ResourcePools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourcePools_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ResourcePools_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResourcePools_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ResourcePools_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ResourcePoolMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourcePoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AddedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_ResourcePoolMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourcePoolMembers_ResourcePools_ResourcePoolId",
                        column: x => x.ResourcePoolId,
                        principalTable: "ResourcePools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResourcePoolMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EscalationLevels_BranchId",
                table: "EscalationLevels",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationLevels_DepartmentId",
                table: "EscalationLevels",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationLevels_HrContactId",
                table: "EscalationLevels",
                column: "HrContactId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationLevels_PrimaryContactId",
                table: "EscalationLevels",
                column: "PrimaryContactId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationLevels_ResourcePoolId",
                table: "EscalationLevels",
                column: "ResourcePoolId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationLevels_SecondaryContactId",
                table: "EscalationLevels",
                column: "SecondaryContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_ResourcePoolId",
                table: "Complaints",
                column: "ResourcePoolId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourcePoolMembers_ResourcePoolId",
                table: "ResourcePoolMembers",
                column: "ResourcePoolId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourcePoolMembers_UserId",
                table: "ResourcePoolMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourcePools_BranchId",
                table: "ResourcePools",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourcePools_CompanyId",
                table: "ResourcePools",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourcePools_DepartmentId",
                table: "ResourcePools",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourcePools_SectionId",
                table: "ResourcePools",
                column: "SectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_ResourcePools_ResourcePoolId",
                table: "Complaints",
                column: "ResourcePoolId",
                principalTable: "ResourcePools",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EscalationLevels_Branches_BranchId",
                table: "EscalationLevels",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EscalationLevels_Departments_DepartmentId",
                table: "EscalationLevels",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EscalationLevels_ResourcePools_ResourcePoolId",
                table: "EscalationLevels",
                column: "ResourcePoolId",
                principalTable: "ResourcePools",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EscalationLevels_Users_HrContactId",
                table: "EscalationLevels",
                column: "HrContactId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EscalationLevels_Users_PrimaryContactId",
                table: "EscalationLevels",
                column: "PrimaryContactId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EscalationLevels_Users_SecondaryContactId",
                table: "EscalationLevels",
                column: "SecondaryContactId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_ResourcePools_ResourcePoolId",
                table: "Complaints");

            migrationBuilder.DropForeignKey(
                name: "FK_EscalationLevels_Branches_BranchId",
                table: "EscalationLevels");

            migrationBuilder.DropForeignKey(
                name: "FK_EscalationLevels_Departments_DepartmentId",
                table: "EscalationLevels");

            migrationBuilder.DropForeignKey(
                name: "FK_EscalationLevels_ResourcePools_ResourcePoolId",
                table: "EscalationLevels");

            migrationBuilder.DropForeignKey(
                name: "FK_EscalationLevels_Users_HrContactId",
                table: "EscalationLevels");

            migrationBuilder.DropForeignKey(
                name: "FK_EscalationLevels_Users_PrimaryContactId",
                table: "EscalationLevels");

            migrationBuilder.DropForeignKey(
                name: "FK_EscalationLevels_Users_SecondaryContactId",
                table: "EscalationLevels");

            migrationBuilder.DropTable(
                name: "ResourcePoolMembers");

            migrationBuilder.DropTable(
                name: "ResourcePools");

            migrationBuilder.DropIndex(
                name: "IX_EscalationLevels_BranchId",
                table: "EscalationLevels");

            migrationBuilder.DropIndex(
                name: "IX_EscalationLevels_DepartmentId",
                table: "EscalationLevels");

            migrationBuilder.DropIndex(
                name: "IX_EscalationLevels_HrContactId",
                table: "EscalationLevels");

            migrationBuilder.DropIndex(
                name: "IX_EscalationLevels_PrimaryContactId",
                table: "EscalationLevels");

            migrationBuilder.DropIndex(
                name: "IX_EscalationLevels_ResourcePoolId",
                table: "EscalationLevels");

            migrationBuilder.DropIndex(
                name: "IX_EscalationLevels_SecondaryContactId",
                table: "EscalationLevels");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_ResourcePoolId",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "EscalationLevels");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "EscalationLevels");

            migrationBuilder.DropColumn(
                name: "HrContactId",
                table: "EscalationLevels");

            migrationBuilder.DropColumn(
                name: "PrimaryContactId",
                table: "EscalationLevels");

            migrationBuilder.DropColumn(
                name: "ResourcePoolAssignmentMethod",
                table: "EscalationLevels");

            migrationBuilder.DropColumn(
                name: "ResourcePoolId",
                table: "EscalationLevels");

            migrationBuilder.DropColumn(
                name: "SecondaryContactId",
                table: "EscalationLevels");

            migrationBuilder.DropColumn(
                name: "ResourcePoolId",
                table: "Complaints");
        }
    }
}
