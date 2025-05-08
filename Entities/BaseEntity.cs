using System.ComponentModel.DataAnnotations.Schema;

namespace atmnr_api.Entities;

public class BaseEntity
{
    [Column("createdby")] public String? CreatedBy { get; set; }
    [Column("updatedby")] public String? UpdatedBy { get; set; }
    [Column("createddate")] public DateTime CreatedDate { get; set; }
    [Column("lastupdate")] public DateTime LastUpdate { get; set; }
}
