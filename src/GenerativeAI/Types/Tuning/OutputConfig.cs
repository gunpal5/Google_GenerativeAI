using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Config for evaluation output during model tuning.
/// </summary>
public class OutputConfig
{
    /// <summary>
    /// Cloud storage destination for evaluation output.
    /// </summary>
    [JsonPropertyName("gcsDestination")]
    public GcsDestination? GcsDestination { get; set; }
}
