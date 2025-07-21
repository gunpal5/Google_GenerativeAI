using System.Text.Json;
using System.Text.Json.Nodes;

namespace GenerativeAI;

/// <summary>
/// Provides extension methods for <see cref="JsonElement"/> objects.
/// </summary>
public static class JsonElementExtensions
{
    /// <summary>
    /// Converts a <see cref="JsonElement"/> to a <see cref="JsonNode"/>.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <returns>An equivalent node.</returns>
    /// <remarks>
    /// This provides a single point of conversion as one is not provided by .Net.
    /// See https://github.com/dotnet/runtime/issues/70427 for more information.
    /// </remarks>
    public static JsonNode? AsNode(this JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Array => JsonArray.Create(element),
        JsonValueKind.Object => JsonObject.Create(element),
        _ => JsonValue.Create(element),
    };
}