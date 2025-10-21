using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Wrapper for batch request (Google AI / MLDev format).
/// </summary>
public class BatchWrapper
{
    /// <summary>
    /// The batch configuration.
    /// </summary>
    [JsonPropertyName("batch")]
    public BatchConfig? Batch { get; set; }
}

/// <summary>
/// Batch configuration containing input config.
/// </summary>
public class BatchConfig
{
    /// <summary>
    /// Input configuration for the batch job.
    /// </summary>
    [JsonPropertyName("inputConfig")]
    public BatchInputConfig? InputConfig { get; set; }

    /// <summary>
    /// Display name for the batch job.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }
}
