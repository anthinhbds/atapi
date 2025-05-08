using System.Text.Json;
using System.Text.Json.Nodes;

public static class JsonValueExtensions
{
    public static void GetJsonValue<T>(this JsonValue element, out T value)
    {
        if (!element.TryGetValue<T>(out value))
        {
            value = (T)Convert.ChangeType(element.GetValue<string>(), typeof(T));
        }
    }
}
