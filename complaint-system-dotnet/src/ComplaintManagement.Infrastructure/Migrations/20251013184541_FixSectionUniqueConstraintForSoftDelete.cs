using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixSectionUniqueConstraintForSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sections_DepartmentId_Code",
                table: "Sections");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_DepartmentId_Code",
                table: "Sections",
                columns: new[] { "DepartmentId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sections_DepartmentId_Code",
                table: "Sections");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_DepartmentId_Code",
                table: "Sections",
                columns: new[] { "DepartmentId", "Code" },
                unique: true);
        }
    }
}
