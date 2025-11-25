using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComplaintManagement.Domain.Entities.Oryggi;

[Table("DesignationMaster")]
public class DesignationMaster
{
    [Key]
    [Column("DesCode")]
    public int DesigCode { get; set; }

    [Column("DesName")]
    public string? DesigName { get; set; }

    [Column("DesName_hindi")]
    public string? DesigDesc { get; set; }

    // Navigation properties
    public virtual ICollection<EmployeeMaster> Employees { get; set; } = new List<EmployeeMaster>();
}
