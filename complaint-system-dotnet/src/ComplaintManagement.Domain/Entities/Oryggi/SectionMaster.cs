using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComplaintManagement.Domain.Entities.Oryggi;

[Table("SectionMaster")]
public class SectionMaster
{
    [Key]
    [Column("SecCode")]
    public int SecCode { get; set; }

    [Column("SecName")]
    public string? SecName { get; set; }

    [Column("Dcode")]
    public int? Dcode { get; set; }

    // Navigation properties
    [ForeignKey(nameof(Dcode))]
    public virtual DeptMaster? Department { get; set; }

    public virtual ICollection<EmployeeMaster> Employees { get; set; } = new List<EmployeeMaster>();
}
