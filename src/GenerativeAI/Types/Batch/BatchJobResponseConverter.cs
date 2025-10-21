using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Custom JSON converter for BatchJobResponse that handles dynamic response parsing
/// based on the metadata type field.
/// </summary>
public class BatchJobResponseConverter : JsonConverter<BatchJobResponse>
{
    /// <summary>
    /// Reads and converts the JSON to a BatchJobResponse object.
    /// Handles both Google AI format (with "metadata" wrapper) and Vertex AI format (flat structure).
    /// </summary>
    public override BatchJobResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token");
        }

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var response = new BatchJobResponse();

        // Read top-level name
        if (root.TryGetProperty("name", out var nameElement))
        {
            response.Name = nameElement.GetString();
        }

        // Read done flag
        if (root.TryGetProperty("done", out var doneElement))
        {
            response.Done = doneElement.GetBoolean();
        }

        // Check if this is Google AI format (has "metadata" property) or Vertex AI format (flat structure)
        if (root.TryGetProperty("metadata", out var metadataElement))
        {
            // Google AI format - deserialize metadata
            response.Metadata = JsonSerializer.Deserialize(metadataElement, TypesSerializerContext.Default.BatchJob);

            // Set name from top-level if metadata exists
            if (response.Metadata != null && !string.IsNullOrEmpty(response.Name))
            {
                response.Metadata.Name = response.Name;
            }
        }
        else
        {
            // Vertex AI format - the entire root is the BatchJob
            response.Metadata = JsonSerializer.Deserialize(root, TypesSerializerContext.Default.BatchJob);
        }

        // Read response and deserialize based on metadata type
        if (root.TryGetProperty("response", out var responseElement))
        {
            var metadataType = response.Metadata?.Type ?? string.Empty;

            if (metadataType.IndexOf("EmbedContent", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                response.EmbeddingOutput = JsonSerializer.Deserialize(responseElement, TypesSerializerContext.Default.BatchEmbeddingOutput);
            }
            else if (metadataType.IndexOf("GenerateContent", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                response.ContentOutput = JsonSerializer.Deserialize(responseElement, TypesSerializerContext.Default.BatchContentOutput);
            }
        }

        return response;
    }

    /// <summary>
    /// Writes the BatchJobResponse object to JSON.
    /// </summary>
    public override void Write(Utf8JsonWriter writer, BatchJobResponse value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        if (!string.IsNullOrEmpty(value.Name))
        {
            writer.WriteString("name", value.Name);
        }

        if (value.Metadata != null)
        {
            writer.WritePropertyName("metadata");
            JsonSerializer.Serialize(writer, value.Metadata, TypesSerializerContext.Default.BatchJob);
        }

        if (value.Done.HasValue)
        {
            writer.WriteBoolean("done", value.Done.Value);
        }

        if (value.EmbeddingOutput != null)
        {
            writer.WritePropertyName("response");
            JsonSerializer.Serialize(writer, value.EmbeddingOutput, TypesSerializerContext.Default.BatchEmbeddingOutput);
        }
        else if (value.ContentOutput != null)
        {
            writer.WritePropertyName("response");
            JsonSerializer.Serialize(writer, value.ContentOutput, TypesSerializerContext.Default.BatchContentOutput);
        }

        writer.WriteEndObject();
    }
}
