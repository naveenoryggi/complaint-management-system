namespace ComplaintManagement.Application.DTOs.SLA;

/// <summary>
/// SLA coverage matrix DTO
/// Shows SLA configuration coverage across categories and priorities
/// </summary>
public class SLACoverageMatrixDto
{
    public List<SLACoverageRowDto> Rows { get; set; } = new();

    // Summary
    public int TotalCategories { get; set; }
    public int CategoriesWithSLA { get; set; }
    public int CategoriesWithoutSLA { get; set; }
    public double CoveragePercentage { get; set; }

    public int TotalPriorities { get; set; }
    public int PrioritiesWithSLA { get; set; }
    public int PrioritiesWithoutSLA { get; set; }

    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// SLA coverage row (category or priority)
/// </summary>
public class SLACoverageRowDto
{
    public Guid EntityId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty; // Category or Priority

    public bool HasSLA { get; set; }
    public string? SLALevelName { get; set; }
    public int? ResponseTimeMinutes { get; set; }
    public int? ResolutionTimeMinutes { get; set; }
    public string? Source { get; set; } // CategoryMapping, PriorityMapping, SystemDefault

    public string CoverageStatus { get; set; } = string.Empty; // Configured, Using Default, Not Configured
    public string StatusColor { get; set; } = "#6c757d";
}
