using GenerativeAI.Types;

namespace GenerativeAI.Core;

/// <summary>
/// Follows continuation tokens for generateContent the way the official google-genai SDK does: when a response's
/// first candidate carries a continuation token and its finish reason is CONTINUATION (or missing, as for a stream
/// cut off mid-generation), the request is re-sent with that token and the hops are merged into one response.
/// </summary>
internal static class ContinuationHelper
{
    /// <summary>
    /// Returns the continuation token to resume with, or null if the response is complete.
    /// </summary>
    public static string? GetResumeToken(GenerateContentResponse? response)
    {
        var candidate = response?.Candidates is { Length: > 0 } candidates ? candidates[0] : null;
        if (candidate == null || string.IsNullOrEmpty(candidate.ContinuationToken))
            return null;

        return candidate.FinishReason is null or FinishReason.CONTINUATION ? candidate.ContinuationToken : null;
    }

    /// <summary>
    /// Merges a continuation hop into the response accumulated so far: candidate parts are appended, token counts
    /// are summed, and the finish reason, finish message and continuation token come from the latest hop.
    /// </summary>
    public static GenerateContentResponse Merge(GenerateContentResponse previous, GenerateContentResponse current)
    {
        previous.Candidates = MergeCandidates(previous.Candidates, current.Candidates);
        previous.UsageMetadata = MergeUsage(previous.UsageMetadata, current.UsageMetadata);
        previous.PromptFeedback = current.PromptFeedback ?? previous.PromptFeedback;
        previous.ModelVersion = current.ModelVersion ?? previous.ModelVersion;
        previous.ResponseId = current.ResponseId ?? previous.ResponseId;
        previous.ModelStatus = current.ModelStatus ?? previous.ModelStatus;
        previous.CreateTime ??= current.CreateTime;
        return previous;
    }

    private static Candidate[]? MergeCandidates(Candidate[]? previous, Candidate[]? current)
    {
        if (previous == null || previous.Length == 0)
            return current;
        if (current == null || current.Length == 0)
            return previous;

        var merged = previous.ToList();
        foreach (var next in current)
        {
            var index = merged.FindIndex(c => (c.Index ?? 0) == (next.Index ?? 0));
            if (index < 0)
            {
                merged.Add(next);
                continue;
            }

            var prior = merged[index];
            if (prior.Content == null)
            {
                prior.Content = next.Content;
            }
            else if (next.Content?.Parts != null)
            {
                AppendParts(prior.Content.Parts, next.Content.Parts);
                prior.Content.Role ??= next.Content.Role;
            }

            prior.FinishReason = next.FinishReason;
            prior.FinishMessage = next.FinishMessage;
            prior.ContinuationToken = next.ContinuationToken;
            prior.SafetyRatings = next.SafetyRatings ?? prior.SafetyRatings;
            prior.CitationMetadata = next.CitationMetadata ?? prior.CitationMetadata;
            prior.GroundingMetadata = next.GroundingMetadata ?? prior.GroundingMetadata;
            prior.UrlContextMetadata = next.UrlContextMetadata ?? prior.UrlContextMetadata;
            prior.LogprobsResult = next.LogprobsResult ?? prior.LogprobsResult;
            prior.AvgLogprobs = next.AvgLogprobs ?? prior.AvgLogprobs;
            if (prior.TokenCount != null || next.TokenCount != null)
                prior.TokenCount = (prior.TokenCount ?? 0) + (next.TokenCount ?? 0);
        }

        return merged.ToArray();
    }

    /// <summary>
    /// Appends the next hop's parts. A continuation resumes mid-output, so when the boundary parts are both plain
    /// text of the same kind (thought or not, no thought signature on the new part) they are joined into one part.
    /// </summary>
    private static void AppendParts(List<Part> target, List<Part> next)
    {
        foreach (var part in next)
        {
            var last = target.Count > 0 ? target[target.Count - 1] : null;
            if (last != null && IsPlainText(last) && IsPlainText(part) && last.Thought == part.Thought &&
                part.ThoughtSignature == null)
            {
                last.Text += part.Text;
                continue;
            }

            target.Add(part);
        }
    }

    private static bool IsPlainText(Part part) =>
        part.Text != null && part.InlineData == null && part.FileData == null && part.FunctionCall == null &&
        part.FunctionResponse == null && part.ExecutableCode == null && part.CodeExecutionResult == null;

    private static UsageMetadata? MergeUsage(UsageMetadata? previous, UsageMetadata? current)
    {
        if (previous == null)
            return current;
        if (current == null)
            return previous;

        previous.PromptTokenCount += current.PromptTokenCount;
        previous.CachedContentTokenCount += current.CachedContentTokenCount;
        previous.CandidatesTokenCount += current.CandidatesTokenCount;
        previous.ToolUsePromptTokenCount += current.ToolUsePromptTokenCount;
        previous.ThoughtsTokenCount += current.ThoughtsTokenCount;
        if (previous.ResponseTokenCount != null || current.ResponseTokenCount != null)
            previous.ResponseTokenCount = (previous.ResponseTokenCount ?? 0) + (current.ResponseTokenCount ?? 0);
        if (previous.TotalTokenCount != null || current.TotalTokenCount != null)
            previous.TotalTokenCount = (previous.TotalTokenCount ?? 0) + (current.TotalTokenCount ?? 0);
        previous.PromptTokensDetails = MergeModalities(previous.PromptTokensDetails, current.PromptTokensDetails);
        previous.CacheTokensDetails = MergeModalities(previous.CacheTokensDetails, current.CacheTokensDetails);
        previous.CandidatesTokensDetails = MergeModalities(previous.CandidatesTokensDetails, current.CandidatesTokensDetails);
        previous.ResponseTokensDetails = MergeModalities(previous.ResponseTokensDetails, current.ResponseTokensDetails);
        previous.ToolUsePromptTokensDetails = MergeModalities(previous.ToolUsePromptTokensDetails, current.ToolUsePromptTokensDetails);
        previous.TrafficType = current.TrafficType ?? previous.TrafficType;
        return previous;
    }

    private static List<ModalityTokenCount>? MergeModalities(List<ModalityTokenCount>? previous,
        List<ModalityTokenCount>? current)
    {
        if (previous == null)
            return current;
        if (current == null)
            return previous;

        foreach (var item in current)
        {
            var existing = previous.FirstOrDefault(p => p.Modality == item.Modality);
            if (existing == null)
                previous.Add(item);
            else
                existing.TokenCount += item.TokenCount;
        }

        return previous;
    }
}
