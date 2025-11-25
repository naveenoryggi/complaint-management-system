using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Communication;
using ComplaintManagement.Domain.Entities.Complaints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComplaintManagement.API.Controllers;

/// <summary>
/// Controller for email ticketing operations - sending replies, viewing email threads, etc.
/// </summary>
[ApiController]
[Route("api/email-ticketing")]
[Authorize]
public class EmailTicketingController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailTicketingService _emailTicketingService;
    private readonly ILogger<EmailTicketingController> _logger;

    public EmailTicketingController(
        IUnitOfWork unitOfWork,
        IEmailTicketingService emailTicketingService,
        ILogger<EmailTicketingController> logger)
    {
        _unitOfWork = unitOfWork;
        _emailTicketingService = emailTicketingService;
        _logger = logger;
    }

    /// <summary>
    /// Helper method to parse email recipients from comma-separated strings
    /// </summary>
    private List<EmailRecipientDto> ParseEmailRecipients(string? emails, string? name)
    {
        if (string.IsNullOrWhiteSpace(emails))
            return new List<EmailRecipientDto>();

        return emails.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(email => new EmailRecipientDto
            {
                EmailAddress = email,
                DisplayName = name
            })
            .ToList();
    }

    /// <summary>
    /// Get all email messages for a specific complaint/ticket
    /// </summary>
    [HttpGet("complaint/{complaintId}/emails")]
    [ProducesResponseType(typeof(Result<IEnumerable<EmailMessage>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetComplaintEmails(Guid complaintId)
    {
        try
        {
            // SECURITY: Get current user ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid currentUserId))
            {
                return Unauthorized(Result.Failure("User information not found"));
            }

            // SECURITY: Get user's company
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out Guid companyId))
            {
                return Unauthorized(Result.Failure("Company information not found"));
            }

            // Verify complaint exists and belongs to user's company
            var complaint = await _unitOfWork.Repository<Complaint>().GetByIdAsync(complaintId);

            if (complaint == null)
            {
                return NotFound(Result.Failure("Complaint not found"));
            }

            if (complaint.CompanyId != companyId)
            {
                _logger.LogWarning("User {UserId} attempted to access emails for complaint {ComplaintId} from different company",
                    currentUserId, complaintId);
                return Forbid();
            }

            // SECURITY: Check if user has permission to view this complaint
            var permissions = User.FindAll("Permission").Select(c => c.Value).ToList();
            bool isAdmin = permissions.Contains("ManageUsers") || permissions.Contains("ManageSettings");
            bool isHandler = permissions.Contains("ViewComplaints") || permissions.Contains("AssignComplaint");
            bool isComplainant = complaint.ComplainantId == currentUserId;

            if (!isAdmin && !isHandler && !isComplainant)
            {
                _logger.LogWarning("User {UserId} attempted to access emails for complaint {ComplaintId} without permission",
                    currentUserId, complaintId);
                return Forbid();
            }

            // Get all email messages for this complaint, ordered by received date
            var emails = await _unitOfWork.Repository<EmailMessage>()
                .FindAsync(e => e.ComplaintId == complaintId, CancellationToken.None);

            var orderedEmails = emails.OrderBy(e => e.ReceivedAt).ToList();

            // SECURITY: Filter internal notes if user is not admin/handler
            if (!isAdmin && !isHandler)
            {
                orderedEmails = orderedEmails.Where(e => !e.IsInternal).ToList();
            }

            // Transform to DTOs with isOutbound boolean property for frontend compatibility
            var emailDtos = orderedEmails.Select(e => new EmailThreadItemDto
            {
                Id = e.Id,
                MessageId = e.MessageId,
                InReplyTo = e.InReplyTo,
                References = e.References,
                Subject = e.Subject,
                FromEmail = e.FromEmail,
                FromName = e.FromName,
                ToRecipients = ParseEmailRecipients(e.ToEmail, e.ToName),
                CcRecipients = ParseEmailRecipients(e.CcEmails, null),
                BccRecipients = ParseEmailRecipients(e.BccEmails, null),
                TextBody = e.TextBody,
                HtmlBody = e.HtmlBody,
                IsOutbound = e.Direction == EmailDirection.Outbound, // Convert enum to boolean
                ReceivedAt = e.ReceivedAt,
                SentAt = e.SentAt, // For outbound emails
                IsRead = e.IsRead,
                IsPrivateNote = e.IsInternal,
                SentByUserId = e.SentByUserId, // User who sent outbound email
                AttachmentCount = 0, // Will be populated separately if needed
                ThreadId = e.ThreadId
            }).ToList();

            _logger.LogInformation("Retrieved {Count} email messages for complaint {ComplaintId}",
                emailDtos.Count, complaintId);

            return Ok(Result<IEnumerable<EmailThreadItemDto>>.Success(emailDtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving emails for complaint {ComplaintId}", complaintId);
            return StatusCode(500, Result.Failure("An error occurred while retrieving email messages"));
        }
    }

    /// <summary>
    /// Get a specific email message by ID
    /// </summary>
    [HttpGet("email/{emailId}")]
    [ProducesResponseType(typeof(Result<EmailMessage>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEmailMessage(Guid emailId)
    {
        try
        {
            // SECURITY: Get current user ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid currentUserId))
            {
                return Unauthorized(Result.Failure("User information not found"));
            }

            // SECURITY: Get user's company
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out Guid companyId))
            {
                return Unauthorized(Result.Failure("Company information not found"));
            }

            var email = await _unitOfWork.Repository<EmailMessage>().GetByIdAsync(emailId);

            if (email == null)
            {
                return NotFound(Result.Failure("Email message not found"));
            }

            // Verify email's complaint belongs to user's company
            if (email.ComplaintId.HasValue)
            {
                var complaint = await _unitOfWork.Repository<Complaint>().GetByIdAsync(email.ComplaintId.Value);
                if (complaint?.CompanyId != companyId)
                {
                    _logger.LogWarning("User {UserId} attempted to access email from different company", currentUserId);
                    return Forbid();
                }

                // SECURITY: Check if user has permission to view this email
                var permissions = User.FindAll("Permission").Select(c => c.Value).ToList();
                bool isAdmin = permissions.Contains("ManageUsers") || permissions.Contains("ManageSettings");
                bool isHandler = permissions.Contains("ViewComplaints") || permissions.Contains("AssignComplaint");
                bool isComplainant = complaint.ComplainantId == currentUserId;

                if (!isAdmin && !isHandler && !isComplainant)
                {
                    return Forbid();
                }

                // SECURITY: Don't show internal notes to complainants
                if (email.IsInternal && !isAdmin && !isHandler)
                {
                    return Forbid();
                }
            }

            return Ok(Result<EmailMessage>.Success(email));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving email {EmailId}", emailId);
            return StatusCode(500, Result.Failure("An error occurred while retrieving the email message"));
        }
    }

    /// <summary>
    /// Send an email reply from a ticket
    /// </summary>
    [HttpPost("send-reply")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SendEmailReply([FromBody] SendEmailReplyRequest request)
    {
        try
        {
            // SECURITY: Get current user ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid currentUserId))
            {
                return Unauthorized(Result.Failure("User information not found"));
            }

            // SECURITY: Get user's company
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out Guid companyId))
            {
                return Unauthorized(Result.Failure("Company information not found"));
            }

            // Validate request
            if (string.IsNullOrWhiteSpace(request.ToEmail) || string.IsNullOrWhiteSpace(request.Subject) ||
                string.IsNullOrWhiteSpace(request.Body))
            {
                return BadRequest(Result.Failure("To email, subject, and body are required"));
            }

            // Verify complaint exists and belongs to user's company
            var complaint = await _unitOfWork.Repository<Complaint>().GetByIdAsync(request.ComplaintId);

            if (complaint == null)
            {
                return NotFound(Result.Failure("Complaint not found"));
            }

            if (complaint.CompanyId != companyId)
            {
                _logger.LogWarning("User {UserId} attempted to send email for complaint {ComplaintId} from different company",
                    currentUserId, request.ComplaintId);
                return Forbid();
            }

            // SECURITY: Check if user has permission to reply to this complaint
            var permissions = User.FindAll("Permission").Select(c => c.Value).ToList();
            bool isAdmin = permissions.Contains("ManageUsers") || permissions.Contains("ManageSettings");
            bool isHandler = permissions.Contains("ViewComplaints") || permissions.Contains("AssignComplaint");
            bool isAssigned = complaint.AssignedToId == currentUserId;

            if (!isAdmin && !isHandler && !isAssigned)
            {
                _logger.LogWarning("User {UserId} attempted to send email for complaint {ComplaintId} without permission",
                    currentUserId, request.ComplaintId);
                return Forbid();
            }

            // Send the email
            var result = await _emailTicketingService.SendTicketReplyAsync(
                request.ComplaintId,
                request.ToEmail,
                request.Subject,
                request.Body,
                request.IsHtml,
                request.CcEmails,
                request.BccEmails,
                currentUserId,
                request.IsInternal);

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "User {UserId} sent email reply for complaint {ComplaintId} to {ToEmail}",
                    currentUserId, request.ComplaintId, request.ToEmail);
            }
            else
            {
                _logger.LogError(
                    "Failed to send email reply for complaint {ComplaintId}: {Error}",
                    request.ComplaintId, result.Message);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email reply for complaint {ComplaintId}", request.ComplaintId);
            return StatusCode(500, Result.Failure("An error occurred while sending the email reply"));
        }
    }

    /// <summary>
    /// Get email attachments for a specific email message
    /// </summary>
    [HttpGet("email/{emailId}/attachments")]
    [ProducesResponseType(typeof(Result<IEnumerable<EmailAttachment>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEmailAttachments(Guid emailId)
    {
        try
        {
            // SECURITY: Get current user ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid currentUserId))
            {
                return Unauthorized(Result.Failure("User information not found"));
            }

            // SECURITY: Get user's company
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out Guid companyId))
            {
                return Unauthorized(Result.Failure("Company information not found"));
            }

            var email = await _unitOfWork.Repository<EmailMessage>().GetByIdAsync(emailId);

            if (email == null)
            {
                return NotFound(Result.Failure("Email message not found"));
            }

            // Verify email's complaint belongs to user's company
            if (email.ComplaintId.HasValue)
            {
                var complaint = await _unitOfWork.Repository<Complaint>().GetByIdAsync(email.ComplaintId.Value);
                if (complaint?.CompanyId != companyId)
                {
                    return Forbid();
                }

                // SECURITY: Check permissions
                var permissions = User.FindAll("Permission").Select(c => c.Value).ToList();
                bool isAdmin = permissions.Contains("ManageUsers") || permissions.Contains("ManageSettings");
                bool isHandler = permissions.Contains("ViewComplaints") || permissions.Contains("ViewAttachments");
                bool isComplainant = complaint.ComplainantId == currentUserId;

                if (!isAdmin && !isHandler && !isComplainant)
                {
                    return Forbid();
                }
            }

            // Get all attachments for this email
            var attachments = await _unitOfWork.Repository<EmailAttachment>()
                .FindAsync(a => a.EmailMessageId == emailId && !a.IsDeleted, CancellationToken.None);

            _logger.LogInformation("Retrieved {Count} attachments for email {EmailId}",
                attachments.Count(), emailId);

            return Ok(Result<IEnumerable<EmailAttachment>>.Success(attachments));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attachments for email {EmailId}", emailId);
            return StatusCode(500, Result.Failure("An error occurred while retrieving email attachments"));
        }
    }

    /// <summary>
    /// Get email statistics for a company (admin only)
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(Result<EmailStatistics>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEmailStatistics()
    {
        try
        {
            // SECURITY: Get user's company
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out Guid companyId))
            {
                return Unauthorized(Result.Failure("Company information not found"));
            }

            // SECURITY: Check permissions - only admins can view statistics
            var permissions = User.FindAll("Permission").Select(c => c.Value).ToList();
            if (!permissions.Contains("ViewReports") && !permissions.Contains("ManageSettings"))
            {
                return Forbid();
            }

            // Get all emails for this company's complaints
            var complaints = await _unitOfWork.Repository<Complaint>()
                .FindAsync(c => c.CompanyId == companyId, CancellationToken.None);
            var complaintIds = complaints.Select(c => c.Id).ToList();

            var emails = await _unitOfWork.Repository<EmailMessage>()
                .FindAsync(e => e.ComplaintId.HasValue && complaintIds.Contains(e.ComplaintId.Value), CancellationToken.None);

            var emailList = emails.ToList();

            var statistics = new EmailStatistics
            {
                TotalEmails = emailList.Count,
                InboundEmails = emailList.Count(e => e.Direction == EmailDirection.Inbound),
                OutboundEmails = emailList.Count(e => e.Direction == EmailDirection.Outbound),
                ProcessedEmails = emailList.Count(e => e.Status == EmailStatus.Processed || e.Status == EmailStatus.Sent),
                FailedEmails = emailList.Count(e => e.Status == EmailStatus.Failed),
                SpamEmails = emailList.Count(e => e.IsSpam),
                InternalNotes = emailList.Count(e => e.IsInternal),
                EmailsLast24Hours = emailList.Count(e => e.ReceivedAt >= DateTime.UtcNow.AddDays(-1)),
                EmailsLast7Days = emailList.Count(e => e.ReceivedAt >= DateTime.UtcNow.AddDays(-7)),
                EmailsLast30Days = emailList.Count(e => e.ReceivedAt >= DateTime.UtcNow.AddDays(-30))
            };

            _logger.LogInformation("Retrieved email statistics for company {CompanyId}", companyId);

            return Ok(Result<EmailStatistics>.Success(statistics));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving email statistics");
            return StatusCode(500, Result.Failure("An error occurred while retrieving email statistics"));
        }
    }
}

/// <summary>
/// Request model for sending email replies
/// </summary>
public class SendEmailReplyRequest
{
    public Guid ComplaintId { get; set; }
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = false;
    public string? CcEmails { get; set; }
    public string? BccEmails { get; set; }
    public bool IsInternal { get; set; } = false;
}

/// <summary>
/// Email statistics model
/// </summary>
public class EmailStatistics
{
    public int TotalEmails { get; set; }
    public int InboundEmails { get; set; }
    public int OutboundEmails { get; set; }
    public int ProcessedEmails { get; set; }
    public int FailedEmails { get; set; }
    public int SpamEmails { get; set; }
    public int InternalNotes { get; set; }
    public int EmailsLast24Hours { get; set; }
    public int EmailsLast7Days { get; set; }
    public int EmailsLast30Days { get; set; }
}

/// <summary>
/// Email thread item DTO - compatible with frontend EmailThreadItemDto interface
/// </summary>
public class EmailThreadItemDto
{
    public Guid Id { get; set; }
    public string MessageId { get; set; } = string.Empty;
    public string? InReplyTo { get; set; }
    public string? References { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public List<EmailRecipientDto> ToRecipients { get; set; } = new();
    public List<EmailRecipientDto> CcRecipients { get; set; } = new();
    public List<EmailRecipientDto> BccRecipients { get; set; } = new();
    public string TextBody { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
    public bool IsOutbound { get; set; } // Boolean instead of enum for frontend compatibility
    public DateTime ReceivedAt { get; set; }
    public DateTime? SentAt { get; set; } // For outbound emails
    public bool IsRead { get; set; }
    public bool IsPrivateNote { get; set; }
    public Guid? SentByUserId { get; set; } // User who sent outbound email
    public int AttachmentCount { get; set; }
    public Guid? ThreadId { get; set; }
}

/// <summary>
/// Email recipient DTO
/// </summary>
public class EmailRecipientDto
{
    public string EmailAddress { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
}
