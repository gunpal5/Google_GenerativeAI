using System.Text.Json;
using CSharpToJsonSchema;
using GenerativeAI.Types;

namespace GenerativeAI.Tools.Extensions;

/// <summary>
/// Extension methods for OpenApiSchema conversions and serialization.
/// </summary>
public static class OpenApiExtensions
{
    /// <summary>
    /// Converts an OpenApiSchema to its JSON string representation.
    /// </summary>
    /// <param name="schema">The OpenApiSchema to serialize.</param>
    /// <returns>A JSON string representation of the schema.</returns>
    public static string ToJson(this OpenApiSchema schema)
    {
        return JsonSerializer.Serialize(schema, OpenApiSchemaSourceGenerationContext.Default.OpenApiSchema);   
    }
    
    /// <summary>
    /// Converts a Schema to an OpenApiSchema.
    /// </summary>
    /// <param name="schema">The Schema to convert.</param>
    /// <returns>An OpenApiSchema equivalent of the input schema.</returns>
    public static OpenApiSchema ToOpenApiSchema(this Schema schema)
    {
        var json = JsonSerializer.Serialize(schema, TypesSerializerContext.Default.Schema);
        var result = JsonSerializer.Deserialize(json, OpenApiSchemaSourceGenerationContext.Default.OpenApiSchema);
        if (result == null)
            throw new InvalidOperationException("Failed to convert Schema to OpenApiSchema. The serialization resulted in null.");
        return result;   
    }
}