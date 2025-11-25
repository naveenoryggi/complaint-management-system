using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComplaintManagement.Domain.Entities.Oryggi;

[Table("DeptMaster")]
public class DeptMaster
{
    [Key]
    [Column("Dcode")]
    public int Dcode { get; set; }

    [Column("Dname")]
    public string? Dname { get; set; }

    [Column("BranchCode")]
    public int? BranchCode { get; set; }

    // Navigation properties
    [ForeignKey(nameof(BranchCode))]
    public virtual BranchMaster? Branch { get; set; }

    public virtual ICollection<SectionMaster> Sections { get; set; } = new List<SectionMaster>();
}
