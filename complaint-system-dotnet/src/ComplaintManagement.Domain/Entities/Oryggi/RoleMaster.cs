using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComplaintManagement.Domain.Entities.Oryggi;

[Table("RoleMaster")]
public class RoleMaster
{
    [Key]
    [Column("RoleId")]
    public int RoleId { get; set; }

    [Column("Role")]
    public string? Role { get; set; }

    [Column("Template")]
    public string? Template { get; set; }
}
