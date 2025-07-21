#if NET6_0_OR_GREATER
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types.Converters;

/// <summary>
/// JSON converter for <see cref="DateOnly"/> values, supporting ISO 8601 date formats.
/// </summary>
public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    // Defines the standard date format for serialization (ISO 8601 date)
    private const string Format = "yyyy-MM-dd";

    /// <summary>
    /// Reads and converts the JSON to a <see cref="DateOnly"/> value.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The converted <see cref="DateOnly"/> value.</returns>
    /// <exception cref="JsonException">Thrown when the JSON cannot be parsed as a valid date.</exception>
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

    /// <summary>
    /// Writes the <see cref="DateOnly"/> value as JSON.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The <see cref="DateOnly"/> value to write.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        var dateTime = new DateTime(value.Year, value.Month, value.Day);
        writer.WriteStringValue(dateTime.ToString("O", CultureInfo.InvariantCulture)); 
    }
}
#endif