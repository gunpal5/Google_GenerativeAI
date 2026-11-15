using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Project-level configuration of the Vertex AI RAG Engine for one location, including the mode of the
/// RAG-managed database (Serverless or Spanner).
/// </summary>
/// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/rag-engine/switching-modes">See Official API Documentation</seealso>
public class RagEngineConfig
{
    /// <summary>
    /// The resource name. Format: <c>projects/{project}/locations/{location}/ragEngineConfig</c>.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// The configuration of the RAG-managed database used by the RAG Engine.
    /// </summary>
    [JsonPropertyName("ragManagedDbConfig")]
    public RagManagedDbConfig? RagManagedDbConfig { get; set; }
}

/// <summary>
/// The configuration of the RAG-managed database. Set exactly one of <see cref="Serverless"/> or
/// <see cref="Spanner"/>; the top-level tier fields are deprecated.
/// </summary>
public class RagManagedDbConfig
{
    /// <summary>
    /// Serverless mode: no provisioned database, billed per use. Recommended for new projects.
    /// </summary>
    [JsonPropertyName("serverless")]
    public RagManagedDbServerless? Serverless { get; set; }

    /// <summary>
    /// Spanner mode: a provisioned Spanner database with the given tier.
    /// </summary>
    [JsonPropertyName("spanner")]
    public RagManagedDbSpanner? Spanner { get; set; }

    /// <summary>
    /// Deprecated. Use <see cref="Spanner"/> with <see cref="RagManagedDbSpanner.Scaled"/>.
    /// </summary>
    [JsonPropertyName("scaled")]
    public RagManagedDbScaled? Scaled { get; set; }

    /// <summary>
    /// Deprecated. Use <see cref="Spanner"/> with <see cref="RagManagedDbSpanner.Basic"/>.
    /// </summary>
    [JsonPropertyName("basic")]
    public RagManagedDbBasic? Basic { get; set; }

    /// <summary>
    /// Deprecated. Use <see cref="Spanner"/> with <see cref="RagManagedDbSpanner.Unprovisioned"/>.
    /// </summary>
    [JsonPropertyName("unprovisioned")]
    public RagManagedDbUnprovisioned? Unprovisioned { get; set; }

    /// <summary>Creates a configuration for Serverless mode.</summary>
    public static RagManagedDbConfig CreateServerless() => new() { Serverless = new RagManagedDbServerless() };

    /// <summary>Creates a configuration for Spanner mode with the Scaled tier.</summary>
    public static RagManagedDbConfig CreateSpannerScaled() =>
        new() { Spanner = new RagManagedDbSpanner { Scaled = new RagManagedDbScaled() } };

    /// <summary>Creates a configuration for Spanner mode with the Basic tier.</summary>
    public static RagManagedDbConfig CreateSpannerBasic() =>
        new() { Spanner = new RagManagedDbSpanner { Basic = new RagManagedDbBasic() } };
}

/// <summary>
/// Serverless mode of the RAG-managed database.
/// </summary>
public class RagManagedDbServerless
{
}

/// <summary>
/// Spanner mode of the RAG-managed database. Set at most one tier.
/// </summary>
public class RagManagedDbSpanner
{
    /// <summary>Production-grade performance with autoscaling.</summary>
    [JsonPropertyName("scaled")]
    public RagManagedDbScaled? Scaled { get; set; }

    /// <summary>Low-cost, low-compute tier.</summary>
    [JsonPropertyName("basic")]
    public RagManagedDbBasic? Basic { get; set; }

    /// <summary>
    /// Disables the RAG Engine and deletes all of its data, which cannot be recovered. Stops billing.
    /// </summary>
    [JsonPropertyName("unprovisioned")]
    public RagManagedDbUnprovisioned? Unprovisioned { get; set; }
}

/// <summary>The Scaled tier of the RAG-managed database.</summary>
public class RagManagedDbScaled
{
}

/// <summary>The Basic tier of the RAG-managed database.</summary>
public class RagManagedDbBasic
{
}

/// <summary>The Unprovisioned tier: disables the RAG Engine and deletes its data.</summary>
public class RagManagedDbUnprovisioned
{
}
