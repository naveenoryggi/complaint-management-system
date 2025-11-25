# SMS/WhatsApp Integration Implementation Plan

## Overview

This document outlines the implementation plan for SMS and WhatsApp messaging integration in the Complaint Management System.

## Current Status

✅ **Already Implemented:**
- `CommunicationChannel` enum includes SMS and WhatsApp
- Event-driven notification system in place
- Template system supports multiple channels
- Notification rules engine ready for multi-channel support

## Architecture

### Communication Flow

```
[Event Trigger] → [NotificationDispatcher] → [Channel Router] → [SMS/WhatsApp Service] → [Provider API]
                                                                                             ↓
                                                                        [Twilio/WhatsApp Business API]
```

### Components Required

1. **SMS Service** (`ISmsService`, `SmsService`)
   - Send SMS messages
   - Handle delivery status callbacks
   - Support multiple providers (Twilio, AWS SNS, etc.)

2. **WhatsApp Service** (`IWhatsAppService`, `WhatsAppService`)
   - Send WhatsApp messages
   - Handle message templates
   - Support WhatsApp Business API

3. **Provider Abstraction**
   - `ISmsProvider` interface
   - `TwilioSmsProvider` implementation
   - `AwsSnsProvider` implementation (optional)

4. **Configuration Models**
   - `SmsConfiguration` entity
   - `WhatsAppConfiguration` entity

5. **Message Queue** (optional but recommended)
   - Queue for rate limiting
   - Retry logic for failed messages
   - Delivery tracking

---

## Implementation Steps

### Phase 1: Backend Services (Estimated: 4-6 hours)

#### Step 1.1: Create SMS Provider Interface

**File:** `ComplaintManagement.Application/Interfaces/Services/ISmsProvider.cs`

```csharp
public interface ISmsProvider
{
    Task<SmsResult> SendSmsAsync(SmsMessage message, CancellationToken cancellationToken = default);
    Task<SmsDeliveryStatus> GetDeliveryStatusAsync(string messageId, CancellationToken cancellationToken = default);
    string ProviderName { get; }
}

public class SmsMessage
{
    public string To { get; set; }
    public string From { get; set; }
    public string Body { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
}

public class SmsResult
{
    public bool Success { get; set; }
    public string MessageId { get; set; }
    public string ErrorMessage { get; set; }
    public decimal Cost { get; set; }
}

public enum SmsDeliveryStatus
{
    Pending,
    Sent,
    Delivered,
    Failed,
    Undelivered
}
```

#### Step 1.2: Implement Twilio SMS Provider

**File:** `ComplaintManagement.Infrastructure/Services/TwilioSmsProvider.cs`

```csharp
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

public class TwilioSmsProvider : ISmsProvider
{
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _fromNumber;

    public string ProviderName => "Twilio";

    public TwilioSmsProvider(IConfiguration configuration)
    {
        _accountSid = configuration["Twilio:AccountSid"];
        _authToken = configuration["Twilio:AuthToken"];
        _fromNumber = configuration["Twilio:FromNumber"];

        TwilioClient.Init(_accountSid, _authToken);
    }

    public async Task<SmsResult> SendSmsAsync(
        SmsMessage message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var messageResource = await MessageResource.CreateAsync(
                to: new PhoneNumber(message.To),
                from: new PhoneNumber(message.From ?? _fromNumber),
                body: message.Body
            );

            return new SmsResult
            {
                Success = messageResource.Status != MessageResource.StatusEnum.Failed,
                MessageId = messageResource.Sid,
                Cost = messageResource.Price ?? 0
            };
        }
        catch (Exception ex)
        {
            return new SmsResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<SmsDeliveryStatus> GetDeliveryStatusAsync(
        string messageId,
        CancellationToken cancellationToken = default)
    {
        var message = await MessageResource.FetchAsync(messageId);

        return message.Status switch
        {
            MessageResource.StatusEnum.Queued => SmsDeliveryStatus.Pending,
            MessageResource.StatusEnum.Sent => SmsDeliveryStatus.Sent,
            MessageResource.StatusEnum.Delivered => SmsDeliveryStatus.Delivered,
            MessageResource.StatusEnum.Failed => SmsDeliveryStatus.Failed,
            MessageResource.StatusEnum.Undelivered => SmsDeliveryStatus.Undelivered,
            _ => SmsDeliveryStatus.Pending
        };
    }
}
```

#### Step 1.3: Create SMS Service

**File:** `ComplaintManagement.Application/Interfaces/Services/ISmsService.cs`

```csharp
public interface ISmsService
{
    Task<bool> SendComplaintNotificationAsync(
        string complaintNumber,
        string recipientPhone,
        string templateCode,
        Dictionary<string, object> variables,
        CancellationToken cancellationToken = default);

    Task<bool> SendSmsAsync(
        string to,
        string message,
        CancellationToken cancellationToken = default);

    Task<SmsDeliveryStatus> CheckDeliveryStatusAsync(
        string messageId,
        CancellationToken cancellationToken = default);
}
```

**File:** `ComplaintManagement.Infrastructure/Services/SmsService.cs`

```csharp
public class SmsService : ISmsService
{
    private readonly ISmsProvider _smsProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SmsService> _logger;
    private readonly ITemplateEngine _templateEngine;

    public SmsService(
        ISmsProvider smsProvider,
        IUnitOfWork unitOfWork,
        ILogger<SmsService> logger,
        ITemplateEngine templateEngine)
    {
        _smsProvider = smsProvider;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _templateEngine = templateEngine;
    }

    public async Task<bool> SendComplaintNotificationAsync(
        string complaintNumber,
        string recipientPhone,
        string templateCode,
        Dictionary<string, object> variables,
        CancellationToken cancellationToken = default)
    {
        // Get SMS template
        var template = await _unitOfWork.CommunicationTemplates
            .FirstOrDefaultAsync(
                t => t.Code == templateCode &&
                     t.Channel == CommunicationChannel.SMS &&
                     !t.IsDeleted,
                cancellationToken);

        if (template == null)
        {
            _logger.LogWarning("SMS template {TemplateCode} not found", templateCode);
            return false;
        }

        // Render template with variables
        var messageBody = _templateEngine.Render(template.Body, variables);

        // Truncate to SMS length limit (160 characters for single SMS, 1600 for concatenated)
        if (messageBody.Length > 160)
        {
            messageBody = messageBody.Substring(0, 157) + "...";
        }

        // Send SMS
        var message = new SmsMessage
        {
            To = recipientPhone,
            Body = messageBody,
            Metadata = new Dictionary<string, string>
            {
                ["ComplaintNumber"] = complaintNumber,
                ["TemplateCode"] = templateCode
            }
        };

        var result = await _smsProvider.SendSmsAsync(message, cancellationToken);

        // Log SMS communication
        if (result.Success)
        {
            await LogSmsAsync(
                recipientPhone,
                messageBody,
                result.MessageId,
                complaintNumber,
                cancellationToken);
        }

        return result.Success;
    }

    public async Task<bool> SendSmsAsync(
        string to,
        string message,
        CancellationToken cancellationToken = default)
    {
        var smsMessage = new SmsMessage
        {
            To = to,
            Body = message
        };

        var result = await _smsProvider.SendSmsAsync(smsMessage, cancellationToken);
        return result.Success;
    }

    public async Task<SmsDeliveryStatus> CheckDeliveryStatusAsync(
        string messageId,
        CancellationToken cancellationToken = default)
    {
        return await _smsProvider.GetDeliveryStatusAsync(messageId, cancellationToken);
    }

    private async Task LogSmsAsync(
        string to,
        string body,
        string messageId,
        string complaintNumber,
        CancellationToken cancellationToken)
    {
        var log = new SmsCommunicationLog
        {
            Id = Guid.NewGuid(),
            ComplaintNumber = complaintNumber,
            RecipientPhone = to,
            MessageBody = body,
            ProviderId = messageId,
            SentAt = DateTime.UtcNow,
            Status = "Sent",
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<SmsCommunicationLog>().AddAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
```

#### Step 1.4: Create WhatsApp Service

**File:** `ComplaintManagement.Infrastructure/Services/WhatsAppService.cs`

```csharp
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

public class WhatsAppService : IWhatsAppService
{
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _fromNumber;
    private readonly ILogger<WhatsAppService> _logger;

    public WhatsAppService(IConfiguration configuration, ILogger<WhatsAppService> logger)
    {
        _accountSid = configuration["Twilio:AccountSid"];
        _authToken = configuration["Twilio:AuthToken"];
        _fromNumber = configuration["Twilio:WhatsAppNumber"]; // e.g., "whatsapp:+14155238886"
        _logger = logger;

        TwilioClient.Init(_accountSid, _authToken);
    }

    public async Task<bool> SendMessageAsync(
        string to,
        string messageBody,
        Dictionary<string, object> variables = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // WhatsApp numbers must be prefixed with "whatsapp:"
            var toWhatsApp = to.StartsWith("whatsapp:") ? to : $"whatsapp:{to}";

            var message = await MessageResource.CreateAsync(
                to: new PhoneNumber(toWhatsApp),
                from: new PhoneNumber(_fromNumber),
                body: messageBody
            );

            _logger.LogInformation(
                "WhatsApp message sent successfully. SID: {MessageSid}",
                message.Sid);

            return message.Status != MessageResource.StatusEnum.Failed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send WhatsApp message to {To}", to);
            return false;
        }
    }

    public async Task<bool> SendTemplateMessageAsync(
        string to,
        string templateName,
        Dictionary<string, object> parameters,
        CancellationToken cancellationToken = default)
    {
        // WhatsApp Business API template messages
        // Templates must be pre-approved by WhatsApp
        try
        {
            var toWhatsApp = to.StartsWith("whatsapp:") ? to : $"whatsapp:{to}";

            // Format template parameters for WhatsApp
            var contentSid = $"HX{templateName}"; // Template SID from WhatsApp
            var contentVariables = System.Text.Json.JsonSerializer.Serialize(parameters);

            var message = await MessageResource.CreateAsync(
                to: new PhoneNumber(toWhatsApp),
                from: new PhoneNumber(_fromNumber),
                contentSid: contentSid,
                contentVariables: contentVariables
            );

            return message.Status != MessageResource.StatusEnum.Failed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send WhatsApp template message to {To}", to);
            return false;
        }
    }
}
```

### Phase 2: Database Entities (Estimated: 2 hours)

#### Create SMS Configuration Entity

**File:** `ComplaintManagement.Domain/Entities/Communication/SmsConfiguration.cs`

```csharp
public class SmsConfiguration : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string ProviderName { get; set; } // "Twilio", "AWS SNS", etc.
    public string AccountSid { get; set; }
    public string AuthToken { get; set; }
    public string FromNumber { get; set; }
    public bool IsEnabled { get; set; }
    public int MaxMessagesPerDay { get; set; } = 1000;
    public decimal CostPerMessage { get; set; }

    // Navigation properties
    public virtual Company Company { get; set; }
}
```

#### Create Communication Log Entity

```csharp
public class SmsCommunicationLog : BaseEntity
{
    public string ComplaintNumber { get; set; }
    public string RecipientPhone { get; set; }
    public string MessageBody { get; set; }
    public string ProviderId { get; set; } // External provider message ID
    public DateTime SentAt { get; set; }
    public string Status { get; set; } // Sent, Delivered, Failed
    public decimal Cost { get; set; }
    public string ErrorMessage { get; set; }
}
```

### Phase 3: Migration (Estimated: 1 hour)

```bash
# Create migration
dotnet ef migrations add AddSmsWhatsAppSupport

# Review and apply
dotnet ef database update
```

### Phase 4: Configuration (Estimated: 1 hour)

**appsettings.json:**

```json
{
  "Twilio": {
    "AccountSid": "YOUR_TWILIO_ACCOUNT_SID",
    "AuthToken": "YOUR_TWILIO_AUTH_TOKEN",
    "FromNumber": "+1234567890",
    "WhatsAppNumber": "whatsapp:+14155238886"
  },
  "Sms": {
    "Enabled": true,
    "Provider": "Twilio",
    "MaxDailyMessages": 1000,
    "RateLimitPerMinute": 100
  },
  "WhatsApp": {
    "Enabled": true,
    "Provider": "Twilio",
    "UseTemplates": true
  }
}
```

### Phase 5: Notification Integration (Estimated: 2 hours)

Update `NotificationDispatcher` to support SMS/WhatsApp:

```csharp
public async Task DispatchEventNotificationsAsync(
    string eventCode,
    Guid complaintId,
    Dictionary<string, object> variables,
    Guid companyId,
    CancellationToken cancellationToken = default)
{
    // Get all active notification rules for this event
    var rules = await GetActiveRulesAsync(eventCode, companyId, cancellationToken);

    foreach (var rule in rules)
    {
        foreach (var channel in rule.Channels)
        {
            switch (channel)
            {
                case CommunicationChannel.Email:
                    await SendEmailNotificationAsync(rule, variables, cancellationToken);
                    break;

                case CommunicationChannel.SMS:
                    await SendSmsNotificationAsync(rule, variables, cancellationToken);
                    break;

                case CommunicationChannel.WhatsApp:
                    await SendWhatsAppNotificationAsync(rule, variables, cancellationToken);
                    break;
            }
        }
    }
}

private async Task SendSmsNotificationAsync(
    NotificationRule rule,
    Dictionary<string, object> variables,
    CancellationToken cancellationToken)
{
    foreach (var recipient in rule.Recipients)
    {
        if (!string.IsNullOrEmpty(recipient.Phone))
        {
            await _smsService.SendComplaintNotificationAsync(
                variables["complaintNumber"].ToString(),
                recipient.Phone,
                rule.TemplateCode,
                variables,
                cancellationToken);
        }
    }
}
```

### Phase 6: Frontend UI (Estimated: 4 hours)

#### SMS/WhatsApp Configuration Component

**File:** `complaint-system-angular/src/app/components/admin/sms-config/sms-config.component.ts`

```typescript
@Component({
  selector: 'app-sms-config',
  templateUrl: './sms-config.component.html'
})
export class SmsConfigComponent implements OnInit {
  configForm: FormGroup;
  providers = ['Twilio', 'AWS SNS', 'Nexmo'];

  constructor(
    private fb: FormBuilder,
    private smsConfigService: SmsConfigService
  ) {
    this.configForm = this.fb.group({
      providerName: ['Twilio', Validators.required],
      accountSid: ['', Validators.required],
      authToken: ['', Validators.required],
      fromNumber: ['', [Validators.required, Validators.pattern(/^\+\d{10,15}$/)]],
      isEnabled: [true],
      maxMessagesPerDay: [1000, [Validators.required, Validators.min(1)]]
    });
  }

  ngOnInit(): void {
    this.loadConfig();
  }

  loadConfig(): void {
    this.smsConfigService.getConfig().subscribe(config => {
      if (config) {
        this.configForm.patchValue(config);
      }
    });
  }

  testConnection(): void {
    const testNumber = prompt('Enter test phone number (with country code):');
    if (testNumber) {
      this.smsConfigService.sendTestSms(testNumber, 'Test message from Complaint Management System')
        .subscribe({
          next: () => alert('Test SMS sent successfully!'),
          error: () => alert('Failed to send test SMS. Check configuration.')
        });
    }
  }

  save(): void {
    if (this.configForm.valid) {
      this.smsConfigService.saveConfig(this.configForm.value).subscribe({
        next: () => alert('SMS configuration saved successfully'),
        error: (err) => alert('Failed to save configuration: ' + err.message)
      });
    }
  }
}
```

---

## Required Package Installation

### Backend NuGet Packages

```bash
# Twilio SDK for SMS and WhatsApp
dotnet add package Twilio

# Optional: AWS SDK for SNS (if using AWS)
dotnet add package AWSSDK.SimpleNotificationService
```

### Frontend NPM Packages

```bash
# International phone number input
npm install ngx-intl-tel-input --save

# Phone number validation
npm install libphonenumber-js --save
```

---

## SMS Templates

Create default SMS templates in database:

```sql
-- Complaint Created Acknowledgment (SMS)
INSERT INTO CommunicationTemplates (Id, Code, Name, Subject, Body, Channel, IsSystemTemplate, CreatedAt)
VALUES (
    NEWID(),
    'COMPLAINT_CREATED_SMS',
    'Complaint Created - SMS',
    NULL,
    'Your complaint {{ComplaintNumber}} has been received. We will respond within {{ResponseTimeHours}}h. Track: {{TrackingUrl}}',
    1, -- SMS
    1,
    GETUTCDATE()
);

-- Complaint Assigned (SMS)
INSERT INTO CommunicationTemplates (Id, Code, Name, Subject, Body, Channel, IsSystemTemplate, CreatedAt)
VALUES (
    NEWID(),
    'COMPLAINT_ASSIGNED_SMS',
    'Complaint Assigned - SMS',
    NULL,
    'Your complaint {{ComplaintNumber}} is now being handled by {{AssignedToName}}. Est. resolution: {{DueDate}}',
    1, -- SMS
    1,
    GETUTCDATE()
);

-- Complaint Resolved (SMS)
INSERT INTO CommunicationTemplates (Id, Code, Name, Subject, Body, Channel, IsSystemTemplate, CreatedAt)
VALUES (
    NEWID(),
    'COMPLAINT_RESOLVED_SMS',
    'Complaint Resolved - SMS',
    NULL,
    'Your complaint {{ComplaintNumber}} has been resolved. Please confirm: {{ConfirmationUrl}}',
    1, -- SMS
    1,
    GETUTCDATE()
);
```

---

## WhatsApp Templates

WhatsApp templates must be pre-approved by WhatsApp. Example template structure:

### Template: Complaint Acknowledgment

**Name:** `complaint_acknowledgment`
**Language:** English (en)
**Category:** TRANSACTIONAL

**Content:**
```
Hello {{1}},

Your complaint #{{2}} has been received and is being processed.

Complaint Type: {{3}}
Priority: {{4}}
Expected Response: {{5}}

We will keep you updated on the progress.

Thank you,
{{6}}
```

**Variables:**
1. Customer Name
2. Complaint Number
3. Category
4. Priority
5. Response Time
6. Company Name

---

## Testing Checklist

### SMS Testing
- [ ] Send test SMS to valid number
- [ ] Verify delivery status callback
- [ ] Test with invalid number (should fail gracefully)
- [ ] Test message truncation (> 160 characters)
- [ ] Test rate limiting
- [ ] Test cost tracking
- [ ] Test international numbers

### WhatsApp Testing
- [ ] Send test WhatsApp message
- [ ] Test template message
- [ ] Test with opt-out user
- [ ] Verify delivery receipts
- [ ] Test media attachments (if supported)

### Integration Testing
- [ ] Create complaint → SMS/WhatsApp sent
- [ ] Assign complaint → Notification sent to handler
- [ ] Resolve complaint → Confirmation sent
- [ ] Escalate complaint → Manager notified

---

## Cost Considerations

### Twilio Pricing (as of 2025)
- SMS (US): ~$0.0075 per message
- SMS (International): $0.02 - $0.15 per message
- WhatsApp (Conversation-based pricing):
  - User-initiated: $0.005 per conversation
  - Business-initiated: $0.025 - $0.135 per conversation

### Budget Planning
For 1000 complaints/month with SMS notifications:
- Acknowledgment: 1000 × $0.0075 = $7.50
- Assignment: 800 × $0.0075 = $6.00
- Resolution: 900 × $0.0075 = $6.75
- **Total: ~$20/month**

---

## Security Considerations

1. **Credential Storage**
   - Store Twilio credentials in Azure Key Vault or AWS Secrets Manager
   - Never commit credentials to source control
   - Use environment variables in production

2. **Phone Number Validation**
   - Validate phone numbers before sending
   - Implement opt-out mechanism
   - Respect Do Not Disturb regulations

3. **Rate Limiting**
   - Implement per-user rate limits
   - Prevent SMS flooding/abuse
   - Monitor unusual sending patterns

4. **Compliance**
   - TCPA compliance (US)
   - GDPR compliance (EU)
   - Obtain consent before sending marketing messages
   - Maintain opt-out list

---

## Monitoring & Logging

### Key Metrics to Track
1. Messages sent per day/month
2. Delivery rate (%)
3. Failed messages count
4. Cost per message
5. Response time from provider
6. Opt-out rate

### Alerts to Configure
1. Daily cost exceeds budget
2. Delivery rate drops below 90%
3. Provider API errors
4. Rate limit reached

---

## Next Steps

1. **Immediate (Before Production)**
   - [ ] Set up Twilio account
   - [ ] Configure phone numbers
   - [ ] Test in staging environment
   - [ ] Create SMS templates
   - [ ] Submit WhatsApp templates for approval

2. **Short Term (First Month)**
   - [ ] Monitor delivery rates
   - [ ] Optimize template content
   - [ ] Gather user feedback
   - [ ] Adjust rate limits if needed

3. **Long Term (3-6 Months)**
   - [ ] Add multiple provider support (fallback)
   - [ ] Implement cost optimization
   - [ ] Add rich media support (MMS, WhatsApp media)
   - [ ] Build analytics dashboard

---

**Implementation Status:** DESIGN COMPLETE - READY FOR DEVELOPMENT
**Estimated Development Time:** 14-18 hours
**Dependencies:** Twilio account, WhatsApp Business API approval
**Priority:** Medium (Nice-to-have feature)
