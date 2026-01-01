namespace ComplaintManagement.Domain.Enums.Product;

/// <summary>
/// Type of product
/// </summary>
public enum ProductType
{
    /// <summary>Physical goods that require inventory tracking</summary>
    Physical = 1,

    /// <summary>Digital goods (software, licenses, downloads)</summary>
    Digital = 2,

    /// <summary>Professional services (installation, consulting, training)</summary>
    Service = 3,

    /// <summary>Recurring subscription products</summary>
    Subscription = 4,

    /// <summary>Bundle of multiple products</summary>
    Bundle = 5,

    /// <summary>Spare parts and consumables</summary>
    SparePart = 6,

    /// <summary>Maintenance and support services</summary>
    Maintenance = 7
}

/// <summary>
/// Product lifecycle status
/// </summary>
public enum ProductStatus
{
    /// <summary>Product is being prepared, not yet available</summary>
    Draft = 0,

    /// <summary>Product is active and available for sale</summary>
    Active = 1,

    /// <summary>Product is temporarily unavailable</summary>
    Inactive = 2,

    /// <summary>Product has been discontinued but still supported</summary>
    Discontinued = 3,

    /// <summary>Product has reached end of life, no longer supported</summary>
    EndOfLife = 4,

    /// <summary>Product is archived for historical reference</summary>
    Archived = 5
}

/// <summary>
/// Billing frequency for subscription products
/// </summary>
public enum BillingFrequency
{
    /// <summary>One-time purchase</summary>
    OneTime = 0,

    /// <summary>Billed monthly</summary>
    Monthly = 1,

    /// <summary>Billed every 3 months</summary>
    Quarterly = 2,

    /// <summary>Billed every 6 months</summary>
    SemiAnnual = 3,

    /// <summary>Billed yearly</summary>
    Annual = 4,

    /// <summary>Billed every 2 years</summary>
    Biennial = 5,

    /// <summary>Billed every 3 years</summary>
    Triennial = 6
}

/// <summary>
/// Unit of measure for products
/// </summary>
public enum UnitOfMeasure
{
    /// <summary>Each/piece</summary>
    Each = 1,

    /// <summary>Box/carton</summary>
    Box = 2,

    /// <summary>Pack/package</summary>
    Pack = 3,

    /// <summary>Kilogram</summary>
    Kg = 4,

    /// <summary>Gram</summary>
    Gram = 5,

    /// <summary>Liter</summary>
    Liter = 6,

    /// <summary>Meter</summary>
    Meter = 7,

    /// <summary>Hour (for services)</summary>
    Hour = 8,

    /// <summary>Day (for services)</summary>
    Day = 9,

    /// <summary>License/seat</summary>
    License = 10,

    /// <summary>User/seat</summary>
    User = 11,

    /// <summary>Set/kit</summary>
    Set = 12
}

/// <summary>
/// Tax applicability type
/// </summary>
public enum TaxType
{
    /// <summary>No tax applicable</summary>
    Exempt = 0,

    /// <summary>Standard GST rate</summary>
    GST = 1,

    /// <summary>IGST for interstate transactions</summary>
    IGST = 2,

    /// <summary>Value Added Tax</summary>
    VAT = 3,

    /// <summary>Custom tax rate</summary>
    Custom = 4
}

/// <summary>
/// Price list type
/// </summary>
public enum PriceListType
{
    /// <summary>Standard/base pricing</summary>
    Standard = 1,

    /// <summary>Partner-specific pricing</summary>
    Partner = 2,

    /// <summary>Customer-specific pricing</summary>
    Customer = 3,

    /// <summary>Volume/tier pricing</summary>
    Volume = 4,

    /// <summary>Promotional pricing</summary>
    Promotional = 5,

    /// <summary>Contract pricing</summary>
    Contract = 6
}
