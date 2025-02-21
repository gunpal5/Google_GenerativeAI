using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenerativeAI;

/// <summary>
/// Represents a class providing default JSON serializer options for consistent
/// serialization and deserialization behavior across the application.
/// </summary>
/// <remarks>
/// The serializer options include:
/// - CamelCase naming policy for property names.
/// - Case-insensitive property name matching during deserialization.
/// - Automatic serialization of enums as strings.
/// - Ignoring null values during serialization.
/// </remarks>
public class DefaultSerializerOptions
{
    public static JsonSerializerOptions Options
    {
        get => new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }
}