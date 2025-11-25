using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace ComplaintManagement.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ComplaintDbContext _context;
    private IDbContextTransaction? _transaction;

    // Lazy-loaded repositories
    private IComplaintRepository? _complaints;
    private IComplaintCategoryRepository? _complaintCategories;
    private IComplaintCommentRepository? _complaintComments;
    private IComplaintAttachmentRepository? _complaintAttachments;
    private IUserRepository? _users;
    private IComplaintRoleRepository? _complaintRoles;
    private IUserComplaintRoleRepository? _userComplaintRoles;
    private ITenantRepository? _tenants;
    private ICompanyRepository? _companies;
    private IBranchRepository? _branches;
    private IDepartmentRepository? _departments;
    private ISectionRepository? _sections;
    private IComplaintInfoSettingsRepository? _complaintInfoSettings;
    private IComplaintStatusMasterRepository? _complaintStatusMasters;
    private IComplaintPriorityMasterRepository? _complaintPriorityMasters;
    private IRefreshTokenRepository? _refreshTokens;

    // Dictionary to cache generic repositories
    private readonly Dictionary<Type, object> _genericRepositories = new();

    public UnitOfWork(ComplaintDbContext context)
    {
        _context = context;
    }

    public IComplaintRepository Complaints => _complaints ??= new ComplaintRepository(_context);
    public IComplaintCategoryRepository ComplaintCategories => _complaintCategories ??= new ComplaintCategoryRepository(_context);
    public IComplaintCommentRepository ComplaintComments => _complaintComments ??= new ComplaintCommentRepository(_context);
    public IComplaintAttachmentRepository ComplaintAttachments => _complaintAttachments ??= new ComplaintAttachmentRepository(_context);
    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IComplaintRoleRepository ComplaintRoles => _complaintRoles ??= new ComplaintRoleRepository(_context);
    public IUserComplaintRoleRepository UserComplaintRoles => _userComplaintRoles ??= new UserComplaintRoleRepository(_context);
    public ITenantRepository Tenants => _tenants ??= new TenantRepository(_context);
    public ICompanyRepository Companies => _companies ??= new CompanyRepository(_context);
    public IBranchRepository Branches => _branches ??= new BranchRepository(_context);
    public IDepartmentRepository Departments => _departments ??= new DepartmentRepository(_context);
    public ISectionRepository Sections => _sections ??= new SectionRepository(_context);
    public IComplaintInfoSettingsRepository ComplaintInfoSettings => _complaintInfoSettings ??= new ComplaintInfoSettingsRepository(_context);
    public IComplaintStatusMasterRepository ComplaintStatusMasters => _complaintStatusMasters ??= new ComplaintStatusMasterRepository(_context);
    public IComplaintPriorityMasterRepository ComplaintPriorityMasters => _complaintPriorityMasters ??= new ComplaintPriorityMasterRepository(_context);
    public IRefreshTokenRepository RefreshTokens => _refreshTokens ??= new RefreshTokenRepository(_context);

    // Generic repository method for entities without specific repositories
    public IRepository<T> Repository<T>() where T : ComplaintManagement.Domain.Entities.BaseEntity
    {
        var type = typeof(T);

        if (!_genericRepositories.ContainsKey(type))
        {
            var repositoryType = typeof(Repository<>).MakeGenericType(type);
            var repositoryInstance = Activator.CreateInstance(repositoryType, _context);
            _genericRepositories[type] = repositoryInstance!;
        }

        return (IRepository<T>)_genericRepositories[type];
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task AddEntityAsync<TEntity>(TEntity entity) where TEntity : class
    {
        await _context.Set<TEntity>().AddAsync(entity);
    }

    public void RemoveEntity<TEntity>(TEntity entity) where TEntity : class
    {
        _context.Set<TEntity>().Remove(entity);
    }

    public void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
    {
        _context.Set<TEntity>().RemoveRange(entities);
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
