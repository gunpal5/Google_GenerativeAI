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
    /// <summary>
    /// Represents a piece of content, including parts and an optional role.
    /// </summary>
    public Content()
    {
        
    }

    /// <summary>
    /// Represents the structured multi-part content of a message, including the role of the producer.
    /// This type is utilized for handling and managing the content within message exchanges.
    /// </summary>
    public Content(IEnumerable<Part> parts, string? role)
    {
        this.Parts = parts.ToList();
        this.Role = role;
    }

    /// <summary>
    /// Represents the content of a message with multiple parts and a specific role.
    /// </summary>
    /// <remarks>
    /// This class encapsulates a multi-part message structure. Each piece of the message content is encapsulated
    /// within the <see cref="Parts"/> property as a collection of <see cref="Part"/> objects. The <see cref="Role"/>
    /// property designates the entity responsible for producing the content of the message. This content may include
    /// plain text, execution results, file data, and more.
    /// </remarks>
    public Content(string prompt, string? role)
    {
        this.Parts = new List<Part> { new Part(){Text = prompt} };
        this.Role = role;
    }

    /// <summary>
    /// Ordered <see cref="Part">Parts</see> that constitute a single message. Parts may have different MIME types.
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