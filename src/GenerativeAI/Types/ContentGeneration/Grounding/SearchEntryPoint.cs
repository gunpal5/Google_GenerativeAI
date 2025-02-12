using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Google search entry point.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#SearchEntryPoint">See Official API Documentation</seealso> 
public class SearchEntryPoint
{
    /// <summary>
    /// Optional. Web content snippet that can be embedded in a web page or an app webview.
    /// </summary>
    [JsonPropertyName("renderedContent")]
    public string? RenderedContent { get; set; }

    /// <summary>
    /// Optional. Base64 encoded JSON representing an array of &lt;search term, search url&gt; tuples.
    /// A base64-encoded string.
    /// </summary>
    [JsonPropertyName("sdkBlob")]
    public string? SdkBlob { get; set; }
}