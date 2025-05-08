using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace atmnr_api.Entities;

[Table("notification")]
public class Notification
{
    [Key, Column("notificationid")] public Guid NotificationId { get; set; }
    [Column("userid")] public String UserId { get; set; }
    [Column("type")] public String Type { get; set; }
    [Column("payload")] public String Payload { get; set; }
    [Column("isread")] public Boolean Isread { get; set; }
    [Column("createddate")] public DateTime Createddate { get; set; }
    [Column("lastupdate")] public DateTime Lastupdate { get; set; }
}
