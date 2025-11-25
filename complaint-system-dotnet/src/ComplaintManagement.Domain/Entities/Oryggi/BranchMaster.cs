using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComplaintManagement.Domain.Entities.Oryggi;

[Table("BranchMaster")]
public class BranchMaster
{
    [Key]
    [Column("BranchCode")]
    public int BranchCode { get; set; }

    [Column("BranchName")]
    public string? BranchName { get; set; }

    [Column("Location")]
    public string? Location { get; set; }

    [Column("Ccode")]
    public int? Ccode { get; set; }

    // Navigation properties
    [ForeignKey(nameof(Ccode))]
    public virtual CompanyMaster? Company { get; set; }

    public virtual ICollection<DeptMaster> Departments { get; set; } = new List<DeptMaster>();
}
