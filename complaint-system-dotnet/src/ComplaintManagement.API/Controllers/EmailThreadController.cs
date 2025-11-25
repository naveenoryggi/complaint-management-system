using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Email;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Communication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace ComplaintManagement.API.Controllers;

/// <summary>
/// Controller for email threading and reply functionality
/// Enables viewing email threads, replying to emails, and managing participants
/// </summary>
[ApiController]
[Route("api/complaints/{complaintId}/emails")]
[Authorize]
public class EmailThreadController : ControllerBase
{
    private readonly IEmailThreadingService _emailThreadingService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EmailThreadController> _logger;

    public EmailThreadController(
        IEmailThreadingService emailThreadingService,
        IUnitOfWork unitOfWork,
        ILogger<EmailThreadController> logger)
    {
        _emailThreadingService = emailThreadingService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Convert Application.DTOs.Email.EmailRecipient to EmailRecipientDto
    /// </summary>
    private static List<EmailRecipientDto> ConvertToEmailRecipientDtos(List<EmailRecipient>? recipients)
    {
        if (recipients == null || recipients.Count == 0)
            return new List<EmailRecipientDto>();

        return recipients.Select(r => new EmailRecipientDto
        {
            EmailAddress = r.EmailAddress,
            DisplayName = r.DisplayName
        }).ToList();
    }

    /// <summary>
    /// Get email thread for complaint
    /// </summary>
    /// <param name="complaintId">Complaint ID</param>
    /// <param name="includePrivateNotes">Include private/internal notes</param>
    /// <returns>List of email messages in thread</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<EmailThreadItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetEmailThread(Guid complaintId, [FromQuery] bool includePrivateNotes = false)
    {
        try
        {
            // Get current user ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "User information not found" });
            }

            var userId = Guid.Parse(userIdClaim);

            // Get email messages for complaint
            var query = _unitOfWork.Repository<EmailMessage>()
                .GetQueryable()
                .Where(em => em.ComplaintId == complaintId);

            // Filter private notes based on parameter
            if (!includePrivateNotes)
            {
                query = query.Where(em => !em.IsInternal);
            }

            // Load emails first, then deserialize JSON (can't do this in expression tree)
            var emailEntities = await query
                .OrderBy(em => em.ReceivedAt)
                .ToListAsync();

            // Map to DTOs with JSON deserialization
            var emails = emailEntities.Select(em => new EmailThreadItemDto
            {
                Id = em.Id,
                MessageId = em.MessageId,
                FromEmail = em.FromEmail,
                FromName = em.FromName,
                ToRecipients = ConvertToEmailRecipientDtos(
                    !string.IsNullOrEmpty(em.ToRecipientsJson)
                        ? JsonSerializer.Deserialize<List<EmailRecipient>>(em.ToRecipientsJson, new JsonSerializerOptions())
                        : null),
                CcRecipients = ConvertToEmailRecipientDtos(
                    !string.IsNullOrEmpty(em.CcRecipientsJson)
                        ? JsonSerializer.Deserialize<List<EmailRecipient>>(em.CcRecipientsJson, new JsonSerializerOptions())
                        : null),
                Subject = em.Subject,
                HtmlBody = em.HtmlBody,
                TextBody = em.TextBody,
                ReceivedAt = em.ReceivedAt,
                SentAt = em.SentAt,
                IsOutbound = em.Direction == EmailDirection.Outbound,
                IsPrivateNote = em.IsInternal,
                IsRead = em.IsRead,
                SentByUserId = em.SentByUserId,
                AttachmentCount = em.Attachments.Count
            }).ToList();

            return Ok(Result<List<EmailThreadItemDto>>.Success(emails));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving email thread for complaint {ComplaintId}", complaintId);
            return StatusCode(500, Result<List<EmailThreadItemDto>>.Failure("An error occurred while retrieving email thread"));
        }
    }

    /// <summary>
    /// Get participants in email thread
    /// </summary>
    /// <param name="complaintId">Complaint ID</param>
    /// <returns>List of participants</returns>
    [HttpGet("participants")]
    [ProducesResponseType(typeof(Result<List<ComplaintEmailParticipant>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetParticipants(Guid complaintId)
    {
        try
        {
            var participants = await _emailThreadingService.GetThreadParticipantsAsync(complaintId);

            return Ok(Result<List<ComplaintEmailParticipant>>.Success(participants));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving participants for complaint {ComplaintId}", complaintId);
            return StatusCode(500, Result<List<ComplaintEmailParticipant>>.Failure("An error occurred while retrieving participants"));
        }
    }

    /// <summary>
    /// Send reply to email
    /// </summary>
    /// <param name="complaintId">Complaint ID</param>
    /// <param name="request">Reply request</param>
    /// <returns>Created email message</returns>
    [HttpPost("reply")]
    [ProducesResponseType(typeof(Result<EmailThreadItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendReply(Guid complaintId, [FromBody] Application.DTOs.Email.SendEmailReplyRequest request)
    {
        try
        {
            // Get current user ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "User information not found" });
            }

            var userId = Guid.Parse(userIdClaim);
            request.ComplaintId = complaintId;

            var result = await _emailThreadingService.SendReplyAsync(request, userId);
            var success = result.Success;
            var message = result.Message;
            var emailMessage = result.EmailMessage;

            if (!success || emailMessage == null)
            {
                return BadRequest(Result.Failure(message));
            }

            // Convert to DTO
            var dto = new EmailThreadItemDto
            {
                Id = emailMessage.Id,
                MessageId = emailMessage.MessageId,
                FromEmail = emailMessage.FromEmail,
                FromName = emailMessage.FromName,
                ToRecipients = ConvertToEmailRecipientDtos(
                    !string.IsNullOrEmpty(emailMessage.ToRecipientsJson)
                        ? JsonSerializer.Deserialize<List<EmailRecipient>>(emailMessage.ToRecipientsJson, new JsonSerializerOptions())
                        : null),
                CcRecipients = ConvertToEmailRecipientDtos(
                    !string.IsNullOrEmpty(emailMessage.CcRecipientsJson)
                        ? JsonSerializer.Deserialize<List<EmailRecipient>>(emailMessage.CcRecipientsJson, new JsonSerializerOptions())
                        : null),
                Subject = emailMessage.Subject,
                HtmlBody = emailMessage.HtmlBody,
                TextBody = emailMessage.TextBody,
                ReceivedAt = emailMessage.ReceivedAt,
                SentAt = emailMessage.SentAt,
                IsOutbound = emailMessage.Direction == EmailDirection.Outbound,
                IsPrivateNote = emailMessage.IsInternal,
                IsRead = emailMessage.IsRead,
                SentByUserId = emailMessage.SentByUserId,
                AttachmentCount = 0
            };

            _logger.LogInformation("Email reply sent for complaint {ComplaintId} by user {UserId}", complaintId, userId);
            return CreatedAtAction(nameof(GetEmailThread), new { complaintId }, Result<EmailThreadItemDto>.Success(dto, message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending email reply for complaint {ComplaintId}", complaintId);
            return StatusCode(500, Result.Failure("An error occurred while sending email reply"));
        }
    }

    /// <summary>
    /// Mark email as read
    /// </summary>
    /// <param name="complaintId">Complaint ID</param>
    /// <param name="emailId">Email message ID</param>
    /// <returns>Success result</returns>
    [HttpPost("{emailId}/mark-read")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAsRead(Guid complaintId, Guid emailId)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "User information not found" });
            }

            var userId = Guid.Parse(userIdClaim);

            await _emailThreadingService.MarkAsReadAsync(emailId, userId);

            return Ok(Result.Success("Email marked as read"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while marking email {EmailId} as read", emailId);
            return StatusCode(500, Result.Failure("An error occurred while marking email as read"));
        }
    }

    /// <summary>
    /// Mark all emails in complaint as read
    /// </summary>
    /// <param name="complaintId">Complaint ID</param>
    /// <returns>Success result</returns>
    [HttpPost("mark-all-read")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead(Guid complaintId)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "User information not found" });
            }

            var userId = Guid.Parse(userIdClaim);

            await _emailThreadingService.MarkAllAsReadAsync(complaintId, userId);

            return Ok(Result.Success("All emails marked as read"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while marking all emails as read for complaint {ComplaintId}", complaintId);
            return StatusCode(500, Result.Failure("An error occurred while marking emails as read"));
        }
    }

    /// <summary>
    /// Get unread email count for complaint
    /// </summary>
    /// <param name="complaintId">Complaint ID</param>
    /// <returns>Unread count</returns>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(Result<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount(Guid complaintId)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "User information not found" });
            }

            var userId = Guid.Parse(userIdClaim);

            var count = await _emailThreadingService.GetUnreadCountAsync(complaintId, userId);

            return Ok(Result<int>.Success(count));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting unread count for complaint {ComplaintId}", complaintId);
            return StatusCode(500, Result<int>.Failure("An error occurred while getting unread count"));
        }
    }

    /// <summary>
    /// Get canned responses (quick reply templates)
    /// </summary>
    /// <param name="categoryId">Optional category ID to filter by</param>
    /// <returns>List of canned responses</returns>
    [HttpGet("/api/canned-responses")]
    [ProducesResponseType(typeof(Result<List<CannedResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCannedResponses([FromQuery] Guid? categoryId = null)
    {
        try
        {
            // Get company ID from claims
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim))
            {
                return BadRequest(Result.Failure("Company information not found"));
            }

            var companyId = Guid.Parse(companyIdClaim);

            var query = _unitOfWork.Repository<CannedResponse>()
                .GetQueryable()
                .Where(cr => cr.CompanyId == companyId && cr.IsActive);

            if (categoryId.HasValue)
            {
                query = query.Where(cr => cr.CategoryId == categoryId || cr.CategoryId == null);
            }

            var responses = await query
                .OrderByDescending(cr => cr.UsageCount)
                .Select(cr => new CannedResponseDto
                {
                    Id = cr.Id,
                    Title = cr.Title,
                    ShortCode = cr.ShortCode,
                    Subject = cr.Subject,
                    Body = cr.Body,
                    UsageCount = cr.UsageCount,
                    CategoryId = cr.CategoryId,
                    CategoryName = cr.Category != null ? cr.Category.Name : null
                })
                .ToListAsync();

            return Ok(Result<List<CannedResponseDto>>.Success(responses));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving canned responses");
            return StatusCode(500, Result<List<CannedResponseDto>>.Failure("An error occurred while retrieving canned responses"));
        }
    }
}
