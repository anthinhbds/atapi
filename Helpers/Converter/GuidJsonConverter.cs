using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace atmnr_api.Helpers;

public class GuidJsonConverter : JsonConverter<Guid>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(Guid).IsAssignableFrom(typeToConvert);
        // return base.CanConvert(typeToConvert);
    }
    public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return Guid.Empty;
        else
        {
            string value = reader.GetString();
            if (string.IsNullOrEmpty(value)) return Guid.Empty;
            else
            {
                return Guid.Parse(value);
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
    {
        if (value == null) writer.WriteNullValue();// writer.WriteStringValue(value.ToString(formats[3]));
        else
        {
            if (value == Guid.Empty) writer.WriteNullValue();// .WriteStringValue(NullDate.ToString(formats[3]));
            else
            {
                Guid dt = ((Guid?)value).Value;
                //writer.WriteValue(dt);
                writer.WriteStringValue(value.ToString());
            }
        }

    }
}