using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace atmnr_api.Entities;

[Table("users")]
public class User : BaseEntity
{
    [Key, Column("userid")] public String UserId { get; set; }
    [Column("firstname")] public String Firstame { get; set; }
    [Column("lastname")] public String Lastname { get; set; }
    [Column("name")] public String Name { get; set; }
    [Column("passwordhash")] public byte[] PasswordHash { get; set; }
    [Column("passwordsalt")] public byte[] PasswordSalt { get; set; }
    [Column("telephone")] public String Telephone { get; set; } = "";
    [Column("email")] public String Email { get; set; } = "";
    [Column("archived")] public Int16 Archived { get; set; } = 0;
    [ForeignKey("UserId")] public ICollection<UserClaim> Claims { get; set; }

}
