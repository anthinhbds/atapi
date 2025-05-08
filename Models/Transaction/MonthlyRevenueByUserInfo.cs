namespace atmnr_api.Models;
public class MonthlyRevenueByUserModel
{
     public String UserId { get; set; }
     public Int16 Month { get; set; }
}
public class MonthlyRevenueByUserInfo
{
     public Guid? TransId { get; set; }
     public Int16? Linenum { get; set; }
     public String? Description { get; set; }
     public Decimal? Amount { get; set; }
     public Decimal? Rate { get; set; }
     public DateTime? Date { get; set; }
     public String? UserId { get; set; }

}

public class MonthlyDetailModel
{
     public String Revenuetype { get; set; }
     public Int16 Month { get; set; }
}
public class MonthlyDetailInfo
{
     public Guid TransId { get; set; }
     public Int16? Linenum { get; set; }
     public String Description { get; set; }
     public Decimal Amount { get; set; }
     public DateTime Date { get; set; }

}
