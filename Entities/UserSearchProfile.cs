using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace atmnr_api.Entities;

[Table("usersearchprofile")]
public class UserSearchProfile : BaseEntity
{
    [Key, Column("id")] public Guid Id { get; set; }
    [Column("userid")] public String UserId { get; set; }
    [Column("formid")] public String FormId { get; set; } = "";
    [Column("profilename")] public String ProfileName { get; set; } = "";
    [Column("searchingcontent")] public String SearchingContent { get; set; } = "";
}
