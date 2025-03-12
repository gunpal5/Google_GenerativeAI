using System.Text.Json;
using CSharpToJsonSchema;
using GenerativeAI.Types;

namespace GenerativeAI.Tools.Extensions;

public static class OpenApiExtensions
{
    public static string ToJson(this OpenApiSchema schema)
    {
        return JsonSerializer.Serialize(schema, OpenApiSchemaSourceGenerationContext.Default.OpenApiSchema);   
    }
    
    public static OpenApiSchema ToOpenApiSchema(this Schema schema)
    {
        var json = JsonSerializer.Serialize(schema, TypesSerializerContext.Default.Schema);
        return JsonSerializer.Deserialize(json, OpenApiSchemaSourceGenerationContext.Default.OpenApiSchema);   
    }
}