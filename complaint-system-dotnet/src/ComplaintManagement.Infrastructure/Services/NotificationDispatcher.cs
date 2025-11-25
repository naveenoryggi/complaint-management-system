using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Communication;
using ComplaintManagement.Domain.Entities.Events;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Notification dispatcher for event-driven notifications
/// </summary>
public class NotificationDispatcher : INotificationDispatcher
{
    private readonly ComplaintDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ITemplateService _templateService;
    private readonly ILogger<NotificationDispatcher> _logger;

    public NotificationDispatcher(
        ComplaintDbContext context,
        IEmailService emailService,
        ITemplateService templateService,
        ILogger<NotificationDispatcher> logger)
    {
        _context = context;
        _emailService = emailService;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task<Result> DispatchEventNotificationsAsync(
        string eventCode,
        Guid entityId,
        Dictionary<string, object> data,
        Guid? companyId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Dispatching notifications for event {EventCode}, entity {EntityId}",
                eventCode, entityId);

            // Find event type
            var eventType = await _context.EventTypes
                .FirstOrDefaultAsync(
                    e => e.Code == eventCode && e.IsActive &&
                    (companyId == null || e.CompanyId == null || e.CompanyId == companyId),
                    cancellationToken);

            if (eventType == null)
            {
                _logger.LogWarning("Event type {EventCode} not found or inactive", eventCode);
                return Result.Failure($"Event type '{eventCode}' not found");
            }

            // Get all active communication rules for this event, ordered by priority
            var rules = await _context.EventCommunicationRules
                .Include(r => r.Template)
                .Where(r => r.EventTypeId == eventType.Id && r.IsActive)
                .Where(r => companyId == null || r.CompanyId == null || r.CompanyId == companyId)
                .OrderBy(r => r.Priority)
                .ToListAsync(cancellationToken);

            if (!rules.Any())
            {
                _logger.LogInformation("No active communication rules found for event {EventCode}", eventCode);
                return Result.Success();
            }

            _logger.LogInformation("Found {RuleCount} active communication rules for event {EventCode}",
                rules.Count, eventCode);

            var successCount = 0;
            var failureCount = 0;

            foreach (var rule in rules)
            {
                try
                {
                    _logger.LogDebug("Processing rule {RuleId} ({RuleName}) - Priority: {Priority}, Channel: {Channel}, RecipientType: {RecipientType}",
                        rule.Id, rule.Name, rule.Priority, rule.Channel, rule.RecipientType);

                    // Check conditions if any
                    if (!string.IsNullOrEmpty(rule.Conditions))
                    {
                        if (!EvaluateConditions(rule.Conditions, data))
                        {
                            _logger.LogDebug("Rule {RuleId} conditions not met, skipping", rule.Id);
                            continue;
                        }
                    }

                    // Apply delay if configured
                    if (rule.DelayMinutes > 0)
                    {
                        _logger.LogInformation(
                            "Rule {RuleId} has delay of {DelayMinutes} minutes, skipping immediate dispatch",
                            rule.Id, rule.DelayMinutes);
                        // TODO: Implement delayed notification scheduling
                        continue;
                    }

                    // Determine recipients
                    _logger.LogDebug("Determining recipients for rule {RuleId} with RecipientType: {RecipientType}",
                        rule.Id, rule.RecipientType);

                    var recipients = await DetermineRecipientsAsync(rule, entityId, data, cancellationToken);

                    _logger.LogDebug("Found {RecipientCount} recipients for rule {RuleId}",
                        recipients.Count(), rule.Id);

                    if (!recipients.Any())
                    {
                        _logger.LogWarning("No recipients determined for rule {RuleId}", rule.Id);
                        continue;
                    }

                    // Process template
                    _logger.LogDebug("Processing template for rule {RuleId}, Template: {TemplateId}",
                        rule.Id, rule.TemplateId);

                    string? processedContent = null;
                    string? subject = null;

                    if (rule.Template != null)
                    {
                        processedContent = _templateService.ProcessTemplate(
                            rule.Template.HtmlBody ?? rule.Template.Body,
                            data);

                        if (!string.IsNullOrEmpty(rule.Template.Subject))
                        {
                            subject = _templateService.ProcessTemplate(rule.Template.Subject, data);
                        }

                        _logger.LogDebug("Template processed for rule {RuleId}, Subject: {Subject}",
                            rule.Id, subject);
                    }

                    // Send notifications based on channel
                    _logger.LogDebug("Sending {Channel} notifications for rule {RuleId} to {RecipientCount} recipients",
                        rule.Channel, rule.Id, recipients.Count());

                    switch (rule.Channel)
                    {
                        case Domain.Enums.CommunicationChannel.Email:
                            await SendEmailNotificationsAsync(
                                rule, recipients, subject, processedContent, entityId, companyId, cancellationToken);
                            break;

                        case Domain.Enums.CommunicationChannel.SMS:
                            _logger.LogWarning("SMS notifications not yet implemented");
                            break;

                        case Domain.Enums.CommunicationChannel.WhatsApp:
                            _logger.LogWarning("WhatsApp notifications not yet implemented");
                            break;

                        default:
                            _logger.LogWarning("Unsupported channel {Channel}", rule.Channel);
                            break;
                    }

                    successCount++;
                    _logger.LogInformation("Successfully processed rule {RuleId} ({RuleName})",
                        rule.Id, rule.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error processing rule {RuleId} ({RuleName}) - Channel: {Channel}, RecipientType: {RecipientType}. " +
                        "Error: {ErrorMessage}. StackTrace: {StackTrace}",
                        rule.Id, rule.Name, rule.Channel, rule.RecipientType, ex.Message, ex.StackTrace);
                    failureCount++;
                }
            }

            _logger.LogInformation(
                "Notification dispatch completed: {SuccessCount} successful, {FailureCount} failed",
                successCount, failureCount);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching event notifications for {EventCode}", eventCode);
            return Result.Failure($"Failed to dispatch notifications: {ex.Message}");
        }
    }

    public async Task<Result<EventType>> RegisterEventTypeAsync(
        string code,
        string name,
        string entityType,
        string? description = null,
        string? category = null,
        List<string>? availableFields = null,
        Guid? companyId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if event type already exists
            var existing = await _context.EventTypes
                .FirstOrDefaultAsync(e => e.Code == code && e.CompanyId == companyId, cancellationToken);

            if (existing != null)
            {
                return Result<EventType>.Failure($"Event type with code '{code}' already exists");
            }

            var eventType = new EventType
            {
                Code = code,
                Name = name,
                EntityType = entityType,
                Description = description,
                Category = category,
                AvailableFields = availableFields != null
                    ? JsonSerializer.Serialize(availableFields)
                    : null,
                IsActive = true,
                IsSystem = false,
                CompanyId = companyId
            };

            _context.EventTypes.Add(eventType);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Registered new event type {Code}", code);

            return Result<EventType>.Success(eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering event type {Code}", code);
            return Result<EventType>.Failure($"Failed to register event type: {ex.Message}");
        }
    }

    private bool EvaluateConditions(string conditionsJson, Dictionary<string, object> data)
    {
        try
        {
            // Simple condition evaluation
            // TODO: Implement more sophisticated condition evaluation
            var conditions = JsonSerializer.Deserialize<Dictionary<string, object>>(conditionsJson);

            if (conditions == null)
            {
                return true;
            }

            foreach (var condition in conditions)
            {
                if (!data.TryGetValue(condition.Key, out var value))
                {
                    return false;
                }

                if (!Equals(value, condition.Value))
                {
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating conditions");
            return false;
        }
    }

    private async Task<List<string>> DetermineRecipientsAsync(
        EventCommunicationRule rule,
        Guid entityId,
        Dictionary<string, object> data,
        CancellationToken cancellationToken)
    {
        var recipients = new List<string>();

        try
        {
            // Get complaint for organizational context
            var complaint = await _context.Complaints
                .Include(c => c.Company)
                    .ThenInclude(c => c.Manager)
                .Include(c => c.Company)
                    .ThenInclude(c => c.SecondaryManager)
                .Include(c => c.Company)
                    .ThenInclude(c => c.HrResponsible)
                .Include(c => c.Branch)
                    .ThenInclude(b => b.Manager)
                .Include(c => c.Branch)
                    .ThenInclude(b => b.SecondaryManager)
                .Include(c => c.Branch)
                    .ThenInclude(b => b.HrResponsible)
                .Include(c => c.Department)
                    .ThenInclude(d => d.Manager)
                .Include(c => c.Department)
                    .ThenInclude(d => d.SecondaryManager)
                .Include(c => c.Department)
                    .ThenInclude(d => d.HrResponsible)
                .Include(c => c.Section)
                    .ThenInclude(s => s.Head)
                .Include(c => c.Section)
                    .ThenInclude(s => s.SecondaryHead)
                .Include(c => c.Section)
                    .ThenInclude(s => s.HrResponsible)
                .Include(c => c.Complainant)
                    .ThenInclude(u => u.Manager)
                .Include(c => c.AssignedTo)
                    .ThenInclude(a => a.Manager)
                .FirstOrDefaultAsync(c => c.Id == entityId, cancellationToken);

            switch (rule.RecipientType)
            {
                case Domain.Enums.RecipientType.SpecificEmails:
                    if (!string.IsNullOrEmpty(rule.SpecificEmails))
                    {
                        var emails = JsonSerializer.Deserialize<List<string>>(rule.SpecificEmails);
                        if (emails != null)
                        {
                            recipients.AddRange(emails);
                        }
                    }
                    break;

                case Domain.Enums.RecipientType.SpecificUsers:
                    if (!string.IsNullOrEmpty(rule.SpecificUserIds))
                    {
                        var userIds = JsonSerializer.Deserialize<List<Guid>>(rule.SpecificUserIds);
                        if (userIds != null)
                        {
                            var users = await _context.Users
                                .Where(u => userIds.Contains(u.Id) && !u.IsDeleted)
                                .Select(u => u.Email)
                                .ToListAsync(cancellationToken);

                            recipients.AddRange(users.Where(e => !string.IsNullOrEmpty(e))!);
                        }
                    }
                    break;

                case Domain.Enums.RecipientType.SpecificRoles:
                    if (!string.IsNullOrEmpty(rule.SpecificRoleIds))
                    {
                        var roleIds = JsonSerializer.Deserialize<List<Guid>>(rule.SpecificRoleIds);
                        if (roleIds != null)
                        {
                            var users = await _context.UserComplaintRoles
                                .Where(ur => roleIds.Contains(ur.ComplaintRoleId) && !ur.User.IsDeleted)
                                .Select(ur => ur.User.Email)
                                .ToListAsync(cancellationToken);

                            recipients.AddRange(users.Where(e => !string.IsNullOrEmpty(e))!);
                        }
                    }
                    break;

                case Domain.Enums.RecipientType.Complainant:
                    if (complaint?.Complainant?.Email != null)
                    {
                        recipients.Add(complaint.Complainant.Email);
                    }
                    break;

                case Domain.Enums.RecipientType.AssignedHandler:
                    if (complaint?.AssignedTo?.Email != null)
                    {
                        recipients.Add(complaint.AssignedTo.Email);
                    }
                    break;

                case Domain.Enums.RecipientType.ComplainantManager:
                    if (complaint?.Complainant?.Manager?.Email != null)
                    {
                        recipients.Add(complaint.Complainant.Manager.Email);
                    }
                    break;

                case Domain.Enums.RecipientType.HandlerManager:
                    if (complaint?.AssignedTo?.Manager?.Email != null)
                    {
                        recipients.Add(complaint.AssignedTo.Manager.Email);
                    }
                    break;

                case Domain.Enums.RecipientType.Administrators:
                    var adminRoles = await _context.ComplaintRoles
                        .Where(r => r.RoleType == Domain.Enums.RoleType.SystemAdmin && !r.IsDeleted)
                        .Select(r => r.Id)
                        .ToListAsync(cancellationToken);

                    if (adminRoles.Any())
                    {
                        var adminEmails = await _context.UserComplaintRoles
                            .Where(ur => adminRoles.Contains(ur.ComplaintRoleId) && !ur.User.IsDeleted)
                            .Select(ur => ur.User.Email)
                            .ToListAsync(cancellationToken);

                        recipients.AddRange(adminEmails.Where(e => !string.IsNullOrEmpty(e))!);
                    }
                    break;

                // Company Level Recipients
                case Domain.Enums.RecipientType.CompanyManager:
                    if (complaint?.Company?.Manager?.Email != null)
                    {
                        recipients.Add(complaint.Company.Manager.Email);
                    }
                    break;

                case Domain.Enums.RecipientType.CompanySecondaryManager:
                    if (complaint?.Company?.SecondaryManager?.Email != null)
                    {
                        recipients.Add(complaint.Company.SecondaryManager.Email);
                    }
                    break;

                case Domain.Enums.RecipientType.CompanyHR:
                    if (complaint?.Company?.HrResponsible?.Email != null)
                    {
                        recipients.Add(complaint.Company.HrResponsible.Email);
                    }
                    break;

                case Domain.Enums.RecipientType.CompanyContactEmail:
                    if (complaint?.Company?.ContactEmail != null)
                    {
                        recipients.Add(complaint.Company.ContactEmail);
                    }
                    break;

                case Domain.Enums.RecipientType.AllCompanyUsers:
                    if (complaint?.CompanyId != null)
                    {
                        var companyUsers = await _context.Users
                            .Where(u => u.CompanyId == complaint.CompanyId && !u.IsDeleted)
                            .Select(u => u.Email)
                            .ToListAsync(cancellationToken);

                        recipients.AddRange(companyUsers.Where(e => !string.IsNullOrEmpty(e))!);
                    }
                    break;

                // Branch Level Recipients
                case Domain.Enums.RecipientType.BranchManager:
                    if (complaint?.Branch?.Manager?.Email != null)
                    {
                        recipients.Add(complaint.Branch.Manager.Email);
                    }
                    break;

                case Domain.Enums.RecipientType.BranchSecondaryManager:
                    if (complaint?.Branch?.SecondaryManager?.Email != null)
                    {
                        recipients.Add(complaint.Branch.SecondaryManager.Email);
                    }
                    break;

                case Domain.Enums.RecipientType.BranchHR:
                    if (complaint?.Branch?.HrResponsible?.Email != null)
                    {
                        recipients.Add(complaint.Branch.HrResponsible.Email);
                    }
                    break;

                case Domain.Enums.RecipientType.BranchContactEmail:
                    if (complaint?.Branch?.ContactEmail != null)
                    {
                        recipients.Add(complaint.Branch.ContactEmail);
                    }
                    break;

                case Domain.Enums.RecipientType.AllBranchUsers:
                    if (complaint?.BranchId != null)
                    {
                        var branchUsers = await _context.Users
                            .Where(u => u.BranchId == complaint.BranchId && !u.IsDeleted)
                            .Select(u => u.Email)
                            .ToListAsync(cancellationToken);

                        recipients.AddRange(branchUsers.Where(e => !string.IsNullOrEmpty(e))!);
                    }
                    break;

                // Department Level Recipients
                case Domain.Enums.RecipientType.DepartmentManager:
                    if (complaint?.Department?.Manager?.Email != null)
                    {
                        recipients.Add(complaint.Department.Manager.Email);
                    }
                    break;

                case Domain.Enums.RecipientType.DepartmentSecondaryManager:
                    if (complaint?.Department?.SecondaryManager?.Email != null)
                    {
                        recipients.Add(complaint.Department.SecondaryManager.Email);
                    }
                    break;

                case Domain.Enums.RecipientType.DepartmentHR:
                    if (complaint?.Department?.HrResponsible?.Email != null)
                    {
                        recipients.Add(complaint.Department.HrResponsible.Email);
                    }
                    break;

                case Domain.Enums.RecipientType.Department:
                case Domain.Enums.RecipientType.AllDepartmentUsers:
                    if (complaint?.DepartmentId != null)
                    {
                        var deptUsers = await _context.Users
                            .Where(u => u.DepartmentId == complaint.DepartmentId && !u.IsDeleted)
                            .Select(u => u.Email)
                            .ToListAsync(cancellationToken);

                        recipients.AddRange(deptUsers.Where(e => !string.IsNullOrEmpty(e))!);
                    }
                    break;

                // Section Level Recipients
                case Domain.Enums.RecipientType.SectionHead:
                    if (complaint?.Section?.Head?.Email != null)
                    {
                        recipients.Add(complaint.Section.Head.Email);
                    }
                    break;

                case Domain.Enums.RecipientType.SectionSecondaryHead:
                    if (complaint?.Section?.SecondaryHead?.Email != null)
                    {
                        recipients.Add(complaint.Section.SecondaryHead.Email);
                    }
                    break;

                case Domain.Enums.RecipientType.SectionHR:
                    if (complaint?.Section?.HrResponsible?.Email != null)
                    {
                        recipients.Add(complaint.Section.HrResponsible.Email);
                    }
                    break;

                case Domain.Enums.RecipientType.Section:
                case Domain.Enums.RecipientType.AllSectionUsers:
                    if (complaint?.SectionId != null)
                    {
                        var sectionUsers = await _context.Users
                            .Where(u => u.SectionId == complaint.SectionId && !u.IsDeleted)
                            .Select(u => u.Email)
                            .ToListAsync(cancellationToken);

                        recipients.AddRange(sectionUsers.Where(e => !string.IsNullOrEmpty(e))!);
                    }
                    break;

                default:
                    _logger.LogWarning("Recipient type {RecipientType} not implemented", rule.RecipientType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error determining recipients for rule {RuleId}", rule.Id);
        }

        return recipients.Where(r => !string.IsNullOrWhiteSpace(r)).Distinct().ToList();
    }

    private async Task SendEmailNotificationsAsync(
        EventCommunicationRule rule,
        List<string> recipients,
        string? subject,
        string? body,
        Guid entityId,
        Guid? companyId,
        CancellationToken cancellationToken)
    {
        foreach (var recipient in recipients)
        {
            try
            {
                var result = await _emailService.SendEmailAsync(
                    recipient,
                    subject ?? "Notification",
                    body ?? "You have a new notification",
                    isHtml: true,
                    cancellationToken: cancellationToken);

                // Log communication
                var log = new CommunicationLog
                {
                    Channel = Domain.Enums.CommunicationChannel.Email,
                    TemplateId = rule.TemplateId,
                    EventCommunicationRuleId = rule.Id,
                    RecipientEmail = recipient,
                    Subject = subject,
                    Body = body ?? string.Empty,
                    Status = result.IsSuccess
                        ? Domain.Enums.CommunicationStatus.Sent
                        : Domain.Enums.CommunicationStatus.Failed,
                    SentAt = result.IsSuccess ? DateTime.UtcNow : null,
                    ErrorMessage = result.IsSuccess ? null : result.Message,
                    EntityId = entityId,
                    EntityType = "Complaint", // TODO: Make dynamic
                    CompanyId = companyId,
                    RetryCount = 0
                };

                _context.CommunicationLogs.Add(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Recipient}", recipient);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
