# Email Composer UI/UX Test Cases
## User-Focused Functional Testing

---

## Test Environment
- **Application**: Complaint Management System
- **Component**: Email Reply Composer (Quill.js Editor)
- **URL**: http://localhost:4200
- **Browser**: Chrome/Edge/Firefox

---

## TC-001: Open Email Composer - New Email
**Objective**: Verify user can open email composer to send new email

**Test Steps**:
1. Login as Admin/Handler
2. Navigate to any complaint detail page
3. Click "Compose Email" button in Email Thread section

**Expected Result**:
- ✅ Email composer modal/panel opens
- ✅ Title shows "New Email" or "Compose"
- ✅ To field is empty and focused
- ✅ Subject field is empty
- ✅ Body editor is empty with placeholder text
- ✅ Toolbar shows all formatting options
- ✅ "Send" and "Cancel" buttons visible

**Pass/Fail**: _______

---

## TC-002: Open Email Composer - Reply
**Objective**: Verify user can reply to existing email

**Test Steps**:
1. Open complaint with existing email thread
2. Expand an email in the thread
3. Click "Reply" button

**Expected Result**:
- ✅ Email composer opens
- ✅ Title shows "Reply" or "Re:"
- ✅ To field pre-filled with original sender's email
- ✅ Subject pre-filled with "Re: [Original Subject]"
- ✅ Body is empty (no quoted text at top)
- ✅ Cursor focused in body editor
- ✅ Original email NOT included in editor initially

**Pass/Fail**: _______

---

## TC-003: Open Email Composer - Reply All
**Objective**: Verify Reply All includes all recipients

**Test Steps**:
1. Open complaint with email that has multiple recipients (To + Cc)
2. Click "Reply All" button

**Expected Result**:
- ✅ Email composer opens
- ✅ To field includes original sender
- ✅ Cc field automatically shown with all Cc recipients
- ✅ Current user's email excluded from recipients
- ✅ Subject shows "Re: [Original Subject]"

**Pass/Fail**: _______

---

## TC-004: Open Email Composer - Forward
**Objective**: Verify forward includes complete email thread

**Test Steps**:
1. Open complaint with email thread (3+ emails)
2. Click "Forward" on any email

**Expected Result**:
- ✅ Email composer opens
- ✅ Title shows "Forward" or "Fwd:"
- ✅ To field is empty
- ✅ Subject shows "Fwd: [Original Subject]"
- ✅ Body contains formatted email thread:
  - "Forwarded Conversation (X messages)" header
  - All messages numbered (1 of X, 2 of X, etc.)
  - Each message shows From, Sent, Subject
  - Direction indicators (Received/Sent)
  - Professional Outlook-style formatting
  - Clean separators between messages

**Pass/Fail**: _______

---

## TC-005: Add Recipients - To Field
**Objective**: Verify user can add email addresses to To field

**Test Steps**:
1. Open email composer
2. Type "test@example.com" in To field
3. Press Enter or comma
4. Type "another@example.com"
5. Press Enter

**Expected Result**:
- ✅ First email appears as chip/badge
- ✅ Second email appears as chip/badge
- ✅ Each chip shows email address
- ✅ Each chip has remove (×) button
- ✅ Input field clears after each email added
- ✅ Can continue adding more emails

**Pass/Fail**: _______

---

## TC-006: Add Recipients - Cc/Bcc Fields
**Objective**: Verify Cc and Bcc fields work correctly

**Test Steps**:
1. Open email composer
2. Click "Cc" button
3. Add email to Cc field
4. Click "Bcc" button
5. Add email to Bcc field

**Expected Result**:
- ✅ Cc field appears when Cc button clicked
- ✅ Bcc field appears when Bcc button clicked
- ✅ Both fields work like To field (chip-based)
- ✅ Can hide fields by clicking button again
- ✅ Recipients persist when fields hidden/shown

**Pass/Fail**: _______

---

## TC-007: Remove Recipients
**Objective**: Verify user can remove email recipients

**Test Steps**:
1. Add 3 emails to To field
2. Click × button on second email chip
3. Try to send email

**Expected Result**:
- ✅ Second email chip removed immediately
- ✅ Remaining chips stay in order
- ✅ Can still send with remaining recipients
- ✅ Smooth animation when removing chip

**Pass/Fail**: _______

---

## TC-008: Email Validation - Invalid Format
**Objective**: Verify invalid email addresses are rejected

**Test Steps**:
1. Try to add invalid emails:
   - "notanemail"
   - "test@"
   - "@example.com"
   - "test @example.com" (space)
2. Try to send

**Expected Result**:
- ✅ Error message: "Invalid email format"
- ✅ Invalid email not added as chip
- ✅ Red border/highlight on input
- ✅ Cannot send with invalid emails

**Pass/Fail**: _______

---

## TC-009: Subject Line - Basic Input
**Objective**: Verify subject line input works correctly

**Test Steps**:
1. Open email composer
2. Type subject: "Test Email Subject Line"
3. Clear subject
4. Type new subject

**Expected Result**:
- ✅ Subject appears as typed
- ✅ No character limit issues (up to 200 chars)
- ✅ Subject persists when switching between fields
- ✅ Subject visible in final sent email

**Pass/Fail**: _______

---

## TC-010: Rich Text Toolbar - Visibility
**Objective**: Verify all toolbar formatting options are visible

**Expected Toolbar Buttons** (from left to right):

**Row 1 - Font Controls**:
- ✅ Font Family dropdown (Sans Serif, Serif, Monospace)
- ✅ Font Size dropdown (Small, Normal, Large, Huge)

**Row 2 - Text Formatting**:
- ✅ Bold (B)
- ✅ Italic (I)
- ✅ Underline (U)
- ✅ Strikethrough (S)

**Row 3 - Colors**:
- ✅ Text Color picker
- ✅ Background Color picker

**Row 4 - Headings**:
- ✅ Heading dropdown (H1, H2, H3, H4, H5, H6, Normal)

**Row 5 - Alignment**:
- ✅ Align Left/Center/Right/Justify

**Row 6 - Lists**:
- ✅ Ordered List (1, 2, 3)
- ✅ Bullet List (•)
- ✅ Checklist (☐)

**Row 7 - Indentation**:
- ✅ Decrease Indent
- ✅ Increase Indent

**Row 8 - Special Formats**:
- ✅ Blockquote
- ✅ Code Block
- ✅ Subscript
- ✅ Superscript

**Row 9 - Direction**:
- ✅ RTL (Right-to-Left)

**Row 10 - Insert Elements**:
- ✅ Link
- ✅ Image
- ✅ Video
- ✅ Formula

**Row 11 - Utility**:
- ✅ Clean Formatting (remove all formatting)

**Pass/Fail**: _______

---

## TC-011: Apply Bold Formatting
**Objective**: Verify bold formatting works correctly

**Test Steps**:
1. Type "This is normal text"
2. Select the word "normal"
3. Click Bold button (B)
4. Type more text after

**Expected Result**:
- ✅ Selected text becomes bold
- ✅ Bold button shows active state (highlighted)
- ✅ New text after is not bold (cursor exits bold mode)
- ✅ Can toggle bold on/off by clicking button again

**Pass/Fail**: _______

---

## TC-012: Apply Multiple Text Styles
**Objective**: Verify multiple formatting can be applied simultaneously

**Test Steps**:
1. Type text
2. Select text
3. Apply Bold, Italic, and Underline
4. Change text color to red

**Expected Result**:
- ✅ Text is bold, italic, underlined, and red
- ✅ All format buttons show active state
- ✅ Formatting persists when clicking away
- ✅ Can remove individual formats independently

**Pass/Fail**: _______

---

## TC-013: Font Size Selection
**Objective**: Verify font size changes work

**Test Steps**:
1. Type "Small text"
2. Select text
3. Click Font Size dropdown
4. Select "Small"
5. Type new text "Large text"
6. Select it and change to "Large"

**Expected Result**:
- ✅ Dropdown shows all size options (Small, Normal, Large, Huge)
- ✅ Text changes to selected size
- ✅ Visual difference clearly visible
- ✅ Proper font size in sent email

**Pass/Fail**: _______

---

## TC-014: Font Family Selection
**Objective**: Verify font family changes work

**Test Steps**:
1. Type text
2. Select text
3. Change font to "Serif"
4. Type more text and change to "Monospace"

**Expected Result**:
- ✅ Font dropdown shows Sans Serif, Serif, Monospace
- ✅ Font changes are visually distinct
- ✅ Monospace shows fixed-width characters
- ✅ Fonts preserved in sent email

**Pass/Fail**: _______

---

## TC-015: Text Color Picker
**Objective**: Verify text color selection works

**Test Steps**:
1. Type text
2. Select text
3. Click Text Color button
4. Select red color
5. Type new text in blue

**Expected Result**:
- ✅ Color picker opens with palette
- ✅ Selected text changes to chosen color
- ✅ Multiple colors can be used in same email
- ✅ Color picker shows recently used colors

**Pass/Fail**: _______

---

## TC-016: Background Color Highlight
**Objective**: Verify background color/highlight works

**Test Steps**:
1. Type "Highlighted text"
2. Select text
3. Click Background Color button
4. Select yellow

**Expected Result**:
- ✅ Background color picker opens
- ✅ Text shows yellow highlight
- ✅ Looks like highlighter marker effect
- ✅ Readable in sent email

**Pass/Fail**: _______

---

## TC-017: Heading Styles
**Objective**: Verify heading formatting works

**Test Steps**:
1. Type "Heading 1"
2. Select text
3. Apply Heading 1 from dropdown
4. Press Enter and type "Normal text"
5. Create Heading 2, Heading 3

**Expected Result**:
- ✅ H1 is largest and bold
- ✅ H2 slightly smaller
- ✅ H3 even smaller
- ✅ Clear size hierarchy (H1 > H2 > H3 > Normal)
- ✅ After Enter, returns to Normal style

**Pass/Fail**: _______

---

## TC-018: Text Alignment
**Objective**: Verify text alignment options work

**Test Steps**:
1. Type paragraph of text
2. Click Align Center
3. Type new paragraph
4. Click Align Right
5. Type another paragraph in Align Left

**Expected Result**:
- ✅ Each paragraph aligns as selected
- ✅ Alignment applies to entire paragraph
- ✅ Visual alignment clear and correct
- ✅ Can justify text

**Pass/Fail**: _______

---

## TC-019: Bullet List Creation
**Objective**: Verify bullet list formatting

**Test Steps**:
1. Click Bullet List button
2. Type "Item 1" and press Enter
3. Type "Item 2" and press Enter
4. Type "Item 3"
5. Press Enter twice to exit list

**Expected Result**:
- ✅ Bullet point appears before each item
- ✅ Pressing Enter creates new bullet
- ✅ Pressing Enter twice exits list
- ✅ Can start list mid-document
- ✅ Proper indentation

**Pass/Fail**: _______

---

## TC-020: Numbered List Creation
**Objective**: Verify ordered/numbered list formatting

**Test Steps**:
1. Click Numbered List button
2. Type 3 items
3. Press Enter after each

**Expected Result**:
- ✅ Numbers auto-increment (1, 2, 3...)
- ✅ Proper numbering sequence
- ✅ Enter creates new numbered item
- ✅ Double Enter exits list

**Pass/Fail**: _______

---

## TC-021: Checklist Creation
**Objective**: Verify checklist formatting

**Test Steps**:
1. Click Checklist button
2. Type "Task 1" and press Enter
3. Type "Task 2" and press Enter
4. Type "Task 3"

**Expected Result**:
- ✅ Checkbox appears before each item
- ✅ Checkboxes are interactive
- ✅ Can check/uncheck items
- ✅ Checked items show checkmark
- ✅ Visual distinction between checked/unchecked

**Pass/Fail**: _______

---

## TC-022: Nested Lists - Indentation
**Objective**: Verify list indentation/nesting works

**Test Steps**:
1. Create bullet list with 3 items
2. On item 2, click "Increase Indent"
3. Type sub-item
4. Click "Decrease Indent"

**Expected Result**:
- ✅ Item indents to create sub-list
- ✅ Visual hierarchy clear (main vs sub-items)
- ✅ Can create multiple nesting levels
- ✅ Decrease indent returns to parent level

**Pass/Fail**: _______

---

## TC-023: Insert Link
**Objective**: Verify hyperlink insertion works

**Test Steps**:
1. Type "Click here"
2. Select text
3. Click Link button
4. Enter URL: https://example.com
5. Click Insert/OK

**Expected Result**:
- ✅ Link dialog opens
- ✅ Can enter URL
- ✅ Text becomes blue and underlined
- ✅ Hovering shows URL tooltip
- ✅ Link clickable in sent email

**Pass/Fail**: _______

---

## TC-024: Insert Image
**Objective**: Verify image insertion works

**Test Steps**:
1. Click Image button
2. Enter image URL or upload image
3. Insert into email

**Expected Result**:
- ✅ Image dialog opens
- ✅ Can paste image URL
- ✅ Can upload from computer
- ✅ Image displays inline in editor
- ✅ Image visible in sent email
- ✅ Proper sizing/scaling

**Pass/Fail**: _______

---

## TC-025: Blockquote Formatting
**Objective**: Verify blockquote/quote formatting

**Test Steps**:
1. Type text
2. Select text
3. Click Blockquote button

**Expected Result**:
- ✅ Text indented with left border/bar
- ✅ Distinct visual style (often gray or italic)
- ✅ Looks professional
- ✅ Can have multiple blockquotes

**Pass/Fail**: _______

---

## TC-026: Code Block Formatting
**Objective**: Verify code block formatting for technical content

**Test Steps**:
1. Click Code Block button
2. Type code:
   ```
   function test() {
     return "Hello";
   }
   ```

**Expected Result**:
- ✅ Monospace font applied
- ✅ Different background color (usually gray)
- ✅ Preserves spacing/indentation
- ✅ Good for sharing code snippets

**Pass/Fail**: _______

---

## TC-027: Subscript and Superscript
**Objective**: Verify subscript/superscript formatting

**Test Steps**:
1. Type "H2O"
2. Select "2" and click Subscript
3. Type "E=mc2"
4. Select "2" and click Superscript

**Expected Result**:
- ✅ Subscript text smaller and lower (H₂O)
- ✅ Superscript text smaller and higher (E=mc²)
- ✅ Useful for formulas and scientific notation

**Pass/Fail**: _______

---

## TC-028: Clean Formatting - Remove All Styles
**Objective**: Verify clean/clear formatting button works

**Test Steps**:
1. Type text with multiple formats (bold, red, heading 1, etc.)
2. Select all text
3. Click Clean button (eraser icon)

**Expected Result**:
- ✅ All formatting removed
- ✅ Text becomes plain/normal
- ✅ Font resets to default
- ✅ Colors removed
- ✅ Size resets to normal

**Pass/Fail**: _______

---

## TC-029: Keyboard Shortcuts - Basic
**Objective**: Verify common keyboard shortcuts work

**Test Shortcuts**:
- Ctrl+B = Bold
- Ctrl+I = Italic
- Ctrl+U = Underline
- Ctrl+Enter = Send email

**Test Steps**:
1. Type text and select it
2. Press Ctrl+B
3. Type new text
4. Press Ctrl+I
5. Press Ctrl+Enter to send

**Expected Result**:
- ✅ Ctrl+B applies bold
- ✅ Ctrl+I applies italic
- ✅ Ctrl+U applies underline
- ✅ Ctrl+Enter sends email (if valid)
- ✅ Pro tip message mentions Ctrl+Enter

**Pass/Fail**: _______

---

## TC-030: Paste Plain Text
**Objective**: Verify pasting removes unwanted formatting

**Test Steps**:
1. Copy formatted text from Word/website (with colors, fonts)
2. Paste into email editor (Ctrl+V)
3. Paste with Ctrl+Shift+V (paste plain)

**Expected Result**:
- ✅ Regular paste preserves some formatting
- ✅ Paste plain removes all formatting
- ✅ No broken HTML
- ✅ Clean professional appearance

**Pass/Fail**: _______

---

## TC-031: Undo/Redo
**Objective**: Verify undo and redo functionality

**Test Steps**:
1. Type "First text"
2. Type "Second text"
3. Press Ctrl+Z (undo)
4. Press Ctrl+Y (redo)

**Expected Result**:
- ✅ Ctrl+Z undoes last action
- ✅ Ctrl+Y redoes undone action
- ✅ Can undo multiple times
- ✅ Undo history preserved during session

**Pass/Fail**: _______

---

## TC-032: Email Body - Character Limit
**Objective**: Verify large emails can be composed

**Test Steps**:
1. Type or paste very long email (5000+ characters)
2. Apply various formatting
3. Try to send

**Expected Result**:
- ✅ No character limit errors (up to reasonable limit)
- ✅ Editor handles long content smoothly
- ✅ Scrolling works within editor
- ✅ Formatting preserved throughout

**Pass/Fail**: _______

---

## TC-033: Canned Responses - Insert
**Objective**: Verify pre-defined response templates work

**Test Steps**:
1. Open email composer
2. Click "Canned Responses" dropdown (if visible)
3. Select a template
4. Edit the inserted text

**Expected Result**:
- ✅ Dropdown shows available templates
- ✅ Selecting template inserts text into body
- ✅ Can edit template text after insertion
- ✅ Template variables replaced correctly

**Pass/Fail**: _______

**Status**: ⚠️ Feature availability depends on configuration

---

## TC-034: Internal Note Toggle
**Objective**: Verify private/internal note option

**Test Steps**:
1. Open email composer
2. Find "Internal Note" or "Private Note" checkbox/toggle
3. Enable it
4. Fill email and send

**Expected Result**:
- ✅ Toggle/checkbox visible
- ✅ When enabled, badge/indicator shows "Private Note"
- ✅ Email NOT sent externally
- ✅ Email visible only to internal staff
- ✅ Clear visual distinction from regular emails

**Pass/Fail**: _______

---

## TC-035: Cancel Email - No Content
**Objective**: Verify cancel closes immediately when empty

**Test Steps**:
1. Open email composer
2. Don't type anything
3. Click "Cancel" button

**Expected Result**:
- ✅ Composer closes immediately
- ✅ NO confirmation dialog
- ✅ Returns to complaint detail page
- ✅ Clean exit

**Pass/Fail**: _______

---

## TC-036: Cancel Email - With Content
**Objective**: Verify cancel prompts confirmation when content exists

**Test Steps**:
1. Open email composer
2. Type subject and body text
3. Add recipients
4. Click "Cancel" button
5. Click "OK" on confirmation
6. Repeat and click "Cancel" on confirmation

**Expected Result**:
- ✅ Confirmation dialog appears: "Are you sure you want to discard your changes?"
- ✅ Clicking OK closes composer and discards
- ✅ Clicking Cancel returns to composer (keeps content)
- ✅ Form is cleared when discarded

**Pass/Fail**: _______

---

## TC-037: Form Validation - No Recipients
**Objective**: Verify validation prevents sending without recipients

**Test Steps**:
1. Open email composer
2. Fill subject and body
3. Leave To field empty
4. Click "Send"

**Expected Result**:
- ✅ Error message: "Please add at least one recipient"
- ✅ To field highlighted in red
- ✅ Email not sent
- ✅ Focus moved to To field

**Pass/Fail**: _______

---

## TC-038: Form Validation - Empty Subject
**Objective**: Verify behavior when subject is empty

**Test Steps**:
1. Fill To field and body
2. Leave subject empty
3. Click "Send"

**Expected Result**:
- ✅ Warning: "Subject is empty. Send anyway?"
- OR ✅ Email sends with "(No Subject)"
- ✅ Subject field highlighted
- ✅ User can decide to add subject or proceed

**Pass/Fail**: _______

---

## TC-039: Form Validation - Empty Body
**Objective**: Verify validation for empty email body

**Test Steps**:
1. Fill To and Subject
2. Leave body completely empty
3. Click "Send"

**Expected Result**:
- ✅ Warning: "Email body is empty. Send anyway?"
- ✅ Can proceed or cancel
- ✅ Body editor highlighted

**Pass/Fail**: _______

---

## TC-040: Send Email - Success Flow
**Objective**: Verify complete successful email send

**Test Steps**:
1. Fill all required fields correctly
2. Apply various formatting
3. Click "Send"
4. Wait for response

**Expected Result**:
- ✅ "Sending..." indicator appears
- ✅ Send button disabled during send
- ✅ Success message: "Email sent successfully"
- ✅ Composer closes automatically
- ✅ Email appears in Email Thread section
- ✅ Email marked as "Sent"
- ✅ Timestamp shows current time

**Pass/Fail**: _______

---

## TC-041: Send Email - Error Handling
**Objective**: Verify graceful error handling on send failure

**Test Steps**:
1. (Simulate error: disconnect network or stop backend)
2. Fill email and send
3. Check error message

**Expected Result**:
- ✅ Error message displayed clearly
- ✅ "Failed to send email. Please try again."
- ✅ Composer stays open (doesn't close)
- ✅ Email content preserved
- ✅ Can retry sending
- ✅ Error auto-dismisses after 5 seconds

**Pass/Fail**: _______

---

## TC-042: Email Preview - Before Send
**Objective**: Verify user can preview email before sending

**Test Steps**:
1. Compose email with rich formatting
2. Look for "Preview" button/option
3. View preview

**Expected Result**:
- ✅ Preview shows email as recipient will see it
- ✅ All formatting preserved
- ✅ Can return to edit from preview
- ✅ Professional appearance

**Pass/Fail**: _______

**Status**: ⚠️ Feature may not be implemented

---

## TC-043: Forwarded Email - Formatting Quality
**Objective**: Verify forwarded emails have clean, professional formatting

**Test Steps**:
1. Forward an email thread (3+ messages)
2. Check the formatted content in editor

**Expected Formatting**:
- ✅ "Forwarded Conversation (X messages)" header in bold
- ✅ Each message numbered clearly
- ✅ Direction indicators (📥 Received, 📤 Sent) - or text equivalents
- ✅ Clean horizontal separators between messages
- ✅ Headers: From, Sent, Subject properly labeled
- ✅ Date/time in readable format
- ✅ NO text shadows
- ✅ NO selection-like highlighting
- ✅ NO table borders/artifacts
- ✅ Outlook-style professional appearance
- ✅ Calibri or similar clean font
- ✅ Blue accent color for headers (#0066cc)

**Pass/Fail**: _______

---

## TC-044: Forwarded Email - Edit Before Send
**Objective**: Verify user can edit forwarded content

**Test Steps**:
1. Forward an email
2. Add your own text at top: "FYI - Please review"
3. Edit subject
4. Send

**Expected Result**:
- ✅ Can add text above forwarded content
- ✅ Cursor positioned correctly
- ✅ Can edit any part of email
- ✅ Your additions + forwarded thread both sent
- ✅ Clean distinction between your text and forwarded content

**Pass/Fail**: _______

---

## TC-045: Multi-Language Support
**Objective**: Verify editor handles international characters

**Test Steps**:
1. Type email in different languages:
   - English: "Hello World"
   - Spanish: "Hola Mundo áéíóú"
   - French: "Bonjour àèéêë"
   - German: "Hallo Welt äöüß"
   - Japanese: "こんにちは世界"
   - Chinese: "你好世界"
   - Arabic: "مرحبا بالعالم"

**Expected Result**:
- ✅ All characters display correctly
- ✅ RTL languages (Arabic) align right when RTL enabled
- ✅ No encoding errors (���)
- ✅ Copy/paste preserves characters
- ✅ Sent emails display correctly

**Pass/Fail**: _______

---

## TC-046: Emoji Support
**Objective**: Verify emojis can be used in emails

**Test Steps**:
1. Type text with emojis: "Hello 😀 Great job! ✅ 📧"
2. Send email

**Expected Result**:
- ✅ Emojis display in editor
- ✅ Emojis preserved in sent email
- ✅ Recipients see emojis correctly
- ✅ No broken characters

**Pass/Fail**: _______

---

## TC-047: Responsive Design - Small Screen
**Objective**: Verify composer works on smaller screens/resized windows

**Test Steps**:
1. Resize browser to 768px width (tablet size)
2. Open email composer
3. Try all features

**Expected Result**:
- ✅ Composer adapts to screen size
- ✅ Toolbar buttons still accessible
- ✅ Text fields full width
- ✅ No horizontal scrolling
- ✅ Buttons stack vertically if needed
- ✅ Usable on tablets

**Pass/Fail**: _______

---

## TC-048: Accessibility - Keyboard Navigation
**Objective**: Verify composer is keyboard-accessible

**Test Steps**:
1. Open composer
2. Tab through all fields
3. Try to format text using keyboard only
4. Send using keyboard

**Expected Result**:
- ✅ Tab moves between fields in logical order
- ✅ All buttons reachable via Tab
- ✅ Focused elements show clear outline
- ✅ Can activate buttons with Enter/Space
- ✅ Escape key closes composer
- ✅ Fully usable without mouse

**Pass/Fail**: _______

---

## TC-049: Accessibility - Screen Reader
**Objective**: Verify composer works with screen readers

**Test Steps** (with screen reader enabled):
1. Open composer
2. Navigate through fields
3. Listen to announcements

**Expected Result**:
- ✅ Fields have proper labels announced
- ✅ Buttons announce their purpose
- ✅ Required fields indicated
- ✅ Error messages announced
- ✅ Success messages announced
- ✅ ARIA labels present

**Pass/Fail**: _______

**Status**: ⚠️ Requires screen reader software

---

## TC-050: Performance - Large Email
**Objective**: Verify performance with complex formatted emails

**Test Steps**:
1. Create email with:
   - 5000+ characters
   - 20+ formatting changes
   - Multiple lists
   - Several images
2. Type additional text
3. Send

**Expected Result**:
- ✅ Editor remains responsive
- ✅ No lag while typing
- ✅ Formatting buttons respond immediately
- ✅ Send completes within 3 seconds
- ✅ No browser freezing

**Pass/Fail**: _______

---

## Test Summary Report

### Testing Date: ___________
### Tester Name: ___________
### Browser(s): ___________

| Test ID | Feature | Status | Notes |
|---------|---------|--------|-------|
| TC-001 | Open Composer - New | ⬜ | |
| TC-002 | Open Composer - Reply | ⬜ | |
| TC-003 | Reply All | ⬜ | |
| TC-004 | Forward | ⬜ | |
| TC-005 | Add Recipients - To | ⬜ | |
| TC-006 | Add Recipients - Cc/Bcc | ⬜ | |
| TC-007 | Remove Recipients | ⬜ | |
| TC-008 | Email Validation | ⬜ | |
| TC-009 | Subject Line | ⬜ | |
| TC-010 | Toolbar Visibility | ⬜ | |
| TC-011 | Bold Formatting | ⬜ | |
| TC-012 | Multiple Styles | ⬜ | |
| TC-013 | Font Size | ⬜ | |
| TC-014 | Font Family | ⬜ | |
| TC-015 | Text Color | ⬜ | |
| TC-016 | Background Color | ⬜ | |
| TC-017 | Headings | ⬜ | |
| TC-018 | Text Alignment | ⬜ | |
| TC-019 | Bullet Lists | ⬜ | |
| TC-020 | Numbered Lists | ⬜ | |
| TC-021 | Checklist | ⬜ | |
| TC-022 | Nested Lists | ⬜ | |
| TC-023 | Insert Link | ⬜ | |
| TC-024 | Insert Image | ⬜ | |
| TC-025 | Blockquote | ⬜ | |
| TC-026 | Code Block | ⬜ | |
| TC-027 | Sub/Superscript | ⬜ | |
| TC-028 | Clean Formatting | ⬜ | |
| TC-029 | Keyboard Shortcuts | ⬜ | |
| TC-030 | Paste Text | ⬜ | |
| TC-031 | Undo/Redo | ⬜ | |
| TC-032 | Long Emails | ⬜ | |
| TC-033 | Canned Responses | ⬜ | |
| TC-034 | Internal Notes | ⬜ | |
| TC-035 | Cancel - Empty | ⬜ | |
| TC-036 | Cancel - With Content | ⬜ | |
| TC-037 | Validation - No Recipients | ⬜ | |
| TC-038 | Validation - No Subject | ⬜ | |
| TC-039 | Validation - No Body | ⬜ | |
| TC-040 | Send Success | ⬜ | |
| TC-041 | Send Error | ⬜ | |
| TC-042 | Email Preview | ⬜ | |
| TC-043 | Forward Formatting | ⬜ | |
| TC-044 | Edit Forwarded | ⬜ | |
| TC-045 | Multi-Language | ⬜ | |
| TC-046 | Emoji Support | ⬜ | |
| TC-047 | Responsive Design | ⬜ | |
| TC-048 | Keyboard Navigation | ⬜ | |
| TC-049 | Screen Reader | ⬜ | |
| TC-050 | Performance | ⬜ | |

**Total Tests**: 50
**Passed**: ___
**Failed**: ___
**Blocked**: ___
**Not Tested**: ___

**Pass Rate**: ____%

---

## Critical Issues Found

1.
2.
3.

---

## Recommendations

1.
2.
3.

---

## Sign-off

**Tester**: _________________
**Date**: _________________
**Status**: ☐ Approved  ☐ Rejected  ☐ Needs Rework
