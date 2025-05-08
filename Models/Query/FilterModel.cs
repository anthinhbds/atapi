using System.Text.Json.Nodes;

namespace atmnr_api.Models;

public class FilterModel
{
    public string Property { get; set; }
    public string? FilterType { get; set; }
    public string Method { get; set; }
    public JsonValue Value { get; set; }
}