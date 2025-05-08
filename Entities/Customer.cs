using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace atmnr_api.Entities;

[Table("customer")]
public class Customer : BaseEntity
{
    [Key, Column("customerid")] public String CustomerId { get; set; }
    [Column("customername")] public String Customername { get; set; }
    [Column("status")] public String Status { get; set; }
    [Column("telephone")] public String Telephone { get; set; }
    [Column("telephone2")] public String Telephone2 { get; set; }
    [Column("telephone3")] public String Telephone3 { get; set; }
    [Column("telephone4")] public String Telephone4 { get; set; }
    [Column("demand")] public String Demand { get; set; }
    [Column("arearange")] public String Arearange { get; set; }
    [Column("pricerange")] public String Pricerange { get; set; }
    [Column("bedroom")] public String Bedroom { get; set; }
    [Column("priority")] public String Priority { get; set; }
    [Column("userid")] public String UserId { get; set; }
    [ForeignKey("UserId")] public User User { get; set; }
    [Column("previoususerid")] public String PrevioususerId { get; set; }
    [Column("notes")] public String Notes { get; set; }
    [Column("leadsource")] public String Leadsource { get; set; }
    [Column("leadsourceother")] public String Leadsourceother { get; set; }
    [Column("project")] public String Project { get; set; }
    [Column("projectother")] public String Projectother { get; set; }
    [Column("furniture")] public String Furniture { get; set; }
    [NotMapped] public DateTime Sortdate { get; set; }
    [NotMapped] public String Displaycustomer { get; set; }

}
