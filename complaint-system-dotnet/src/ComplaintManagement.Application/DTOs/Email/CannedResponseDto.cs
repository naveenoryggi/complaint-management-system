namespace ComplaintManagement.Application.DTOs.Email;

/// <summary>
/// Canned response (quick reply template)
/// </summary>
public class CannedResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ShortCode { get; set; }
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
}
