using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace atmnr_api.Entities;

[Table("transactionmember")]
public class TransactionMember : BaseEntity
{
    [Column("transid")] public Guid TransId { get; set; }
    [Column("userid")] public String UserId { get; set; }
    [Column("rate")] public Decimal Rate { get; set; }
    [Column("notes")] public String Notes { get; set; }
}
