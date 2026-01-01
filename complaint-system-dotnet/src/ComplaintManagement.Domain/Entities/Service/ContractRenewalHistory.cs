using ComplaintManagement.Domain.Enums.Service;

namespace ComplaintManagement.Domain.Entities.Service;

/// <summary>
/// History of contract renewals
/// </summary>
public class ContractRenewalHistory : BaseEntity
{
    /// <summary>
    /// Parent contract
    /// </summary>
    public Guid ContractId { get; set; }

    /// <summary>
    /// Renewal sequence number
    /// </summary>
    public int RenewalNumber { get; set; }

    #region Period

    /// <summary>
    /// Previous contract end date
    /// </summary>
    public DateTime PreviousEndDate { get; set; }

    /// <summary>
    /// New contract start date
    /// </summary>
    public DateTime NewStartDate { get; set; }

    /// <summary>
    /// New contract end date
    /// </summary>
    public DateTime NewEndDate { get; set; }

    /// <summary>
    /// Renewal period in months
    /// </summary>
    public int RenewalPeriodMonths { get; set; }

    #endregion

    #region Action

    /// <summary>
    /// Type of renewal action
    /// </summary>
    public RenewalAction Action { get; set; }

    /// <summary>
    /// Date when renewal was processed
    /// </summary>
    public DateTime RenewalDate { get; set; }

    /// <summary>
    /// User who processed the renewal
    /// </summary>
    public Guid? ProcessedBy { get; set; }

    /// <summary>
    /// Customer contact who approved/requested
    /// </summary>
    public Guid? CustomerContactId { get; set; }

    #endregion

    #region Financials

    /// <summary>
    /// Previous contract value
    /// </summary>
    public decimal? PreviousValue { get; set; }

    /// <summary>
    /// New contract value
    /// </summary>
    public decimal? NewValue { get; set; }

    /// <summary>
    /// Price change percentage
    /// </summary>
    public decimal? PriceChangePercent { get; set; }

    /// <summary>
    /// Currency
    /// </summary>
    public string Currency { get; set; } = "INR";

    /// <summary>
    /// Discount applied for renewal
    /// </summary>
    public decimal? RenewalDiscount { get; set; }

    /// <summary>
    /// Invoice number for renewal
    /// </summary>
    public string? InvoiceNumber { get; set; }

    /// <summary>
    /// PO number for renewal
    /// </summary>
    public string? PONumber { get; set; }

    #endregion

    #region Changes

    /// <summary>
    /// Previous contract type
    /// </summary>
    public ContractType? PreviousType { get; set; }

    /// <summary>
    /// New contract type (if changed)
    /// </summary>
    public ContractType? NewType { get; set; }

    /// <summary>
    /// Previous coverage type
    /// </summary>
    public CoverageType? PreviousCoverageType { get; set; }

    /// <summary>
    /// New coverage type (if changed)
    /// </summary>
    public CoverageType? NewCoverageType { get; set; }

    /// <summary>
    /// JSON describing what changed
    /// </summary>
    public string? ChangesSummary { get; set; }

    #endregion

    #region Communication

    /// <summary>
    /// Date renewal notice was sent
    /// </summary>
    public DateTime? NoticeSentDate { get; set; }

    /// <summary>
    /// Date customer responded
    /// </summary>
    public DateTime? CustomerResponseDate { get; set; }

    /// <summary>
    /// Customer's response/notes
    /// </summary>
    public string? CustomerResponse { get; set; }

    #endregion

    #region Notes

    /// <summary>
    /// Reason for the action
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Internal notes
    /// </summary>
    public string? Notes { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Parent contract
    /// </summary>
    public virtual Contract Contract { get; set; } = null!;

    #endregion
}
