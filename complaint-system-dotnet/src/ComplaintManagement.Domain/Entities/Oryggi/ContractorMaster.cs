using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComplaintManagement.Domain.Entities.Oryggi;

[Table("ContractorMaster")]
public class ContractorMaster
{
    [Key]
    [Column("ContractorID")]
    public int ContractorID { get; set; }

    [Column("Contractor_Code")]
    public string? ContractorCode { get; set; }

    [Column("Name")]
    public string? Name { get; set; }

    [Column("Address")]
    public string? Address { get; set; }

    [Column("Email")]
    public string? Email { get; set; }

    [Column("Mobile")]
    public string? Mobile { get; set; }

    [Column("Description")]
    public string? Description { get; set; }

    [Column("ContactPerson")]
    public string? ContactPerson { get; set; }
}
