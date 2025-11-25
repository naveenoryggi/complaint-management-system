namespace ComplaintManagement.Domain.Configuration;

public class OryggiSettings
{
    public bool Enabled { get; set; } = true;
    public bool SyncOnStartup { get; set; } = false;
    public int SyncIntervalHours { get; set; } = 6;
    public TableMappings TableMappings { get; set; } = new();
    public FieldMappings FieldMappings { get; set; } = new();
}

public class TableMappings
{
    public string CompanyTable { get; set; } = "CompanyMaster";
    public string BranchTable { get; set; } = "BranchMaster";
    public string DepartmentTable { get; set; } = "DeptMaster";
    public string SectionTable { get; set; } = "SectionMaster";
    public string EmployeeTable { get; set; } = "EmployeeMaster";
}

public class FieldMappings
{
    public CompanyFieldMapping Company { get; set; } = new();
    public BranchFieldMapping Branch { get; set; } = new();
    public DepartmentFieldMapping Department { get; set; } = new();
    public SectionFieldMapping Section { get; set; } = new();
    public EmployeeFieldMapping Employee { get; set; } = new();
}

public class CompanyFieldMapping
{
    public string KeyField { get; set; } = "Ccode";
    public string NameField { get; set; } = "CName";
    public string AddressField { get; set; } = "Address";
    public string EmailField { get; set; } = "Email";
    public string PhoneField { get; set; } = "TelephoneNo";
}

public class BranchFieldMapping
{
    public string KeyField { get; set; } = "BranchCode";
    public string NameField { get; set; } = "BranchName";
    public string LocationField { get; set; } = "Location";
    public string CompanyKeyField { get; set; } = "Ccode";
}

public class DepartmentFieldMapping
{
    public string KeyField { get; set; } = "Dcode";
    public string NameField { get; set; } = "Dname";
    public string BranchKeyField { get; set; } = "BranchCode";
}

public class SectionFieldMapping
{
    public string KeyField { get; set; } = "SecCode";
    public string NameField { get; set; } = "SecName";
    public string DepartmentKeyField { get; set; } = "Dcode";
}

public class EmployeeFieldMapping
{
    public string KeyField { get; set; } = "Ecode";
    public string EmployeeCodeField { get; set; } = "CorpEmpCode";
    public string EmailField { get; set; } = "E_mail";
    public string PhoneField { get; set; } = "Telephone1";
    public string PhoneSecondaryField { get; set; } = "Telephone2";
    public string FirstNameField { get; set; } = "FName";
    public string LastNameField { get; set; } = "LName";
    public string FullNameField { get; set; } = "EmpName";
    public string ManagerKeyField { get; set; } = "ReportingHeadEcode";
    public string SectionKeyField { get; set; } = "SecCode";
    public string DateOfJoiningField { get; set; } = "DateofJoin";
    public string DateOfBirthField { get; set; } = "DateofBirth";
    public string IsActiveField { get; set; } = "Active";
}
