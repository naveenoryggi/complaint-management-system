using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEscalationPermissionsToExistingRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add ViewEscalation and ManageEscalation permissions to System Admin role
            migrationBuilder.Sql(@"
                INSERT INTO ComplaintRolePermissions (Id, ComplaintRoleId, PermissionType, IsGranted, CreatedAt, IsDeleted)
                SELECT NEWID(), cr.Id, 'ViewEscalation', 1, GETUTCDATE(), 0
                FROM ComplaintRoles cr
                WHERE cr.Code = 'SYSTEM_ADMIN'
                AND cr.IsDeleted = 0
                AND NOT EXISTS (
                    SELECT 1 FROM ComplaintRolePermissions crp
                    WHERE crp.ComplaintRoleId = cr.Id AND crp.PermissionType = 'ViewEscalation' AND crp.IsDeleted = 0
                );

                INSERT INTO ComplaintRolePermissions (Id, ComplaintRoleId, PermissionType, IsGranted, CreatedAt, IsDeleted)
                SELECT NEWID(), cr.Id, 'ManageEscalation', 1, GETUTCDATE(), 0
                FROM ComplaintRoles cr
                WHERE cr.Code = 'SYSTEM_ADMIN'
                AND cr.IsDeleted = 0
                AND NOT EXISTS (
                    SELECT 1 FROM ComplaintRolePermissions crp
                    WHERE crp.ComplaintRoleId = cr.Id AND crp.PermissionType = 'ManageEscalation' AND crp.IsDeleted = 0
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove ViewEscalation and ManageEscalation permissions from System Admin role
            migrationBuilder.Sql(@"
                DELETE FROM ComplaintRolePermissions
                WHERE PermissionType IN ('ViewEscalation', 'ManageEscalation')
                AND ComplaintRoleId IN (
                    SELECT Id FROM ComplaintRoles WHERE Code = 'SYSTEM_ADMIN' AND IsDeleted = 0
                );
            ");
        }
    }
}
