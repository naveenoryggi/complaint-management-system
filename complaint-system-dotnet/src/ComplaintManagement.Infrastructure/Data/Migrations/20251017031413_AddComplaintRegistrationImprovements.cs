using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddComplaintRegistrationImprovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlternatePhone",
                table: "Complaints",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Complaints",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "Complaints",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeCode",
                table: "Complaints",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreferredContactMethod",
                table: "Complaints",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "SectionId",
                table: "Complaints",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_SectionId",
                table: "Complaints",
                column: "SectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_Sections_SectionId",
                table: "Complaints",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_Sections_SectionId",
                table: "Complaints");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_SectionId",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "AlternatePhone",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "EmployeeCode",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "PreferredContactMethod",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "SectionId",
                table: "Complaints");
        }
    }
}
