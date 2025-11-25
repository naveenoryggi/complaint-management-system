using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComplaintManagement.Domain.Entities.Oryggi;

[Table("EmployeeMaster")]
public class EmployeeMaster
{
    [Key]
    [Column("Ecode")]
    public int Ecode { get; set; }

    [Column("CorpEmpCode")]
    public string? CorpEmpCode { get; set; }

    [Column("E_mail")]
    public string? E_mail { get; set; }

    [Column("Telephone1")]
    public string? Telephone1 { get; set; }

    [Column("Telephone2")]
    public string? Telephone2 { get; set; }

    [Column("FName")]
    public string? FName { get; set; }

    [Column("LName")]
    public string? LName { get; set; }

    [Column("EmpName")]
    public string? EmpName { get; set; }

    [Column("ReportingHeadEcode")]
    public int? ReportingHeadEcode { get; set; }

    [Column("SecCode")]
    public int? SecCode { get; set; }

    [Column("DateofJoin")]
    public DateTime? DateofJoin { get; set; }

    [Column("DateofBirth")]
    public DateTime? DateofBirth { get; set; }

    [Column("Active")]
    public bool? Active { get; set; }

    [Column("Password")]
    public string? Password { get; set; }

    [Column("DesCode")]
    public int? DesigCode { get; set; }

    [Column("Role")]
    public string? Role { get; set; }

    // Navigation properties
    [ForeignKey(nameof(DesigCode))]
    public virtual DesignationMaster? Designation { get; set; }
    [ForeignKey(nameof(SecCode))]
    public virtual SectionMaster? Section { get; set; }

    [ForeignKey(nameof(ReportingHeadEcode))]
    public virtual EmployeeMaster? Manager { get; set; }

    public virtual ICollection<EmployeeMaster> Subordinates { get; set; } = new List<EmployeeMaster>();
}
