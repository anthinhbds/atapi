using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace atmnr_api.Entities;

[Table("ward")]
public class Ward : BaseEntity
{
    [Key, Column("wardid")] public String WardId { get; set; }
    [Column("wardname")] public String Wardname { get; set; }
    [Column("districtid")] public String DistrictId { get; set; }
}
