using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace atmnr_api.Entities;

[Table("viewstatistic")]
public class ViewStatistic
{
    [Key, Column("id")] public Int16 Id { get; set; }
    [Column("objectid")] public String ObjectId { get; set; }
    [Column("parentid")] public String ParentId { get; set; }
    [Column("datefrom")] public DateTime Datefrom { get; set; }
    [Column("dateto")] public DateTime Dateto { get; set; }
    [Column("type")] public String Type { get; set; }
    [Column("traffictext")] public String Traffictext { get; set; }
    [Column("totalview")] public Int16 Totalview { get; set; }
    [Column("createddate")] public DateTime Createddate { get; set; }
}
