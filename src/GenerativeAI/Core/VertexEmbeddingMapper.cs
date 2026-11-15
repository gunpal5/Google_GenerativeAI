using GenerativeAI.Types;

namespace GenerativeAI.Core;

/// <summary>
/// Converts between the SDK's embedding types (Gemini API format) and the request/response formats
/// that Vertex AI uses, matching the official google-genai SDK:
/// Gemini (and model-as-a-service) embedding models use <c>embedContent</c> with options nested in
/// <c>embedContentConfig</c>; other text embedding models use <c>predict</c> with <c>instances</c>.
/// </summary>
internal static class VertexEmbeddingMapper
{
    /// <summary>
    /// The predict task name used by Vertex AI text embedding models.
    /// </summary>
    public const string PredictTask = "predict";

    /// <summary>
    /// True if the model uses the Vertex AI <c>embedContent</c> method rather than <c>predict</c>.
    /// </summary>
    public static bool UsesEmbedContent(string model)
    {
        return ContainsIgnoreCase(ModelIdOf(model), "gemini") || IsMaas(model);
    }

    /// <summary>
    /// True for Vertex AI model-as-a-service embedding models, which embed one content per request.
    /// </summary>
    public static bool IsMaas(string model) => ContainsIgnoreCase(ModelIdOf(model), "maas");

    private static string ModelIdOf(string model) => model.Split('/').Last();

    private static bool ContainsIgnoreCase(string value, string search)
    {
#if NET6_0_OR_GREATER
        return value.Contains(search, StringComparison.OrdinalIgnoreCase);
#else
        return value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
#endif
    }

    public static VertexEmbedContentRequest ToEmbedContentRequest(EmbedContentRequest request) => new()
    {
        Content = request.Content,
        EmbedContentConfig = new VertexEmbedContentConfig
        {
            TaskType = request.TaskType,
            Title = request.Title,
            OutputDimensionality = request.OutputDimensionality,
            AutoTruncate = request.AutoTruncate,
            DocumentOcr = request.DocumentOcr,
            AudioTrackExtraction = request.AudioTrackExtraction
        }
    };

    public static EmbedContentResponse ToEmbedContentResponse(VertexEmbedContentResponse response)
    {
        var embedding = response.Embedding;
        if (embedding != null && response.UsageMetadata != null)
        {
            embedding.Statistics ??= new ContentEmbeddingStatistics();
            embedding.Statistics.TokenCount ??= response.UsageMetadata.PromptTokenCount;
            embedding.Statistics.TokensDetails ??= response.UsageMetadata.PromptTokensDetails;
        }

        if (embedding != null && response.Truncated != null)
        {
            embedding.Statistics ??= new ContentEmbeddingStatistics();
            embedding.Statistics.Truncated ??= response.Truncated;
        }

        return new EmbedContentResponse { Embedding = embedding };
    }

    public static VertexEmbeddingPredictRequest ToPredictRequest(List<EmbedContentRequest> requests)
    {
        var first = requests[0];
        foreach (var request in requests)
        {
            if (request.OutputDimensionality != first.OutputDimensionality || request.AutoTruncate != first.AutoTruncate)
                throw new NotSupportedException(
                    "Vertex AI text embedding models apply OutputDimensionality and AutoTruncate to the whole batch; " +
                    "all requests in a batch must use the same values.");
            if (request.DocumentOcr != null || request.AudioTrackExtraction != null)
                throw new NotSupportedException(
                    "DocumentOcr and AudioTrackExtraction are only supported by Gemini embedding models.");
        }

        return new VertexEmbeddingPredictRequest
        {
            Instances = requests.Select(r => new VertexEmbeddingInstance
            {
                Content = TextOf(r.Content),
                TaskType = r.TaskType,
                Title = r.Title,
                MimeType = r.MimeType
            }).ToList(),
            Parameters = first.OutputDimensionality == null && first.AutoTruncate == null
                ? null
                : new VertexEmbeddingParameters
                {
                    OutputDimensionality = first.OutputDimensionality,
                    AutoTruncate = first.AutoTruncate
                }
        };
    }

    public static List<ContentEmbedding> FromPredictResponse(VertexEmbeddingPredictResponse response) =>
        (response.Predictions ?? new List<VertexEmbeddingPrediction>())
        .Select(p => new ContentEmbedding
        {
            Values = p.Embeddings?.Values,
            Statistics = p.Embeddings?.Statistics == null
                ? null
                : new ContentEmbeddingStatistics
                {
                    TokenCount = p.Embeddings.Statistics.TokenCount,
                    Truncated = p.Embeddings.Statistics.Truncated
                }
        })
        .ToList();

    private static string TextOf(Content? content)
    {
        if (content?.Parts == null || content.Parts.Count == 0)
            throw new ArgumentException("The content to embed is empty.", nameof(content));

        if (content.Parts.Any(p => p.Text == null))
            throw new NotSupportedException(
                "Vertex AI text embedding models only embed text. Use a Gemini embedding model for other content.");

        return string.Join("\n", content.Parts.Select(p => p.Text));
    }
}
