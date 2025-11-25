SELECT COUNT(*) AS FilteredEmployees
FROM EmployeeMaster
WHERE (Active = 1 OR Active IS NULL)
  AND Ecode != 1
  AND (CorpEmpCode IS NULL OR CHARINDEX('_', CorpEmpCode) = 0);
