using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace atmnr_api.Entities;

[Table("customerjourney")]
public class CustomerJourney : BaseEntity
{
    [Key, Column("customerid")] public String CustomerId { get; set; }
    [ForeignKey("CustomerId")] public Customer Customer { get; set; }
    [Column("demand")] public String Demand { get; set; } = "";
    [Column("status")] public String Status { get; set; }
    [Column("userid")] public String UserId { get; set; }
    [Column("finance")] public String Finance { get; set; } = "";
    [Column("searching")] public String Searching { get; set; } = "";
    [Column("quality")] public String Quality { get; set; } = "";
    [Column("comments")] public String Comments { get; set; } = "";
}
