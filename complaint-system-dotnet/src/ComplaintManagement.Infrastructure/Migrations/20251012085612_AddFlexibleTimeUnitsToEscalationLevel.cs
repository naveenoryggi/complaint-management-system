using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFlexibleTimeUnitsToEscalationLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TriggerAfterValue",
                table: "EscalationLevels",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TriggerTimeUnit",
                table: "EscalationLevels",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TriggerAfterValue",
                table: "EscalationLevels");

            migrationBuilder.DropColumn(
                name: "TriggerTimeUnit",
                table: "EscalationLevels");
        }
    }
}
