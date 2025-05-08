using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace atmnr_api.Entities;

[Table("sttoken")]
public class RefreshToken
{
    [Key, Column("id")] public Guid Id { get; set; }
    [Column("token")] public String Token { get; set; }
    [Column("userid")] public String UserId { get; set; }
    [Column("ipaddress")] public String Ipaddress { get; set; } = "";
    [Column("expires")] public DateTime Expires { get; set; }
    [Column("isrevoked")] public bool IsRevoked { get; set; }
    [Column("revokedbyip")] public String Revokedbyip { get; set; } = "";
    [Column("createddate")] public DateTime Createddate { get; set; }
}
