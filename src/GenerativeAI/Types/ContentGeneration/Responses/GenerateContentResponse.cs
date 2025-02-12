using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Response from the model supporting multiple candidate responses.
/// Safety ratings and content filtering are reported for both prompt in
/// <see cref="PromptFeedback"/> and for each candidate in <see cref="FinishReason"/>
/// and in <see cref="Candidate.SafetyRatings"/>. The API:
/// - Returns either all requested candidates or none of them
/// - Returns no candidates at all only if there was something wrong with the prompt
///   (check <see cref="PromptFeedback"/>)
/// - Reports feedback on each candidate in <see cref="Candidate.FinishReason"/> and
///   <see cref="Candidate.SafetyRatings"/>.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#generatecontentresponse">See Official API Documentation</seealso>
public class GenerateContentResponse
{
    
    /// <summary>
    /// Candidate responses from the model.
    /// </summary>
    [JsonPropertyName("candidates")]
    public Candidate[]? Candidates { get; set; }

    /// <summary>
    /// Returns the prompt's feedback related to the content filters.
    /// </summary>
    [JsonPropertyName("promptFeedback")]
    public PromptFeedback? PromptFeedback { get; set; }

    /// <summary>
    /// Output only. Metadata on the generation requests' token usage.
    /// </summary>
    [JsonPropertyName("usageMetadata")]
    public UsageMetadata? UsageMetadata { get; set; }

    /// <summary>
    /// Output only. The model version used to generate the response.
    /// </summary>
    [JsonPropertyName("modelVersion")]
    public string? ModelVersion { get; set; }

    /// <summary>
    /// Converts the GenerateContentResponse instance to its string representation.
    /// </summary>
    /// <returns>A formatted string displaying the properties of the object.</returns>
    public override string ToString()
    {
        var text = this.Text();
        if (string.IsNullOrEmpty(text))
        {
            var candidatesStr = Candidates is not null
                ? string.Join(", ", Candidates.Select(c => c.ToString()))
                : "null";
            var feedbackStr = PromptFeedback?.ToString() ?? "null";
            var metadataStr = UsageMetadata?.ToString() ?? "null";
            var versionStr = ModelVersion ?? "null";

            return
                $"GenerateContentResponse {{ Candidates = [{candidatesStr}], PromptFeedback = {feedbackStr}, UsageMetadata = {metadataStr}, ModelVersion = {versionStr} }}";
        }
        return text;
    }

    /// <summary>
    /// Converts the GenerateContentResponse instance to its string representation explicitly.
    /// </summary>
    /// <param name="response">The GenerateContentResponse instance to convert.</param>
    /// <returns>A string that represents the content of the GenerateContentResponse instance.</returns>
    public static explicit operator string(GenerateContentResponse response)
    {
        return response.Text() ?? string.Empty;
    }
}