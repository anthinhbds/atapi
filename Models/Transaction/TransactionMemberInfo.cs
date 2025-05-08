namespace atmnr_api.Models;
public class TransactionMemberInfo
{
     public Guid? TransId { get; set; }
     public String? UserId { get; set; }
     public Decimal? Rate { get; set; }
     public String? Notes { get; set; }
}
