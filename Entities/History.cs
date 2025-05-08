using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace atmnr_api.Entities;

[Table("stlog")]
public class History
{
    [Key, Column("logid")] public Guid LogId { get; set; }
    [Column("actiondate")] public DateTime Actiondate { get; set; }
    [Column("actiontype")] public String Actiontype { get; set; }
    [Column("formid")] public String FormId { get; set; }
    [Column("referenceid")] public String ReferenceId { get; set; }
    [Column("contentlog")] public String Contentlog { get; set; }
    [Column("userid")] public String UserId { get; set; }
}
