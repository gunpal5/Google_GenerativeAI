using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Updates to the context of the current live session.
/// Only fields that are set will be updated. Updates are guaranteed to be processed in order with the rest of the inputs.
/// </summary>
public class LiveClientContextUpdate
{
    /// <summary>
    /// Updated system instruction for the model. If set, overrides <see cref="LiveClientSetup.SystemInstruction"/>.
    /// The system instructions are part of the model preamble, so updating them invalidates the prefix cache.
    /// Only update this field when strictly necessary as it might have a performance impact on model generation.
    /// </summary>
    [JsonPropertyName("systemInstruction")]
    public Content? SystemInstruction { get; set; }

    /// <summary>
    /// An updated list of tools the model may use to generate subsequent responses. If set, this list replaces the
    /// previously provided tools. The tools are part of the model preamble, so updating them invalidates the prefix cache.
    /// </summary>
    [JsonPropertyName("tools")]
    public ContextUpdateTools? Tools { get; set; }
}

/// <summary>
/// A wrapper around the list of tools in a <see cref="LiveClientContextUpdate"/>.
/// The wrapper distinguishes "no tools update" (wrapper unset) from "clear all tools" (wrapper set with an empty list).
/// </summary>
public class ContextUpdateTools
{
    /// <summary>
    /// The list of tools the model may use to generate the next response. May be empty to clear all tools.
    /// </summary>
    [JsonPropertyName("tools")]
    public Tool[]? Tools { get; set; }
}
