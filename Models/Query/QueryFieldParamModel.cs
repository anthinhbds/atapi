namespace atmnr_api.Models;

public class QueryFieldParamModel
{
    public IEnumerable<FilterModel>? Filter { get; set; }
    public string FieldName { get; set; }
}