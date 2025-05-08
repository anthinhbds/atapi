namespace atmnr_api.Models;
public class CustomerInfo
{
     public String? CustomerId { get; set; }
     public String? Customername { get; set; }
     public String? Status { get; set; }
     public String? Telephone { get; set; }
     public String? Telephone2 { get; set; }
     public String? Telephone3 { get; set; }
     public String? Telephone4 { get; set; }
     public String? Demand { get; set; }
     public String? Arearange { get; set; }
     public String? Pricerange { get; set; }
     public String? Bedroom { get; set; }
     public String? Priority { get; set; }
     public String? UserId { get; set; }
     public UserInfo? User { get; set; }
     public String? PrevioususerId { get; set; }
     public String? Notes { get; set; }
     public String? Leadsource { get; set; }
     public String? Leadsourceother { get; set; }
     public String? Project { get; set; }
     public String? Projectother { get; set; }
     public String? Furniture { get; set; }
     public String? CreatedBy { get; set; }
     public DateTime? CreatedDate { get; set; }
     public String? UpdatedBy { get; set; }
     public DateTime? LastUpdate { get; set; }
     public IEnumerable<CustomerNoteInfo>? Details { get; set; }
     public IEnumerable<CustomerNoteInfo>? D_Details { get; set; }
     public String? Displaycustomer { get; set; }
}
public class CustomerSummaryInfo
{
     public String Key { get; set; }
     public int Count { get; set; }
}

// public class AssignmentAaprtmentModel
// {
//      public string[] Ids { get; set; }
//      public String Assignee { get; set; }
// }
