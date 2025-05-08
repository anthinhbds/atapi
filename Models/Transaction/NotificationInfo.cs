namespace atmnr_api.Models;
public class NotificationInfo
{
     public Guid? NotificationId { get; set; }
     public String? UserId { get; set; }
     public String? Type { get; set; }
     public String? Payload { get; set; }
     public Boolean? Isread { get; set; }
     public DateTime? Createddate { get; set; }
     public DateTime? Lastupdate { get; set; }

}
