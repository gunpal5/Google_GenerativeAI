using System.Text.Json;
using GenerativeAI.Types;

namespace GenerativeAI;

/// <summary>
/// Provides extension methods for working with objects in the GenerativeAI library.
/// Includes utilities for converting objects into schema representations.
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Converts an object to a <see cref="Schema"/> representation.
    /// This method evaluates the properties and structure of the provided object
    /// and generates a corresponding schema representation.
    /// </summary>
    /// <param name="obj">The object to be converted into a schema representation.</param>
    /// <param name="options">Optional JSON serializer options for customizing the schema generation.</param>
    /// <returns>A <see cref="Schema"/> instance that represents the structure of the provided object.</returns>
    public static Schema ToSchema(this object obj, JsonSerializerOptions? options = null)
    {
        return Schema.FromObject(obj, options);
    }
}