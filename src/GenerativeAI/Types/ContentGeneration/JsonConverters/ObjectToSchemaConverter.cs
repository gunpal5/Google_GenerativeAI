using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using Json.Schema;
using Json.Schema.Generation;
using Json.Schema.Generation.Intents;

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
        var actualType = valueToWrite is Type type? type : valueToWrite.GetType();
        
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
            
             var builder = new JsonSchemaBuilder();
            
             var constructedSchema = builder
                 .FromType(actualType, generatorConfig)
                 .Build().ToJsonDocument();
             //Work around to avoid type as array
             var schema = ConvertToSchema(constructedSchema);
            
            JsonSerializer.Serialize(jsonWriter, schema, schema.GetType(), jsonOptions);
        }
    }

    private Schema ConvertToSchema(JsonDocument constructedSchema)
    {
        var node = constructedSchema.RootElement.AsNode();
        ConvertNullableProperties(node);


        var x1 = node;
        var x2 = JsonSerializer.Serialize(x1);
        var schema = JsonSerializer.Deserialize<Schema>(x2);
        return schema;
    }

    private static void ConvertNullableProperties(JsonNode? node)
    {
        // If the node is an object, look for a "type" property or nested definitions
        if (node is JsonObject obj)
        {
            // If "type" is an array, remove "null" and collapse if it leaves only one type
            if (obj.TryGetPropertyValue("type", out var typeValue) && typeValue is JsonArray array)
            {
                foreach (JsonValue r in array)
                {
                    if(r.GetString() == "null")
                        continue;
                    obj["type"] = r.GetString();
                    break;
                }
            }

            // Recursively convert any nested schema in "properties"
            if (obj.TryGetPropertyValue("properties", out var propertiesNode) && propertiesNode is JsonObject propertiesObj)
            {
                foreach (var property in propertiesObj)
                {
                    ConvertNullableProperties(property.Value);
                }
            }

            // Recursively convert any nested schema in "items"
            if (obj.TryGetPropertyValue("items", out var itemsNode))
            {
                ConvertNullableProperties(itemsNode);
            }
        }

        // If the node is an array, traverse each element
        if (node is JsonArray arr)
        {
            foreach (var element in arr)
            {
                ConvertNullableProperties(element);
            }
        }
    }

   
}
