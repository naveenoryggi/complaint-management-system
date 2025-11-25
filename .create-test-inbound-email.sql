-- Create a test INBOUND email to verify buttons appear

DECLARE @ComplaintId UNIQUEIDENTIFIER = 'e9dc50f7-493c-4e13-a5a0-dc42085d4fca'; -- First complaint
DECLARE @CompanyId UNIQUEIDENTIFIER = 'fe28cd85-4226-4daa-9e45-66a3d51877fa';
DECLARE @EmailId UNIQUEIDENTIFIER = NEWID();

-- Insert INBOUND email (Direction = 1)
INSERT INTO EmailMessages (
    Id,
    ComplaintId,
    CompanyId,
    MessageId,
    Subject,
    FromEmail,
    FromName,
    TextBody,
    Direction,        -- 1 = Inbound
    ReceivedAt,
    SentAt,
    IsRead,
    IsPrivateNote,
    IsDeleted,
    CreatedAt,
    UpdatedAt
) VALUES (
    @EmailId,
    @ComplaintId,
    @CompanyId,
    'TEST-INBOUND-' + CAST(NEWID() AS NVARCHAR(50)),
    'Re: Complaint Response - Testing Reply Buttons',
    'customer@example.com',
    'Test Customer',
    'This is a test inbound email from a customer. Reply, Reply All, and Forward buttons should appear for this email.',
    1,                -- Direction = 1 (Inbound)
    GETUTCDATE(),
    GETUTCDATE(),
    0,                -- Unread
    0,                -- Not a private note
    0,                -- Not deleted
    GETUTCDATE(),
    GETUTCDATE()
);

-- Add TO recipient
INSERT INTO ComplaintEmailParticipants (
    Id,
    EmailMessageId,
    ParticipantType,
    EmailAddress,
    DisplayName,
    CreatedAt
) VALUES (
    NEWID(),
    @EmailId,
    0,  -- To
    'marketing@oryggitech.com',
    'Oryggi Tech Support',
    GETUTCDATE()
);

-- Verify insertion
SELECT
    Id,
    Subject,
    FromEmail,
    Direction,
    CASE
        WHEN Direction = 0 THEN 'Outbound'
        WHEN Direction = 1 THEN 'Inbound'
        ELSE 'Unknown'
    END AS DirectionName,
    IsRead
FROM EmailMessages
WHERE ComplaintId = @ComplaintId
ORDER BY ReceivedAt DESC;
