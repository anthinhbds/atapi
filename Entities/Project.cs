using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace atmnr_api.Entities;

[Table("project")]
public class Project : BaseEntity
{
    [Key, Column("projectid")] public String ProjectId { get; set; }
    [Column("projectname")] public String Projectname { get; set; }
    [Column("status")] public String Status { get; set; } = "";
    [Column("arearange")] public String Arearange { get; set; } = "";
    [Column("owner")] public String Owner { get; set; } = "";
    [Column("streetid")] public Int16 StreetId { get; set; }
    [Column("wardid")] public Int16 WardId { get; set; }
    [Column("districtid")] public String DistrictId { get; set; }
    [Column("archived")] public Int16 Archived { get; set; } = 0;
    [ForeignKey("DistrictId")] public District District { get; set; }
}
