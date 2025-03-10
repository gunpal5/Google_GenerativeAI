using System.Text.Json.Serialization;
using CSharpToJsonSchema;

namespace GenerativeAI.Tools;

[JsonSerializable(typeof(OpenApiSchema))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public partial class OpenApiSchemaSourceGenerationContext:JsonSerializerContext
{
    
}