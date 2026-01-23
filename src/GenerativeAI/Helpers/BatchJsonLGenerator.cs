using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using GenerativeAI.Types;

namespace GenerativeAI.Helpers;

/// <summary>
/// Provides helper methods for generating JSONL (JSON Lines) content for batch processing.
/// This is the centralized helper for creating batch input files for both Vertex AI and Google AI.
/// </summary>
/// <remarks>
/// JSONL format consists of JSON objects separated by newlines, with no commas between lines.
/// Each line represents a complete JSON object suitable for batch API requests.
///
/// For Vertex AI: Use these methods to create JSONL files to upload to GCS.
/// For Google AI: Use <see cref="InlinedRequest"/> directly with the BatchClient.
/// </remarks>
public static class BatchJsonLGenerator
{
    #region Public API - Content Generation

    /// <summary>
    /// Generates JSONL content for batch content generation requests.
    /// This is the primary method for creating batch input files.
    /// </summary>
    /// <param name="prompts">List of prompt texts to process.</param>
    /// <param name="options">Optional batch options including schema, system instruction, etc.</param>
    /// <returns>A string containing properly formatted JSONL for batch prediction.</returns>
    /// <example>
    /// <code>
    /// // Simple prompts without schema
    /// var jsonl = BatchJsonLGenerator.GenerateContentJsonL(
    ///     new[] { "What is AI?", "Explain machine learning" });
    ///
    /// // With schema for structured output
    /// var jsonl = BatchJsonLGenerator.GenerateContentJsonL(
    ///     new[] { "Create profile for John, age 30" },
    ///     new BatchOptions { ResponseSchemaType = typeof(Person) });
    /// </code>
    /// </example>
    public static string GenerateContentJsonL(
        IEnumerable<string> prompts,
        BatchOptions? options = null)
    {
        var opts = options ?? new BatchOptions();
        var jsonOptions = GetJsonLOptions();
        var sb = new StringBuilder();

        // Pre-generate schema once if needed
        Schema? schema = null;
        if (opts.ResponseSchemaType != null)
        {
            schema = GoogleSchemaHelper.ConvertToSchema(opts.ResponseSchemaType, jsonOptions);
        }
        else if (opts.ResponseSchema != null)
        {
            schema = opts.ResponseSchema;
        }

        foreach (var prompt in prompts)
        {
            var request = BuildContentRequest(prompt, schema, opts.SystemInstruction);
            var wrapped = new Dictionary<string, object?> { ["request"] = request };
            var json = JsonSerializer.Serialize(wrapped, jsonOptions);
            sb.AppendLine(json);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Generates JSONL content for batch content generation with schema from a generic type.
    /// </summary>
    /// <typeparam name="TSchema">The C# type to generate response schema from.</typeparam>
    /// <param name="prompts">List of prompt texts to process.</param>
    /// <param name="systemInstruction">Optional system instruction for the requests.</param>
    /// <returns>A string containing properly formatted JSONL for batch prediction.</returns>
    /// <example>
    /// <code>
    /// var jsonl = BatchJsonLGenerator.GenerateContentJsonL&lt;Person&gt;(
    ///     new[] { "Create profile for John, age 30" });
    /// </code>
    /// </example>
    public static string GenerateContentJsonL<TSchema>(
        IEnumerable<string> prompts,
        Content? systemInstruction = null)
        where TSchema : class
    {
        return GenerateContentJsonL(prompts, new BatchOptions
        {
            ResponseSchemaType = typeof(TSchema),
            SystemInstruction = systemInstruction
        });
    }

    /// <summary>
    /// Generates JSONL content for batch content generation with file attachments.
    /// </summary>
    /// <param name="requests">List of batch request items with prompts and optional files.</param>
    /// <param name="options">Optional batch options including schema, system instruction, etc.</param>
    /// <returns>A string containing properly formatted JSONL for batch prediction.</returns>
    /// <example>
    /// <code>
    /// var requests = new[]
    /// {
    ///     new BatchRequestItem("Describe this image", "gs://bucket/image.jpg", "image/jpeg"),
    ///     new BatchRequestItem("Summarize this document", "gs://bucket/doc.pdf", "application/pdf")
    /// };
    /// var jsonl = BatchJsonLGenerator.GenerateContentWithFilesJsonL(requests);
    /// </code>
    /// </example>
    public static string GenerateContentWithFilesJsonL(
        IEnumerable<BatchRequestItem> requests,
        BatchOptions? options = null)
    {
        var opts = options ?? new BatchOptions();
        var jsonOptions = GetJsonLOptions();
        var sb = new StringBuilder();

        // Pre-generate schema once if needed
        Schema? schema = null;
        if (opts.ResponseSchemaType != null)
        {
            schema = GoogleSchemaHelper.ConvertToSchema(opts.ResponseSchemaType, jsonOptions);
        }
        else if (opts.ResponseSchema != null)
        {
            schema = opts.ResponseSchema;
        }

        foreach (var item in requests)
        {
            var request = BuildContentRequestWithFiles(item, schema, opts.SystemInstruction);
            var wrapped = new Dictionary<string, object?> { ["request"] = request };
            var json = JsonSerializer.Serialize(wrapped, jsonOptions);
            sb.AppendLine(json);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Generates JSONL content for batch content generation with file attachments and schema.
    /// </summary>
    /// <typeparam name="TSchema">The C# type to generate response schema from.</typeparam>
    /// <param name="requests">List of batch request items with prompts and optional files.</param>
    /// <param name="systemInstruction">Optional system instruction for the requests.</param>
    /// <returns>A string containing properly formatted JSONL for batch prediction.</returns>
    public static string GenerateContentWithFilesJsonL<TSchema>(
        IEnumerable<BatchRequestItem> requests,
        Content? systemInstruction = null)
        where TSchema : class
    {
        return GenerateContentWithFilesJsonL(requests, new BatchOptions
        {
            ResponseSchemaType = typeof(TSchema),
            SystemInstruction = systemInstruction
        });
    }

    /// <summary>
    /// Generates JSONL content from InlinedRequest objects.
    /// Use this when you have pre-built InlinedRequest objects with all settings configured.
    /// </summary>
    /// <param name="requests">List of InlinedRequest objects to serialize.</param>
    /// <returns>A string containing JSONL content with "request" wrapper format.</returns>
    public static string GenerateFromInlinedRequests(IEnumerable<InlinedRequest> requests)
    {
        var jsonOptions = GetJsonLOptions();
        var sb = new StringBuilder();

        foreach (var req in requests)
        {
            var requestObj = new Dictionary<string, object?>
            {
                ["contents"] = req.Contents
            };

            if (req.GenerationConfig != null)
                requestObj["generationConfig"] = req.GenerationConfig;

            if (req.SafetySettings != null && req.SafetySettings.Count > 0)
                requestObj["safetySettings"] = req.SafetySettings;

            if (req.Tools != null && req.Tools.Count > 0)
                requestObj["tools"] = req.Tools;

            if (req.ToolConfig != null)
                requestObj["toolConfig"] = req.ToolConfig;

            if (req.SystemInstruction != null)
                requestObj["systemInstruction"] = req.SystemInstruction;

            if (req.CachedContent != null)
                requestObj["cachedContent"] = req.CachedContent;

            var wrapped = new Dictionary<string, object?> { ["request"] = requestObj };
            var json = JsonSerializer.Serialize(wrapped, jsonOptions);
            sb.AppendLine(json);
        }

        return sb.ToString();
    }

    #endregion

    #region Public API - Embeddings

    /// <summary>
    /// Generates JSONL content for batch embedding requests.
    /// </summary>
    /// <param name="texts">List of text strings to generate embeddings for.</param>
    /// <returns>A string containing JSONL content for embeddings.</returns>
    /// <example>
    /// <code>
    /// var jsonl = BatchJsonLGenerator.GenerateEmbeddingJsonL(
    ///     new[] { "Hello world", "How are you?" });
    /// </code>
    /// </example>
    public static string GenerateEmbeddingJsonL(IEnumerable<string> texts)
    {
        var jsonOptions = GetJsonLOptions();
        var sb = new StringBuilder();

        foreach (var text in texts)
        {
            var contentObj = new Dictionary<string, object?>
            {
                ["parts"] = new[] { new Dictionary<string, string?> { ["text"] = text } }
            };

            var requestObj = new Dictionary<string, object?> { ["content"] = contentObj };
            var wrapped = new Dictionary<string, object?> { ["request"] = requestObj };

            var json = JsonSerializer.Serialize(wrapped, jsonOptions);
            sb.AppendLine(json);
        }

        return sb.ToString();
    }

    #endregion

    #region Public API - Generic

    /// <summary>
    /// Generates JSONL content from any list of objects.
    /// Each object is serialized as a complete JSON line.
    /// </summary>
    /// <typeparam name="T">The type of objects to serialize.</typeparam>
    /// <param name="items">List of objects to serialize as JSONL.</param>
    /// <returns>A string containing JSONL content.</returns>
    public static string GenerateJsonL<T>(IEnumerable<T> items) where T : class
    {
        var jsonOptions = GetJsonLOptions();
        var sb = new StringBuilder();

        foreach (var item in items)
        {
            var json = JsonSerializer.Serialize(item, jsonOptions);
            sb.AppendLine(json);
        }

        return sb.ToString();
    }

    #endregion

    #region Legacy Methods (for backward compatibility)

    /// <summary>
    /// Generates JSONL content for Vertex AI batch prediction with optional schema support.
    /// </summary>
    /// <param name="prompts">List of prompt texts to process.</param>
    /// <param name="responseSchemaType">Optional C# type to generate response schema from.</param>
    /// <param name="systemInstruction">Optional system instruction for the requests.</param>
    /// <returns>A string containing properly formatted JSONL for Vertex AI batch prediction.</returns>
    [Obsolete("Use GenerateContentJsonL with BatchOptions instead.")]
    public static string GenerateVertexBatchJsonL(
        IEnumerable<string> prompts,
        Type? responseSchemaType = null,
        Content? systemInstruction = null)
    {
        return GenerateContentJsonL(prompts, new BatchOptions
        {
            ResponseSchemaType = responseSchemaType,
            SystemInstruction = systemInstruction
        });
    }

    /// <summary>
    /// Generates JSONL content for Vertex AI batch prediction with schema from generic type.
    /// </summary>
    /// <typeparam name="TSchema">Optional C# type to generate response schema from.</typeparam>
    /// <param name="prompts">List of prompt texts to process.</param>
    /// <param name="systemInstruction">Optional system instruction for the requests.</param>
    /// <returns>A string containing properly formatted JSONL for Vertex AI batch prediction.</returns>
    [Obsolete("Use GenerateContentJsonL<TSchema> instead.")]
    public static string GenerateVertexBatchJsonL<TSchema>(
        IEnumerable<string> prompts,
        Content? systemInstruction = null)
        where TSchema : class
    {
        return GenerateContentJsonL<TSchema>(prompts, systemInstruction);
    }

    /// <summary>
    /// Generates JSONL content for content generation batch requests.
    /// </summary>
    [Obsolete("Use GenerateContentJsonL with BatchOptions instead.")]
    public static string GenerateContentJsonL(
        IEnumerable<string> prompts,
        string model,
        Type? responseSchemaType = null,
        Content? systemInstruction = null,
        JsonSerializerOptions? options = null)
    {
        return GenerateContentJsonL(prompts, new BatchOptions
        {
            ResponseSchemaType = responseSchemaType,
            SystemInstruction = systemInstruction
        });
    }

    /// <summary>
    /// Generates JSONL content for content generation batch requests with schema.
    /// </summary>
    [Obsolete("Use GenerateContentJsonL<TSchema> instead.")]
    public static string GenerateContentJsonL<TSchema>(
        IEnumerable<string> prompts,
        string model,
        Content? systemInstruction = null,
        JsonSerializerOptions? options = null)
        where TSchema : class
    {
        return GenerateContentJsonL<TSchema>(prompts, systemInstruction);
    }

    /// <summary>
    /// Generates JSONL content from InlinedRequest objects.
    /// </summary>
    [Obsolete("Use GenerateFromInlinedRequests instead.")]
    public static string GenerateInlinedRequestJsonL(
        IEnumerable<InlinedRequest> requests,
        JsonSerializerOptions? options = null)
    {
        return GenerateFromInlinedRequests(requests);
    }

    /// <summary>
    /// Generates JSONL content for embedding batch requests.
    /// </summary>
    [Obsolete("Use GenerateEmbeddingJsonL without model parameter instead.")]
    public static string GenerateEmbeddingJsonL(
        IEnumerable<string> texts,
        string model,
        JsonSerializerOptions? options = null)
    {
        return GenerateEmbeddingJsonL(texts);
    }

    /// <summary>
    /// Generates JSONL content with file attachments.
    /// </summary>
    [Obsolete("Use GenerateContentWithFilesJsonL with BatchRequestItem instead.")]
    public static string GenerateContentWithFilesJsonL(
        IEnumerable<(string prompt, (string bucket, string name, string mimeType) file)> promptsWithFiles,
        Type? responseSchemaType = null,
        Content? systemInstruction = null,
        JsonSerializerOptions? options = null)
    {
        var items = promptsWithFiles.Select(p => new BatchRequestItem(
            p.prompt,
            $"gs://{p.file.bucket}/{p.file.name}",
            p.file.mimeType));

        return GenerateContentWithFilesJsonL(items, new BatchOptions
        {
            ResponseSchemaType = responseSchemaType,
            SystemInstruction = systemInstruction
        });
    }

    /// <summary>
    /// Generates JSONL content with file attachments and schema.
    /// </summary>
    [Obsolete("Use GenerateContentWithFilesJsonL<TSchema> with BatchRequestItem instead.")]
    public static string GenerateContentWithFilesJsonL<TSchema>(
        IEnumerable<(string prompt, (string bucket, string name, string mimeType) file)> promptsWithFiles,
        Content? systemInstruction = null,
        JsonSerializerOptions? options = null)
        where TSchema : class
    {
        var items = promptsWithFiles.Select(p => new BatchRequestItem(
            p.prompt,
            $"gs://{p.file.bucket}/{p.file.name}",
            p.file.mimeType));

        return GenerateContentWithFilesJsonL<TSchema>(items, systemInstruction);
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Gets JsonSerializerOptions configured for JSONL generation with proper type resolution.
    /// </summary>
    private static JsonSerializerOptions GetJsonLOptions()
    {
        if (JsonSerializer.IsReflectionEnabledByDefault)
        {
#pragma warning disable IL2026, IL3050
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                WriteIndented = false, // JSONL requires single-line JSON
                Converters = { new JsonStringEnumConverter() },
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                TypeInfoResolver = TypesSerializerContext.Default,
            };

            foreach (var resolver in DefaultSerializerOptions.CustomJsonTypeResolvers)
            {
                if (!options.TypeInfoResolverChain.Contains(resolver))
                    options.TypeInfoResolverChain.Add(resolver);
            }
            options.TypeInfoResolverChain.Add(new DefaultJsonTypeInfoResolver());
#pragma warning restore IL2026, IL3050
            return options;
        }
        else
        {
            var options = new JsonSerializerOptions(GenerateObjectJsonContext.Default.Options)
            {
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                TypeInfoResolver = TypesSerializerContext.Default,
            };

            foreach (var resolver in DefaultSerializerOptions.CustomJsonTypeResolvers)
            {
                if (!options.TypeInfoResolverChain.Contains(resolver))
                    options.TypeInfoResolverChain.Add(resolver);
            }

            return options;
        }
    }

    private static Dictionary<string, object?> BuildContentRequest(
        string prompt,
        Schema? schema,
        Content? systemInstruction)
    {
        var request = new Dictionary<string, object?>
        {
            ["contents"] = new[]
            {
                new Dictionary<string, object?>
                {
                    ["role"] = "user",
                    ["parts"] = new[] { new Dictionary<string, string?> { ["text"] = prompt } }
                }
            }
        };

        if (schema != null)
        {
            request["generationConfig"] = new Dictionary<string, object?>
            {
                ["responseMimeType"] = "application/json",
                ["responseSchema"] = schema
            };
        }

        if (systemInstruction != null)
            request["systemInstruction"] = systemInstruction;

        return request;
    }

    private static Dictionary<string, object?> BuildContentRequestWithFiles(
        BatchRequestItem item,
        Schema? schema,
        Content? systemInstruction)
    {
        var parts = new List<Dictionary<string, object?>>
        {
            new() { ["text"] = item.Prompt }
        };

        if (!string.IsNullOrEmpty(item.FileUri))
        {
            parts.Add(new Dictionary<string, object?>
            {
                ["fileData"] = new Dictionary<string, string?>
                {
                    ["mimeType"] = item.MimeType,
                    ["fileUri"] = item.FileUri
                }
            });
        }

        var request = new Dictionary<string, object?>
        {
            ["contents"] = new[]
            {
                new Dictionary<string, object?>
                {
                    ["role"] = "user",
                    ["parts"] = parts
                }
            }
        };

        if (schema != null)
        {
            request["generationConfig"] = new Dictionary<string, object?>
            {
                ["responseMimeType"] = "application/json",
                ["responseSchema"] = schema
            };
        }

        if (systemInstruction != null)
            request["systemInstruction"] = systemInstruction;

        return request;
    }

    #endregion
}

/// <summary>
/// Options for configuring batch JSONL generation.
/// </summary>
public class BatchOptions
{
    /// <summary>
    /// Gets or sets the C# type to generate response schema from.
    /// When set, responses will be structured according to this type's schema.
    /// </summary>
    public Type? ResponseSchemaType { get; set; }

    /// <summary>
    /// Gets or sets a pre-built schema for structured output.
    /// Use this instead of ResponseSchemaType when you have a custom schema.
    /// </summary>
    public Schema? ResponseSchema { get; set; }

    /// <summary>
    /// Gets or sets the system instruction for all requests in the batch.
    /// </summary>
    public Content? SystemInstruction { get; set; }
}

/// <summary>
/// Represents a single batch request item with optional file attachment.
/// </summary>
public class BatchRequestItem
{
    /// <summary>
    /// Creates a new batch request item with just a prompt.
    /// </summary>
    /// <param name="prompt">The prompt text.</param>
    public BatchRequestItem(string prompt)
    {
        Prompt = prompt;
    }

    /// <summary>
    /// Creates a new batch request item with a prompt and file attachment.
    /// </summary>
    /// <param name="prompt">The prompt text.</param>
    /// <param name="fileUri">The GCS URI of the file (e.g., "gs://bucket/file.jpg").</param>
    /// <param name="mimeType">The MIME type of the file.</param>
    public BatchRequestItem(string prompt, string fileUri, string mimeType)
    {
        Prompt = prompt;
        FileUri = fileUri;
        MimeType = mimeType;
    }

    /// <summary>
    /// Gets or sets the prompt text.
    /// </summary>
    public string Prompt { get; set; }

    /// <summary>
    /// Gets or sets the GCS URI of the attached file (optional).
    /// </summary>
    public string? FileUri { get; set; }

    /// <summary>
    /// Gets or sets the MIME type of the attached file (optional).
    /// </summary>
    public string? MimeType { get; set; }
}
