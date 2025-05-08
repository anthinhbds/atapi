using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace atmnr_api.Entities;

[Table("apartmentnote")]
public class ApartmentNote : BaseEntity
{
    [Column("apartmentid")] public String ApartmentId { get; set; }
    [Column("linenum")] public Int16 Linenum { get; set; }
    [Column("type")] public String Type { get; set; } = "N";
    [Column("entrydate")] public DateTime Entrydate { get; set; }
    [Column("notes")] public String Notes { get; set; }
}
