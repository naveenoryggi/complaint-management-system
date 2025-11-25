# COMPLAINT VIEWING & EMAIL TICKETING - COMPREHENSIVE GUIDE

**Date:** November 16, 2025
**Status:** ✅ Complete Demonstration

---

## 📧 HOW COMPLAINTS ARE CREATED

### **Method 1: Email Ticketing (Automatic)**

**How it works:**
1. Customer sends email to: `support@oryggitech.com`
2. Email Polling Service fetches email (polls every 2 minutes)
3. System automatically creates complaint
4. Auto-response sent to customer: "✅ Ticket Created: CMP-XXXXXXXX"
5. Complaint appears in dashboard immediately

**Example from screenshots:**
- **Complaint:** CMP-20251115-0487
- **Customer:** Naveen Chandra
- **Subject:** "Test"
- **Created:** 16/11/2025, 02:15 am
- **Email Thread:** 2 emails (1 received, 1 sent)

**Benefits:**
- ✅ No manual entry required
- ✅ Customers don't need portal access
- ✅ Automatic acknowledgement
- ✅ Full email conversation preserved
- ✅ Reply directly from portal

---

### **Method 2: Online Registration (Manual)**

**How it works:**
1. User logs into portal
2. Clicks "Create New Complaint"
3. Fills complaint form:
   - Title
   - Description
   - Category (Product Quality, Service Issue, etc.)
   - Priority (Low, Normal, High, Critical)
   - Attachments (optional)
4. Submits → Complaint created with status "Submitted"

**Benefits:**
- ✅ More structured data entry
- ✅ Required fields enforced
- ✅ File attachments supported
- ✅ Immediate confirmation

---

## 👀 VIEWING LATEST TICKETS & CUSTOMER RESPONSES

### **1. ADMIN VIEW** (Full Access)

**Dashboard Features:**
- **Displays:** ALL complaints from all users/branches/departments
- **Total complaints shown:** 487 results
- **View indicator:** "View: Administrator (All Complaints)"

**How to see latest tickets:**

#### **A. Recent Complaints Section** (Dashboard)
```
Location: Dashboard → Recent Complaints
Shows: 10 most recent complaints
Sorting: Newest first (by creation date)
```

**Each complaint card shows:**
- Complaint Number (e.g., CMP-20251115-0487)
- Status badge (Submitted, In Progress, Resolved, etc.)
- Priority badge (Low, Normal, High, Critical)
- Title/Subject
- Complainant name
- Branch/Department
- Category
- Creation timestamp (e.g., "16/11/2025, 02:15 am")
- Assigned handler (or "Unassigned")
- Action buttons: **Assign** | **View**

#### **B. Email Response Indicators**

**Visual cues for customer responses:**
- 🔴 **"Unread" badge** - New customer email not yet opened
- 📊 **Email count** - "2 total, 1 received, 1 sent"
- ⏰ **Relative time** - "13 hours ago"
- 🟢 **"Received" badge** - Green badge = Email from customer
- 🔵 **"Sent" badge** - Blue badge = Email from support team

**How to identify tickets with customer responses:**
1. Look for email count > 0
2. Check for "Unread" badge
3. Look at "Last polled" timestamp in email config
4. Filter by recent dates

#### **C. Complaint Detail View**

**When you click "View" button, you see:**

**Left Panel - Complaint Information:**
- Complaint Number
- Title and Description
- Status and Priority badges
- Category
- Submitted date/time
- Assigned handler
- Escalation level
- Edit button (admin/handler only)

**Left Panel - Complainant Information:**
- Personal Details:
  - Name
  - Employee Code
  - Preferred Contact Method
- Organizational Details:
  - Company
  - Branch
  - Department
  - Section
- Manager Details:
  - Manager name
  - Email (clickable)
  - Phone (clickable)

**Right Panel - Actions:**
- **Assign Complaint** - Assign to handler
- **Escalate** - Escalate to higher level
- **Close Complaint** - Mark as closed
- **View History** - See all status changes

**Right Panel - Metadata:**
- Comments count
- Attachments count
- Anonymous flag

**Right Panel - Comments Section:**
- Add comment textbox
- List of all comments with timestamps
- "Hide" button to collapse

**Bottom - Email Thread Section:**

**Email Thread Header:**
- Total email count
- Received count
- Sent count
- **"Compose Email"** button
- Sort options (oldest/newest first)
- Expand/Collapse all
- Refresh button

**Each Email in Thread:**
- Sender avatar (initials)
- Sender name and type (Received/Sent)
- Timestamp (relative and absolute)
- Subject line
- Email body (collapsible)
- Action buttons:
  - **Reply** - Reply to sender
  - **Reply All** - Reply to all recipients
  - **Forward** - Forward to someone else
  - **Internal Note** - Add private note (not sent as email)

#### **D. Email Reply Composer**

**When you click "Reply" button:**

**Features:**
- **To:** Pre-filled with customer email
- **Cc/Bcc:** Available via buttons
- **Subject:** Auto-filled with "Re: [Original Subject]"
- **Message:** Rich text editor with formatting toolbar

**Rich Text Editor Toolbar:**
- Text formatting: Bold, Italic, Underline, Strikethrough
- Paragraph styles: Blockquote, Code block
- Headings: H1, H2
- Lists: Numbered, Bullet
- Script: Subscript, Superscript
- Indentation: Decrease, Increase
- Font size: Small, Normal, Large, Huge
- Font style: Normal, Heading 1-6
- Colors: Text color, Background color
- Insert: Link, Image
- Clear formatting

**Pro Tips displayed:**
- Use Ctrl+Enter to send quickly
- Press Enter or comma to add recipients

**Buttons:**
- **Cancel** - Discard draft (with confirmation)
- **Send** - Send email to customer

---

### **2. HANDLER VIEW** (Assigned Complaints Only)

**What Handlers Can See:**

✅ **Dashboard shows:**
- "View: Handler (My Assigned Complaints)"
- ONLY complaints assigned to them
- Total: X assigned complaints

✅ **Can access:**
- Full complaint details for assigned complaints
- Email thread for assigned complaints
- Reply to customer emails
- Update complaint status
- Add comments
- Upload attachments
- Escalate complaints
- View complaint history

❌ **Cannot see:**
- Unassigned complaints
- Complaints assigned to other handlers
- All complaints list
- Admin panel options

**Example:**
If "Tushar pandith" is logged in as Handler:
- He sees ONLY CMP-2025-1153 (assigned to him)
- He does NOT see CMP-20251115-0487 (unassigned)
- He does NOT see CMP-2025-1155 (assigned to "Updated Admin")

**Dashboard Statistics:**
- Shows statistics for ONLY assigned complaints
- Charts/graphs filtered by assigned complaints
- "My Assigned Complaints" count

---

### **3. COMPLAINANT VIEW** (Own Complaints Only)

**What Complainants Can See:**

✅ **Dashboard shows:**
- "My Complaints" list
- ONLY complaints they created
- Total: X my complaints

✅ **Can access:**
- View their own complaint details
- See complaint status and updates
- View email thread for their complaints
- Reply to emails from support team
- Add comments
- Upload additional attachments
- View resolution details

❌ **Cannot see:**
- Other people's complaints
- Admin/Handler internal notes
- Assignment details
- Escalation information
- Handler names/details
- Admin panel options

**Example:**
If "NAVEEN CHANDRA" logs in as Complainant:
- He sees ONLY CMP-20251115-0487 (his complaint)
- He does NOT see CMP-20251115-0486 (created by LinkedIn)
- He does NOT see any other user's complaints

**Complaint Detail View:**
- **Can see:** Status, Priority, Description, Email thread, Updates
- **Cannot see:** "Assigned To" field, "Escalate" button, Internal notes
- **Can do:** Add comments, Reply to emails, Upload files
- **Cannot do:** Assign, Escalate, Change status manually

**Dashboard Statistics:**
- Shows statistics for ONLY their complaints
- Personal complaint timeline
- "My Complaints" count by status

---

## 🔍 FILTERING & SEARCHING

### **Filter Options (All User Types):**

**Filter by Status:**
- All Statuses
- Ticket Received
- Submitted
- Under Review
- In Progress
- Escalated
- Pending Info
- Resolved
- Closed
- Rejected
- Reopened

**Filter by Priority:**
- All Priorities
- Low
- Normal
- High
- Critical
- Urgent

**Search by:**
- Complaint Number (e.g., CMP-20251115-0487)
- Title/Subject
- Complainant Name

**Sort by:**
- Newest First (default)
- Oldest First
- Priority (High to Low)
- Status

---

## 📊 DASHBOARD STATISTICS (Role-Based)

### **Admin View:**
Shows statistics for **ALL complaints**:
- Ticket Received: 0
- Submitted: 484
- Under Review: 0
- In Progress: 2
- Escalated: 0
- Pending Info: 0
- Resolved: 1
- Closed: 0
- Rejected: 0
- Reopened: 0

**Additional metrics:**
- Average resolution time
- Percentage change vs previous period
- Total complaints: 487 results

### **Handler View:**
Shows statistics for **ASSIGNED complaints only**:
- My Assigned: X
- In Progress: Y
- Resolved: Z
- Average time: A hours

### **Complainant View:**
Shows statistics for **OWN complaints only**:
- My Complaints: X
- Open: Y
- Resolved: Z
- Pending: A

---

## 🔔 NOTIFICATION INDICATORS

### **Email-Based Tickets (Visual Cues):**

1. **Unread Badge** (Red)
   - Indicates new customer email not yet read
   - Appears on email in thread
   - Example: "Unread" badge on customer's email

2. **Email Count Indicator**
   - Shows total conversation count
   - Example: "2 total, 1 received, 1 sent"
   - Helps identify active conversations

3. **Timestamp Indicators**
   - Relative time: "13 hours ago"
   - Absolute time: "16/11/2025, 01:34:00 am"
   - Helps sort by recency

4. **Email Type Badges**
   - 🟢 Green "Received" - From customer
   - 🔵 Blue "Sent" - From support team
   - Easy visual differentiation

---

## 📸 SCREENSHOTS CAPTURED

1. **complaints-view-01-admin-dashboard.png**
   - Admin dashboard showing all 487 complaints
   - Recent complaints section
   - Statistics widgets
   - Filter and search options

2. **complaints-view-02-complaint-detail.png**
   - Full complaint detail view
   - Complainant information
   - Email thread section showing 2 emails
   - Action buttons (Assign, Escalate, Close)
   - Unread email indicator

3. **complaints-view-03-email-reply-composer.png**
   - Email reply modal
   - Rich text editor with formatting toolbar
   - Pre-filled recipient and subject
   - Pro tips for keyboard shortcuts

---

## ✅ KEY DIFFERENCES SUMMARY

| Feature | Admin | Handler | Complainant |
|---------|-------|---------|-------------|
| **View All Complaints** | ✅ Yes | ❌ No | ❌ No |
| **View Assigned Complaints** | ✅ Yes | ✅ Yes (only theirs) | ❌ No |
| **View Own Complaints** | ✅ Yes | ✅ Yes | ✅ Yes (only theirs) |
| **See Email Thread** | ✅ All | ✅ Assigned only | ✅ Own only |
| **Reply to Emails** | ✅ Yes | ✅ Assigned only | ✅ Own only |
| **Assign Complaints** | ✅ Yes | ❌ No | ❌ No |
| **Escalate** | ✅ Yes | ✅ Assigned only | ❌ No |
| **Close Complaints** | ✅ Yes | ✅ Assigned only | ❌ No |
| **See Internal Notes** | ✅ Yes | ✅ Yes | ❌ No |
| **Create Complaints** | ✅ Yes | ✅ Yes | ✅ Yes |
| **Dashboard Statistics** | All complaints | Assigned only | Own only |

---

## 🎯 BEST PRACTICES

### **For Admins:**
1. Check "Recent Complaints" daily for new submissions
2. Look for "Unread" badges to prioritize responses
3. Assign complaints to appropriate handlers quickly
4. Monitor email thread for customer follow-ups
5. Use filters to focus on high-priority tickets

### **For Handlers:**
1. Check "My Assigned Complaints" regularly
2. Reply to customer emails within SLA timeframes
3. Update complaint status as work progresses
4. Add internal notes for team communication
5. Escalate when necessary

### **For Complainants:**
1. Submit complaints via web form or email
2. Check "My Complaints" for status updates
3. Reply to support team emails promptly
4. Add comments for additional information
5. Wait for resolution notifications

---

## 🔐 SECURITY & PRIVACY

**Role-Based Access Control (RBAC):**
- ✅ Users can ONLY see what their role permits
- ✅ Complainants cannot see other users' data
- ✅ Handlers cannot see unassigned or other handlers' complaints
- ✅ Admins have full visibility for management purposes

**Email Privacy:**
- ✅ Email addresses not exposed to unauthorized users
- ✅ Internal notes hidden from complainants
- ✅ Email thread preserved for audit trail
- ✅ Sensitive data protected

---

## 💡 HOW TO SEE LATEST TICKETS WITH CUSTOMER RESPONSES

### **Quick Method:**
1. Go to Dashboard
2. Look at "Recent Complaints" section
3. Check for email count > 0
4. Look for "Unread" badges
5. Click "View" on any complaint
6. Scroll to "Email Thread" section
7. Expand emails to see customer responses

### **Detailed Method:**
1. Dashboard → Filter by "All Statuses"
2. Sort by "Newest First"
3. Click "View" on top complaint
4. Check Email Thread section
5. Look for green "Received" badges (customer emails)
6. Click email to expand full content
7. Click "Reply" to respond

---

## 📝 ADDITIONAL NOTES

### **Email Ticketing System:**
- Polls email account every 2 minutes (configurable)
- Automatically creates complaints from incoming emails
- Sends auto-acknowledgement to customer
- Preserves full email conversation
- Supports attachments in emails
- Thread timeout: 30 days (configurable)

### **Manual Complaint Creation:**
- Available to all authenticated users
- Required fields enforced
- File uploads supported
- Instant notification to admins
- No email thread (unless customer replies via email)

### **Customer Experience:**
- Can register complaints via web portal OR email
- Receive auto-acknowledgement immediately
- Get email updates on status changes
- Can reply via email without logging in
- See full conversation history when logged in

---

## 🚀 NEXT STEPS

To demonstrate specific views:

1. **To see Handler view:**
   - Log out from Admin account
   - Log in as Handler user
   - Dashboard will show only assigned complaints

2. **To see Complainant view:**
   - Log out from current account
   - Log in as Complainant user
   - Dashboard will show only own complaints

3. **To test email ticketing:**
   - Send email to support@oryggitech.com
   - Wait 2 minutes for polling
   - Check dashboard for new complaint
   - Verify auto-response received

4. **To test manual complaint creation:**
   - Click "Create New Complaint" button
   - Fill complaint form
   - Submit and verify creation

---

**Report Generated:** 2025-11-16 09:40 UTC
**System Status:** ✅ Running
**Email Polling:** ✅ Active
**Total Complaints:** 487
**Latest Ticket:** CMP-20251115-0487 (13 hours ago)
