namespace atmnr_api.Models;

public class QueryParamModel
{
    public IEnumerable<FilterModel>? Filter { get; set; }
    public IEnumerable<SortModel>? Sort { get; set; }
    public String? searchString { get; set; }
    public String? FormType { get; set; }
}