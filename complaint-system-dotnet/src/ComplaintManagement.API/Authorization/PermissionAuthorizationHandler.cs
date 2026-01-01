using Microsoft.AspNetCore.Authorization;

namespace ComplaintManagement.API.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // System Admin bypass - has all permissions
        var roles = context.User.Claims
            .Where(c => c.Type == "RoleCode" || c.Type == System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        if (roles.Any(r => r == "SYSTEM_ADMIN" || r == "ADMIN"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Get all permission claims from the user
        var permissions = context.User.Claims
            .Where(c => c.Type == "Permission")
            .Select(c => c.Value)
            .ToList();

        // Check if the user has the required permission
        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
