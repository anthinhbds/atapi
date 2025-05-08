namespace atmnr_api.Models;
public class ViewStatisticInfo
{
    public Int16? Id { get; set; }
    public String? ObjectId { get; set; }
    public String? ParentId { get; set; }
    public DateTime? Datefrom { get; set; }
    public DateTime? Dateto { get; set; }
    public String? Type { get; set; }
    public String? Traffictext { get; set; }
    public Int16? Totalview { get; set; }
    public DateTime? Createddate { get; set; }
}
