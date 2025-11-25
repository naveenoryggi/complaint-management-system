# Check if Escalated status exists for the complaint's company
$query = @"
SELECT
    csm.Id AS StatusId,
    csm.Name AS StatusName,
    csm.IsActive,
    csm.IsDeleted
FROM ComplaintStatusMasters csm
WHERE csm.CompanyId = (
    SELECT CompanyId FROM Complaints WHERE Id = 'dc5f95da-92d1-40f9-8ed3-1b91f0b70c34'
)
AND LOWER(csm.Name) = 'escalated'
AND csm.IsDeleted = 0
"@

Write-Host "Checking if Escalated status exists for complaint's company..."
$result = Invoke-Sqlcmd -ServerInstance "localhost\SQLEXPRESS" -Database "ComplaintManagementDB" -Query $query

if ($result) {
    Write-Host "✓ Escalated status EXISTS:" -ForegroundColor Green
    $result | Format-Table -AutoSize
} else {
    Write-Host "✗ Escalated status NOT FOUND for this company!" -ForegroundColor Red

    # Get company info
    $companyQuery = "SELECT c.CompanyId, co.Name AS CompanyName FROM Complaints c JOIN Companies co ON c.CompanyId = co.Id WHERE c.Id = 'dc5f95da-92d1-40f9-8ed3-1b91f0b70c34'"
    $companyInfo = Invoke-Sqlcmd -ServerInstance "localhost\SQLEXPRESS" -Database "ComplaintManagementDB" -Query $companyQuery

    Write-Host "Complaint belongs to company:"
    $companyInfo | Format-Table -AutoSize

    Write-Host "Adding Escalated status for this company..."

    $insertQuery = @"
INSERT INTO ComplaintStatusMasters (
    Id, CompanyId, Name, Description, Code, ColorCode, IconClass,
    DisplayOrder, IsActive, IsSystem, IsFinal, CreatedAt, IsDeleted
)
VALUES (
    NEWID(),
    '$($companyInfo.CompanyId)',
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
)
"@

    Invoke-Sqlcmd -ServerInstance "localhost\SQLEXPRESS" -Database "ComplaintManagementDB" -Query $insertQuery
    Write-Host "✓ Escalated status added successfully!" -ForegroundColor Green
}
