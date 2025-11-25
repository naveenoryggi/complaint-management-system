namespace ComplaintManagement.Domain.Enums;

/// <summary>
/// Defines who should receive a communication
/// </summary>
public enum RecipientType
{
    /// <summary>
    /// The person who submitted the complaint
    /// </summary>
    Complainant = 0,

    /// <summary>
    /// The user assigned to handle the complaint
    /// </summary>
    AssignedHandler = 1,

    /// <summary>
    /// The person who created/updated the entity
    /// </summary>
    Creator = 2,

    /// <summary>
    /// Specific users (defined in SpecificUserIds)
    /// </summary>
    SpecificUsers = 3,

    /// <summary>
    /// Users with specific roles (defined in SpecificRoleIds)
    /// </summary>
    SpecificRoles = 4,

    /// <summary>
    /// Specific email addresses (defined in SpecificEmails)
    /// </summary>
    SpecificEmails = 5,

    /// <summary>
    /// Manager of the complainant
    /// </summary>
    ComplainantManager = 6,

    /// <summary>
    /// Manager of the assigned handler
    /// </summary>
    HandlerManager = 7,

    /// <summary>
    /// All users in the complaint's department
    /// </summary>
    Department = 8,

    /// <summary>
    /// All users in the complaint's section
    /// </summary>
    Section = 9,

    /// <summary>
    /// All administrators
    /// </summary>
    Administrators = 10,

    // Company Level Recipients
    /// <summary>
    /// Primary manager/head of the company
    /// </summary>
    CompanyManager = 11,

    /// <summary>
    /// Secondary/deputy manager of the company
    /// </summary>
    CompanySecondaryManager = 12,

    /// <summary>
    /// HR responsible for the company
    /// </summary>
    CompanyHR = 13,

    /// <summary>
    /// Company contact email (from Company.ContactEmail)
    /// </summary>
    CompanyContactEmail = 14,

    /// <summary>
    /// All users in the company
    /// </summary>
    AllCompanyUsers = 15,

    // Branch Level Recipients
    /// <summary>
    /// Primary manager/head of the branch
    /// </summary>
    BranchManager = 16,

    /// <summary>
    /// Secondary/deputy manager of the branch
    /// </summary>
    BranchSecondaryManager = 17,

    /// <summary>
    /// HR responsible for the branch
    /// </summary>
    BranchHR = 18,

    /// <summary>
    /// Branch contact email (from Branch.ContactEmail)
    /// </summary>
    BranchContactEmail = 19,

    /// <summary>
    /// All users in the branch
    /// </summary>
    AllBranchUsers = 20,

    // Department Level Recipients
    /// <summary>
    /// Primary manager/head of the department
    /// </summary>
    DepartmentManager = 21,

    /// <summary>
    /// Secondary/deputy manager of the department
    /// </summary>
    DepartmentSecondaryManager = 22,

    /// <summary>
    /// HR responsible for the department
    /// </summary>
    DepartmentHR = 23,

    /// <summary>
    /// All users in the department
    /// </summary>
    AllDepartmentUsers = 24,

    // Section Level Recipients
    /// <summary>
    /// Primary head of the section
    /// </summary>
    SectionHead = 25,

    /// <summary>
    /// Secondary/deputy head of the section
    /// </summary>
    SectionSecondaryHead = 26,

    /// <summary>
    /// HR responsible for the section
    /// </summary>
    SectionHR = 27,

    /// <summary>
    /// All users in the section
    /// </summary>
    AllSectionUsers = 28
}
