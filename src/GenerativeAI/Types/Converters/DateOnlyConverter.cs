#if NET6_0_OR_GREATER
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types.Converters;


public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    // Defines the standard date format for serialization (ISO 8601 date)
    private const string Format = "yyyy-MM-dd";

    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Unexpected token type: {reader.TokenType}. Expected String.");
        }

        string? dateString = reader.GetString();

        if (string.IsNullOrWhiteSpace(dateString))
        {
             throw new JsonException("Cannot convert null or empty string to DateOnly.");
        }

        if (DateTimeOffset.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTimeOffset dateTimeOffset))
        {
           return DateOnly.FromDateTime(dateTimeOffset.Date);
        }

        if (DateOnly.TryParseExact(dateString, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly dateOnly))
        {
            return dateOnly;
        }
        throw new JsonException($"Could not parse \"{dateString}\" as a valid date or datetime format for DateOnly. Expected formats like '{Format}' or ISO 8601 DateTimeOffset.");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        var dateTime = new DateTime(value.Year, value.Month, value.Day);
        writer.WriteStringValue(dateTime.ToString("O", CultureInfo.InvariantCulture)); 
    }
}
#endif