using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSectionManagers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sections_Users_HeadId",
                table: "Sections");

            migrationBuilder.AddColumn<Guid>(
                name: "HrResponsibleId",
                table: "Sections",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SecondaryHeadId",
                table: "Sections",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sections_HrResponsibleId",
                table: "Sections",
                column: "HrResponsibleId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_SecondaryHeadId",
                table: "Sections",
                column: "SecondaryHeadId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_Users_HeadId",
                table: "Sections",
                column: "HeadId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_Users_HrResponsibleId",
                table: "Sections",
                column: "HrResponsibleId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_Users_SecondaryHeadId",
                table: "Sections",
                column: "SecondaryHeadId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sections_Users_HeadId",
                table: "Sections");

            migrationBuilder.DropForeignKey(
                name: "FK_Sections_Users_HrResponsibleId",
                table: "Sections");

            migrationBuilder.DropForeignKey(
                name: "FK_Sections_Users_SecondaryHeadId",
                table: "Sections");

            migrationBuilder.DropIndex(
                name: "IX_Sections_HrResponsibleId",
                table: "Sections");

            migrationBuilder.DropIndex(
                name: "IX_Sections_SecondaryHeadId",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "HrResponsibleId",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "SecondaryHeadId",
                table: "Sections");

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_Users_HeadId",
                table: "Sections",
                column: "HeadId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
