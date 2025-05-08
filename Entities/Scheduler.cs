using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace atmnr_api.Entities;

[Table("sttaskscheduler")]
public class Scheduler
{
    [Key, Column("schedulerid")] public String SchedulerId { get; set; }
    [Column("lastrun")] public DateTime Lastrun { get; set; }
    [Column("nextrunning")] public DateTime Nextrunning { get; set; }
    [Column("periodtype")] public String Periodtype { get; set; }
    [Column("day")] public Int16 Day { get; set; }
    [Column("hour")] public Int16 Hour { get; set; }
    [Column("minute")] public Int16 Minute { get; set; }
    [Column("isrepeat")] public String Isrepeat { get; set; }
    [Column("repeatevery")] public Int16 Repeatevery { get; set; }

}
