using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Identifier for a part within a <see cref="GroundingPassage">GroundingPassage</see>.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#GroundingPassageId">See Official API Documentation</seealso> 
public class GroundingPassageId
{
    /// <summary>
    /// Output only. ID of the passage matching the <see cref="GenerateAnswerRequest">GenerateAnswerRequest</see>'s <see cref="GroundingPassage.Id">GroundingPassage.Id</see>.
    /// </summary>
    [JsonPropertyName("passageId")]
    public string? PassageId { get; set; }

    /// <summary>
    /// Output only. Index of the part within the <see cref="GenerateAnswerRequest">GenerateAnswerRequest</see>'s <see cref="GroundingPassage.Id">GroundingPassage.content</see>.
    /// </summary>
    [JsonPropertyName("partIndex")]
    public int? PartIndex { get; set; }
}