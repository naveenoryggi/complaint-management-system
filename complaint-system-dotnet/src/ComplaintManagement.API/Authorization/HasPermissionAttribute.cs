using Microsoft.AspNetCore.Authorization;

namespace ComplaintManagement.API.Authorization;

/// <summary>
/// Attribute for requiring specific permissions on controller actions
/// </summary>
public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission)
    {
        Policy = $"Permission:{permission}";
    }
}
