using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace atmnr_api.Entities;

[Table("assignmentlog")]
public class AssignmentLog : BaseEntity
{
    [Key, Column("id")] public Guid Id { get; set; }
    [Column("formid")] public String FormId { get; set; }
    [Column("referenceid")] public String ReferenceId { get; set; }
    [Column("date")] public DateTime Date { get; set; }
    [Column("assignee")] public String Assignee { get; set; }
    [ForeignKey("Assignee")] public User User { get; set; }
    [Column("accepting")] public String Accepting { get; set; }
}
