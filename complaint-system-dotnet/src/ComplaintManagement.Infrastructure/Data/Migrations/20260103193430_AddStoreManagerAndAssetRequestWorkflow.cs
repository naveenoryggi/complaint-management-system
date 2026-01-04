using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreManagerAndAssetRequestWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Stores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PrimaryManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SecondaryManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ApprovalTimeoutHours = table.Column<int>(type: "int", nullable: false, defaultValue: 48),
                    AutoEscalateToSecondary = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RequireDeptApprovalForAssignment = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_Stores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stores_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Stores_Users_PrimaryManagerId",
                        column: x => x.PrimaryManagerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Stores_Users_SecondaryManagerId",
                        column: x => x.SecondaryManagerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AssetRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RequestType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    AssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetAssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RequestedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignToUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssignToCustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReturnReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CurrentApproverId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovalDeadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EscalationLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Purpose = table.Column<int>(type: "int", nullable: true),
                    ExpectedReturnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TransferToStoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TransferToUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_AssetRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetRequests_AssetAssignments_AssetAssignmentId",
                        column: x => x.AssetAssignmentId,
                        principalTable: "AssetAssignments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssetRequests_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetRequests_Customers_AssignToCustomerId",
                        column: x => x.AssignToCustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssetRequests_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssetRequests_Stores_TransferToStoreId",
                        column: x => x.TransferToStoreId,
                        principalTable: "Stores",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssetRequests_Users_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssetRequests_Users_AssignToUserId",
                        column: x => x.AssignToUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssetRequests_Users_CurrentApproverId",
                        column: x => x.CurrentApproverId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssetRequests_Users_RejectedById",
                        column: x => x.RejectedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssetRequests_Users_RequestedById",
                        column: x => x.RequestedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetRequests_Users_TransferToUserId",
                        column: x => x.TransferToUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StoreUserRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_StoreUserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoreUserRoles_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StoreUserRoles_Users_AssignedById",
                        column: x => x.AssignedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StoreUserRoles_Users_RevokedById",
                        column: x => x.RevokedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StoreUserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RequestApprovalHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    ActionAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    EscalationLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    PreviousStatus = table.Column<int>(type: "int", nullable: true),
                    NewStatus = table.Column<int>(type: "int", nullable: true),
                    PreviousApproverId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NewApproverId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_RequestApprovalHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestApprovalHistory_AssetRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "AssetRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestApprovalHistory_Users_ActorId",
                        column: x => x.ActorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RequestApprovalHistory_Users_NewApproverId",
                        column: x => x.NewApproverId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RequestApprovalHistory_Users_PreviousApproverId",
                        column: x => x.PreviousApproverId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 19, 34, 27, 661, DateTimeKind.Utc).AddTicks(6808), new DateTime(2026, 1, 3, 19, 34, 27, 661, DateTimeKind.Utc).AddTicks(6945) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 19, 34, 27, 661, DateTimeKind.Utc).AddTicks(7233), new DateTime(2026, 1, 3, 19, 34, 27, 661, DateTimeKind.Utc).AddTicks(7233) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 19, 34, 27, 661, DateTimeKind.Utc).AddTicks(7236), new DateTime(2026, 1, 3, 19, 34, 27, 661, DateTimeKind.Utc).AddTicks(7237) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 19, 34, 27, 661, DateTimeKind.Utc).AddTicks(7240), new DateTime(2026, 1, 3, 19, 34, 27, 661, DateTimeKind.Utc).AddTicks(7240) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 19, 34, 27, 661, DateTimeKind.Utc).AddTicks(7243), new DateTime(2026, 1, 3, 19, 34, 27, 661, DateTimeKind.Utc).AddTicks(7243) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 19, 34, 27, 664, DateTimeKind.Utc).AddTicks(680), new DateTime(2026, 1, 3, 19, 34, 27, 664, DateTimeKind.Utc).AddTicks(682) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 19, 34, 27, 664, DateTimeKind.Utc).AddTicks(689), new DateTime(2026, 1, 3, 19, 34, 27, 664, DateTimeKind.Utc).AddTicks(689) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 19, 34, 27, 664, DateTimeKind.Utc).AddTicks(692), new DateTime(2026, 1, 3, 19, 34, 27, 664, DateTimeKind.Utc).AddTicks(692) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 19, 34, 27, 664, DateTimeKind.Utc).AddTicks(695), new DateTime(2026, 1, 3, 19, 34, 27, 664, DateTimeKind.Utc).AddTicks(696) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 19, 34, 27, 664, DateTimeKind.Utc).AddTicks(698), new DateTime(2026, 1, 3, 19, 34, 27, 664, DateTimeKind.Utc).AddTicks(699) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 19, 34, 27, 664, DateTimeKind.Utc).AddTicks(701), new DateTime(2026, 1, 3, 19, 34, 27, 664, DateTimeKind.Utc).AddTicks(702) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 19, 34, 27, 664, DateTimeKind.Utc).AddTicks(705), new DateTime(2026, 1, 3, 19, 34, 27, 664, DateTimeKind.Utc).AddTicks(705) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 19, 34, 27, 664, DateTimeKind.Utc).AddTicks(708), new DateTime(2026, 1, 3, 19, 34, 27, 664, DateTimeKind.Utc).AddTicks(709) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 19, 34, 27, 664, DateTimeKind.Utc).AddTicks(755), new DateTime(2026, 1, 3, 19, 34, 27, 664, DateTimeKind.Utc).AddTicks(755) });

            migrationBuilder.CreateIndex(
                name: "IX_AssetRequests_ApprovalDeadline",
                table: "AssetRequests",
                column: "ApprovalDeadline");

            migrationBuilder.CreateIndex(
                name: "IX_AssetRequests_ApprovedById",
                table: "AssetRequests",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_AssetRequests_AssetAssignmentId",
                table: "AssetRequests",
                column: "AssetAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetRequests_AssetId",
                table: "AssetRequests",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetRequests_AssignToCustomerId",
                table: "AssetRequests",
                column: "AssignToCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetRequests_AssignToUserId",
                table: "AssetRequests",
                column: "AssignToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetRequests_CompanyId_RequestNumber",
                table: "AssetRequests",
                columns: new[] { "CompanyId", "RequestNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetRequests_CompanyId_Status",
                table: "AssetRequests",
                columns: new[] { "CompanyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AssetRequests_CurrentApproverId_Status",
                table: "AssetRequests",
                columns: new[] { "CurrentApproverId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AssetRequests_RejectedById",
                table: "AssetRequests",
                column: "RejectedById");

            migrationBuilder.CreateIndex(
                name: "IX_AssetRequests_RequestedById",
                table: "AssetRequests",
                column: "RequestedById");

            migrationBuilder.CreateIndex(
                name: "IX_AssetRequests_StoreId",
                table: "AssetRequests",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetRequests_TransferToStoreId",
                table: "AssetRequests",
                column: "TransferToStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetRequests_TransferToUserId",
                table: "AssetRequests",
                column: "TransferToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestApprovalHistory_ActorId_ActionAt",
                table: "RequestApprovalHistory",
                columns: new[] { "ActorId", "ActionAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RequestApprovalHistory_NewApproverId",
                table: "RequestApprovalHistory",
                column: "NewApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestApprovalHistory_PreviousApproverId",
                table: "RequestApprovalHistory",
                column: "PreviousApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestApprovalHistory_RequestId",
                table: "RequestApprovalHistory",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_BranchId",
                table: "Stores",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_CompanyId_Code",
                table: "Stores",
                columns: new[] { "CompanyId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stores_PrimaryManagerId",
                table: "Stores",
                column: "PrimaryManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_SecondaryManagerId",
                table: "Stores",
                column: "SecondaryManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreUserRoles_AssignedById",
                table: "StoreUserRoles",
                column: "AssignedById");

            migrationBuilder.CreateIndex(
                name: "IX_StoreUserRoles_RevokedById",
                table: "StoreUserRoles",
                column: "RevokedById");

            migrationBuilder.CreateIndex(
                name: "IX_StoreUserRoles_StoreId",
                table: "StoreUserRoles",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreUserRoles_StoreId_UserId_Role_Active",
                table: "StoreUserRoles",
                columns: new[] { "StoreId", "UserId", "Role" },
                unique: true,
                filter: "[IsActive] = 1 AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_StoreUserRoles_UserId",
                table: "StoreUserRoles",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestApprovalHistory");

            migrationBuilder.DropTable(
                name: "StoreUserRoles");

            migrationBuilder.DropTable(
                name: "AssetRequests");

            migrationBuilder.DropTable(
                name: "Stores");

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 16, 39, 22, 539, DateTimeKind.Utc).AddTicks(7141), new DateTime(2026, 1, 3, 16, 39, 22, 539, DateTimeKind.Utc).AddTicks(7268) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 16, 39, 22, 539, DateTimeKind.Utc).AddTicks(7489), new DateTime(2026, 1, 3, 16, 39, 22, 539, DateTimeKind.Utc).AddTicks(7490) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 16, 39, 22, 539, DateTimeKind.Utc).AddTicks(7492), new DateTime(2026, 1, 3, 16, 39, 22, 539, DateTimeKind.Utc).AddTicks(7492) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 16, 39, 22, 539, DateTimeKind.Utc).AddTicks(7494), new DateTime(2026, 1, 3, 16, 39, 22, 539, DateTimeKind.Utc).AddTicks(7494) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 16, 39, 22, 539, DateTimeKind.Utc).AddTicks(7496), new DateTime(2026, 1, 3, 16, 39, 22, 539, DateTimeKind.Utc).AddTicks(7497) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6120), new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6122) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6127), new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6127) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6129), new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6130) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6132), new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6132) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6134), new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6135) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6137), new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6137) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6139), new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6140) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6142), new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6142) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6144), new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6144) });
        }
    }
}
