using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.MasterData;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.EmployeeCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.Phone)
            .HasMaxLength(50);

        builder.Property(u => u.JobTitle)
            .HasMaxLength(200);

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(500);

        builder.Property(u => u.OryggiEmployeeId)
            .HasMaxLength(100);

        // Password Management Properties
        builder.Property(u => u.PasswordExpiresAt);

        builder.Property(u => u.MustChangePasswordOnNextLogin)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.PasswordNeverExpires)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.PasswordChangedAt);

        builder.Property(u => u.PasswordChangedBy);

        builder.Property(u => u.FailedLoginAttempts)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(u => u.AccountLockedUntil);

        builder.Property(u => u.LastPasswordChangeRequiredNotificationSentAt);

        // Authentication Provider Properties
        builder.Property(u => u.AuthenticationProviderType)
            .IsRequired()
            .HasConversion<string>() // Store as string in database
            .HasMaxLength(50)
            .HasDefaultValue(AuthenticationProviderType.Local);

        builder.Property(u => u.ExternalUserId)
            .HasMaxLength(500);

        builder.Property(u => u.ExternalUsername)
            .HasMaxLength(200);

        builder.Property(u => u.IdentityProvider)
            .HasMaxLength(100);

        builder.Property(u => u.LastExternalSyncAt);

        builder.Property(u => u.ExternalSyncEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.SSOEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.LocalPasswordEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.PreferredAuthMethod)
            .HasMaxLength(50);

        // Computed column for FullName
        builder.Ignore(u => u.FullName);

        // Indexes
        builder.HasIndex(u => u.EmployeeCode)
            .IsUnique();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasIndex(u => u.OryggiEmployeeId);

        builder.HasIndex(u => u.CompanyId);

        builder.HasIndex(u => u.ManagerId);

        // Password Management Indexes
        builder.HasIndex(u => u.PasswordExpiresAt)
            .HasDatabaseName("IX_Users_PasswordExpiresAt");

        builder.HasIndex(u => u.AccountLockedUntil)
            .HasDatabaseName("IX_Users_AccountLockedUntil");

        // Authentication Provider Indexes
        builder.HasIndex(u => u.AuthenticationProviderType)
            .HasDatabaseName("IX_Users_AuthenticationProviderType");

        builder.HasIndex(u => u.ExternalUserId)
            .HasDatabaseName("IX_Users_ExternalUserId");

        // Composite index for external user lookup
        builder.HasIndex(u => new { u.AuthenticationProviderType, u.ExternalUserId })
            .HasDatabaseName("IX_Users_AuthProviderType_ExternalUserId");

        // Relationships
        builder.HasOne(u => u.Company)
            .WithMany(c => c.Users)
            .HasForeignKey(u => u.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Branch)
            .WithMany(b => b.Users)
            .HasForeignKey(u => u.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(u => u.Department)
            .WithMany(d => d.Users)
            .HasForeignKey(u => u.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(u => u.Section)
            .WithMany(s => s.Users)
            .HasForeignKey(u => u.SectionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(u => u.Manager)
            .WithMany(m => m.Subordinates)
            .HasForeignKey(u => u.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.SubmittedComplaints)
            .WithOne(c => c.Complainant)
            .HasForeignKey(c => c.ComplainantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.AssignedComplaints)
            .WithOne(c => c.AssignedTo)
            .HasForeignKey(c => c.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(u => u.UserComplaintRoles)
            .WithOne(ucr => ucr.User)
            .HasForeignKey(ucr => ucr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Comments)
            .WithOne(cc => cc.Commenter)
            .HasForeignKey(cc => cc.CommentedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Password Management Relationships
        builder.HasMany(u => u.PasswordHistories)
            .WithOne(ph => ph.User)
            .HasForeignKey(ph => ph.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.PasswordAuditLogs)
            .WithOne(pal => pal.User)
            .HasForeignKey(pal => pal.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // External User Mappings Relationship
        builder.HasMany(u => u.ExternalUserMappings)
            .WithOne(eum => eum.User)
            .HasForeignKey(eum => eum.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
