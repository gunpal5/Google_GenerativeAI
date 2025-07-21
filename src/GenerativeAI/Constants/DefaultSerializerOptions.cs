using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using GenerativeAI.Types;
using GenerativeAI.Types.Converters;

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
    /// <summary>
    /// Gets a list of custom type resolvers used to provide additional or specialized
    /// type resolution logic for JSON serialization and deserialization processes.
    /// </summary>
    /// <remarks>
    /// Custom type resolvers are essential for extending or modifying the default behavior
    /// of JSON serialization, enabling support for application-specific or complex data types
    /// that may not be adequately handled by the default resolvers.
    /// This property is used for compatibility with NativeAOT in JsonMode and QuickTools.
    /// </remarks>
    public static List<IJsonTypeInfoResolver> CustomJsonTypeResolvers { get; } = new();

    /// <summary>
    /// Provides a centralized and consistent configuration for JSON serialization and deserialization settings.
    /// </summary>
    /// <remarks>
    /// These options are used only for handling serialization for Google Services. It won't be used anywhere else.
    /// For specifying custom type resolvers or <see cref="JsonSerializerContext"/>, use the <see cref="CustomJsonTypeResolvers"/> property.
    /// </remarks>
    public static JsonSerializerOptions Options
    {
        get
        {
            if (JsonSerializer.IsReflectionEnabledByDefault)
            {
#pragma warning disable IL2026, IL3050
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() },
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    TypeInfoResolver = TypesSerializerContext.Default,
                    UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement
                };
                options.TypeInfoResolverChain.Add(new DefaultJsonTypeInfoResolver());
#pragma warning restore IL2026, IL3050

                AddConverters(options);
                return options;
            }
            else
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true,
                    //Converters = { new JsonStringEnumConverter() },
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    TypeInfoResolver = TypesSerializerContext.Default,
                    UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement
                };

                AddConverters(options);
                return options;
            }
        }
    }


    /// <summary>
    /// Provides a pre-configured instance of <see cref="JsonSerializerOptions"/> optimized for
    /// generating JSON during serialization and deserialization, including custom resolvers and settings for specific cases.
    /// </summary>
    /// <remarks>
    /// This property adjusts various JSON serialization settings such as default ignore condition,
    /// string encoding modes, and options for writing indented JSON. It also integrates custom resolvers
    /// and type information to fully support complex serialization demands.
    /// In environments where reflection is disabled due to AOT (Ahead-Of-Time compilation) or trimming,
    /// the returned options apply specific pre-compiled settings to ensure compatibility and maintain functionality.
    /// </remarks>
    public static JsonSerializerOptions GenerateObjectJsonOptions
    {
        get
        {
            JsonSerializerOptions options;

            if (JsonSerializer.IsReflectionEnabledByDefault)
            {
#pragma warning disable IL2026, IL3050
                // Keep in sync with the JsonSourceGenerationOptions attribute on JsonContext below.
                options = new(JsonSerializerDefaults.Web)
                {
                    Converters = { new JsonStringEnumConverter() },
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true,
                };

                AddCustomResolvers(options);
                options.TypeInfoResolverChain.Add(new DefaultJsonTypeInfoResolver());
#pragma warning restore IL2026, IL3050
            }
            else
            {
                options = new(GenerateObjectJsonContext.Default.Options)
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true,
                };

                AddCustomResolvers(options);
            }

            AddConverters(options);
            options.MakeReadOnly();
            return options;
        }
    }

    private static void AddConverters(JsonSerializerOptions options)
    {
#if NET8_0_OR_GREATER
        options.Converters.Add(new DateOnlyJsonConverter());
        options.Converters.Add(new TimeOnlyJsonConverter());
#endif
    }

    private static void AddCustomResolvers(JsonSerializerOptions options)
    {
        foreach (var resolver in CustomJsonTypeResolvers.Where(resolver =>
                     !options.TypeInfoResolverChain.Contains(resolver)))
        {
            options.TypeInfoResolverChain.Add(resolver);
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