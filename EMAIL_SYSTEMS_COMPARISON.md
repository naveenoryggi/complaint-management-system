# Email Systems Comparison - Visual Guide

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                  COMPLAINT MANAGEMENT SYSTEM                 │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌─────────────────────┐        ┌──────────────────────┐   │
│  │  EMAIL SETTINGS     │        │  EMAIL TICKETING     │   │
│  │  (Outgoing)         │        │  (Incoming)          │   │
│  └─────────────────────┘        └──────────────────────┘   │
│           │                                 ▲                │
│           │                                 │                │
└───────────┼─────────────────────────────────┼────────────────┘
            │                                 │
            │ Send                       Poll │ Receive
            ▼                                 │
    ┌───────────────┐                 ┌──────────────┐
    │   SMTP        │                 │    IMAP      │
    │   Server      │                 │    Server    │
    └───────────────┘                 └──────────────┘
            │                                 ▲
            │ Deliver                    Send │
            ▼                                 │
    ┌───────────────┐                 ┌──────────────┐
    │  End Users    │                 │  End Users   │
    │ (Recipients)  │                 │  (Senders)   │
    └───────────────┘                 └──────────────┘
```

---

## EMAIL SETTINGS (Outgoing) - Notification System

### Purpose
Send emails FROM the system TO users when events occur

### Flow Diagram
```
System Event               Notification        Email
(Complaint Created)   →   Engine         →    Sent to User
    │                         │                     │
    ├─ Status Changed         ├─ Template          ├─ john@company.com
    ├─ SLA Breach            ├─ Variables         ├─ jane@company.com
    ├─ Escalation            └─ SMTP              └─ admin@company.com
    └─ Assignment                  ▼
                              Office365/Gmail
                              SMTP Server
```

### Configuration Location
```
Admin Panel
  └── Communication Settings
       └── Email Settings Management
            ├── SMTP Server: smtp.office365.com
            ├── Port: 587
            ├── Authentication: OAuth / Basic
            └── From Address: notifications@company.com
```

### Use Cases
1. **Complaint Status Updates**
   ```
   From: notifications@company.com
   To: john.doe@customer.com
   Subject: Your complaint #COMP-123 status updated to "In Progress"
   ```

2. **SLA Breach Alerts**
   ```
   From: alerts@company.com
   To: manager@company.com
   Subject: URGENT: Complaint #COMP-456 SLA breach - 2 hours overdue
   ```

3. **Assignment Notifications**
   ```
   From: system@company.com
   To: technician@company.com
   Subject: New complaint assigned to you: #COMP-789
   ```

### Technical Details
- **Protocol**: SMTP (Simple Mail Transfer Protocol)
- **Direction**: Outbound only
- **Trigger**: System events
- **Frequency**: On-demand (event-driven)
- **Authentication**: OAuth 2.0 or App Password

---

## EMAIL TICKETING (Incoming) - Complaint Creation System

### Purpose
Receive emails FROM users and convert them into complaints automatically

### Flow Diagram
```
User Sends Email       System Polls        Complaint
support@company.com ← Inbox          →    Created
    │                      │                   │
john@customer.com          ├─ IMAP             ├─ Title: Email Subject
Subject: Printer issue     ├─ Parse            ├─ Description: Body
Body: 3rd floor...        └─ OAuth            ├─ Attachments
Attachments: photo.jpg         ▼               └─ Complainant: john@
                          Create Complaint
                          + Auto-Reply
```

### Configuration Location
```
Admin Panel
  └── Communication Settings
       └── Email Ticketing Configuration
            ├── Monitor Email: support@company.com
            ├── IMAP Server: outlook.office365.com
            ├── Port: 993
            ├── Authentication: OAuth 2.0
            ├── Polling Interval: 5 minutes
            └── Auto-Acknowledgement: Enabled
```

### Use Cases
1. **Customer Sends Complaint via Email**
   ```
   From: john.doe@customer.com
   To: support@company.com
   Subject: Printer not working in Office 3
   Body: The printer on the 3rd floor has been offline...
   Attachments: error-photo.jpg

   → System creates Complaint #COMP-991
   ```

2. **Customer Replies to Email**
   ```
   From: john.doe@customer.com
   To: support@company.com
   Subject: Re: Printer not working in Office 3
   Body: Is there any update on this?

   → System adds comment to existing Complaint #COMP-991
   ```

3. **System Sends Auto-Acknowledgement**
   ```
   From: support@company.com
   To: john.doe@customer.com
   Subject: Re: Printer not working in Office 3
   Body: Thank you! Your complaint has been registered.
         Ticket Number: COMP-991
         We'll respond within 24 hours.
   ```

### Technical Details
- **Protocol**: IMAP (Internet Message Access Protocol)
- **Direction**: Inbound + Outbound (for replies)
- **Trigger**: Email received
- **Frequency**: Polling (every 5 minutes default)
- **Authentication**: OAuth 2.0 (required for modern providers)

---

## Side-by-Side Comparison

| Feature | Email Settings (Outgoing) | Email Ticketing (Incoming) |
|---------|--------------------------|---------------------------|
| **Purpose** | Send notifications | Receive complaints |
| **Direction** | System → Users | Users → System |
| **Protocol** | SMTP | IMAP/Microsoft Graph |
| **Port** | 587 (TLS) or 465 (SSL) | 993 (SSL) |
| **Trigger** | System events | Email received |
| **Frequency** | Event-driven | Poll-based (5 min) |
| **Creates** | Email messages | Complaints |
| **Template Support** | ✓ Yes | ✓ Yes (auto-reply) |
| **OAuth Support** | ✓ Yes | ✓ Yes (required) |
| **Multiple Configs** | 1 per company | Multiple per company |

---

## OAuth Setup - Which System Needs It?

### Email Settings (Outgoing) - OAuth Optional
```
✓ Can use Basic Auth + App Password
✓ Can use OAuth 2.0 (recommended)
✗ Legacy systems may still support basic SMTP auth
```

### Email Ticketing (Incoming) - OAuth Required
```
✓ Must use OAuth 2.0
✗ Basic Auth deprecated by Microsoft (Oct 2022)
✗ Gmail requires OAuth for IMAP access
✓ Token auto-refresh every 60 minutes
```

---

## Real-World Example Scenario

### Company Setup

**Outgoing Email (Notifications):**
```
From: notifications@company.com
SMTP: smtp.office365.com:587
Auth: OAuth 2.0
Purpose: Send alerts to users
```

**Incoming Email (Ticketing):**
```
Monitor: support@company.com
IMAP: outlook.office365.com:993
Auth: OAuth 2.0
Purpose: Receive complaints from customers
Polling: Every 5 minutes
```

### User Journey

1. **Customer sends complaint**
   ```
   john@customer.com → support@company.com
   "My laptop is not working"
   ```

2. **System polls inbox (Email Ticketing)**
   - Fetches email via IMAP
   - Creates Complaint #COMP-100
   - Assigns to IT department

3. **System sends auto-acknowledgement (Email Ticketing)**
   ```
   support@company.com → john@customer.com
   "Thank you! Ticket #COMP-100 created"
   ```

4. **IT tech updates complaint**
   - Changes status to "In Progress"

5. **System sends notification (Email Settings)**
   ```
   notifications@company.com → john@customer.com
   "Your complaint #COMP-100 is now In Progress"
   ```

6. **Customer replies to support email**
   ```
   john@customer.com → support@company.com
   "Thanks! When can I expect it fixed?"
   ```

7. **System polls inbox again (Email Ticketing)**
   - Finds reply email
   - Adds as comment to #COMP-100
   - Notifies assigned tech

8. **Tech resolves complaint**
   - Marks as "Resolved"

9. **System sends resolution notification (Email Settings)**
   ```
   notifications@company.com → john@customer.com
   "Your complaint #COMP-100 has been resolved!"
   ```

---

## Configuration Steps Summary

### For Email Settings (Outgoing)
```
1. Admin Panel → Email Settings Management
2. Click "Add Email Configuration"
3. Enter SMTP details
4. Select OAuth 2.0
5. Authorize with Microsoft/Google
6. Test connection
7. Save
```

### For Email Ticketing (Incoming)
```
1. Admin Panel → Email Ticketing Config
2. Click "Add Email Configuration"
3. Enter monitored email address
4. Enter IMAP details
5. Select OAuth 2.0
6. Configure polling interval
7. Authorize with Microsoft/Google
8. Enable auto-acknowledgement (optional)
9. Test connection
10. Save
```

---

## Quick Reference

### When to use Email Settings?
- ✓ Want to send notifications to users
- ✓ Need to send SLA alerts
- ✓ Want to send status updates
- ✓ Need to send bulk emails

### When to use Email Ticketing?
- ✓ Want users to create complaints via email
- ✓ Need to monitor a support inbox
- ✓ Want to convert emails to tickets automatically
- ✓ Need email threading (replies as comments)

---

## Next Steps

Run the setup script:
```powershell
.\setup-email-ticketing-oauth.ps1
```

Or configure manually in the UI:
```
http://localhost:4200
→ Login as Admin
→ Admin Panel
→ Communication Settings
→ Email Ticketing Config
→ Add Configuration
```
