using System.Net;
using System.Text;
using System.Text.Json.Nodes;

namespace GenerativeAI.Tests.Clients;

/// <summary>
/// An offline <see cref="HttpMessageHandler"/> that records requests and replies with queued responses
/// (the last response is reused once the queue is exhausted).
/// </summary>
internal sealed class RecordingHttpHandler : HttpMessageHandler
{
    private readonly Queue<(HttpStatusCode Status, byte[] Body, string ContentType)> _responses = new();
    private (HttpStatusCode Status, byte[] Body, string ContentType) _last = (HttpStatusCode.OK, "{}"u8.ToArray(), "application/json");

    public List<RecordedRequest> Requests { get; } = new();

    public RecordingHttpHandler Reply(string json, HttpStatusCode status = HttpStatusCode.OK)
    {
        _responses.Enqueue((status, Encoding.UTF8.GetBytes(json), "application/json"));
        return this;
    }

    public RecordingHttpHandler ReplyBytes(byte[] body, string contentType = "application/octet-stream")
    {
        _responses.Enqueue((HttpStatusCode.OK, body, contentType));
        return this;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var body = request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        var headers = request.Headers.ToDictionary(h => h.Key, h => string.Join(",", h.Value),
            StringComparer.OrdinalIgnoreCase);
        Requests.Add(new RecordedRequest(request.Method, request.RequestUri!, headers,
            string.IsNullOrEmpty(body) ? null : JsonNode.Parse(body)));

        if (_responses.Count > 0)
            _last = _responses.Dequeue();
        var content = new ByteArrayContent(_last.Body);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(_last.ContentType);
        return new HttpResponseMessage(_last.Status) { Content = content };
    }
}

internal sealed record RecordedRequest(HttpMethod Method, Uri Url, Dictionary<string, string> Headers, JsonNode? Body);
