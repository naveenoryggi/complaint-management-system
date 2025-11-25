using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComplaintManagement.Domain.Entities.Oryggi;

[Table("CompanyMaster")]
public class CompanyMaster
{
    [Key]
    [Column("Ccode")]
    public int Ccode { get; set; }

    [Column("CName")]
    public string? CName { get; set; }

    [Column("Address")]
    public string? Address { get; set; }

    [Column("Email")]
    public string? Email { get; set; }

    [Column("TelephoneNo")]
    public string? TelephoneNo { get; set; }

    // Navigation properties
    public virtual ICollection<BranchMaster> Branches { get; set; } = new List<BranchMaster>();
}
