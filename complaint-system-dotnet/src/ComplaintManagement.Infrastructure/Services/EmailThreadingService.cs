using ComplaintManagement.Application.DTOs.Email;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Communication;
using ComplaintManagement.Domain.Entities.Complaints;
using ComplaintManagement.Domain.Entities.MasterData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Service for email threading and reply functionality
/// Enables Reply, Reply All, Forward from complaint tickets
/// </summary>
public class EmailThreadingService : IEmailThreadingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailThreadingService> _logger;

    public EmailThreadingService(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<EmailThreadingService> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Send reply to email from complaint ticket
    /// </summary>
    public async Task<(bool Success, string Message, EmailMessage? EmailMessage)> SendReplyAsync(
        SendEmailReplyRequest request,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Step 1: Get original message if this is a reply
            EmailMessage? originalMessage = null;
            if (request.InReplyToEmailMessageId.HasValue)
            {
                originalMessage = await _unitOfWork.Repository<EmailMessage>()
                    .GetByIdAsync(request.InReplyToEmailMessageId.Value, cancellationToken);

                if (originalMessage == null)
                    return (false, "Original email message not found", null);
            }

            // Step 2: Get complaint
            var complaint = await _unitOfWork.Complaints.GetByIdAsync(request.ComplaintId, cancellationToken);
            if (complaint == null)
                return (false, "Complaint not found", null);

            // Step 3: Determine recipients based on ReplyType
            var recipients = DetermineRecipients(request, originalMessage, complaint);

            // Step 4: Build email message with thread headers
            var threadInfo = BuildThreadHeaders(originalMessage, complaint);

            // Step 5: Get current user
            var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId, cancellationToken);

            // Step 6: Create EmailMessage record
            var emailMessage = new EmailMessage
            {
                Id = Guid.NewGuid(),
                CompanyId = complaint.CompanyId,
                ComplaintId = complaint.Id,
                MessageId = GenerateMessageId(),
                InReplyTo = threadInfo.InReplyTo,
                References = threadInfo.References,
                ThreadId = threadInfo.ThreadGuid,

                FromEmail = currentUser?.Email ?? "noreply@complaintmanagement.com",
                FromName = currentUser?.FullName ?? "Complaint Management System",

                ToRecipientsJson = JsonSerializer.Serialize(recipients.To),
                CcRecipientsJson = JsonSerializer.Serialize(recipients.Cc),
                BccRecipientsJson = JsonSerializer.Serialize(recipients.Bcc),

                Subject = request.Subject,
                HtmlBody = request.HtmlBody,
                TextBody = request.PlainTextBody ?? StripHtml(request.HtmlBody),
                IsHtml = true,

                Direction = EmailDirection.Outbound,
                IsInternal = request.IsPrivateNote,
                SentByUserId = currentUserId,
                SentAt = DateTime.UtcNow,
                ReceivedAt = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow,

                Status = EmailStatus.Sending
            };

            // Step 7: If private note, don't send email - just save to database
            if (request.IsPrivateNote)
            {
                emailMessage.Status = EmailStatus.Processed;
                emailMessage.IsInternal = true;

                await _unitOfWork.AddEntityAsync(emailMessage);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Private note added to complaint {ComplaintId} by user {UserId}",
                    complaint.Id, currentUserId);

                return (true, "Private note added successfully", emailMessage);
            }

            // Step 8: Send actual email via SMTP
            try
            {
                var toAddresses = recipients.To.Select(r => r.EmailAddress).ToList();

                if (toAddresses.Any())
                {
                    // Don't pass 'from'/'fromName' - let EmailService use the configured
                    // EmailServerSettings.FromEmail which matches the authenticated SMTP account.
                    // This prevents "SendAsDenied" errors from Microsoft 365/Exchange.
                    // The emailMessage stores the user's email for display in the UI thread view.
                    await _emailService.SendEmailAsync(
                        toAddresses,
                        request.Subject,
                        request.HtmlBody,
                        isHtml: true,
                        cancellationToken: cancellationToken);

                    emailMessage.Status = EmailStatus.Sent;
                    emailMessage.SentAt = DateTime.UtcNow;

                    _logger.LogInformation("Email sent for complaint {ComplaintNumber} - MessageId: {MessageId}",
                        complaint.ComplaintNumber, emailMessage.MessageId);
                }
                else
                {
                    _logger.LogWarning("No recipients for email in complaint {ComplaintId}", complaint.Id);
                    return (false, "No recipients specified", null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email for complaint {ComplaintId}", complaint.Id);
                emailMessage.Status = EmailStatus.Failed;
                emailMessage.Failed = true;
                emailMessage.FailureReason = ex.Message;
            }

            // Step 9: Save to database
            await _unitOfWork.AddEntityAsync(emailMessage);

            // Step 10: Update complaint participants
            await UpdateComplaintParticipantsAsync(complaint.Id, recipients, currentUserId, cancellationToken);

            // Step 11: Clear HasCustomerResponse flag since handler has replied
            complaint.HasCustomerResponse = false;
            complaint.LastResponseFrom = "Handler";
            complaint.LastResponseAt = DateTime.UtcNow;
            complaint.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Handler replied to complaint {ComplaintId} - HasCustomerResponse cleared", complaint.Id);

            return (true, "Email sent successfully", emailMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendReplyAsync for complaint {ComplaintId}", request.ComplaintId);
            return (false, $"Error sending email: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Get all participants in a complaint email thread
    /// </summary>
    public async Task<List<ComplaintEmailParticipant>> GetThreadParticipantsAsync(
        Guid complaintId,
        CancellationToken cancellationToken = default)
    {
        var participants = await _unitOfWork.Repository<ComplaintEmailParticipant>()
            .FindAsync(p => p.ComplaintId == complaintId && p.IsActive, cancellationToken);

        return participants.ToList();
    }

    /// <summary>
    /// Mark email as read by user
    /// When a customer reply is read, clear the HasCustomerResponse flag
    /// </summary>
    public async Task MarkAsReadAsync(Guid emailMessageId, Guid userId, CancellationToken cancellationToken = default)
    {
        var emailMessage = await _unitOfWork.Repository<EmailMessage>()
            .GetByIdAsync(emailMessageId, cancellationToken);

        if (emailMessage != null && !emailMessage.IsRead)
        {
            emailMessage.IsRead = true;
            emailMessage.ReadBy = userId;
            emailMessage.ReadAt = DateTime.UtcNow;

            // If this is an inbound customer email, clear the HasCustomerResponse flag on the complaint
            if (emailMessage.ComplaintId.HasValue && emailMessage.Direction == EmailDirection.Inbound)
            {
                var complaint = await _unitOfWork.Complaints.GetByIdAsync(emailMessage.ComplaintId.Value, cancellationToken);
                if (complaint != null && complaint.HasCustomerResponse)
                {
                    complaint.HasCustomerResponse = false;
                    complaint.UpdatedAt = DateTime.UtcNow;
                    _logger.LogInformation("Cleared HasCustomerResponse for complaint {ComplaintId} - email marked as read by user {UserId}",
                        complaint.Id, userId);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Mark all emails in complaint as read
    /// When all customer replies are read, clear the HasCustomerResponse flag
    /// </summary>
    public async Task MarkAllAsReadAsync(Guid complaintId, Guid userId, CancellationToken cancellationToken = default)
    {
        var emailMessages = await _unitOfWork.Repository<EmailMessage>()
            .FindAsync(em => em.ComplaintId == complaintId && !em.IsRead, cancellationToken);

        bool hasUnreadInboundEmails = false;
        foreach (var email in emailMessages)
        {
            email.IsRead = true;
            email.ReadBy = userId;
            email.ReadAt = DateTime.UtcNow;

            // Track if any inbound (customer) emails were marked as read
            if (email.Direction == EmailDirection.Inbound)
            {
                hasUnreadInboundEmails = true;
            }
        }

        // If any inbound emails were marked as read, clear HasCustomerResponse
        if (hasUnreadInboundEmails)
        {
            var complaint = await _unitOfWork.Complaints.GetByIdAsync(complaintId, cancellationToken);
            if (complaint != null && complaint.HasCustomerResponse)
            {
                complaint.HasCustomerResponse = false;
                complaint.UpdatedAt = DateTime.UtcNow;
                _logger.LogInformation("Cleared HasCustomerResponse for complaint {ComplaintId} - all emails marked as read by user {UserId}",
                    complaintId, userId);
            }
        }

        if (emailMessages.Any())
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Get unread email count for a complaint
    /// </summary>
    public async Task<int> GetUnreadCountAsync(Guid complaintId, Guid userId, CancellationToken cancellationToken = default)
    {
        var count = await _unitOfWork.Repository<EmailMessage>()
            .CountAsync(em => em.ComplaintId == complaintId &&
                             !em.IsRead &&
                             em.Direction == EmailDirection.Inbound,
                       cancellationToken);

        return count;
    }

    #region Private Helper Methods

    /// <summary>
    /// Determine recipients based on reply type
    /// </summary>
    private (List<EmailRecipient> To, List<EmailRecipient> Cc, List<EmailRecipient> Bcc) DetermineRecipients(
        SendEmailReplyRequest request,
        EmailMessage? originalMessage,
        Complaint complaint)
    {
        var to = new List<EmailRecipient>();
        var cc = new List<EmailRecipient>();
        var bcc = new List<EmailRecipient>();

        switch (request.ReplyType)
        {
            case ReplyType.Reply:
                // Reply to sender only
                if (originalMessage != null)
                {
                    to.Add(new EmailRecipient
                    {
                        EmailAddress = originalMessage.FromEmail,
                        DisplayName = originalMessage.FromName
                    });
                }
                else if (complaint.Complainant != null)
                {
                    // New email - use complainant
                    to.Add(new EmailRecipient
                    {
                        EmailAddress = complaint.Complainant.Email,
                        DisplayName = complaint.Complainant.FullName
                    });
                }
                break;

            case ReplyType.ReplyAll:
                // Reply to all participants (To + CC + From)
                if (originalMessage != null)
                {
                    // Add original sender
                    to.Add(new EmailRecipient
                    {
                        EmailAddress = originalMessage.FromEmail,
                        DisplayName = originalMessage.FromName
                    });

                    // Add all original To recipients
                    if (!string.IsNullOrEmpty(originalMessage.ToRecipientsJson))
                    {
                        try
                        {
                            var originalTo = JsonSerializer.Deserialize<List<EmailRecipient>>(originalMessage.ToRecipientsJson);
                            if (originalTo != null)
                                to.AddRange(originalTo);
                        }
                        catch { }
                    }

                    // Add all original CC recipients
                    if (!string.IsNullOrEmpty(originalMessage.CcRecipientsJson))
                    {
                        try
                        {
                            var originalCc = JsonSerializer.Deserialize<List<EmailRecipient>>(originalMessage.CcRecipientsJson);
                            if (originalCc != null)
                                cc.AddRange(originalCc);
                        }
                        catch { }
                    }
                }
                break;

            case ReplyType.Forward:
            case ReplyType.NewEmail:
            case ReplyType.PrivateNote:
                // Use provided recipients
                to = request.ToRecipients;
                cc = request.CcRecipients;
                bcc = request.BccRecipients;
                break;
        }

        return (to, cc, bcc);
    }

    /// <summary>
    /// Build email thread headers for proper threading
    /// </summary>
    private (Guid? ThreadGuid, string InReplyTo, string References) BuildThreadHeaders(
        EmailMessage? originalMessage,
        Complaint complaint)
    {
        Guid? threadGuid;
        string inReplyTo;
        string references;

        if (originalMessage != null)
        {
            // This is a reply - maintain thread
            threadGuid = originalMessage.ThreadId ?? Guid.NewGuid();
            inReplyTo = originalMessage.MessageId;

            // Build References header (accumulates all Message-IDs in thread)
            if (!string.IsNullOrEmpty(originalMessage.References))
            {
                references = $"{originalMessage.References} {originalMessage.MessageId}";
            }
            else
            {
                references = originalMessage.MessageId;
            }
        }
        else
        {
            // New thread - generate new IDs
            threadGuid = Guid.NewGuid();
            inReplyTo = GenerateMessageId();
            references = inReplyTo;
        }

        return (threadGuid, inReplyTo, references);
    }

    /// <summary>
    /// Generate unique Message-ID for email
    /// </summary>
    private string GenerateMessageId()
    {
        return $"<{Guid.NewGuid()}@complaintmanagement.com>";
    }

    /// <summary>
    /// Update complaint participants list
    /// </summary>
    private async Task UpdateComplaintParticipantsAsync(
        Guid complaintId,
        (List<EmailRecipient> To, List<EmailRecipient> Cc, List<EmailRecipient> Bcc) recipients,
        Guid addedBy,
        CancellationToken cancellationToken)
    {
        // Add all new participants to ComplaintEmailParticipant table
        var allParticipants = new List<(string Email, string? Name, string Type)>();

        recipients.To.ForEach(r => allParticipants.Add((r.EmailAddress, r.DisplayName, "To")));
        recipients.Cc.ForEach(r => allParticipants.Add((r.EmailAddress, r.DisplayName, "CC")));
        recipients.Bcc.ForEach(r => allParticipants.Add((r.EmailAddress, r.DisplayName, "BCC")));

        foreach (var (email, name, type) in allParticipants)
        {
            // Check if already exists
            var exists = await _unitOfWork.Repository<ComplaintEmailParticipant>()
                .AnyAsync(p => p.ComplaintId == complaintId &&
                              p.EmailAddress == email &&
                              p.ParticipantType == type,
                         cancellationToken);

            if (!exists)
            {
                var participant = new ComplaintEmailParticipant
                {
                    Id = Guid.NewGuid(),
                    ComplaintId = complaintId,
                    EmailAddress = email,
                    DisplayName = name,
                    ParticipantType = type,
                    AddedBy = addedBy,
                    AddedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _unitOfWork.AddEntityAsync(participant);
            }
        }
    }

    /// <summary>
    /// Strip HTML tags from content
    /// </summary>
    private string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;

        // Simple HTML stripping
        return Regex.Replace(html, "<.*?>", string.Empty).Trim();
    }

    #endregion
}
