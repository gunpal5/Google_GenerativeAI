using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The response message for Operations.ListOperations.
/// </summary>
public class GoogleLongRunningListOperationsResponse
{
    /// <summary>
    /// The standard List next-page token.
    /// </summary>
    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; } 

    /// <summary>
    /// A list of operations that matches the specified filter in the request.
    /// </summary>
    [JsonPropertyName("operations")]
    public System.Collections.Generic.ICollection<GoogleLongRunningOperation>? Operations { get; set; } 
}