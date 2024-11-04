using System.Text.Json;
using System.Text.Json.Serialization;

namespace NovaVoice.GroqApiClient;

public class UnixEpochDateTimeJsonConverter : JsonConverter<DateTime>
{
    private readonly DateTime _epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
            {
                var seconds = reader.GetInt64();
                return _epoch.AddSeconds(seconds);
            }
            
            case JsonTokenType.String:
            {
                var s = reader.GetString();
            
                if (long.TryParse(s, out var seconds))
                    return _epoch.AddSeconds(seconds);

                if (DateTime.TryParse(s, out DateTime dt))
                    return dt; 
            
                throw new JsonException($"Unable to convert string '{s}' to DateTime.");
            }
            
            default:
                throw new JsonException($"Unexpected token type: {reader.TokenType}");
        }
    }


    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var seconds = (long)(value.ToUniversalTime() - _epoch).TotalSeconds;
        writer.WriteNumberValue(seconds);
    }
}