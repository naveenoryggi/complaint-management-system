BEGIN TRANSACTION;
ALTER TABLE [EmailConfigurations] ADD [PollingIntervalSeconds] int NULL;

CREATE TABLE [EmailResponseHistories] (
    [Id] uniqueidentifier NOT NULL,
    [ComplaintId] uniqueidentifier NOT NULL,
    [EmailMessageId] uniqueidentifier NULL,
    [SentBy] uniqueidentifier NOT NULL,
    [SentTo] nvarchar(1000) NOT NULL,
    [CarbonCopy] nvarchar(1000) NULL,
    [BlindCarbonCopy] nvarchar(1000) NULL,
    [Subject] nvarchar(500) NOT NULL,
    [Body] nvarchar(max) NOT NULL,
    [IsHtml] bit NOT NULL DEFAULT CAST(1 AS bit),
    [SentAt] datetime2 NOT NULL,
    [DeliveryStatus] nvarchar(50) NULL DEFAULT N'Sent',
    [ErrorMessage] nvarchar(2000) NULL,
    [MessageId] nvarchar(500) NULL,
    [HasAttachments] bit NOT NULL,
    [AttachmentIds] nvarchar(2000) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] uniqueidentifier NULL,
    CONSTRAINT [PK_EmailResponseHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EmailResponseHistories_Complaints_ComplaintId] FOREIGN KEY ([ComplaintId]) REFERENCES [Complaints] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_EmailResponseHistories_EmailMessages_EmailMessageId] FOREIGN KEY ([EmailMessageId]) REFERENCES [EmailMessages] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_EmailResponseHistories_Users_SentBy] FOREIGN KEY ([SentBy]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

UPDATE [ComplaintPriorityMasters] SET [CreatedAt] = '2025-11-14T12:59:57.5188356Z', [UpdatedAt] = '2025-11-14T12:59:57.5188585Z'
WHERE [Id] = '20000000-0000-0000-0000-000000000001';
SELECT @@ROWCOUNT;


UPDATE [ComplaintPriorityMasters] SET [CreatedAt] = '2025-11-14T12:59:57.5189046Z', [UpdatedAt] = '2025-11-14T12:59:57.5189048Z'
WHERE [Id] = '20000000-0000-0000-0000-000000000002';
SELECT @@ROWCOUNT;


UPDATE [ComplaintPriorityMasters] SET [CreatedAt] = '2025-11-14T12:59:57.5189054Z', [UpdatedAt] = '2025-11-14T12:59:57.5189055Z'
WHERE [Id] = '20000000-0000-0000-0000-000000000003';
SELECT @@ROWCOUNT;


UPDATE [ComplaintPriorityMasters] SET [CreatedAt] = '2025-11-14T12:59:57.5189062Z', [UpdatedAt] = '2025-11-14T12:59:57.5189063Z'
WHERE [Id] = '20000000-0000-0000-0000-000000000004';
SELECT @@ROWCOUNT;


UPDATE [ComplaintPriorityMasters] SET [CreatedAt] = '2025-11-14T12:59:57.5189068Z', [UpdatedAt] = '2025-11-14T12:59:57.5189069Z'
WHERE [Id] = '20000000-0000-0000-0000-000000000005';
SELECT @@ROWCOUNT;


UPDATE [ComplaintStatusMasters] SET [CreatedAt] = '2025-11-14T12:59:57.5236425Z', [UpdatedAt] = '2025-11-14T12:59:57.5236432Z'
WHERE [Id] = '10000000-0000-0000-0000-000000000001';
SELECT @@ROWCOUNT;


UPDATE [ComplaintStatusMasters] SET [CreatedAt] = '2025-11-14T12:59:57.5236448Z', [UpdatedAt] = '2025-11-14T12:59:57.5236448Z'
WHERE [Id] = '10000000-0000-0000-0000-000000000002';
SELECT @@ROWCOUNT;


UPDATE [ComplaintStatusMasters] SET [CreatedAt] = '2025-11-14T12:59:57.5236477Z', [UpdatedAt] = '2025-11-14T12:59:57.5236478Z'
WHERE [Id] = '10000000-0000-0000-0000-000000000003';
SELECT @@ROWCOUNT;


UPDATE [ComplaintStatusMasters] SET [CreatedAt] = '2025-11-14T12:59:57.5236483Z', [UpdatedAt] = '2025-11-14T12:59:57.5236484Z'
WHERE [Id] = '10000000-0000-0000-0000-000000000004';
SELECT @@ROWCOUNT;


UPDATE [ComplaintStatusMasters] SET [CreatedAt] = '2025-11-14T12:59:57.5236489Z', [UpdatedAt] = '2025-11-14T12:59:57.5236490Z'
WHERE [Id] = '10000000-0000-0000-0000-000000000005';
SELECT @@ROWCOUNT;


UPDATE [ComplaintStatusMasters] SET [CreatedAt] = '2025-11-14T12:59:57.5236495Z', [UpdatedAt] = '2025-11-14T12:59:57.5236496Z'
WHERE [Id] = '10000000-0000-0000-0000-000000000006';
SELECT @@ROWCOUNT;


UPDATE [ComplaintStatusMasters] SET [CreatedAt] = '2025-11-14T12:59:57.5236501Z', [UpdatedAt] = '2025-11-14T12:59:57.5236502Z'
WHERE [Id] = '10000000-0000-0000-0000-000000000007';
SELECT @@ROWCOUNT;


UPDATE [ComplaintStatusMasters] SET [CreatedAt] = '2025-11-14T12:59:57.5236507Z', [UpdatedAt] = '2025-11-14T12:59:57.5236508Z'
WHERE [Id] = '10000000-0000-0000-0000-000000000008';
SELECT @@ROWCOUNT;


UPDATE [ComplaintStatusMasters] SET [CreatedAt] = '2025-11-14T12:59:57.5236514Z', [UpdatedAt] = '2025-11-14T12:59:57.5236515Z'
WHERE [Id] = '10000000-0000-0000-0000-000000000009';
SELECT @@ROWCOUNT;


CREATE INDEX [IX_EmailResponseHistories_ComplaintId] ON [EmailResponseHistories] ([ComplaintId]);

CREATE INDEX [IX_EmailResponseHistories_ComplaintId_SentAt] ON [EmailResponseHistories] ([ComplaintId], [SentAt]);

CREATE INDEX [IX_EmailResponseHistories_DeliveryStatus] ON [EmailResponseHistories] ([DeliveryStatus]);

CREATE INDEX [IX_EmailResponseHistories_EmailMessageId] ON [EmailResponseHistories] ([EmailMessageId]);

CREATE INDEX [IX_EmailResponseHistories_SentAt] ON [EmailResponseHistories] ([SentAt]);

CREATE INDEX [IX_EmailResponseHistories_SentBy] ON [EmailResponseHistories] ([SentBy]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251114130000_AddEmailThreadingSupport', N'9.0.9');

COMMIT;
GO

