using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.Escalation;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.Infrastructure.Repositories;

public class EscalationMatrixRepository : Repository<EscalationMatrix>, IEscalationMatrixRepository
{
    public EscalationMatrixRepository(ComplaintDbContext context) : base(context)
    {
    }

    public async Task<List<EscalationMatrix>> GetByCompanyIdAsync(Guid companyId, bool includeInactive = false)
    {
        var query = _dbSet.Where(em => em.CompanyId == companyId);

        if (!includeInactive)
        {
            query = query.Where(em => em.IsActive);
        }

        return await query
            .Include(em => em.Category)
            .Include(em => em.Branch)
            .Include(em => em.Department)
            .Include(em => em.EscalationLevels.Where(el => el.IsActive))
            .OrderByDescending(em => em.Priority)
            .ToListAsync();
    }

    public async Task<List<EscalationMatrix>> GetActiveMatricesByCompanyIdAsync(Guid companyId)
    {
        return await _dbSet
            .Where(em => em.CompanyId == companyId && em.IsActive)
            .Include(em => em.Category)
            .Include(em => em.Branch)
            .Include(em => em.Department)
            .Include(em => em.EscalationLevels.Where(el => el.IsActive))
            .OrderByDescending(em => em.Priority)
            .ToListAsync();
    }

    public async Task<EscalationMatrix?> GetApplicableMatrixAsync(
        Guid companyId,
        Guid? categoryId = null,
        Guid? branchId = null,
        Guid? departmentId = null)
    {
        // Find the most specific matching matrix
        // Priority order: Department > Branch > Category > Company-wide
        // Within each level, use the Priority field

        var query = _dbSet
            .Where(em => em.CompanyId == companyId && em.IsActive)
            .Include(em => em.EscalationLevels.Where(el => el.IsActive));

        // Try to find most specific match first
        EscalationMatrix? match = null;

        // 1. Try Department + Category specific
        if (departmentId.HasValue && categoryId.HasValue)
        {
            match = await query
                .Where(em => em.DepartmentId == departmentId && em.CategoryId == categoryId)
                .OrderByDescending(em => em.Priority)
                .FirstOrDefaultAsync();
            if (match != null) return match;
        }

        // 2. Try Branch + Category specific
        if (branchId.HasValue && categoryId.HasValue)
        {
            match = await query
                .Where(em => em.BranchId == branchId && em.CategoryId == categoryId)
                .OrderByDescending(em => em.Priority)
                .FirstOrDefaultAsync();
            if (match != null) return match;
        }

        // 3. Try Department specific
        if (departmentId.HasValue)
        {
            match = await query
                .Where(em => em.DepartmentId == departmentId && em.CategoryId == null)
                .OrderByDescending(em => em.Priority)
                .FirstOrDefaultAsync();
            if (match != null) return match;
        }

        // 4. Try Branch specific
        if (branchId.HasValue)
        {
            match = await query
                .Where(em => em.BranchId == branchId && em.CategoryId == null)
                .OrderByDescending(em => em.Priority)
                .FirstOrDefaultAsync();
            if (match != null) return match;
        }

        // 5. Try Category specific
        if (categoryId.HasValue)
        {
            match = await query
                .Where(em => em.CategoryId == categoryId &&
                            em.BranchId == null &&
                            em.DepartmentId == null)
                .OrderByDescending(em => em.Priority)
                .FirstOrDefaultAsync();
            if (match != null) return match;
        }

        // 6. Fall back to company-wide default
        match = await query
            .Where(em => em.CategoryId == null &&
                        em.BranchId == null &&
                        em.DepartmentId == null)
            .OrderByDescending(em => em.Priority)
            .FirstOrDefaultAsync();

        return match;
    }

    public async Task<EscalationMatrix?> GetWithLevelsAsync(Guid id)
    {
        return await _dbSet
            .Include(em => em.Category)
            .Include(em => em.Branch)
            .Include(em => em.Department)
            .Include(em => em.EscalationLevels.OrderBy(el => el.Level))
                .ThenInclude(el => el.AssignToUser)
            .FirstOrDefaultAsync(em => em.Id == id);
    }
}
