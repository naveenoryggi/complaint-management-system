namespace ComplaintManagement.Domain.Enums;

/// <summary>
/// Defines the available licensed modules in the system.
/// Each module can be independently activated based on license.
/// </summary>
public enum LicenseModule
{
    /// <summary>
    /// Core complaint management - always enabled
    /// </summary>
    Core = 0,

    /// <summary>
    /// Customer/Partner relationship management
    /// </summary>
    CRM_Customer = 1,

    /// <summary>
    /// Product catalog management
    /// </summary>
    CRM_Product = 2,

    /// <summary>
    /// Sales pipeline, quotes, orders (Future)
    /// </summary>
    CRM_Sales = 3,

    /// <summary>
    /// Project management
    /// </summary>
    CRM_Project = 14,

    /// <summary>
    /// Service contracts and warranties
    /// </summary>
    ServiceContract = 4,

    /// <summary>
    /// Asset tracking and management
    /// </summary>
    AssetManagement = 5,

    /// <summary>
    /// External customer/partner portal access
    /// </summary>
    CustomerPortal = 6,

    /// <summary>
    /// Advanced BI reporting and dashboards (Future)
    /// </summary>
    AdvancedReporting = 7,

    /// <summary>
    /// Third-party API access
    /// </summary>
    APIAccess = 8,

    /// <summary>
    /// Custom branding and white-labeling
    /// </summary>
    WhiteLabeling = 9,

    /// <summary>
    /// Email ticketing system
    /// </summary>
    EmailTicketing = 10,

    /// <summary>
    /// Multi-language support
    /// </summary>
    MultiLanguage = 11,

    /// <summary>
    /// Advanced SLA management
    /// </summary>
    AdvancedSLA = 12,

    /// <summary>
    /// Workflow automation
    /// </summary>
    WorkflowAutomation = 13
}
