using System.Text.Json;

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
    public JsonBlock()
    {
        
    }

    /// <summary>
    /// Deserializes the JSON data of the current instance into an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The target type to which the JSON data will be deserialized. Must be a class.</typeparam>
    /// <returns>An instance of type <typeparamref name="T"/> if deserialization is successful, or null if an error occurs.</returns>
    public T? ToObject<T>() where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(Json, DefaultSerializerOptions.Options);
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}