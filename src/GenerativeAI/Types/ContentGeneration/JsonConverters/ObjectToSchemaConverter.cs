using Json.More;
using Json.Schema;
using Json.Schema.Generation;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// This converter interprets objects and writes them as JSON schema definitions.
/// </summary>
sealed class ObjectToJsonSchemaConverter : JsonConverter<object>
{
    /// <summary>
    /// Reads an incoming JSON value and deserializes it as a dynamic object.
    /// </summary>
    /// <param name="jsonReader">The Utf8JsonReader instance to read from.</param>
    /// <param name="targetType">The expected type of the object being read.</param>
    /// <param name="jsonOptions">Options for customizing the deserialization process.</param>
    /// <returns>The deserialized dynamic object or null if deserialization fails.</returns>
    public override object? Read(
        ref Utf8JsonReader jsonReader,
        Type targetType,
        JsonSerializerOptions jsonOptions)
    {
        return JsonSerializer.Deserialize<dynamic>(jsonReader.GetString()!, jsonOptions);
    }

    /// <summary>
    /// Writes an object as JSON output. For known structure types such as JsonDocument,
    /// JsonElement, JsonNode, or Schema, it writes the JSON representation directly.
    /// For other object types, it generates a JSON schema representation and writes it.
    /// </summary>
    /// <param name="jsonWriter">The Utf8JsonWriter to which the value will be written.</param>
    /// <param name="valueToWrite">The object to be serialized and written.</param>
    /// <param name="jsonOptions">Options for customizing the serialization process.</param>
    public override void Write(
        Utf8JsonWriter jsonWriter,
        object valueToWrite,
        JsonSerializerOptions jsonOptions)
    {
        var actualType = valueToWrite is Type type ? type : valueToWrite.GetType();

        if (actualType == typeof(JsonDocument) ||
            actualType == typeof(JsonElement) ||
            actualType == typeof(JsonNode) ||
            actualType == typeof(Schema))
        {
            var clonedOptions = new JsonSerializerOptions(jsonOptions);
            clonedOptions.Converters.Remove(this);
            JsonSerializer.Serialize(jsonWriter, valueToWrite, actualType, clonedOptions);
        }
        else
        {
            var naming = jsonOptions.PropertyNamingPolicy ?? JsonNamingPolicy.CamelCase;

#if NET_6_0_OR_GREATER
             var propertyResolver = PropertyNameResolvers.CamelCase;
             if(naming == JsonNamingPolicy.CamelCase)
                 propertyResolver = PropertyNameResolvers.CamelCase;
             else if (naming == JsonNamingPolicy.KebabCaseLower)
             {
                 propertyResolver = PropertyNameResolvers.KebabCase;
             }
             else if (naming == JsonNamingPolicy.KebabCaseUpper)
             {
                 propertyResolver = PropertyNameResolvers.UpperKebabCase;
             }
             else if (naming == JsonNamingPolicy.SnakeCaseLower)
             {
                 propertyResolver = PropertyNameResolvers.LowerSnakeCase;
             }
             else if (naming == JsonNamingPolicy.SnakeCaseUpper)
             {
                 propertyResolver = PropertyNameResolvers.UpperSnakeCase;
             }
var generatorConfig = new SchemaGeneratorConfiguration
             {
PropertyNameResolver = propertyResolver,
             };

#else
            var generatorConfig = new SchemaGeneratorConfiguration();
#endif

            var builder = new JsonSchemaBuilder();

            var constructedSchema = builder
                .FromType(actualType, generatorConfig)
                .Build().ToJsonDocument();


            //Work around to avoid type as array
            var schema = GoogleSchemaHelper.ConvertToCompatibleSchemaSubset(constructedSchema);

            JsonSerializer.Serialize(jsonWriter, schema, schema.GetType(), jsonOptions);
        }
    }
}