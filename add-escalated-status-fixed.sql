SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

USE ComplaintManagementDB;
GO

PRINT 'Adding Escalated status for Oryggi Technologies...';

INSERT INTO ComplaintStatusMasters (
    Id,
    CompanyId,
    Name,
    Description,
    Code,
    ColorCode,
    IconClass,
    DisplayOrder,
    IsActive,
    IsSystem,
    IsFinal,
    CreatedAt,
    IsDeleted
)
VALUES (
    NEWID(),
    'FE28CD85-4226-4DAA-9E45-66A3D51877FA',
    'Escalated',
    'Complaint has been escalated to higher management level',
    'ESCALATED',
    '#FFA500',
    'fas fa-level-up-alt',
    6,
    1,
    1,
    0,
    GETUTCDATE(),
    0
);

PRINT 'Escalated status added successfully!';

SELECT
    c.Name AS CompanyName,
    csm.Name AS StatusName,
    csm.Code,
    csm.ColorCode,
    csm.DisplayOrder,
    csm.IsActive
FROM ComplaintStatusMasters csm
JOIN Companies c ON csm.CompanyId = c.Id
WHERE csm.Name = 'Escalated'
AND csm.CompanyId = 'FE28CD85-4226-4DAA-9E45-66A3D51877FA';
GO
