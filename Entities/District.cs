using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace atmnr_api.Entities;

[Table("district")]
public class District : BaseEntity
{
    [Key, Column("districtid")] public String DistrictId { get; set; }
    [Column("districtname")] public String Districtname { get; set; }
}
