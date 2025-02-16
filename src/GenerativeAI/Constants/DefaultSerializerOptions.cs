using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenerativeAI;

internal class DefaultSerializerOptions
{
    internal static JsonSerializerOptions Options
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