namespace atmnr_api.Models;
public class ApartmentInfo
{
     public String? ApartmentId { get; set; }
     public String? Demand { get; set; }
     public String? ProjectId { get; set; }
     public ProjectInfo? Project { get; set; }
     public Decimal? Salesprice { get; set; }
     public Decimal? Rentprice { get; set; }
     public Decimal? Salesfee { get; set; }
     public Decimal? Rentfee { get; set; }
     public Decimal? Area { get; set; }
     public Int16? Bedroom { get; set; }
     public String? Apartmentview { get; set; }
     public String? Status { get; set; }
     public String? Apartmentno { get; set; }
     public String? Owner { get; set; }
     public String? Telephone { get; set; }
     public String? Telephone2 { get; set; }
     public String? Telephone3 { get; set; }
     public String? Telephone4 { get; set; }
     public String? Telephone5 { get; set; }
     public String? Lookupcode { get; set; }
     public String? Furniture { get; set; }
     public String? Banconyview { get; set; }
     public String? UserId { get; set; }
     public UserInfo? User { get; set; }
     public String? PrevioususerId { get; set; }
     // public DateTime? Allocateddate { get; set; }
     public String? Notes { get; set; }
     public String? Priority { get; set; }
     public String? Ispartner { get; set; }
     public String? Partnername { get; set; }
     public String? Partnertelephone { get; set; }
     public String? CreatedBy { get; set; }
     public DateTime? CreatedDate { get; set; }
     public String? UpdatedBy { get; set; }
     public DateTime? LastUpdate { get; set; }
     public DateTime? Expireddate { get; set; }

     public IEnumerable<ApartmentNoteInfo>? Details { get; set; }
     public IEnumerable<ApartmentNoteInfo>? D_Details { get; set; }

}
public class ApartmentSummaryInfo
{
     public String Key { get; set; }
     public int Count { get; set; }
}

public class AssignmentAaprtmentModel
{
     public string[] Ids { get; set; }
     public String Assignee { get; set; }
}

public class TelephoneExistsModel
{
     public string apartmentId { get; set; }
     public string[] phones { get; set; }
}

public class ApartmentCheckTelephoneExistsInfo
{
     public string userid { get; set; }
     public string username { get; set; }
     public string? owner { get; set; }
     public string? apartmentno { get; set; }
     public string? customername { get; set; }
     public string? projectname { get; set; }
}
public class TelephoneExistsInfo
{
     public bool exists { get; set; }
     public ApartmentCheckTelephoneExistsInfo[] items { get; set; }
}

public class ApartmentComboInfo
{
     public String? ApartmentId { get; set; }
     public String? Apartmentno { get; set; }
     public String? Owner { get; set; }
     public ProjectInfo? Project { get; set; }
     public String? Displayapartment { get; set; }

}