using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace atmnr_api.Entities;

[Table("userclaim")]
public class UserClaim : BaseEntity
{
    [Key, Column("userclaimid")] public Guid UserClaimid { get; set; }
    [Column("userid")] public String UserId { get; set; }
    [Column("claimid")] public String ClaimId { get; set; } = "";
    [Column("claimvalue")] public String Claimvalue { get; set; } = "";
    // [Column("archived")] public Int16 Archived { get; set; } = 0;
}
