namespace atmnr_api.Models;
public class CustomerJourneyInfo
{
     public String? CustomerId { get; set; }
     public CustomerInfo? Customer { get; set; }
     public String? Demand { get; set; }
     public String? Status { get; set; }
     public String? UserId { get; set; }
     public String? Finance { get; set; }
     public String? Searching { get; set; }
     public String? Quality { get; set; }
     public String? Comments { get; set; }
     public String? CreatedBy { get; set; }
     public DateTime? CreatedDate { get; set; }
     public String? UpdatedBy { get; set; }
     public DateTime? LastUpdate { get; set; }
     public IEnumerable<CustomerJourneyDetInfo>? Details { get; set; }
     public IEnumerable<CustomerJourneyDetInfo>? D_Details { get; set; }
}