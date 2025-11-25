using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddComplaintInformationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComplaintInformationSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShowEmployeeCodeToHandlers = table.Column<bool>(type: "bit", nullable: false),
                    ShowEmailToHandlers = table.Column<bool>(type: "bit", nullable: false),
                    ShowPhoneToHandlers = table.Column<bool>(type: "bit", nullable: false),
                    ShowAlternatePhoneToHandlers = table.Column<bool>(type: "bit", nullable: false),
                    ShowCompanyToHandlers = table.Column<bool>(type: "bit", nullable: false),
                    ShowBranchToHandlers = table.Column<bool>(type: "bit", nullable: false),
                    ShowDepartmentToHandlers = table.Column<bool>(type: "bit", nullable: false),
                    ShowSectionToHandlers = table.Column<bool>(type: "bit", nullable: false),
                    ShowJobTitleToHandlers = table.Column<bool>(type: "bit", nullable: false),
                    ShowManagerDetailsToHandlers = table.Column<bool>(type: "bit", nullable: false),
                    ShowDateOfJoiningToHandlers = table.Column<bool>(type: "bit", nullable: false),
                    ShowPreviousComplaintsToHandlers = table.Column<bool>(type: "bit", nullable: false),
                    ShowEmployeeAddressToManagement = table.Column<bool>(type: "bit", nullable: false),
                    ShowEmergencyContactToManagement = table.Column<bool>(type: "bit", nullable: false),
                    ShowPerformanceMetricsToManagement = table.Column<bool>(type: "bit", nullable: false),
                    MaskPersonalInfoInLogs = table.Column<bool>(type: "bit", nullable: false),
                    RedactInfoAfterClosure = table.Column<bool>(type: "bit", nullable: false),
                    DataRetentionDays = table.Column<int>(type: "int", nullable: false),
                    IncludeEmployeeCodeInReports = table.Column<bool>(type: "bit", nullable: false),
                    IncludeEmailInReports = table.Column<bool>(type: "bit", nullable: false),
                    IncludePhoneInReports = table.Column<bool>(type: "bit", nullable: false),
                    MaskEmailInReports = table.Column<bool>(type: "bit", nullable: false),
                    MaskPhoneInReports = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_ComplaintInformationSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintInformationSettings_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintInformationSettings_CompanyId",
                table: "ComplaintInformationSettings",
                column: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComplaintInformationSettings");
        }
    }
}
