using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Request body of <c>files:register</c>, which registers Google Cloud Storage files with the Gemini API
/// Files service.
/// </summary>
public class RegisterFilesRequest
{
    /// <summary>
    /// The Cloud Storage URIs to register, e.g. <c>gs://bucket/object</c>.
    /// </summary>
    [JsonPropertyName("uris")]
    public List<string>? Uris { get; set; }
}

/// <summary>
/// Response of <c>files:register</c>.
/// </summary>
public class RegisterFilesResponse
{
    /// <summary>
    /// The registered files.
    /// </summary>
    [JsonPropertyName("files")]
    public List<RemoteFile>? Files { get; set; }
}
