using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using GenerativeAI.Types;

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
        get
        {
            if (JsonSerializer.IsReflectionEnabledByDefault)
            {
                #pragma disable warning IL2026, IL3050
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() },
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    TypeInfoResolver = TypesSerializerContext.Default,
                    UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode
                };
                options.TypeInfoResolverChain.Add(new DefaultJsonTypeInfoResolver());
                #pragma restore warning IL2026, IL3050

                return options;
            }
            else
            {
                return new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true,
                    //Converters = { new JsonStringEnumConverter() },
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    TypeInfoResolver = TypesSerializerContext.Default,
                    UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode
                };
            }
        } 
    }


    public static JsonSerializerOptions GenerateObjectJsonOptions
    {
        get
        {
            JsonSerializerOptions options;

            if (JsonSerializer.IsReflectionEnabledByDefault)
            {
                #pragma disable warning IL2026, IL3050
                // Keep in sync with the JsonSourceGenerationOptions attribute on JsonContext below.
                options = new(JsonSerializerDefaults.Web)
                {
                    TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
                    Converters = { new JsonStringEnumConverter() },
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true,
                };
                #pragma restore warning IL2026, IL3050
            }
            else
            {
                options = new(GenerateObjectJsonContext.Default.Options)
                {
                    // Compile-time encoder setting not yet available
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                };
            }

            options.MakeReadOnly();
            return options;
        }
    }
}

// Keep in sync with CreateDefaultOptions above.
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web,
    UseStringEnumConverter = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = true)]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(List<object>))]
[JsonSerializable(typeof(IDictionary<string, object?>))]
[JsonSerializable(typeof(JsonDocument))]
[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(JsonNode))]
[JsonSerializable(typeof(IEnumerable<string>))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(float))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(TimeSpan))]
[JsonSerializable(typeof(DateTimeOffset))]

[EditorBrowsable(EditorBrowsableState.Never)] // Never use JsonContext directly, use DefaultOptions instead.
internal sealed partial class GenerateObjectJsonContext : JsonSerializerContext
{
    
}