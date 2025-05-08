namespace atmnr_api.Models;
public class HistoryInfo
{
    public Guid? LogId { get; set; }
    public DateTime? Actiondate { get; set; }
    public String? Actiontype { get; set; }
    public String? FormId { get; set; }
    public String? ReferenceId { get; set; }
    public String? Contentlog { get; set; }
    public String? UserId { get; set; }
}

public class HistoryParamsModel
{
    public String FormId { get; set; }
    public String ReferenceId { get; set; }
}
