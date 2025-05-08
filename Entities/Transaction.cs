using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace atmnr_api.Entities;

[Table("transaction")]
public class Transaction : BaseEntity
{
    [Key, Column("transid")] public Guid TransId { get; set; }
    [Column("transno")] public String Transno { get; set; }
    [Column("transtype")] public String Transtype { get; set; }
    [Column("transdate")] public DateTime Transdate { get; set; }
    [Column("status")] public String Status { get; set; }
    [Column("objectid")] public String ObjectId { get; set; }
    [Column("customerid")] public String CustomerId { get; set; } = "";
    [ForeignKey("ObjectId")] public Apartment Apartment { get; set; }
    [ForeignKey("ObjectId")] public User Employee { get; set; }
    [Column("description")] public String Description { get; set; }
    [Column("notes")] public String Notes { get; set; }
    [Column("totalamount")] public Decimal Totalamount { get; set; }
}
