namespace ComplaintManagement.Application.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    // Specific repositories
    IComplaintRepository Complaints { get; }
    IComplaintCategoryRepository ComplaintCategories { get; }
    IComplaintCommentRepository ComplaintComments { get; }
    IComplaintAttachmentRepository ComplaintAttachments { get; }
    IUserRepository Users { get; }
    IComplaintRoleRepository ComplaintRoles { get; }
    IUserComplaintRoleRepository UserComplaintRoles { get; }
    ITenantRepository Tenants { get; }
    ICompanyRepository Companies { get; }
    IBranchRepository Branches { get; }
    IDepartmentRepository Departments { get; }
    ISectionRepository Sections { get; }
    IComplaintInfoSettingsRepository ComplaintInfoSettings { get; }
    IComplaintStatusMasterRepository ComplaintStatusMasters { get; }
    IComplaintPriorityMasterRepository ComplaintPriorityMasters { get; }
    IRefreshTokenRepository RefreshTokens { get; }

    // Generic repository for entities without specific repositories
    IRepository<T> Repository<T>() where T : ComplaintManagement.Domain.Entities.BaseEntity;

    // Transaction methods
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    // Direct entity management
    Task AddEntityAsync<TEntity>(TEntity entity) where TEntity : class;
    void RemoveEntity<TEntity>(TEntity entity) where TEntity : class;
    void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
}
