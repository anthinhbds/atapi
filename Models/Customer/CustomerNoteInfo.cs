namespace atmnr_api.Models;
public class CustomerNoteInfo
{
     public String? CustomerId { get; set; }
     public Int16? Linenum { get; set; }
     public String? Type { get; set; }
     public DateTime? Entrydate { get; set; }
     public String? Notes { get; set; }
}
