using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCountsToSyncLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UsersCreated",
                table: "SyncLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsersProcessed",
                table: "SyncLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsersUpdated",
                table: "SyncLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsersCreated",
                table: "SyncLogs");

            migrationBuilder.DropColumn(
                name: "UsersProcessed",
                table: "SyncLogs");

            migrationBuilder.DropColumn(
                name: "UsersUpdated",
                table: "SyncLogs");
        }
    }
}
