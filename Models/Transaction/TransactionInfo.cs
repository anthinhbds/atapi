namespace atmnr_api.Models;
public class TransactionInfo
{
     public Guid? TransId { get; set; }
     public String? Transno { get; set; }
     public String? Transtype { get; set; }
     public DateTime? Transdate { get; set; }
     public String? Status { get; set; }
     public String? ObjectId { get; set; }
     public String? CustomerId { get; set; }
     public ApartmentInfo? Apartment { get; set; }
     public UserInfo? Employee { get; set; }
     public String? Description { get; set; }
     public String? Notes { get; set; }
     public Decimal? Totalamount { get; set; }
     public String? CreatedBy { get; set; }
     public DateTime? CreatedDate { get; set; }
     public String? UpdatedBy { get; set; }
     public DateTime? LastUpdate { get; set; }
     public IEnumerable<TransactionDetailInfo>? Details { get; set; }
     public IEnumerable<TransactionDetailInfo>? D_Details { get; set; }
     public IEnumerable<TransactionMemberInfo>? Members { get; set; }
     public IEnumerable<TransactionMemberInfo>? D_Members { get; set; }
}
public class TransactionSummaryInfo
{
     public String Key { get; set; }
     public int Count { get; set; }
}

// public class AssignmentAaprtmentModel
// {
//      public string[] Ids { get; set; }
//      public String Assignee { get; set; }
// }
