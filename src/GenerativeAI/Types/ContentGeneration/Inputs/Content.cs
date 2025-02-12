using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The base structured datatype containing multi-part content of a message.
/// A <see cref="Content"/> includes a <see cref="Role"/> field designating the producer of the <see cref="Content"/>
/// and a <see cref="Parts"/> field containing multi-part data that contains the content of the message turn.
/// </summary>
/// <seealso href="https://ai.google.dev/api/caching#Content"/>
public sealed class Content
{
    public Content(IEnumerable<Part> parts, string? role)
    {
        this.Parts = parts.ToList();
        this.Role = role;
    }

    /// <summary>
    /// Ordered <see cref="Parts"/> that constitute a single message. Parts may have different MIME types.
    /// </summary>
    [JsonPropertyName("parts")]
    public List<Part> Parts { get; set; } = new List<Part>();

    /// <summary>
    /// Optional. The producer of the content. Must be either 'user' or 'model'.
    /// Useful to set for multi-turn conversations, otherwise can be left blank or unset.
    /// </summary>
    [JsonPropertyName("role")]
    public string? Role { get; set; } 
}