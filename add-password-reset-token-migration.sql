-- Migration: Add PasswordResetToken table for self-service password reset
-- Date: 2025-11-15
-- Description: Adds PasswordResetTokens table and related indexes for secure password recovery

-- Create PasswordResetTokens table
CREATE TABLE [dbo].[PasswordResetTokens] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Token] NVARCHAR(100) NOT NULL,
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [Email] NVARCHAR(256) NOT NULL,
    [ExpiresAt] DATETIME2(7) NOT NULL,
    [IsUsed] BIT NOT NULL DEFAULT 0,
    [UsedAt] DATETIME2(7) NULL,
    [RequestIpAddress] NVARCHAR(50) NULL,
    [ResetIpAddress] NVARCHAR(50) NULL,
    [RequestUserAgent] NVARCHAR(500) NULL,
    [ResetUserAgent] NVARCHAR(500) NULL,
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] UNIQUEIDENTIFIER NULL,
    [UpdatedAt] DATETIME2(7) NULL,
    [UpdatedBy] UNIQUEIDENTIFIER NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [DeletedAt] DATETIME2(7) NULL,
    [DeletedBy] UNIQUEIDENTIFIER NULL,

    CONSTRAINT [FK_PasswordResetTokens_Users_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

-- Create indexes for performance
CREATE UNIQUE INDEX [IX_PasswordResetTokens_Token]
    ON [dbo].[PasswordResetTokens] ([Token])
    WHERE [IsDeleted] = 0;

CREATE INDEX [IX_PasswordResetTokens_UserId]
    ON [dbo].[PasswordResetTokens] ([UserId]);

CREATE INDEX [IX_PasswordResetTokens_Email_CreatedAt]
    ON [dbo].[PasswordResetTokens] ([Email], [CreatedAt]);

CREATE INDEX [IX_PasswordResetTokens_ExpiresAt_IsUsed]
    ON [dbo].[PasswordResetTokens] ([ExpiresAt], [IsUsed])
    WHERE [IsDeleted] = 0;

-- Add comment
EXEC sp_addextendedproperty
    @name = N'MS_Description',
    @value = N'Self-service password reset tokens with 24-hour expiration and single-use enforcement',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'PasswordResetTokens';

PRINT 'PasswordResetTokens table created successfully';
