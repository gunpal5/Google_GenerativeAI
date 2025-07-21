using System.Text.Json.Serialization;
using CSharpToJsonSchema;

namespace GenerativeAI.Tools;

/// <summary>
/// JSON source generation context for OpenApiSchema serialization.
/// </summary>
[JsonSerializable(typeof(OpenApiSchema))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public partial class OpenApiSchemaSourceGenerationContext:JsonSerializerContext
{
    
}