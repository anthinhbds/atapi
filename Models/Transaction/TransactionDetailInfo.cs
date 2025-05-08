namespace atmnr_api.Models;
public class TransactionDetailInfo
{
     public Guid? TransId { get; set; }
     public Int16? Linenum { get; set; }
     public DateTime? Date { get; set; }
     public Decimal? Amount { get; set; }
     public String? Notes { get; set; }
}
