using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The <see cref="Schema">Schema</see> object allows the definition of input and output data types.
/// These types can be objects, but also primitives and arrays. Represents a select
/// subset of an <see href="https://spec.openapis.org/oas/v3.0.3#schema">OpenAPI 3.0 schema object</see>.
/// </summary>
/// <seealso href="https://ai.google.dev/api/caching#Schema">See Official API Documentation</seealso> 
public class Schema
{
    /// <summary>
    /// Required. Data type.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; }

    /// <summary>
    /// Optional. The format of the data. This is used only for primitive datatypes.
    /// Supported formats:
    /// for NUMBER type: float, double
    /// for INTEGER type: int32, int64
    /// for STRING type: enum
    /// </summary>
    [JsonPropertyName("format")]
    public string? Format { get; set; }

    /// <summary>
    /// Optional. A brief description of the parameter. This could contain examples of use.
    /// Parameter description may be formatted as Markdown.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Optional. Indicates if the value may be null.
    /// </summary>
    [JsonPropertyName("nullable")]
    public bool? Nullable { get; set; }

    /// <summary>
    /// Optional. Possible values of the element of Type.STRING with enum format.
    /// For example we can define an Enum Direction as:
    /// {type:STRING, format:enum, enum:["EAST", NORTH", "SOUTH", "WEST"]}
    /// </summary>
    [JsonPropertyName("enum")]
    public List<string>? Enum { get; set; }

    /// <summary>
    /// Optional. Maximum number of the elements for Type.ARRAY.
    /// </summary>
    [JsonPropertyName("maxItems")]
    public long? MaxItems { get; set; }

    /// <summary>
    /// Optional. Minimum number of the elements for Type.ARRAY.
    /// </summary>
    [JsonPropertyName("minItems")]
    public long? MinItems { get; set; }

    /// <summary>
    /// Optional. Properties of Type.OBJECT.
    /// An object containing a list of <c>"key": value</c> pairs.
    /// Example: <c>{ "name": "wrench", "mass": "1.3kg", "count": "3" }</c>.
    /// </summary>
    [JsonPropertyName("properties")]
    public Dictionary<string, Schema>? Properties { get; set; }

    /// <summary>
    /// Optional. Required properties of Type.OBJECT.
    /// </summary>
    [JsonPropertyName("required")]
    public List<string>? Required { get; set; }

    /// <summary>
    /// Optional. The order of the properties. Not a standard field in open api spec.
    /// Used to determine the order of the properties in the response.
    /// </summary>
    [JsonPropertyName("propertyOrdering")]
    public List<string>? PropertyOrdering { get; set; }

    /// <summary>
    /// Optional. Schema of the elements of Type.ARRAY.
    /// </summary>
    [JsonPropertyName("items")]
    public Schema? Items { get; set; }
}

/// <summary>
/// The <see cref="SourceGenerationContext">SourceGenerationContext</see> class is a custom
/// source generation context for improving the performance of JSON serialization and deserialization.
/// This is achieved by leveraging the <see cref="System.Text.Json.Serialization.JsonSourceGenerationOptionsAttribute"/>
/// to configure source generation options and define types for serialization at compile-time.
/// </summary>
/// <seealso cref="JsonSerializerContext"/>
/// <seealso href="https://learn.microsoft.com/dotnet/standard/serialization/system-text-json-source-generation">See Official Documentation on JSON Source Generation</seealso>
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Schema))]
public partial class SchemaSourceGenerationContext : JsonSerializerContext
{
}