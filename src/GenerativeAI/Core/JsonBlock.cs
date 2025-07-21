using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace GenerativeAI.Core;

/// <summary>
/// Represents a block of JSON data with associated metadata.
/// </summary>
public class JsonBlock
{
    /// <summary>
    /// Gets or sets the JSON content represented as a string.
    /// </summary>
    public string Json { get; set; }

    /// <summary>
    /// Gets or sets the line number associated with the JSON block.
    /// </summary>
    /// <remarks>
    /// This property represents the specific line number in a source or context
    /// that corresponds to the JSON data represented by the object.
    /// </remarks>
    public int LineNumber { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the JSON content represents an array.
    /// </summary>
    public bool IsArray { get; set; }

    /// <summary>
    /// Represents a block of JSON data along with its associated line number.
    /// </summary>
    public JsonBlock(string json, int lineNumber = 0 ,bool isArray = false)
    {
        Json = json;
        LineNumber = lineNumber;
        IsArray = isArray;
    }

    /// <summary>
    /// Represents a block of JSON data along with its associated line number
    /// </summary>
    public JsonBlock():this("")
    {
        
    }

    /// <summary>
    /// Deserializes the JSON data of the current instance into an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to which the JSON data will be deserialized. Must be a class.</typeparam>
    /// <param name="options">Optional <see cref="JsonSerializerOptions"/> to configure the deserialization process. Default options will be used if not provided.</param>
    /// <returns>An instance of type <typeparamref name="T"/> if deserialization is successful; otherwise, null in case of an error.</returns>
    /// <remarks>
    /// For NativeAOT/Trimming JsonSerializerOptions are required, you can ignore it if you have already specified a resolver in <c ref="DefaultSerializerOptions.CustomJsonTypeResolvers"/>
    /// </remarks>
    public T? ToObject<T>(JsonSerializerOptions? options = null) where T : class
    {
        try
        {
            if (options == null && !JsonSerializer.IsReflectionEnabledByDefault)
                throw new InvalidOperationException(
                    "JsonSerializerOptions must be provided when reflection is disabled for AOT and Trimming.");
            if (options == null)
                options = DefaultSerializerOptions.GenerateObjectJsonOptions;
            var newOptions = new JsonSerializerOptions(options)
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };
            
            var typeInfo = newOptions.GetTypeInfo(typeof(T));
            if (typeInfo == null)
                throw new InvalidOperationException("Unable to get type information for type T.");
            return JsonSerializer.Deserialize(Json, typeInfo) as T;
            
        }
        catch (JsonException)
        {
            return null;
        }
    }
}