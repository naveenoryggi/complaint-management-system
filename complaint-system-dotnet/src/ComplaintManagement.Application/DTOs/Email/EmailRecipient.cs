namespace ComplaintManagement.Application.DTOs.Email;

/// <summary>
/// Email recipient with display name and email address
/// </summary>
public class EmailRecipient
{
    public string EmailAddress { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
}
