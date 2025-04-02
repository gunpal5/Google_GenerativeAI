#if NET6_0_OR_GREATER
namespace GenerativeAI.Types.Converters;

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    private const string TimeFormat = "O";

    // Reads JSON and converts it to TimeOnly
    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Check if the token is a string
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Unexpected token type: {reader.TokenType}. Expected String.");
        }

        string? timeString = reader.GetString();

        // Handle null or empty strings (TimeOnly is non-nullable)
        if (string.IsNullOrWhiteSpace(timeString))
        {
             throw new JsonException("Cannot convert null or empty string to TimeOnly.");
        }
        
        if (DateTimeOffset.TryParse(timeString, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset dateTimeOffset))
        {
            return TimeOnly.FromDateTime(dateTimeOffset.DateTime);
        }
      
        if (TimeOnly.TryParse(timeString, CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly timeOnly))
        {
            return timeOnly;
        }

        throw new JsonException($"Could not parse \"{timeString}\" as a valid time format for TimeOnly. Expected formats like 'HH:mm:ss.fffffff' or an ISO 8601 DateTimeOffset string.");
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("O", CultureInfo.InvariantCulture));
    }
}
#endif