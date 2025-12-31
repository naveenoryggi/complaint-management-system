using ComplaintManagement.Domain.Enums.CRM;

namespace ComplaintManagement.Application.DTOs.CRM;

/// <summary>
/// Customer location DTO for API responses
/// </summary>
public class CustomerLocationDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public LocationType Type { get; set; }
    public string TypeName => Type.ToString();
    public string? Description { get; set; }

    // Address
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string Country { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string FullAddress => GetFullAddress();

    // Geolocation
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? TimeZone { get; set; }

    // Contact
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Fax { get; set; }

    // Operational Details
    public string? OperatingHours { get; set; }
    public string? AccessInstructions { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }

    // Status Flags
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; }
    public bool IsBillingAddress { get; set; }
    public bool IsShippingAddress { get; set; }
    public bool IsServiceEnabled { get; set; }

    // External References
    public string? ExternalLocationId { get; set; }

    // Metadata
    public string? Notes { get; set; }
    public string? Tags { get; set; }

    // Statistics
    public int ContactCount { get; set; }
    public int AssetCount { get; set; }
    public int ActiveTicketCount { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    private string GetFullAddress()
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(AddressLine1)) parts.Add(AddressLine1);
        if (!string.IsNullOrEmpty(AddressLine2)) parts.Add(AddressLine2);
        if (!string.IsNullOrEmpty(City)) parts.Add(City);
        if (!string.IsNullOrEmpty(State)) parts.Add(State);
        if (!string.IsNullOrEmpty(PostalCode)) parts.Add(PostalCode);
        if (!string.IsNullOrEmpty(Country)) parts.Add(Country);
        return string.Join(", ", parts);
    }
}

/// <summary>
/// Customer location summary for list views
/// </summary>
public class CustomerLocationSummaryDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public LocationType Type { get; set; }
    public string TypeName => Type.ToString();
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string Country { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; }
    public int ContactCount { get; set; }
    public int AssetCount { get; set; }
}

/// <summary>
/// Request to create a new customer location
/// </summary>
public class CreateCustomerLocationRequest
{
    public Guid CustomerId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public LocationType Type { get; set; } = LocationType.Branch;
    public string? Description { get; set; }

    // Address
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string Country { get; set; } = string.Empty;
    public string? PostalCode { get; set; }

    // Geolocation
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? TimeZone { get; set; }

    // Contact
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Fax { get; set; }

    // Operational Details
    public string? OperatingHours { get; set; }
    public string? AccessInstructions { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }

    // Status Flags
    public bool IsPrimary { get; set; }
    public bool IsBillingAddress { get; set; }
    public bool IsShippingAddress { get; set; }
    public bool IsServiceEnabled { get; set; } = true;

    // External References
    public string? ExternalLocationId { get; set; }

    // Notes
    public string? Notes { get; set; }
    public string? Tags { get; set; }
}

/// <summary>
/// Request to update an existing customer location
/// </summary>
public class UpdateCustomerLocationRequest
{
    public string Name { get; set; } = string.Empty;
    public LocationType Type { get; set; }
    public string? Description { get; set; }

    // Address
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string Country { get; set; } = string.Empty;
    public string? PostalCode { get; set; }

    // Geolocation
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? TimeZone { get; set; }

    // Contact
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Fax { get; set; }

    // Operational Details
    public string? OperatingHours { get; set; }
    public string? AccessInstructions { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }

    // Status Flags
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; }
    public bool IsBillingAddress { get; set; }
    public bool IsShippingAddress { get; set; }
    public bool IsServiceEnabled { get; set; }

    // External References
    public string? ExternalLocationId { get; set; }

    // Notes
    public string? Notes { get; set; }
    public string? Tags { get; set; }
}

/// <summary>
/// Customer location dropdown/lookup item
/// </summary>
public class CustomerLocationLookupDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public LocationType Type { get; set; }
    public bool IsPrimary { get; set; }
}
