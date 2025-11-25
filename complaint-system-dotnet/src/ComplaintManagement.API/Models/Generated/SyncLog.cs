using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class SyncLog
{
    public Guid Id { get; set; }

    public Guid SyncLogId { get; set; }

    public Guid TenantId { get; set; }

    public string SyncType { get; set; } = null!;

    public DateTime SyncStartedAt { get; set; }

    public DateTime? SyncCompletedAt { get; set; }

    public string Status { get; set; } = null!;

    public int CompaniesProcessed { get; set; }

    public int BranchesProcessed { get; set; }

    public int DepartmentsProcessed { get; set; }

    public int SectionsProcessed { get; set; }

    public int EmployeesProcessed { get; set; }

    public int CompaniesCreated { get; set; }

    public int BranchesCreated { get; set; }

    public int DepartmentsCreated { get; set; }

    public int SectionsCreated { get; set; }

    public int EmployeesCreated { get; set; }

    public int CompaniesUpdated { get; set; }

    public int BranchesUpdated { get; set; }

    public int DepartmentsUpdated { get; set; }

    public int SectionsUpdated { get; set; }

    public int EmployeesUpdated { get; set; }

    public string? ErrorMessage { get; set; }

    public string? ErrorDetails { get; set; }

    public TimeOnly? Duration { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public int UsersCreated { get; set; }

    public int UsersProcessed { get; set; }

    public int UsersUpdated { get; set; }

    public int EmployeesFailed { get; set; }

    public int UsersFailed { get; set; }
}
