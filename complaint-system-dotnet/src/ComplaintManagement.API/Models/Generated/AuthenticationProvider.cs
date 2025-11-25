using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class AuthenticationProvider
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public string ProviderType { get; set; } = null!;

    public string ProviderName { get; set; } = null!;

    public bool IsEnabled { get; set; }

    public bool IsDefault { get; set; }

    public int Priority { get; set; }

    public string? EmailDomain { get; set; }

    public string? Addomain { get; set; }

    public string? Adserver { get; set; }

    public int? Adport { get; set; }

    public string? AdbaseDn { get; set; }

    public string? AduserFilter { get; set; }

    public bool? AduseSsl { get; set; }

    public string? AdserviceAccountUsername { get; set; }

    public string? AdserviceAccountPasswordEncrypted { get; set; }

    public string? SamlentityId { get; set; }

    public string? Samlssourl { get; set; }

    public string? Samlslourl { get; set; }

    public string? Samlcertificate { get; set; }

    public string? SamlsigningAlgorithm { get; set; }

    public string? OauthClientId { get; set; }

    public string? OauthClientSecretEncrypted { get; set; }

    public string? OauthAuthorizationUrl { get; set; }

    public string? OauthTokenUrl { get; set; }

    public string? OauthUserInfoUrl { get; set; }

    public string? OauthScopes { get; set; }

    public string? OauthRedirectUri { get; set; }

    public string? AzureAdtenantId { get; set; }

    public string? AzureAdclientId { get; set; }

    public string? AzureAdclientSecretEncrypted { get; set; }

    public string? AzureAdinstance { get; set; }

    public string? CustomApiendpoint { get; set; }

    public string? CustomApimethod { get; set; }

    public string? CustomApiheaders { get; set; }

    public string? CustomApirequestTemplate { get; set; }

    public string? CustomApikeyEncrypted { get; set; }

    public bool JitprovisioningEnabled { get; set; }

    public Guid? AutoAssignRoleId { get; set; }

    public bool SyncAttributesOnLogin { get; set; }

    public string? AttributeMapping { get; set; }

    public string? GroupToRoleMapping { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime? LastUsedAt { get; set; }

    public DateTime? LastHealthCheckAt { get; set; }

    public bool? LastHealthCheckSuccess { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual ComplaintRole? AutoAssignRole { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual ICollection<ExternalUserMapping> ExternalUserMappings { get; set; } = new List<ExternalUserMapping>();
}
