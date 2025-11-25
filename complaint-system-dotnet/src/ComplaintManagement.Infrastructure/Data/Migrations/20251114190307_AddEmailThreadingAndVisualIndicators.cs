using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailThreadingAndVisualIndicators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BccRecipientsJson",
                table: "EmailMessages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CcRecipientsJson",
                table: "EmailMessages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadAt",
                table: "EmailMessages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReadBy",
                table: "EmailMessages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReadByUserId",
                table: "EmailMessages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToRecipientsJson",
                table: "EmailMessages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CannedResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShortCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UsageCount = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CannedResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CannedResponses_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CannedResponses_ComplaintCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ComplaintCategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CannedResponses_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComplaintEmailParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComplaintId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParticipantType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AddedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_ComplaintEmailParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintEmailParticipants_Complaints_ComplaintId",
                        column: x => x.ComplaintId,
                        principalTable: "Complaints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComplaintEmailParticipants_Users_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 19, 3, 3, 762, DateTimeKind.Utc).AddTicks(8181), new DateTime(2025, 11, 14, 19, 3, 3, 762, DateTimeKind.Utc).AddTicks(8546) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 19, 3, 3, 762, DateTimeKind.Utc).AddTicks(9121), new DateTime(2025, 11, 14, 19, 3, 3, 762, DateTimeKind.Utc).AddTicks(9123) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 19, 3, 3, 762, DateTimeKind.Utc).AddTicks(9130), new DateTime(2025, 11, 14, 19, 3, 3, 762, DateTimeKind.Utc).AddTicks(9130) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 19, 3, 3, 762, DateTimeKind.Utc).AddTicks(9135), new DateTime(2025, 11, 14, 19, 3, 3, 762, DateTimeKind.Utc).AddTicks(9136) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 19, 3, 3, 762, DateTimeKind.Utc).AddTicks(9140), new DateTime(2025, 11, 14, 19, 3, 3, 762, DateTimeKind.Utc).AddTicks(9140) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4262), new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4268) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4288), new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4289) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4294), new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4294) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4299), new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4299) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4303), new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4304) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4309), new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4310) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4314), new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4314) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4318), new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4319) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4323), new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4323) });

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_ReadByUserId",
                table: "EmailMessages",
                column: "ReadByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CannedResponses_CategoryId",
                table: "CannedResponses",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CannedResponses_CompanyId",
                table: "CannedResponses",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CannedResponses_CreatedByUserId",
                table: "CannedResponses",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintEmailParticipants_AddedByUserId",
                table: "ComplaintEmailParticipants",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintEmailParticipants_ComplaintId",
                table: "ComplaintEmailParticipants",
                column: "ComplaintId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailMessages_Users_ReadByUserId",
                table: "EmailMessages",
                column: "ReadByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailMessages_Users_ReadByUserId",
                table: "EmailMessages");

            migrationBuilder.DropTable(
                name: "CannedResponses");

            migrationBuilder.DropTable(
                name: "ComplaintEmailParticipants");

            migrationBuilder.DropIndex(
                name: "IX_EmailMessages_ReadByUserId",
                table: "EmailMessages");

            migrationBuilder.DropColumn(
                name: "BccRecipientsJson",
                table: "EmailMessages");

            migrationBuilder.DropColumn(
                name: "CcRecipientsJson",
                table: "EmailMessages");

            migrationBuilder.DropColumn(
                name: "ReadAt",
                table: "EmailMessages");

            migrationBuilder.DropColumn(
                name: "ReadBy",
                table: "EmailMessages");

            migrationBuilder.DropColumn(
                name: "ReadByUserId",
                table: "EmailMessages");

            migrationBuilder.DropColumn(
                name: "ToRecipientsJson",
                table: "EmailMessages");

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 13, 5, 13, 819, DateTimeKind.Utc).AddTicks(9297), new DateTime(2025, 11, 14, 13, 5, 13, 819, DateTimeKind.Utc).AddTicks(9416) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 13, 5, 13, 819, DateTimeKind.Utc).AddTicks(9628), new DateTime(2025, 11, 14, 13, 5, 13, 819, DateTimeKind.Utc).AddTicks(9629) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 13, 5, 13, 819, DateTimeKind.Utc).AddTicks(9633), new DateTime(2025, 11, 14, 13, 5, 13, 819, DateTimeKind.Utc).AddTicks(9633) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 13, 5, 13, 819, DateTimeKind.Utc).AddTicks(9636), new DateTime(2025, 11, 14, 13, 5, 13, 819, DateTimeKind.Utc).AddTicks(9637) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 13, 5, 13, 819, DateTimeKind.Utc).AddTicks(9640), new DateTime(2025, 11, 14, 13, 5, 13, 819, DateTimeKind.Utc).AddTicks(9640) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7758), new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7760) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7766), new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7767) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7769), new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7770) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7772), new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7772) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7776), new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7776) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7778), new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7779) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7781), new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7781) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7783), new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7784) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7787), new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7787) });
        }
    }
}
