using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The fields to update on a (tuned) model, mirroring <c>UpdateModelConfig</c> of the google-genai SDK.
/// Only the fields that are set are sent.
/// </summary>
public class UpdateModelConfig
{
    /// <summary>
    /// The new display name of the model.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// The new description of the model.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// The checkpoint id to use as the model's default checkpoint.
    /// </summary>
    [JsonPropertyName("defaultCheckpointId")]
    public string? DefaultCheckpointId { get; set; }
}

/// <summary>
/// Request body of the Vertex AI <c>computeTokens</c> method.
/// </summary>
public class ComputeTokensRequest
{
    /// <summary>
    /// The contents to tokenize.
    /// </summary>
    [JsonPropertyName("contents")]
    public List<Content>? Contents { get; set; }
}

/// <summary>
/// Response of the Vertex AI <c>computeTokens</c> method.
/// </summary>
public class ComputeTokensResponse
{
    /// <summary>
    /// The tokens info for each input content.
    /// </summary>
    [JsonPropertyName("tokensInfo")]
    public List<TokensInfo>? TokensInfo { get; set; }
}

/// <summary>
/// Tokens info with a list of tokens and the corresponding list of token ids.
/// </summary>
public class TokensInfo
{
    /// <summary>
    /// Optional. The role of the corresponding content.
    /// </summary>
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    /// <summary>
    /// A list of token ids from the input.
    /// </summary>
    [JsonPropertyName("tokenIds")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public List<long>? TokenIds { get; set; }

    /// <summary>
    /// A list of tokens from the input (raw bytes; base64 encoded on the wire).
    /// </summary>
    [JsonPropertyName("tokens")]
    public List<byte[]>? Tokens { get; set; }
}
