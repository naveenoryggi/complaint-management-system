using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchManagers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "HrResponsibleId",
                table: "Branches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ManagerId",
                table: "Branches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SecondaryManagerId",
                table: "Branches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Branches_HrResponsibleId",
                table: "Branches",
                column: "HrResponsibleId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_ManagerId",
                table: "Branches",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_SecondaryManagerId",
                table: "Branches",
                column: "SecondaryManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_Users_HrResponsibleId",
                table: "Branches",
                column: "HrResponsibleId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_Users_ManagerId",
                table: "Branches",
                column: "ManagerId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_Users_SecondaryManagerId",
                table: "Branches",
                column: "SecondaryManagerId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Branches_Users_HrResponsibleId",
                table: "Branches");

            migrationBuilder.DropForeignKey(
                name: "FK_Branches_Users_ManagerId",
                table: "Branches");

            migrationBuilder.DropForeignKey(
                name: "FK_Branches_Users_SecondaryManagerId",
                table: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_Branches_HrResponsibleId",
                table: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_Branches_ManagerId",
                table: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_Branches_SecondaryManagerId",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "HrResponsibleId",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "SecondaryManagerId",
                table: "Branches");
        }
    }
}
