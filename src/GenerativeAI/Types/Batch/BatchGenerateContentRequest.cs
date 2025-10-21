using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Request for batch content generation.
/// </summary>
/// <seealso href="https://ai.google.dev/api/batch#method:-models.batchgeneratecontent">See Official API Documentation</seealso>
public class BatchGenerateContentRequest
{
    /// <summary>
    /// Required. The source configuration specifying input data location (GCS, BigQuery, or inlined requests).
    /// </summary>
    [JsonPropertyName("src")]
    public BatchJobSource Src { get; set; } = null!;

    /// <summary>
    /// Optional. The destination configuration for output data.
    /// </summary>
    [JsonPropertyName("dest")]
    public BatchJobDestination? Dest { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchGenerateContentRequest"/> class.
    /// </summary>
    public BatchGenerateContentRequest()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchGenerateContentRequest"/> class with specified source and destination.
    /// </summary>
    /// <param name="src">The source configuration.</param>
    /// <param name="dest">The optional destination configuration.</param>
    public BatchGenerateContentRequest(BatchJobSource src, BatchJobDestination? dest = null)
    {
        Src = src;
        Dest = dest;
    }
}
