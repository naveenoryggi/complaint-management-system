using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Communication;
using ComplaintManagement.Domain.Entities.Complaints;
using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Entities.Roles;
using ComplaintManagement.Domain.Enums;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit;
using System.Text.RegularExpressions;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Email Ticketing Service - Handles IMAP fetching, SMTP sending, threading, and ticket creation from emails
/// </summary>
public class EmailTicketingService : IEmailTicketingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EmailTicketingService> _logger;
    private readonly ITemplateService _templateService;
    private readonly EmailOAuthService _oauthService;
    private readonly IEncryptionService _encryptionService;

    public EmailTicketingService(
        IUnitOfWork unitOfWork,
        ILogger<EmailTicketingService> logger,
        ITemplateService templateService,
        EmailOAuthService oauthService,
        IEncryptionService encryptionService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _templateService = templateService;
        _oauthService = oauthService;
        _encryptionService = encryptionService;
    }

    #region IMAP Email Fetching

    /// <summary>
    /// Fetches new emails from IMAP server and processes them into tickets
    /// </summary>
    public async Task<Result<EmailProcessingResult>> FetchAndProcessEmailsAsync(
        Guid emailConfigurationId,
        CancellationToken cancellationToken = default)
    {
        var result = new EmailProcessingResult();

        try
        {
            // Get email configuration
            var config = await _unitOfWork.Repository<EmailConfiguration>()
                .GetByIdAsync(emailConfigurationId, cancellationToken);

            if (config == null)
            {
                return Result<EmailProcessingResult>.Failure("Email configuration not found");
            }

            if (!config.IsEnabled)
            {
                return Result<EmailProcessingResult>.Failure("Email configuration is disabled");
            }

            _logger.LogInformation("Starting email fetch for company {CompanyId}", config.CompanyId);

            using var client = new ImapClient();

            // Connect to IMAP server
            await client.ConnectAsync(config.ImapHost, config.ImapPort, config.ImapUseSsl, cancellationToken);

            // Authenticate using OAuth2 or basic auth
            await AuthenticateImapClientAsync(client, config, cancellationToken);

            // Open inbox folder
            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadWrite, cancellationToken);

            // Search for unread emails only
            var uids = await inbox.SearchAsync(SearchQuery.NotSeen, cancellationToken);
            result.TotalEmailsFetched = uids.Count;

            _logger.LogInformation("Found {Count} unread emails", uids.Count);

            // Process each email
            foreach (var uid in uids)
            {
                try
                {
                    var message = await inbox.GetMessageAsync(uid, cancellationToken);

                    // Process the email into a ticket
                    var processResult = await ProcessEmailMessageAsync(message, config, cancellationToken);

                    if (processResult.IsSuccess)
                    {
                        if (processResult.Data == "NEW_TICKET")
                        {
                            result.NewTicketsCreated++;
                        }
                        else if (processResult.Data == "UPDATED_TICKET")
                        {
                            result.ExistingTicketsUpdated++;
                        }
                        else if (processResult.Data == "IGNORED")
                        {
                            result.EmailsIgnored++;
                        }

                        // Mark email as read
                        await inbox.AddFlagsAsync(uid, MessageFlags.Seen, true, cancellationToken);
                    }
                    else
                    {
                        result.EmailsFailed++;
                        result.Errors.Add($"Email from {message.From}: {processResult.Message}");
                        _logger.LogError("Failed to process email from {From}: {Error}", message.From, processResult.Message);
                    }
                }
                catch (Exception ex)
                {
                    result.EmailsFailed++;
                    result.Errors.Add($"Exception processing email UID {uid}: {ex.Message}");
                    _logger.LogError(ex, "Exception processing email UID {UID}", uid);
                }
            }

            // Disconnect
            await client.DisconnectAsync(true, cancellationToken);

            // Update last polled time
            config.LastPolledAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Email fetch completed. New: {New}, Updated: {Updated}, Ignored: {Ignored}, Failed: {Failed}",
                result.NewTicketsCreated, result.ExistingTicketsUpdated, result.EmailsIgnored, result.EmailsFailed);

            return Result<EmailProcessingResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching emails for configuration {ConfigId}", emailConfigurationId);
            result.Errors.Add($"Fatal error: {ex.Message}");
            return Result<EmailProcessingResult>.Failure($"Error fetching emails: {ex.Message}");
        }
    }

    #endregion

    #region Email Processing and Ticket Creation

    private async Task<Result<string>> ProcessEmailMessageAsync(
        MimeMessage message,
        EmailConfiguration config,
        CancellationToken cancellationToken)
    {
        try
        {
            // Extract email metadata
            var messageId = message.MessageId ?? Guid.NewGuid().ToString();
            var inReplyTo = message.InReplyTo;
            var references = message.References?.FirstOrDefault();
            var subject = message.Subject ?? "(No Subject)";
            var fromAddress = message.From.Mailboxes.FirstOrDefault()?.Address ?? "";
            var fromName = message.From.Mailboxes.FirstOrDefault()?.Name ?? "";

            // Check if this is an auto-reply or bounce message (ignore these)
            if (IsAutoReplyOrBounce(message))
            {
                _logger.LogInformation("Ignoring auto-reply/bounce from {From}", fromAddress);
                await SaveEmailMessageAsync(message, config, null, EmailStatus.Ignored, cancellationToken);
                return Result<string>.Success("IGNORED");
            }

            // Try to find parent complaint using threading
            var parentComplaintId = await FindParentComplaintFromThreadingAsync(
                messageId, inReplyTo, references, subject, config.CompanyId, cancellationToken);

            Guid complaintId;

            if (parentComplaintId.HasValue)
            {
                // This is a reply to an existing ticket
                complaintId = parentComplaintId.Value;
                _logger.LogInformation("Email threaded to existing complaint {ComplaintId}", complaintId);

                // Save email message linked to complaint
                await SaveEmailMessageAsync(message, config, complaintId, EmailStatus.Processed, cancellationToken);

                // Update complaint status if needed (e.g., from "Waiting on Customer" to "New Response")
                await UpdateComplaintStatusOnNewEmailAsync(complaintId, cancellationToken);

                return Result<string>.Success("UPDATED_TICKET");
            }
            else
            {
                // Create new complaint/ticket from this email
                complaintId = await CreateComplaintFromEmailAsync(message, config, cancellationToken);
                _logger.LogInformation("Created new complaint {ComplaintId} from email", complaintId);

                // Save email message linked to new complaint
                await SaveEmailMessageAsync(message, config, complaintId, EmailStatus.Processed, cancellationToken);

                // Send auto-acknowledgement if enabled
                if (config.SendAutoAcknowledgement)
                {
                    await SendAutoAcknowledgementAsync(complaintId, fromAddress, config, cancellationToken);
                }

                return Result<string>.Success("NEW_TICKET");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing email message");
            return Result<string>.Failure($"Error processing email: {ex.Message}");
        }
    }

    private async Task<Guid> CreateComplaintFromEmailAsync(
        MimeMessage message,
        EmailConfiguration config,
        CancellationToken cancellationToken)
    {
        var fromAddress = message.From.Mailboxes.FirstOrDefault()?.Address ?? "";
        var fromName = message.From.Mailboxes.FirstOrDefault()?.Name ?? fromAddress;
        var subject = message.Subject ?? "(No Subject)";
        var body = GetEmailBody(message);

        // Truncate description if too long (max 4000 characters to be safe)
        const int maxDescriptionLength = 4000;
        if (body.Length > maxDescriptionLength)
        {
            body = body.Substring(0, maxDescriptionLength - 50) + "\n\n[Email content truncated - too long]";
            _logger.LogWarning("Email body truncated from {OriginalLength} to {MaxLength} characters", body.Length, maxDescriptionLength);
        }

        // Get or create complainant user from email address
        var complainantId = await GetOrCreateComplainantFromEmailAsync(fromAddress, fromName, config.CompanyId, cancellationToken);

        // Get default category and status for email-origin complaints
        var categories = await _unitOfWork.Repository<ComplaintCategory>()
            .FindAsync(c => c.IsActive, cancellationToken);
        var defaultCategory = categories.FirstOrDefault(c => c.Code == "EMAIL") ?? categories.FirstOrDefault();

        var statuses = await _unitOfWork.Repository<ComplaintStatusMaster>()
            .FindAsync(s => s.IsActive, cancellationToken);
        var newStatus = statuses.FirstOrDefault(s => s.Code == "NEW") ?? statuses.FirstOrDefault();

        var priorities = await _unitOfWork.Repository<ComplaintPriorityMaster>()
            .FindAsync(p => p.IsActive, cancellationToken);
        var defaultPriority = priorities.FirstOrDefault(p => p.Code == "MEDIUM") ?? priorities.FirstOrDefault();

        // Create complaint
        var complaint = new Complaint
        {
            Id = Guid.NewGuid(),
            CompanyId = config.CompanyId,
            Title = subject.Length > 200 ? subject.Substring(0, 200) : subject,
            Description = body,
            CategoryId = defaultCategory?.Id ?? Guid.Empty,
            StatusMasterId = newStatus?.Id ?? Guid.Empty,
            PriorityMasterId = defaultPriority?.Id ?? Guid.Empty,
            ComplaintNumber = await GenerateComplaintNumberAsync(config.CompanyId, cancellationToken),
            SubmittedAt = DateTime.UtcNow,
            ComplainantId = complainantId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Complaint>().AddAsync(complaint, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created complaint {ComplaintNumber} from email {FromEmail}", complaint.ComplaintNumber, fromAddress);

        return complaint.Id;
    }

    /// <summary>
    /// Gets an existing user by email or creates a new complainant user
    /// </summary>
    private async Task<Guid> GetOrCreateComplainantFromEmailAsync(
        string email,
        string name,
        Guid companyId,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("GetOrCreateComplainant: Email={Email}, CompanyId={CompanyId}", email, companyId);

        // Try to find existing user by email and company (including soft-deleted ones to avoid duplicate key errors)
        // Note: Email is globally unique (one person = one company), but we filter by company for data segmentation
        var existingUser = await _unitOfWork.Repository<User>()
            .GetQueryable()
            .IgnoreQueryFilters() // Include soft-deleted records
            .FirstOrDefaultAsync(u => u.Email == email && u.CompanyId == companyId, cancellationToken);

        _logger.LogDebug("Existing user search result: {Found}", existingUser != null ? "FOUND" : "NOT FOUND");

        if (existingUser != null)
        {
            // If user is soft-deleted, reactivate them
            if (existingUser.IsDeleted)
            {
                _logger.LogInformation("Reactivating soft-deleted user {UserId} for email {Email}", existingUser.Id, email);
                existingUser.IsDeleted = false;
                existingUser.DeletedAt = null;
                existingUser.DeletedBy = null;
                existingUser.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<User>().Update(existingUser);
            }
            else
            {
                _logger.LogDebug("Found existing user {UserId} for email {Email}", existingUser.Id, email);
            }
            return existingUser.Id;
        }

        // Create new complainant user
        _logger.LogInformation("Creating new complainant user for email {Email}", email);

        // Split name into first and last name
        var nameParts = name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var firstName = nameParts.Length > 0 ? nameParts[0] : email.Split('@')[0];
        var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

        // Get Complainant role
        var roles = await _unitOfWork.Repository<ComplaintRole>()
            .FindAsync(r => r.Code == "COMPLAINANT" && r.IsActive, cancellationToken);
        var complainantRole = roles.FirstOrDefault();

        if (complainantRole == null)
        {
            _logger.LogWarning("Complainant role not found, using first available active role");
            var allRoles = await _unitOfWork.Repository<ComplaintRole>()
                .FindAsync(r => r.IsActive, cancellationToken);
            complainantRole = allRoles.FirstOrDefault();
        }

        if (complainantRole == null)
        {
            throw new InvalidOperationException("No active roles found in the system. Cannot create user.");
        }

        var userId = Guid.NewGuid();

        // Generate unique employee code for email-created users
        var employeeCode = $"EMAIL-{userId.ToString().Substring(0, 8).ToUpper()}";

        var newUser = new User
        {
            Id = userId,
            Email = email,
            EmployeeCode = employeeCode,
            FirstName = firstName,
            LastName = lastName,
            CompanyId = companyId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            // Note: Random password - this user can only create tickets via email
            // They would need to go through password reset to login to the portal
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString())
        };

        await _unitOfWork.Repository<User>().AddAsync(newUser, cancellationToken);

        // Create UserComplaintRole record to assign the role
        var userRole = new UserComplaintRole
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ComplaintRoleId = complainantRole.Id,
            CompanyId = companyId,
            EffectiveFrom = DateTime.UtcNow,
            IsPrimary = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<UserComplaintRole>().AddAsync(userRole, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created new complainant user {UserId} for email {Email} with role {RoleId}", newUser.Id, email, complainantRole.Id);

        return newUser.Id;
    }

    private async Task<string> GenerateComplaintNumberAsync(Guid companyId, CancellationToken cancellationToken)
    {
        var complaints = await _unitOfWork.Repository<Complaint>()
            .FindAsync(c => c.CompanyId == companyId, cancellationToken);
        var count = complaints.Count() + 1;
        return $"CMP-{DateTime.UtcNow:yyyyMMdd}-{count:D4}";
    }

    #endregion

    #region SMTP Email Sending

    /// <summary>
    /// Sends an email reply from a ticket/complaint
    /// </summary>
    public async Task<Result<Guid>> SendTicketReplyAsync(
        Guid complaintId,
        string toEmail,
        string subject,
        string body,
        bool isHtml,
        string? ccEmails = null,
        string? bccEmails = null,
        Guid? sentByUserId = null,
        bool isInternal = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get complaint
            var complaint = await _unitOfWork.Repository<Complaint>()
                .GetByIdAsync(complaintId, cancellationToken);

            if (complaint == null)
            {
                return Result<Guid>.Failure("Complaint not found");
            }

            // Get email configuration for this company
            var configs = await _unitOfWork.Repository<EmailConfiguration>()
                .FindAsync(c => c.CompanyId == complaint.CompanyId && c.IsEnabled, cancellationToken);
            var config = configs.FirstOrDefault();

            if (config == null)
            {
                return Result<Guid>.Failure("Email configuration not found or disabled");
            }

            // Build email message
            var message = new MimeMessage();

            // Use separate SMTP From address if configured, otherwise use main account
            var fromEmail = config.UseSeparateSmtpAccount && !string.IsNullOrEmpty(config.SmtpSeparateFromEmail)
                ? config.SmtpSeparateFromEmail
                : config.FromEmail;
            var fromName = config.UseSeparateSmtpAccount && !string.IsNullOrEmpty(config.SmtpSeparateFromName)
                ? config.SmtpSeparateFromName
                : config.FromName;

            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));

            if (!string.IsNullOrEmpty(ccEmails))
            {
                foreach (var cc in ccEmails.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    message.Cc.Add(MailboxAddress.Parse(cc.Trim()));
                }
            }

            if (!string.IsNullOrEmpty(bccEmails))
            {
                foreach (var bcc in bccEmails.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    message.Bcc.Add(MailboxAddress.Parse(bcc.Trim()));
                }
            }

            message.Subject = subject;

            // Build body
            var bodyBuilder = new BodyBuilder();
            if (isHtml)
            {
                bodyBuilder.HtmlBody = body;
            }
            else
            {
                bodyBuilder.TextBody = body;
            }

            message.Body = bodyBuilder.ToMessageBody();

            // Add threading headers for proper email threading
            var previousEmails = await _unitOfWork.Repository<EmailMessage>()
                .FindAsync(e => e.ComplaintId == complaintId && e.Direction == EmailDirection.Inbound, cancellationToken);
            var lastInboundEmail = previousEmails.OrderByDescending(e => e.ReceivedAt).FirstOrDefault();

            if (lastInboundEmail != null)
            {
                message.InReplyTo = lastInboundEmail.MessageId;
                message.References.Add(lastInboundEmail.MessageId);
            }

            // Send email via SMTP
            using var smtpClient = new SmtpClient();

            // Determine the correct SecureSocketOptions based on port
            var secureSocketOptions = config.SmtpPort switch
            {
                587 => MailKit.Security.SecureSocketOptions.StartTls,  // Port 587 uses STARTTLS
                465 => MailKit.Security.SecureSocketOptions.SslOnConnect,  // Port 465 uses SSL/TLS
                _ => config.SmtpUseSsl ? MailKit.Security.SecureSocketOptions.SslOnConnect : MailKit.Security.SecureSocketOptions.Auto
            };

            await smtpClient.ConnectAsync(config.SmtpHost, config.SmtpPort, secureSocketOptions, cancellationToken);

            // Authenticate using OAuth2 or basic auth
            await AuthenticateSmtpClientAsync(smtpClient, config, cancellationToken);

            await smtpClient.SendAsync(message, cancellationToken);
            await smtpClient.DisconnectAsync(true, cancellationToken);

            // Save email message to database
            var emailMessage = new EmailMessage
            {
                Id = Guid.NewGuid(),
                CompanyId = complaint.CompanyId,
                ComplaintId = complaintId,
                MessageId = message.MessageId,
                InReplyTo = lastInboundEmail?.MessageId,
                Subject = subject,
                FromEmail = fromEmail, // Use the determined from email (separate SMTP or main account)
                FromName = fromName,   // Use the determined from name (separate SMTP or main account)
                ToEmail = toEmail,
                CcEmails = ccEmails,
                BccEmails = bccEmails,
                TextBody = isHtml ? "" : body,
                HtmlBody = isHtml ? body : null,
                IsHtml = isHtml,
                Direction = EmailDirection.Outbound,
                Status = EmailStatus.Sent,
                ReceivedAt = DateTime.UtcNow,
                SentAt = DateTime.UtcNow,
                SentByUserId = sentByUserId,
                IsInternal = isInternal,
                ThreadPosition = previousEmails.Count()
            };

            await _unitOfWork.Repository<EmailMessage>().AddAsync(emailMessage, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Sent email reply for complaint {ComplaintId} to {To}", complaintId, toEmail);

            return Result<Guid>.Success(emailMessage.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending ticket reply for complaint {ComplaintId}", complaintId);
            return Result<Guid>.Failure($"Error sending email: {ex.Message}");
        }
    }

    #endregion

    #region Auto-Acknowledgement

    /// <summary>
    /// Sends an auto-acknowledgement email for a new ticket
    /// Uses template system if configured, otherwise uses default HTML
    /// </summary>
    public async Task<Result> SendAutoAcknowledgementAsync(
        Guid complaintId,
        string toEmail,
        EmailConfiguration config,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Load complaint with related data
            var complaints = await _unitOfWork.Repository<Complaint>()
                .FindAsync(c => c.Id == complaintId, cancellationToken);
            var complaint = complaints.FirstOrDefault();

            if (complaint == null)
            {
                return Result.Failure("Complaint not found");
            }

            // Load related entities for template variables
            var statusMaster = complaint.StatusMasterId != Guid.Empty
                ? await _unitOfWork.Repository<ComplaintStatusMaster>().GetByIdAsync(complaint.StatusMasterId, cancellationToken)
                : null;

            var priorityMaster = complaint.PriorityMasterId != Guid.Empty
                ? await _unitOfWork.Repository<ComplaintPriorityMaster>().GetByIdAsync(complaint.PriorityMasterId, cancellationToken)
                : null;

            var company = await _unitOfWork.Repository<Company>().GetByIdAsync(config.CompanyId, cancellationToken);

            string subject;
            string body;
            bool isHtml = true;

            // Try to use template if configured
            if (!string.IsNullOrEmpty(config.AutoAcknowledgementTemplateId) &&
                Guid.TryParse(config.AutoAcknowledgementTemplateId, out var templateId))
            {
                var templates = await _unitOfWork.Repository<CommunicationTemplate>()
                    .FindAsync(t => t.Id == templateId && t.IsActive, cancellationToken);
                var template = templates.FirstOrDefault();

                if (template != null)
                {
                    _logger.LogInformation("Using template {TemplateId} for auto-acknowledgement", templateId);

                    // Build template data dictionary with all available variables
                    var templateData = new Dictionary<string, object>
                    {
                        // Ticket variables
                        { "TicketNumber", complaint.ComplaintNumber ?? "N/A" },
                        { "ComplaintNumber", complaint.ComplaintNumber ?? "N/A" },
                        { "ComplaintId", complaint.Id.ToString() },
                        { "Title", complaint.Title ?? "No Title" },
                        { "Description", complaint.Description ?? "" },
                        { "Status", statusMaster?.Name ?? "New" },
                        { "Priority", priorityMaster?.Name ?? "Medium" },
                        { "SubmittedAt", complaint.SubmittedAt.ToString("yyyy-MM-dd HH:mm") },

                        // Company/System variables
                        { "CompanyName", company?.Name ?? "Support Team" },
                        { "SupportEmail", config.FromEmail },
                        { "FromEmail", config.FromEmail },
                        { "FromName", config.FromName },

                        // Customer variables (basic - can be enhanced)
                        { "CustomerName", "Valued Customer" }, // TODO: Extract from complainant if available
                        { "CustomerEmail", toEmail },

                        // URL variables (placeholder - configure based on your deployment)
                        { "StatusLink", $"https://yoursite.com/complaints/{complaint.Id}" },
                        { "TrackingUrl", $"https://yoursite.com/complaints/{complaint.Id}" },
                        { "ComplaintUrl", $"https://yoursite.com/complaints/{complaint.Id}" },

                        // Date/Time variables
                        { "CurrentDate", DateTime.UtcNow.ToString("MMMM dd, yyyy") },
                        { "CurrentTime", DateTime.UtcNow.ToString("HH:mm") }
                    };

                    // Process subject
                    subject = !string.IsNullOrEmpty(template.Subject)
                        ? _templateService.ProcessTemplate(template.Subject, templateData)
                        : $"Ticket Created: {complaint.ComplaintNumber}";

                    // Process body (use HTML if available, otherwise plain text)
                    body = !string.IsNullOrEmpty(template.HtmlBody)
                        ? _templateService.ProcessTemplate(template.HtmlBody, templateData)
                        : _templateService.ProcessTemplate(template.Body, templateData);

                    isHtml = !string.IsNullOrEmpty(template.HtmlBody);

                    _logger.LogInformation("Processed template for complaint {ComplaintId}, subject: {Subject}",
                        complaintId, subject);
                }
                else
                {
                    _logger.LogWarning("Template {TemplateId} not found or inactive, using default", templateId);
                    (subject, body) = GetDefaultAutoAcknowledgement(complaint, config);
                }
            }
            else
            {
                _logger.LogInformation("No template configured for auto-acknowledgement, using default");
                (subject, body) = GetDefaultAutoAcknowledgement(complaint, config);
            }

            // Send the email - IMPORTANT: Capture and check the result!
            _logger.LogInformation("AUTO-ACK: Calling SendTicketReplyAsync for complaint {ComplaintId} to {ToEmail}", complaintId, toEmail);
            var sendResult = await SendTicketReplyAsync(
                complaintId,
                toEmail,
                subject,
                body,
                isHtml: isHtml,
                isInternal: false,
                cancellationToken: cancellationToken);

            if (!sendResult.IsSuccess)
            {
                _logger.LogError("AUTO-ACK: SendTicketReplyAsync FAILED for complaint {ComplaintId}: {Error}",
                    complaintId, sendResult.Message);
                return Result.Failure($"Failed to send email: {sendResult.Message}");
            }

            _logger.LogInformation("AUTO-ACK: SUCCESS - Sent auto-acknowledgement for complaint {ComplaintId} to {Email}, EmailMessageId: {EmailId}",
                complaintId, toEmail, sendResult.Data);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending auto-acknowledgement for complaint {ComplaintId}", complaintId);
            return Result.Failure($"Error sending auto-acknowledgement: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets default auto-acknowledgement content when no template is configured
    /// </summary>
    private (string subject, string body) GetDefaultAutoAcknowledgement(Complaint complaint, EmailConfiguration config)
    {
        var subject = $"Ticket Created: {complaint.ComplaintNumber}";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='background: #4CAF50; color: white; padding: 20px; text-align: center;'>
                        <h2 style='margin: 0;'>✅ Ticket Created Successfully</h2>
                    </div>
                    <div style='background: #f9f9f9; padding: 20px; border: 1px solid #ddd;'>
                        <p>Dear Valued Customer,</p>
                        <p>Thank you for contacting us. We have received your request and created a support ticket.</p>

                        <div style='background: white; padding: 15px; border-left: 4px solid #4CAF50; margin: 20px 0;'>
                            <p style='margin: 5px 0;'><strong style='color: #4CAF50;'>Ticket Number:</strong> {complaint.ComplaintNumber}</p>
                            <p style='margin: 5px 0;'><strong style='color: #4CAF50;'>Subject:</strong> {complaint.Title}</p>
                            <p style='margin: 5px 0;'><strong style='color: #4CAF50;'>Status:</strong> New</p>
                            <p style='margin: 5px 0;'><strong style='color: #4CAF50;'>Submitted:</strong> {complaint.SubmittedAt:yyyy-MM-dd HH:mm}</p>
                        </div>

                        <h3>What Happens Next:</h3>
                        <p>Our support team will review your request and respond as soon as possible.</p>
                        <p><em>Need to add more details? Simply reply to this email.</em></p>
                    </div>
                    <div style='text-align: center; padding: 20px; color: #666; font-size: 12px;'>
                        <p>Best regards,<br/>
                        <strong>{config.FromName}</strong><br/>
                        {config.FromEmail}</p>
                    </div>
                </div>
            </body>
            </html>";

        return (subject, body);
    }

    #endregion

    #region Email Threading

    /// <summary>
    /// Finds parent complaint based on email threading headers
    /// </summary>
    public async Task<Guid?> FindParentComplaintFromThreadingAsync(
        string messageId,
        string? inReplyTo,
        string? references,
        string subject,
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        // Try to find by In-Reply-To header
        if (!string.IsNullOrEmpty(inReplyTo))
        {
            var emails = await _unitOfWork.Repository<EmailMessage>()
                .FindAsync(e => e.MessageId == inReplyTo && e.CompanyId == companyId, cancellationToken);
            var parentEmail = emails.FirstOrDefault();
            if (parentEmail?.ComplaintId != null)
            {
                return parentEmail.ComplaintId;
            }
        }

        // Try to find by References header
        if (!string.IsNullOrEmpty(references))
        {
            var emails = await _unitOfWork.Repository<EmailMessage>()
                .FindAsync(e => e.MessageId == references && e.CompanyId == companyId, cancellationToken);
            var parentEmail = emails.FirstOrDefault();
            if (parentEmail?.ComplaintId != null)
            {
                return parentEmail.ComplaintId;
            }
        }

        // Try to find by subject matching (look for "Re:" or ticket number in subject)
        var cleanSubject = CleanSubjectForMatching(subject);
        var ticketNumber = ExtractTicketNumber(subject);

        if (!string.IsNullOrEmpty(ticketNumber))
        {
            var complaints = await _unitOfWork.Repository<Complaint>()
                .FindAsync(c => c.ComplaintNumber == ticketNumber && c.CompanyId == companyId, cancellationToken);
            return complaints.FirstOrDefault()?.Id;
        }

        return null;
    }

    private string CleanSubjectForMatching(string subject)
    {
        // Remove Re:, Fwd:, etc.
        var cleaned = Regex.Replace(subject, @"^(Re|RE|Fw|FW|Fwd|FWD):\s*", "", RegexOptions.IgnoreCase);
        return cleaned.Trim();
    }

    private string? ExtractTicketNumber(string subject)
    {
        // Look for ticket numbers like CMP-20241111-0001
        var match = Regex.Match(subject, @"CMP-\d{8}-\d{4}");
        return match.Success ? match.Value : null;
    }

    #endregion

    #region Connection Testing

    public async Task<Result> TestImapConnectionAsync(
        EmailConfiguration config,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new ImapClient();
            await client.ConnectAsync(config.ImapHost, config.ImapPort, config.ImapUseSsl, cancellationToken);

            // Authenticate using OAuth2 or basic auth
            await AuthenticateImapClientAsync(client, config, cancellationToken);

            await client.DisconnectAsync(true, cancellationToken);

            return Result.Success("IMAP connection successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "IMAP connection test failed");
            return Result.Failure($"IMAP connection failed: {ex.Message}");
        }
    }

    public async Task<Result> TestSmtpConnectionAsync(
        EmailConfiguration config,
        string testRecipient,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Use separate SMTP From address if configured, otherwise use main account
            var fromEmail = config.UseSeparateSmtpAccount && !string.IsNullOrEmpty(config.SmtpSeparateFromEmail)
                ? config.SmtpSeparateFromEmail
                : config.FromEmail;
            var fromName = config.UseSeparateSmtpAccount && !string.IsNullOrEmpty(config.SmtpSeparateFromName)
                ? config.SmtpSeparateFromName
                : config.FromName;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(MailboxAddress.Parse(testRecipient));
            message.Subject = "Test Email from Complaint Management System";
            message.Body = new TextPart("plain")
            {
                Text = config.UseSeparateSmtpAccount
                    ? $"This is a test email to verify SMTP configuration using separate SMTP account ({fromEmail})."
                    : "This is a test email to verify SMTP configuration."
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(config.SmtpHost, config.SmtpPort, config.SmtpUseSsl, cancellationToken);

            // Authenticate using OAuth2 or basic auth (supports separate SMTP account)
            await AuthenticateSmtpClientAsync(client, config, cancellationToken);

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            var successMessage = config.UseSeparateSmtpAccount
                ? $"SMTP connection successful using separate account ({fromEmail}), test email sent"
                : "SMTP connection successful, test email sent";

            return Result.Success(successMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP connection test failed");
            return Result.Failure($"SMTP connection failed: {ex.Message}");
        }
    }

    #endregion

    #region Helper Methods

    private async Task SaveEmailMessageAsync(
        MimeMessage mimeMessage,
        EmailConfiguration config,
        Guid? complaintId,
        EmailStatus status,
        CancellationToken cancellationToken)
    {
        var fromAddress = mimeMessage.From.Mailboxes.FirstOrDefault()?.Address ?? "";
        var fromName = mimeMessage.From.Mailboxes.FirstOrDefault()?.Name ?? "";
        var toAddress = mimeMessage.To.Mailboxes.FirstOrDefault()?.Address ?? "";
        var body = GetEmailBody(mimeMessage);

        var emailMessage = new EmailMessage
        {
            Id = Guid.NewGuid(),
            CompanyId = config.CompanyId,
            ComplaintId = complaintId,
            MessageId = mimeMessage.MessageId ?? Guid.NewGuid().ToString(),
            InReplyTo = mimeMessage.InReplyTo,
            References = mimeMessage.References?.FirstOrDefault(),
            Subject = mimeMessage.Subject ?? "(No Subject)",
            FromEmail = fromAddress,
            FromName = fromName,
            ToEmail = toAddress,
            TextBody = body,
            HtmlBody = mimeMessage.HtmlBody,
            IsHtml = !string.IsNullOrEmpty(mimeMessage.HtmlBody),
            Direction = EmailDirection.Inbound,
            Status = status,
            ReceivedAt = mimeMessage.Date.UtcDateTime,
            ProcessedAt = DateTime.UtcNow,
            RawHeaders = mimeMessage.Headers.ToString(),
            ThreadPosition = 0 // Will be updated if threading is detected
        };

        await _unitOfWork.Repository<EmailMessage>().AddAsync(emailMessage, cancellationToken);

        // Handle attachments
        foreach (var attachment in mimeMessage.Attachments)
        {
            if (attachment is MimePart mimePart)
            {
                await SaveAttachmentAsync(emailMessage.Id, mimePart, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task SaveAttachmentAsync(Guid emailMessageId, MimePart mimePart, CancellationToken cancellationToken)
    {
        // For now, just save metadata - actual file storage would need to be implemented
        var attachment = new EmailAttachment
        {
            Id = Guid.NewGuid(),
            EmailMessageId = emailMessageId,
            FileName = mimePart.FileName ?? "attachment",
            ContentType = mimePart.ContentType.MimeType,
            FileSizeBytes = 0, // Would need to calculate from stream
            FileExtension = Path.GetExtension(mimePart.FileName ?? ""),
            StoragePath = $"email-attachments/{emailMessageId}/{mimePart.FileName}",
            IsInline = mimePart.ContentDisposition?.Disposition == "inline",
            ContentId = mimePart.ContentId,
            IsScanned = false,
            IsSafe = true
        };

        await _unitOfWork.Repository<EmailAttachment>().AddAsync(attachment, cancellationToken);
    }

    private string GetEmailBody(MimeMessage message)
    {
        if (!string.IsNullOrEmpty(message.TextBody))
        {
            return message.TextBody;
        }
        else if (!string.IsNullOrEmpty(message.HtmlBody))
        {
            // Strip HTML tags for text version
            return Regex.Replace(message.HtmlBody, "<.*?>", string.Empty);
        }
        return "(No content)";
    }

    private bool IsAutoReplyOrBounce(MimeMessage message)
    {
        // Check for auto-reply headers
        if (message.Headers.Contains("Auto-Submitted") &&
            message.Headers["Auto-Submitted"] != "no")
        {
            return true;
        }

        // Check for delivery status notifications
        if (message.Headers.Contains("Content-Type") &&
            message.Headers["Content-Type"].Contains("delivery-status"))
        {
            return true;
        }

        // Check for out-of-office replies
        if (message.Subject != null &&
            (message.Subject.Contains("Out of Office", StringComparison.OrdinalIgnoreCase) ||
             message.Subject.Contains("Automatic reply", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return false;
    }

    private async Task UpdateComplaintStatusOnNewEmailAsync(Guid complaintId, CancellationToken cancellationToken)
    {
        // Update complaint status when new email arrives (e.g., from "Waiting on Customer" to "Customer Responded")
        var complaint = await _unitOfWork.Repository<Complaint>().GetByIdAsync(complaintId, cancellationToken);
        if (complaint != null)
        {
            // Mark complaint as having a customer response that needs attention
            complaint.HasCustomerResponse = true;
            complaint.LastResponseFrom = "Customer";
            complaint.LastResponseAt = DateTime.UtcNow;
            complaint.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Complaint>().Update(complaint);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("New email received for complaint {ComplaintId} - HasCustomerResponse set to true", complaintId);
        }
    }

    /// <summary>
    /// Authenticates IMAP client using OAuth2 or basic authentication
    /// </summary>
    private async Task AuthenticateImapClientAsync(
        ImapClient client,
        EmailConfiguration config,
        CancellationToken cancellationToken)
    {
        if (config.AuthenticationType == EmailAuthenticationType.OAuth2)
        {
            // Check if token needs refresh
            if (_oauthService.TokenNeedsRefresh(config.OAuthTokenExpiresAt))
            {
                if (string.IsNullOrEmpty(config.ImapUsername))
                {
                    throw new UnauthorizedAccessException("Email address is missing. Please complete email configuration.");
                }

                // Refresh the access token using database refresh token (not MSAL cache)
                // This works with tokens obtained via browser OAuth flow
                var tokenResult = await _oauthService.RefreshAccessTokenAsync(config);

                // Update configuration with new tokens
                config.OAuthAccessToken = tokenResult.AccessToken;
                if (!string.IsNullOrEmpty(tokenResult.RefreshToken))
                {
                    config.OAuthRefreshToken = tokenResult.RefreshToken;
                }
                config.OAuthTokenExpiresAt = tokenResult.ExpiresOn;

                // Save updated tokens
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Refreshed OAuth access token for email configuration {ConfigId}, new expiry: {ExpiresAt}",
                    config.Id, config.OAuthTokenExpiresAt);
            }

            // Authenticate with OAuth2
            var oauth2 = new SaslMechanismOAuth2(config.ImapUsername, config.OAuthAccessToken);
            await client.AuthenticateAsync(oauth2, cancellationToken);
        }
        else
        {
            // Basic authentication - SECURITY: Decrypt password before use
            if (string.IsNullOrEmpty(config.ImapPassword))
            {
                throw new InvalidOperationException("IMAP password is required for basic authentication");
            }

            string decryptedPassword;
            try
            {
                decryptedPassword = _encryptionService.DecryptPassword(config.ImapPassword);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to decrypt IMAP password for config {ConfigId}", config.Id);
                throw new InvalidOperationException("Failed to decrypt IMAP password. Please update your email configuration.", ex);
            }

            await client.AuthenticateAsync(config.ImapUsername, decryptedPassword, cancellationToken);
        }
    }

    /// <summary>
    /// Authenticates SMTP client using OAuth2 or basic authentication
    /// Supports separate SMTP account when UseSeparateSmtpAccount = true
    /// </summary>
    private async Task AuthenticateSmtpClientAsync(
        SmtpClient client,
        EmailConfiguration config,
        CancellationToken cancellationToken)
    {
        // Determine which credentials to use based on UseSeparateSmtpAccount setting
        var useSeparateAccount = config.UseSeparateSmtpAccount;
        var authType = useSeparateAccount && config.SmtpAuthenticationType.HasValue
            ? config.SmtpAuthenticationType.Value
            : config.AuthenticationType;

        if (authType == EmailAuthenticationType.OAuth2)
        {
            // OAuth2 Authentication
            if (useSeparateAccount)
            {
                // Use separate SMTP OAuth credentials
                _logger.LogInformation("Using separate SMTP OAuth account for authentication");

                // Check if separate SMTP token needs refresh
                if (_oauthService.TokenNeedsRefresh(config.SmtpSeparateOAuthTokenExpiresAt))
                {
                    if (string.IsNullOrEmpty(config.SmtpSeparateUsername))
                    {
                        throw new UnauthorizedAccessException("Separate SMTP email address is missing. Please complete SMTP configuration.");
                    }

                    _logger.LogInformation("Refreshing separate SMTP OAuth access token for email configuration {ConfigId}", config.Id);

                    // Refresh the separate SMTP access token
                    var tokenResult = await _oauthService.RefreshSmtpAccessTokenAsync(config);

                    // Update configuration with new SMTP tokens
                    config.SmtpSeparateOAuthAccessToken = tokenResult.AccessToken;
                    if (!string.IsNullOrEmpty(tokenResult.RefreshToken))
                    {
                        config.SmtpSeparateOAuthRefreshToken = tokenResult.RefreshToken;
                    }
                    config.SmtpSeparateOAuthTokenExpiresAt = tokenResult.ExpiresOn;

                    // Save updated tokens
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Refreshed separate SMTP OAuth access token for email configuration {ConfigId}, new expiry: {ExpiresAt}",
                        config.Id, config.SmtpSeparateOAuthTokenExpiresAt);
                }

                // Authenticate with separate SMTP OAuth2 credentials
                var oauth2 = new SaslMechanismOAuth2(config.SmtpSeparateUsername, config.SmtpSeparateOAuthAccessToken);
                await client.AuthenticateAsync(oauth2, cancellationToken);
            }
            else
            {
                // Use main account OAuth credentials (backward compatibility)
                _logger.LogInformation("Using main account OAuth credentials for SMTP authentication");

                // Check if token needs refresh
                if (_oauthService.TokenNeedsRefresh(config.OAuthTokenExpiresAt))
                {
                    if (string.IsNullOrEmpty(config.SmtpUsername))
                    {
                        throw new UnauthorizedAccessException("Email address is missing. Please complete email configuration.");
                    }

                    // Refresh the access token using database refresh token (not MSAL cache)
                    // This works with tokens obtained via browser OAuth flow
                    var tokenResult = await _oauthService.RefreshAccessTokenAsync(config);

                    // Update configuration with new tokens
                    config.OAuthAccessToken = tokenResult.AccessToken;
                    if (!string.IsNullOrEmpty(tokenResult.RefreshToken))
                    {
                        config.OAuthRefreshToken = tokenResult.RefreshToken;
                    }
                    config.OAuthTokenExpiresAt = tokenResult.ExpiresOn;

                    // Save updated tokens
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Refreshed OAuth access token for email configuration {ConfigId}, new expiry: {ExpiresAt}",
                        config.Id, config.OAuthTokenExpiresAt);
                }

                // Authenticate with OAuth2
                var oauth2 = new SaslMechanismOAuth2(config.SmtpUsername, config.OAuthAccessToken);
                await client.AuthenticateAsync(oauth2, cancellationToken);
            }
        }
        else
        {
            // Basic Authentication
            if (useSeparateAccount)
            {
                // Use separate SMTP basic auth credentials
                _logger.LogInformation("Using separate SMTP account with basic authentication");

                if (string.IsNullOrEmpty(config.SmtpSeparateUsername) || string.IsNullOrEmpty(config.SmtpSeparatePassword))
                {
                    throw new UnauthorizedAccessException("Separate SMTP username or password is missing. Please complete SMTP configuration.");
                }

                // SECURITY: Decrypt separate SMTP password before use
                string decryptedPassword;
                try
                {
                    decryptedPassword = _encryptionService.DecryptPassword(config.SmtpSeparatePassword);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to decrypt separate SMTP password for config {ConfigId}", config.Id);
                    throw new InvalidOperationException("Failed to decrypt separate SMTP password. Please update your email configuration.", ex);
                }

                await client.AuthenticateAsync(config.SmtpSeparateUsername, decryptedPassword, cancellationToken);
            }
            else
            {
                // Use main account credentials (backward compatibility)
                _logger.LogInformation("Using main account credentials for SMTP basic authentication");

                // SECURITY: Decrypt password before use
                if (string.IsNullOrEmpty(config.SmtpPassword))
                {
                    throw new InvalidOperationException("SMTP password is required for basic authentication");
                }

                string decryptedPassword;
                try
                {
                    decryptedPassword = _encryptionService.DecryptPassword(config.SmtpPassword);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to decrypt SMTP password for config {ConfigId}", config.Id);
                    throw new InvalidOperationException("Failed to decrypt SMTP password. Please update your email configuration.", ex);
                }

                await client.AuthenticateAsync(config.SmtpUsername, decryptedPassword, cancellationToken);
            }
        }
    }

    #endregion
}
