using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserContactsToCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "HrResponsibleId",
                table: "Companies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ManagerId",
                table: "Companies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SecondaryManagerId",
                table: "Companies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_HrResponsibleId",
                table: "Companies",
                column: "HrResponsibleId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_ManagerId",
                table: "Companies",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_SecondaryManagerId",
                table: "Companies",
                column: "SecondaryManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_Users_HrResponsibleId",
                table: "Companies",
                column: "HrResponsibleId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_Users_ManagerId",
                table: "Companies",
                column: "ManagerId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_Users_SecondaryManagerId",
                table: "Companies",
                column: "SecondaryManagerId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_Users_HrResponsibleId",
                table: "Companies");

            migrationBuilder.DropForeignKey(
                name: "FK_Companies_Users_ManagerId",
                table: "Companies");

            migrationBuilder.DropForeignKey(
                name: "FK_Companies_Users_SecondaryManagerId",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_HrResponsibleId",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_ManagerId",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_SecondaryManagerId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "HrResponsibleId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "SecondaryManagerId",
                table: "Companies");
        }
    }
}
