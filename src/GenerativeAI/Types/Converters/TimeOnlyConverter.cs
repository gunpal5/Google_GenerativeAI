#if NET6_0_OR_GREATER
namespace GenerativeAI.Types.Converters;

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// JSON converter for <see cref="TimeOnly"/> values.
/// </summary>
public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{

    // Reads JSON and converts it to TimeOnly
    /// <summary>
    /// Reads and converts the JSON to a <see cref="TimeOnly"/> value.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The converted <see cref="TimeOnly"/> value.</returns>
    /// <exception cref="JsonException">Thrown when the JSON cannot be parsed as a valid time.</exception>
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

    /// <summary>
    /// Writes the <see cref="TimeOnly"/> value as JSON.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The <see cref="TimeOnly"/> value to write.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(writer);
#else
        if (writer == null) throw new ArgumentNullException(nameof(writer));
#endif
        writer.WriteStringValue(value.ToString("O", CultureInfo.InvariantCulture));
    }
}
#endif