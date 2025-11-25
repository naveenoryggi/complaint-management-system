-- Add "Escalated" status to ComplaintStatusMasters for all companies
USE ComplaintManagementDB;
GO

-- Check if Escalated status already exists
IF NOT EXISTS (SELECT 1 FROM ComplaintStatusMasters WHERE Name = 'Escalated')
BEGIN
    PRINT 'Adding Escalated status for all companies...';

    -- Insert Escalated status for each company
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
    SELECT
        NEWID() AS Id,
        c.Id AS CompanyId,
        'Escalated' AS Name,
        'Complaint has been escalated to higher management level' AS Description,
        'ESCALATED' AS Code,
        '#FFA500' AS ColorCode,  -- Orange color
        'fas fa-level-up-alt' AS IconClass,
        6 AS DisplayOrder,  -- After In Progress (assuming it's display order 5)
        1 AS IsActive,
        1 AS IsSystem,  -- System status - cannot be deleted
        0 AS IsFinal,   -- Not a final status (complaints can continue after escalation)
        GETUTCDATE() AS CreatedAt,
        0 AS IsDeleted
    FROM Companies c
    WHERE c.IsDeleted = 0
    AND NOT EXISTS (
        SELECT 1
        FROM ComplaintStatusMasters csm
        WHERE csm.CompanyId = c.Id
        AND csm.Name = 'Escalated'
        AND csm.IsDeleted = 0
    );

    PRINT 'Escalated status added successfully!';

    -- Show what was created
    SELECT
        c.Name AS CompanyName,
        csm.Name AS StatusName,
        csm.Code,
        csm.ColorCode,
        csm.DisplayOrder,
        csm.IsSystem,
        csm.CreatedAt
    FROM ComplaintStatusMasters csm
    JOIN Companies c ON csm.CompanyId = c.Id
    WHERE csm.Name = 'Escalated'
    AND csm.IsDeleted = 0;
END
ELSE
BEGIN
    PRINT 'Escalated status already exists!';

    -- Show existing Escalated statuses
    SELECT
        c.Name AS CompanyName,
        csm.Name AS StatusName,
        csm.Code,
        csm.ColorCode,
        csm.DisplayOrder,
        csm.IsSystem,
        csm.CreatedAt
    FROM ComplaintStatusMasters csm
    JOIN Companies c ON csm.CompanyId = c.Id
    WHERE csm.Name = 'Escalated'
    AND csm.IsDeleted = 0;
END
GO
