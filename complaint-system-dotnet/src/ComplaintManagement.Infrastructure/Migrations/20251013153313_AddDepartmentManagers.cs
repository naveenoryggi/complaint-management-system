using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentManagers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Users_ManagerId",
                table: "Departments");

            migrationBuilder.AddColumn<Guid>(
                name: "HrResponsibleId",
                table: "Departments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SecondaryManagerId",
                table: "Departments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_HrResponsibleId",
                table: "Departments",
                column: "HrResponsibleId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_SecondaryManagerId",
                table: "Departments",
                column: "SecondaryManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Users_HrResponsibleId",
                table: "Departments",
                column: "HrResponsibleId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Users_ManagerId",
                table: "Departments",
                column: "ManagerId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Users_SecondaryManagerId",
                table: "Departments",
                column: "SecondaryManagerId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Users_HrResponsibleId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Users_ManagerId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Users_SecondaryManagerId",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_HrResponsibleId",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_SecondaryManagerId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "HrResponsibleId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "SecondaryManagerId",
                table: "Departments");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Users_ManagerId",
                table: "Departments",
                column: "ManagerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
