namespace atmnr_api.Models;
public class AssignmentLogInfo
{
     public String? Id { get; set; }
     public String? FormId { get; set; }
     public String? ReferenceId { get; set; }
     public DateTime? Date { get; set; }
     public String? Assignee { get; set; }
     public UserInfo? User { get; set; }
     public String? Accepting { get; set; }
}
