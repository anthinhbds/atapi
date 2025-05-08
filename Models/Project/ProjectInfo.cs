namespace atmnr_api.Models;
public class ProjectInfo
{
    public String? ProjectId { get; set; }
    public String? Projectname { get; set; }
    public String? Status { get; set; }
    public String? Arearange { get; set; }
    public String? Owner { get; set; }
    public Int16? StreetId { get; set; }
    public Int16? WardId { get; set; }
    public String? DistrictId { get; set; }
    public Int16? Archived { get; set; }
    public DistrictInfo? District { get; set; }
}

public class ProjectSummaryInfo
{
    public Int16 Archived { get; set; }
    public int Count { get; set; }
}