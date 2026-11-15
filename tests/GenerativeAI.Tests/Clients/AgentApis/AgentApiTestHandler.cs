using System.Net;
using System.Text;

namespace GenerativeAI.Tests.Clients.AgentApis;

/// <summary>A request captured by <see cref="AgentApiTestHandler"/>.</summary>
internal sealed record CapturedAgentApiRequest(
    HttpMethod Method,
    Uri Url,
    string? Body,
    Dictionary<string, string> Headers,
    string? ContentType);

/// <summary>
/// An <see cref="HttpMessageHandler"/> that records requests and replies with queued JSON responses.
/// </summary>
internal sealed class AgentApiTestHandler : HttpMessageHandler
{
    private readonly Queue<(string Body, Dictionary<string, string>? Headers, string MediaType)> _responses = new();

    public List<CapturedAgentApiRequest> Requests { get; } = new();

    public AgentApiTestHandler Enqueue(string body, Dictionary<string, string>? headers = null,
        string mediaType = "application/json")
    {
        _responses.Enqueue((body, headers, mediaType));
        return this;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var body = request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        var headers = request.Headers.ToDictionary(h => h.Key, h => string.Join(",", h.Value),
            StringComparer.OrdinalIgnoreCase);
        Requests.Add(new CapturedAgentApiRequest(request.Method, request.RequestUri!, body, headers,
            request.Content?.Headers.ContentType?.MediaType));

        var (responseBody, responseHeaders, mediaType) =
            _responses.Count > 0 ? _responses.Dequeue() : ("{}", null, "application/json");
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseBody, Encoding.UTF8, mediaType)
        };
        if (responseHeaders != null)
            foreach (var header in responseHeaders)
                response.Headers.TryAddWithoutValidation(header.Key, header.Value);
        return response;
    }

    public static GoogleAIPlatformAdapter GooglePlatform() => new("test-key");

    public static VertextPlatformAdapter VertexPlatform() =>
        new("test-project", "us-central1", accessToken: "test-token", validateAccessToken: false);
}
