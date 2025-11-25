-- Investigation: Why is CMP-2025-1110 escalation failing?
-- Complaint ID: dc5f95da-92d1-40f9-8ed3-1b91f0b70c34

USE ComplaintManagementDB;
GO

PRINT '=== 1. COMPLAINT DETAILS ===';
SELECT
    c.Id,
    c.ComplaintNumber,
    c.Title,
    c.CurrentEscalationLevel,
    cat.Id AS CategoryId,
    cat.Name AS CategoryName,
    c.BranchId,
    c.DepartmentId,
    c.SectionId,
    c.CompanyId
FROM Complaints c
JOIN ComplaintCategories cat ON c.CategoryId = cat.Id
WHERE c.Id = 'dc5f95da-92d1-40f9-8ed3-1b91f0b70c34';

PRINT '';
PRINT '=== 2. ESCALATION POLICIES (ALL) ===';
SELECT
    ep.Id,
    ep.Name,
    ep.IsActive,
    ep.AutoEscalationEnabled,
    ep.EscalationMatrixId,
    ep.Priority,
    ep.Specificity,
    ep.CompanyId,
    ep.BranchId,
    ep.DepartmentId,
    ep.SectionId,
    ep.CategoryId,
    ep.CreatedAt
FROM EscalationPolicies ep
WHERE ep.IsDeleted = 0
ORDER BY ep.Specificity DESC, ep.Priority DESC;

PRINT '';
PRINT '=== 3. ESCALATION MATRICES (ALL) ===';
SELECT
    em.Id,
    em.Name,
    em.Description,
    em.IsActive,
    em.CreatedAt,
    em.UpdatedAt,
    (SELECT COUNT(*) FROM EscalationLevels WHERE EscalationMatrixId = em.Id) AS LevelCount
FROM EscalationMatrices em
WHERE em.IsDeleted = 0
ORDER BY em.CreatedAt DESC;

PRINT '';
PRINT '=== 4. ESCALATION LEVELS (FOR EACH MATRIX) ===';
SELECT
    el.Id,
    el.EscalationMatrixId,
    em.Name AS MatrixName,
    el.Level,
    el.EscalationAfterHours,
    el.EscalationTimeUnit,
    el.ResourcePoolId,
    rp.Name AS ResourcePoolName,
    el.NotificationTemplateId
FROM EscalationLevels el
JOIN EscalationMatrices em ON el.EscalationMatrixId = em.Id
LEFT JOIN ResourcePools rp ON el.ResourcePoolId = rp.Id
ORDER BY el.EscalationMatrixId, el.Level;

PRINT '';
PRINT '=== 5. RESOURCE POOLS (ALL) ===';
SELECT
    rp.Id,
    rp.Name,
    rp.Description,
    rp.IsActive,
    (SELECT COUNT(*) FROM ResourcePoolMembers WHERE ResourcePoolId = rp.Id AND IsDeleted = 0) AS MemberCount
FROM ResourcePools rp
WHERE rp.IsDeleted = 0;

PRINT '';
PRINT '=== 6. POLICY RESOLUTION FOR COMPLAINT CMP-2025-1110 ===';
-- Check which policy would apply to this complaint
SELECT
    ep.Id,
    ep.Name,
    ep.EscalationMatrixId,
    em.Name AS MatrixName,
    ep.Specificity,
    ep.Priority,
    CASE
        WHEN ep.CategoryId IS NOT NULL THEN 'Category-Level'
        WHEN ep.SectionId IS NOT NULL THEN 'Section-Level'
        WHEN ep.DepartmentId IS NOT NULL THEN 'Department-Level'
        WHEN ep.BranchId IS NOT NULL THEN 'Branch-Level'
        WHEN ep.CompanyId IS NOT NULL THEN 'Company-Level'
        ELSE 'Unknown'
    END AS PolicyScope
FROM EscalationPolicies ep
LEFT JOIN EscalationMatrices em ON ep.EscalationMatrixId = em.Id
WHERE ep.IsDeleted = 0
  AND ep.IsActive = 1
  AND (
      -- Category match
      (ep.CategoryId = 'a4e6d993-ea9b-442f-a803-e61356c56760')
      OR
      -- Company match (assuming all complaints belong to same company)
      (ep.CategoryId IS NULL AND ep.SectionId IS NULL AND ep.DepartmentId IS NULL AND ep.BranchId IS NULL)
  )
ORDER BY ep.Specificity DESC, ep.Priority DESC;

PRINT '';
PRINT '=== 7. ESCALATION HISTORY FOR THIS COMPLAINT ===';
SELECT
    eh.Id,
    eh.ComplaintId,
    eh.FromLevel,
    eh.ToLevel,
    eh.EscalatedAt,
    eh.EscalatedById,
    eh.Reason
FROM EscalationHistory eh
WHERE eh.ComplaintId = 'dc5f95da-92d1-40f9-8ed3-1b91f0b70c34'
ORDER BY eh.EscalatedAt DESC;
