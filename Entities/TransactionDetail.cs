using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace atmnr_api.Entities;

[Table("transactiondetail")]
public class TransactionDetail : BaseEntity
{
    [Column("transid")] public Guid TransId { get; set; }
    [Column("linenum")] public Int16 Linenum { get; set; }
    [Column("date")] public DateTime Date { get; set; }
    [Column("amount")] public Decimal Amount { get; set; }
    [Column("notes")] public String Notes { get; set; }
}
