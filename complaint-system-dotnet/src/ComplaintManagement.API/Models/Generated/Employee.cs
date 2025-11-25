using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class Employee
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid? SectionId { get; set; }

    public string EmployeeCode { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? AlternatePhone { get; set; }

    public DateTime? DateOfJoining { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public Guid? ManagerId { get; set; }

    public bool IsActive { get; set; }

    public string? OryggiEmployeeId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual ICollection<Employee> InverseManager { get; set; } = new List<Employee>();

    public virtual Employee? Manager { get; set; }

    public virtual Section? Section { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;
}
