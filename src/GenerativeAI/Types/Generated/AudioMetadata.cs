using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Metadata describes the input audio content. Set this in `Part.audio_metadata` or
/// `FunctionResponsePart.audio_metadata` when `inline_data` or `file_data` contains audio to configure static
/// audio preprocessing.
/// </summary>
/// <remarks>Generated from python-genai by scripts/drift/update_dtos.py.</remarks>
public class AudioMetadata
{
    /// <summary>
    /// The end offset of the audio, relative to the start of the audio stream.
    /// </summary>
    [JsonPropertyName("endOffset")]
    public string? EndOffset { get; set; }

    /// <summary>
    /// The start offset of the audio, relative to the start of the audio stream.
    /// </summary>
    [JsonPropertyName("startOffset")]
    public string? StartOffset { get; set; }
}
