using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace atmnr_api.Helpers;

public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    static string[] formats = new string[]{
                    "yyyy-MM-dd",
                     "yyyy-MM-dd HH:mm:ss",
                     "yyyy-MM-dd HH:mm",
                     "yyyy-MM-ddTHH:mm:sszzz"
                };
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(DateTime).IsAssignableFrom(typeToConvert);
        // return base.CanConvert(typeToConvert);
    }
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var rootElement = jsonDoc.RootElement;



        if (jsonDoc == null)
        {
            return new DateTime(Constants.MinDate.Year, Constants.MinDate.Month, Constants.MinDate.Day);
        }
        if (rootElement.TryGetDateTime(out var dateTime))
        {
            if (dateTime.Kind == DateTimeKind.Unspecified)
            {
                return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            }

            return dateTime;
        }
        if (rootElement.ValueKind == JsonValueKind.String)
        {
            string s = rootElement.ToString();
            DateTime result;
            if (DateTime.TryParseExact(s, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return result;
            }
        }
        return new DateTime(Constants.MinDate.Year, Constants.MinDate.Month, Constants.MinDate.Day);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        if (value == null) writer.WriteNullValue();// writer.WriteStringValue(value.ToString(formats[3]));
        else
        {
            if (((DateTime?)value).Value == new DateTime(Constants.MinDate.Year, Constants.MinDate.Month, Constants.MinDate.Day)) writer.WriteNullValue();// .WriteStringValue(NullDate.ToString(formats[3]));
            else
            {
                DateTime dt = ((DateTime?)value).Value;
                //writer.WriteValue(dt);
                writer.WriteStringValue(value.ToString(formats[3]));
            }
        }

    }
}