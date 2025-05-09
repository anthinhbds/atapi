namespace atmnr_api.Models;
public class CustomerJourneyDetInfo
{
     public String? CustomerId { get; set; }
     public DateTime? Journeydate { get; set; }
     public String? Project { get; set; }
     public String? Feedback { get; set; }
     public String? Problem { get; set; }
     public String? Nextstep { get; set; }
     public String? Projectname { get; set; }
     public String? Notes { get; set; }
}

public class CustomerJourneySummaryInfo
{
     public String Key { get; set; }
     public int Count { get; set; }
}