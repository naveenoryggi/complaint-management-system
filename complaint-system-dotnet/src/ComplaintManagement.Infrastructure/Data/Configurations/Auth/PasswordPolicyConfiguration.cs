using ComplaintManagement.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Auth;

/// <summary>
/// Entity Framework configuration for PasswordPolicy entity
/// </summary>
public class PasswordPolicyConfiguration : IEntityTypeConfiguration<PasswordPolicy>
{
    public void Configure(EntityTypeBuilder<PasswordPolicy> builder)
    {
        // Table name
        builder.ToTable("PasswordPolicy");

        // Primary key
        builder.HasKey(pp => pp.Id);

        // Properties
        builder.Property(pp => pp.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(pp => pp.CompanyId)
            .IsRequired();

        builder.Property(pp => pp.MinimumLength)
            .IsRequired()
            .HasDefaultValue(8);

        builder.Property(pp => pp.RequireUppercase)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(pp => pp.RequireLowercase)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(pp => pp.RequireDigit)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(pp => pp.RequireSpecialCharacter)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(pp => pp.PasswordExpirationDays)
            .IsRequired()
            .HasDefaultValue(90);

        builder.Property(pp => pp.PasswordExpirationWarningDays)
            .IsRequired()
            .HasDefaultValue(7);

        builder.Property(pp => pp.MaxFailedLoginAttempts)
            .IsRequired()
            .HasDefaultValue(5);

        builder.Property(pp => pp.AccountLockoutDurationMinutes)
            .IsRequired()
            .HasDefaultValue(15);

        builder.Property(pp => pp.PasswordHistoryCount)
            .IsRequired()
            .HasDefaultValue(5);

        builder.Property(pp => pp.MinimumPasswordAgeDays)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(pp => pp.EnablePasswordComplexity)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(pp => pp.AllowSkipPasswordChange)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(pp => pp.SendPasswordExpirationEmails)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(pp => pp.SendPasswordSetEmails)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(pp => pp.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(pp => pp.CreatedBy)
            .IsRequired();

        builder.Property(pp => pp.UpdatedAt)
            .IsRequired(false);

        builder.Property(pp => pp.UpdatedBy)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(pp => pp.CompanyId)
            .IsUnique() // One policy per company
            .HasDatabaseName("IX_PasswordPolicy_CompanyId");

        // Relationships
        builder.HasOne(pp => pp.Company)
            .WithMany()
            .HasForeignKey(pp => pp.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
