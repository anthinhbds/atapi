using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace atmnr_api.Entities;

[Table("customerjourneydet")]
public class CustomerJourneyDet : BaseEntity
{
    [Key, Column("customerid")] public String CustomerId { get; set; }
    [Column("journeydate")] public DateTime Journeydate { get; set; }
    [Column("project")] public String Project { get; set; } = "";
    [Column("notes")] public String Notes { get; set; } = "";
    [Column("feedback")] public String Feedback { get; set; } = "";
    [Column("problem")] public String Problem { get; set; } = "";
    [Column("nextstep")] public String Nextstep { get; set; } = "";
    [NotMapped] public String Projectname { get; set; } = "";
}
