namespace ComplaintManagement.API.Authorization;

/// <summary>
/// Marks an action to skip license module checking.
/// Useful for lookup endpoints that need to be accessible from related modules.
/// The endpoint still requires authentication (unless [AllowAnonymous] is also applied).
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class SkipLicenseCheckAttribute : Attribute
{
}
