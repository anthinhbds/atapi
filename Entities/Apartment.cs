using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace atmnr_api.Entities;

[Table("apartment")]
public class Apartment : BaseEntity
{
    [Key, Column("apartmentid")] public String ApartmentId { get; set; }
    [Column("demand")] public String Demand { get; set; }
    [Column("projectid")] public String ProjectId { get; set; }
    [ForeignKey("ProjectId")] public Project Project { get; set; }
    [Column("salesprice")] public Decimal Salesprice { get; set; }
    [Column("rentprice")] public Decimal Rentprice { get; set; }
    [Column("salesfee")] public Decimal Salesfee { get; set; }
    [Column("rentfee")] public Decimal Rentfee { get; set; }
    [Column("area")] public Decimal Area { get; set; }
    [Column("bedroom")] public Int16 Bedroom { get; set; }
    [Column("apartmentview")] public String Apartmentview { get; set; }
    [Column("status")] public String Status { get; set; }
    [Column("apartmentno")] public String Apartmentno { get; set; }
    [Column("owner")] public String Owner { get; set; }
    [Column("telephone")] public String Telephone { get; set; }
    [Column("telephone2")] public String Telephone2 { get; set; }
    [Column("telephone3")] public String Telephone3 { get; set; }
    [Column("telephone4")] public String Telephone4 { get; set; }
    [Column("telephone5")] public String Telephone5 { get; set; }
    [Column("lookupcode")] public String Lookupcode { get; set; }
    [Column("furniture")] public String Furniture { get; set; }
    [Column("banconyview")] public String Banconyview { get; set; }
    [Column("userid")] public String UserId { get; set; }
    [ForeignKey("UserId")] public User User { get; set; }
    [Column("previoususerid")] public String PrevioususerId { get; set; }
    [Column("notes")] public String Notes { get; set; }
    [Column("priority")] public String Priority { get; set; }
    [Column("ispartner")] public String Ispartner { get; set; }
    [Column("partnername")] public String Partnername { get; set; }
    [Column("partnertelephone")] public String Partnertelephone { get; set; }
    [Column("expireddate")] public DateTime Expireddate { get; set; }

    [NotMapped] public DateTime Sortdate { get; set; }
    [NotMapped] public String Displayapartment { get; set; }

    //Mirgation field
    // [Column("mir_projectid")] public String MirProjectid { get; set; } = "";
}
