using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Service;

/// <summary>
/// Represents a physical stock location (warehouse, store, bin, etc.)
/// </summary>
public class StockLocation : BaseEntity
{
    /// <summary>Company that owns this location</summary>
    public Guid CompanyId { get; set; }

    /// <summary>Parent location ID (for hierarchy: Warehouse → Zone → Bin)</summary>
    public Guid? ParentLocationId { get; set; }

    /// <summary>Unique location code (e.g., WH-001, BIN-A1-01)</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Location name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Location description</summary>
    public string? Description { get; set; }

    /// <summary>Type of location</summary>
    public StockLocationType Type { get; set; } = StockLocationType.Warehouse;

    /// <summary>Full address of the location</summary>
    public string? Address { get; set; }

    /// <summary>City</summary>
    public string? City { get; set; }

    /// <summary>State/Province</summary>
    public string? State { get; set; }

    /// <summary>Country</summary>
    public string? Country { get; set; }

    /// <summary>Postal/ZIP code</summary>
    public string? PostalCode { get; set; }

    /// <summary>GPS latitude</summary>
    public double? Latitude { get; set; }

    /// <summary>GPS longitude</summary>
    public double? Longitude { get; set; }

    /// <summary>Contact person name</summary>
    public string? ContactPerson { get; set; }

    /// <summary>Contact phone</summary>
    public string? ContactPhone { get; set; }

    /// <summary>Contact email</summary>
    public string? ContactEmail { get; set; }

    /// <summary>Maximum storage capacity</summary>
    public int? MaxCapacity { get; set; }

    /// <summary>Unit for capacity (items, kg, cubic meters, etc.)</summary>
    public string? CapacityUnit { get; set; }

    /// <summary>Whether location is active</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Whether this is the default location for new stock</summary>
    public bool IsDefault { get; set; }

    /// <summary>Sort order for display</summary>
    public int SortOrder { get; set; }

    /// <summary>Additional notes</summary>
    public string? Notes { get; set; }

    #region Navigation Properties

    /// <summary>Parent company</summary>
    public virtual Company Company { get; set; } = null!;

    /// <summary>Parent location</summary>
    public virtual StockLocation? ParentLocation { get; set; }

    /// <summary>Child locations</summary>
    public virtual ICollection<StockLocation> ChildLocations { get; set; } = new List<StockLocation>();

    /// <summary>Stock items at this location</summary>
    public virtual ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();

    /// <summary>Stock movements from this location</summary>
    public virtual ICollection<StockMovement> MovementsFrom { get; set; } = new List<StockMovement>();

    /// <summary>Stock movements to this location</summary>
    public virtual ICollection<StockMovement> MovementsTo { get; set; } = new List<StockMovement>();

    #endregion
}

/// <summary>
/// Type of stock location
/// </summary>
public enum StockLocationType
{
    Warehouse = 0,
    Store = 1,
    Zone = 2,
    Aisle = 3,
    Rack = 4,
    Shelf = 5,
    Bin = 6,
    ServiceCenter = 7,
    CustomerSite = 8,
    InTransit = 9,
    Quarantine = 10,
    Returns = 11
}
