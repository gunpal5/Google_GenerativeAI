using GenerativeAI.Core;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Clients;

/// <summary>
/// A client for the Decisions API (<c>POST /decisions</c>): evaluates structured questions (yes/no "NOUL", choice
/// and score) against a shared multimodal input with a decision model such as
/// <see cref="DecisionModels.GeminiDecision1"/>, returning calibrated probabilities. Follows python-genai's
/// <c>client.decisions</c>.
/// </summary>
public class DecisionsClient : AgentApiClientBase
{
    private const string Collection = "decisions";

    /// <summary>
    /// Initializes a new instance of the <see cref="DecisionsClient"/> class.
    /// </summary>
    /// <param name="platform">The platform adapter (Google AI or Vertex AI).</param>
    /// <param name="httpClient">Optional HTTP client.</param>
    /// <param name="logger">Optional logger.</param>
    public DecisionsClient(IPlatformAdapter platform, HttpClient? httpClient = null, ILogger? logger = null)
        : base(platform, httpClient, logger)
    {
    }

    /// <summary>
    /// Evaluates the decision questions of <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request (model, input and at least one question).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The answers, keyed by question identifier.</returns>
    public Task<CreateDecisionResponse> CreateAsync(CreateDecisionRequest request,
        CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(request);
#else
        if (request == null) throw new ArgumentNullException(nameof(request));
#endif
        if (string.IsNullOrEmpty(request.Model))
            throw new ArgumentException("'Model' is required.", nameof(request));
        if (request.Input == null)
            throw new ArgumentException("'Input' is required.", nameof(request));
        if (request.Questions == null || request.Questions.Count == 0)
            throw new ArgumentException("At least one question is required.", nameof(request));

        return SendRequestAsync<CreateDecisionRequest, CreateDecisionResponse>(HttpMethod.Post,
            GetCollectionUrl(Collection), request, cancellationToken);
    }

    /// <summary>
    /// Evaluates decision questions against an input.
    /// </summary>
    /// <param name="model">The decision model (e.g. <see cref="DecisionModels.GeminiDecision1"/>).</param>
    /// <param name="input">The shared input: a string, a content block or content blocks.</param>
    /// <param name="questions">The questions, keyed by caller-defined identifiers.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The answers, keyed by question identifier.</returns>
    public Task<CreateDecisionResponse> CreateAsync(string model, InteractionInput input,
        IDictionary<string, DecisionQuestion> questions, CancellationToken cancellationToken = default)
    {
        return CreateAsync(new CreateDecisionRequest
        {
            Model = model,
            Input = input,
            Questions = questions == null ? null : new Dictionary<string, DecisionQuestion>(questions)
        }, cancellationToken);
    }
}
