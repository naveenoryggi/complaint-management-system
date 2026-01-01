namespace ComplaintManagement.Domain.Entities.CRM;

/// <summary>
/// Junction table for Many-to-Many relationship between Partners and Customers
/// Allows a customer to have multiple partners and vice versa
/// </summary>
public class PartnerCustomer : BaseEntity
{
    #region Relationships

    /// <summary>
    /// Partner ID (foreign key)
    /// </summary>
    public Guid PartnerId { get; set; }

    /// <summary>
    /// Customer ID (foreign key)
    /// </summary>
    public Guid CustomerId { get; set; }

    #endregion

    #region Relationship Details

    /// <summary>
    /// Whether this is the primary/main partner for the customer
    /// </summary>
    public bool IsPrimaryPartner { get; set; }

    /// <summary>
    /// Date when the partnership for this customer started
    /// </summary>
    public DateTime? RelationshipStart { get; set; }

    /// <summary>
    /// Date when the partnership ended (null if still active)
    /// </summary>
    public DateTime? RelationshipEnd { get; set; }

    /// <summary>
    /// Commission percentage for this specific customer
    /// (overrides partner default if set)
    /// </summary>
    public decimal? CommissionPercent { get; set; }

    /// <summary>
    /// Discount percentage for this specific customer
    /// (overrides partner default if set)
    /// </summary>
    public decimal? DiscountPercent { get; set; }

    #endregion

    #region Permissions

    /// <summary>
    /// Whether partner can create tickets for this customer
    /// </summary>
    public bool CanCreateTickets { get; set; } = true;

    /// <summary>
    /// Whether partner can view all tickets for this customer
    /// </summary>
    public bool CanViewAllTickets { get; set; } = true;

    /// <summary>
    /// Whether partner can manage contacts for this customer
    /// </summary>
    public bool CanManageContacts { get; set; }

    /// <summary>
    /// Whether partner can view contracts for this customer
    /// </summary>
    public bool CanViewContracts { get; set; } = true;

    /// <summary>
    /// Whether partner can view assets for this customer
    /// </summary>
    public bool CanViewAssets { get; set; } = true;

    #endregion

    #region Additional Information

    /// <summary>
    /// Relationship status (active/inactive)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Reason for relationship end (if applicable)
    /// </summary>
    public string? EndReason { get; set; }

    /// <summary>
    /// Additional notes about the relationship
    /// </summary>
    public string? Notes { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Partner in this relationship
    /// </summary>
    public Partner Partner { get; set; } = null!;

    /// <summary>
    /// Customer in this relationship
    /// </summary>
    public Customer Customer { get; set; } = null!;

    #endregion
}
