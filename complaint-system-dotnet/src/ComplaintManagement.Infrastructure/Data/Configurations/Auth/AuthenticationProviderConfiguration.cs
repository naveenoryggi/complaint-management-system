using ComplaintManagement.Domain.Entities.Auth;
using ComplaintManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Auth;

/// <summary>
/// Entity Framework configuration for AuthenticationProvider entity
/// </summary>
public class AuthenticationProviderConfiguration : IEntityTypeConfiguration<AuthenticationProvider>
{
    public void Configure(EntityTypeBuilder<AuthenticationProvider> builder)
    {
        // Table name
        builder.ToTable("AuthenticationProviders");

        // Primary key
        builder.HasKey(ap => ap.Id);

        // Properties
        builder.Property(ap => ap.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(ap => ap.CompanyId)
            .IsRequired();

        builder.Property(ap => ap.ProviderType)
            .IsRequired()
            .HasConversion<string>() // Store as string in database
            .HasMaxLength(50);

        builder.Property(ap => ap.ProviderName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ap => ap.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(ap => ap.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ap => ap.Priority)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(ap => ap.EmailDomain)
            .HasMaxLength(200);

        // Active Directory / LDAP Properties
        builder.Property(ap => ap.ADDomain)
            .HasMaxLength(200);

        builder.Property(ap => ap.ADServer)
            .HasMaxLength(200);

        builder.Property(ap => ap.ADPort);

        builder.Property(ap => ap.ADBaseDN)
            .HasMaxLength(500);

        builder.Property(ap => ap.ADUserFilter)
            .HasMaxLength(500);

        builder.Property(ap => ap.ADUseSSL);

        builder.Property(ap => ap.ADServiceAccountUsername)
            .HasMaxLength(200);

        builder.Property(ap => ap.ADServiceAccountPasswordEncrypted)
            .HasMaxLength(1000);

        // SAML Properties
        builder.Property(ap => ap.SAMLEntityId)
            .HasMaxLength(500);

        builder.Property(ap => ap.SAMLSSOUrl)
            .HasMaxLength(500);

        builder.Property(ap => ap.SAMLSLOUrl)
            .HasMaxLength(500);

        builder.Property(ap => ap.SAMLCertificate)
            .HasMaxLength(5000); // X.509 certificates can be large

        builder.Property(ap => ap.SAMLSigningAlgorithm)
            .HasMaxLength(200);

        // OAuth 2.0 / OIDC Properties
        builder.Property(ap => ap.OAuthClientId)
            .HasMaxLength(500);

        builder.Property(ap => ap.OAuthClientSecretEncrypted)
            .HasMaxLength(1000);

        builder.Property(ap => ap.OAuthAuthorizationUrl)
            .HasMaxLength(500);

        builder.Property(ap => ap.OAuthTokenUrl)
            .HasMaxLength(500);

        builder.Property(ap => ap.OAuthUserInfoUrl)
            .HasMaxLength(500);

        builder.Property(ap => ap.OAuthScopes)
            .HasMaxLength(500);

        builder.Property(ap => ap.OAuthRedirectUri)
            .HasMaxLength(500);

        // Azure AD Properties
        builder.Property(ap => ap.AzureADTenantId)
            .HasMaxLength(100);

        builder.Property(ap => ap.AzureADClientId)
            .HasMaxLength(500);

        builder.Property(ap => ap.AzureADClientSecretEncrypted)
            .HasMaxLength(1000);

        builder.Property(ap => ap.AzureADInstance)
            .HasMaxLength(500);

        // Custom API Properties
        builder.Property(ap => ap.CustomAPIEndpoint)
            .HasMaxLength(500);

        builder.Property(ap => ap.CustomAPIMethod)
            .HasMaxLength(10);

        builder.Property(ap => ap.CustomAPIHeaders)
            .HasMaxLength(2000);

        builder.Property(ap => ap.CustomAPIRequestTemplate)
            .HasMaxLength(2000);

        builder.Property(ap => ap.CustomAPIKeyEncrypted)
            .HasMaxLength(1000);

        // JIT Provisioning Properties
        builder.Property(ap => ap.JITProvisioningEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(ap => ap.AutoAssignRoleId);

        builder.Property(ap => ap.SyncAttributesOnLogin)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(ap => ap.AttributeMapping)
            .HasMaxLength(5000); // JSON

        builder.Property(ap => ap.GroupToRoleMapping)
            .HasMaxLength(5000); // JSON

        // Audit Properties
        builder.Property(ap => ap.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(ap => ap.CreatedBy)
            .IsRequired();

        builder.Property(ap => ap.UpdatedAt);

        builder.Property(ap => ap.UpdatedBy);

        builder.Property(ap => ap.LastUsedAt);

        builder.Property(ap => ap.LastHealthCheckAt);

        builder.Property(ap => ap.LastHealthCheckSuccess);

        // Indexes
        builder.HasIndex(ap => ap.CompanyId)
            .HasDatabaseName("IX_AuthenticationProviders_CompanyId");

        builder.HasIndex(ap => ap.ProviderType)
            .HasDatabaseName("IX_AuthenticationProviders_ProviderType");

        builder.HasIndex(ap => ap.IsEnabled)
            .HasDatabaseName("IX_AuthenticationProviders_IsEnabled");

        builder.HasIndex(ap => ap.IsDefault)
            .HasDatabaseName("IX_AuthenticationProviders_IsDefault");

        builder.HasIndex(ap => ap.EmailDomain)
            .HasDatabaseName("IX_AuthenticationProviders_EmailDomain");

        // Composite index for efficient provider lookup
        builder.HasIndex(ap => new { ap.CompanyId, ap.IsEnabled })
            .HasDatabaseName("IX_AuthenticationProviders_CompanyId_IsEnabled");

        // Composite index for default provider lookup
        builder.HasIndex(ap => new { ap.CompanyId, ap.IsDefault })
            .HasDatabaseName("IX_AuthenticationProviders_CompanyId_IsDefault");

        // Relationships
        builder.HasOne(ap => ap.Company)
            .WithMany()
            .HasForeignKey(ap => ap.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ap => ap.AutoAssignRole)
            .WithMany()
            .HasForeignKey(ap => ap.AutoAssignRoleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(ap => ap.ExternalUserMappings)
            .WithOne(eum => eum.AuthenticationProvider)
            .HasForeignKey(eum => eum.AuthenticationProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
