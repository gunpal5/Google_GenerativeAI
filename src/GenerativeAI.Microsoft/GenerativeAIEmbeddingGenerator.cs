using System.Runtime.CompilerServices;
using GenerativeAI.Core;
using GenerativeAI.Exceptions;
using GenerativeAI.Microsoft.Extensions;
using GenerativeAI.Types;
using Microsoft.Extensions.AI;

namespace GenerativeAI.Microsoft;

/// <summary>
/// Provides embedding generation capabilities using Google's Generative AI models through the Microsoft.Extensions.AI abstraction.
/// </summary>
public sealed class GenerativeAIEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    /// <summary>
    /// Gets the underlying EmbeddingModel instance.
    /// </summary>
    public EmbeddingModel Model { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GenerativeAIEmbeddingGenerator"/> class.
    /// </summary>
    /// <param name="apiKey">The API key for authenticating with Google AI.</param>
    /// <param name="modelName">The name of the embedding model to use. Defaults to "text-embedding-004".</param>
    public GenerativeAIEmbeddingGenerator(string apiKey, string modelName = GoogleAIModels.GeminiEmbedding)
    {
        Model = new EmbeddingModel(apiKey, modelName);
        Metadata = new EmbeddingGeneratorMetadata(modelName, new Uri("https://ai.google.dev/"));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GenerativeAIEmbeddingGenerator"/> class.
    /// </summary>
    /// <param name="adapter">The platform adapter to use for API calls.</param>
    /// <param name="modelName">The name of the embedding model to use. Defaults to "text-embedding-004".</param>
    public GenerativeAIEmbeddingGenerator(IPlatformAdapter adapter, string modelName = GoogleAIModels.GeminiEmbedding)
    {
        Model = new EmbeddingModel(adapter, modelName);
        Metadata = new EmbeddingGeneratorMetadata(modelName, new Uri("https://ai.google.dev/"));
    }

    /// <inheritdoc/>
    public EmbeddingGeneratorMetadata Metadata { get; }

    /// <inheritdoc/>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(values);
#else
        if (values == null) throw new ArgumentNullException(nameof(values));
#endif

        var valuesList = values.ToList();
        if (valuesList.Count == 0)
        {
            return new GeneratedEmbeddings<Embedding<float>>([]);
        }

        try
        {
            // Create batch embed request
            var requests = valuesList.Select(text => new EmbedContentRequest
            {
                Content = new Content
                {
                    Role = Roles.User,
                    Parts = [new Part { Text = text }]
                },
                Model = Model.Model,
                TaskType = GetTaskType(options),
                OutputDimensionality = options?.Dimensions
            });

            // Call the batch embed API
            var response = await Model.BatchEmbedContentAsync(requests, cancellationToken).ConfigureAwait(false);

            // Convert response to Microsoft.Extensions.AI format
            var embeddings = new List<Embedding<float>>();
            
            if (response?.Embeddings != null)
            {
                foreach (var embedding in response.Embeddings)
                {
                    if (embedding?.Values != null)
                    {
                        embeddings.Add(new Embedding<float>(embedding.Values.ToArray()));
                    }
                }
            }

            // Create usage data if available
            UsageDetails? usage = null;
            if (response != null)
            {
                usage = new UsageDetails
                {
                    InputTokenCount = valuesList.Sum(v => EstimateTokenCount(v)),
                    TotalTokenCount = valuesList.Sum(v => EstimateTokenCount(v))
                };
            }

            return new GeneratedEmbeddings<Embedding<float>>(embeddings)
            {
                Usage = usage
            };
        }
        catch (Exception ex)
        {
            throw new GenerativeAIException(
                "Failed to generate embeddings",
                $"An error occurred while generating embeddings: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        if (serviceKey == null && serviceType?.IsInstanceOfType(this) == true)
        {
            return this;
        }
        return null;
    }

    private static TaskType GetTaskType(EmbeddingGenerationOptions? options)
    {
        if (options?.AdditionalProperties?.TryGetValue("TaskType", out var taskTypeObj) == true)
        {
            if (taskTypeObj is TaskType taskType)
                return taskType;
            
            if (taskTypeObj is string taskTypeStr && Enum.TryParse<TaskType>(taskTypeStr, true, out var parsedTaskType))
                return parsedTaskType;
        }

        // Default task type
        return TaskType.RETRIEVAL_DOCUMENT;
    }


    private static int EstimateTokenCount(string text)
    {
        // Simple token estimation: ~4 characters per token
        return Math.Max(1, text.Length / 4);
    }
}