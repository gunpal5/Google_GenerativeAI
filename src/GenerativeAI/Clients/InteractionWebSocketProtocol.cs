using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using GenerativeAI.Exceptions;
using GenerativeAI.Types;
using GenerativeAI.Types.Converters;

namespace GenerativeAI.Clients;

/// <summary>
/// [Experimental] The wire rules of live interactions (the Interactions API over WebSocket), as pure functions:
/// endpoint URL, handshake headers, client payload serialization and server frame parsing. Used by
/// <see cref="InteractionWebSocketConnection"/>; mirrors python-genai's <c>_interactions_websocket</c> module.
/// </summary>
public static class InteractionWebSocketProtocol
{
    /// <summary>The path of the live interactions endpoint, after the API version.</summary>
    public const string LiveInteractionsPath = "interactions/live";

    /// <summary>The API revision python-genai sends in the <c>api-revision</c> handshake header.</summary>
    public const string DefaultApiRevision = "2026-05-20";

    /// <summary>The prefix of ephemeral interaction auth tokens (see <see cref="InteractionAuthTokensClient"/>).</summary>
    public const string AuthTokenPrefix = "auth_tokens/";

    private const string ApiKeyHeader = "x-goog-api-key";
    private const string AuthorizationHeader = "Authorization";
    private const string ApiRevisionHeader = "api-revision";

    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    private static readonly Lazy<JsonSerializerOptions> LazyOptions = new(() =>
    {
        var options = DefaultSerializerOptions.Options;
        options.TypeInfoResolverChain.Insert(0, InteractionsSerializerContext.Default);
        options.WriteIndented = false;
        options.MakeReadOnly();
        return options;
    });

    /// <summary>
    /// Server event types after which a live interaction is terminal: the connection is closed and no more events
    /// are read (<c>interaction.completed</c>, <c>interaction.canceled</c>, <c>interaction.cancelled</c>,
    /// <c>reconnection_required</c>).
    /// </summary>
    public static IReadOnlyCollection<string> TerminalEventTypes { get; } = new[]
    {
        InteractionEventTypes.InteractionCompleted,
        "interaction.canceled",
        InteractionEventTypes.InteractionCancelled,
        "reconnection_required"
    };

    /// <summary>
    /// The JSON options used for live interaction frames: the Interactions source-generated context, compact output.
    /// </summary>
    public static JsonSerializerOptions SerializerOptions => LazyOptions.Value;

    /// <summary>
    /// Returns whether <paramref name="eventType"/> ends a live interaction (see <see cref="TerminalEventTypes"/>).
    /// </summary>
    /// <param name="eventType">The server event type.</param>
    /// <returns><c>true</c> for a terminal event.</returns>
    public static bool IsTerminalEventType(string? eventType) =>
        eventType != null && TerminalEventTypes.Contains(eventType, StringComparer.Ordinal);

    /// <summary>
    /// Builds the WebSocket URL from the HTTP base URL: <c>http</c>/<c>ws</c> become <c>ws</c>, anything else
    /// <c>wss</c>; the base path and query are kept, e.g.
    /// <c>wss://generativelanguage.googleapis.com/v1beta/interactions/live</c>.
    /// </summary>
    /// <param name="baseUrl">The HTTP base URL without the API version (e.g. <c>https://generativelanguage.googleapis.com</c>).</param>
    /// <param name="apiVersion">The API version (e.g. <c>v1beta</c>); omitted when empty.</param>
    /// <returns>The WebSocket URL.</returns>
    public static Uri BuildWebSocketUrl(string baseUrl, string? apiVersion)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentException("The base URL is required.", nameof(baseUrl));

        var uri = new Uri(baseUrl.TrimEnd('/'), UriKind.Absolute);
        var scheme = uri.Scheme is "http" or "ws" ? "ws" : "wss";
        var path = string.Join("/", new[] { uri.AbsolutePath, apiVersion ?? string.Empty, LiveInteractionsPath }
            .Select(part => part.Trim('/'))
            .Where(part => part.Length > 0));
        return new Uri($"{scheme}://{uri.Authority}/{path}{uri.Query}");
    }

    /// <summary>
    /// Builds the WebSocket handshake headers: <c>api-revision</c>, then the platform's authentication headers
    /// (<c>x-goog-api-key</c> and/or <c>Authorization</c>), then <paramref name="extraHeaders"/> (replacing headers of
    /// the same name, case-insensitively). When the API key is an ephemeral token (<c>auth_tokens/...</c>), any
    /// <c>Authorization</c> header is dropped and the token is sent as <c>x-goog-api-key</c>.
    /// </summary>
    /// <param name="authHeaders">The platform's authentication headers.</param>
    /// <param name="extraHeaders">Additional headers to set or replace.</param>
    /// <param name="apiRevision">The <c>api-revision</c> header value; omitted when <c>null</c>.</param>
    /// <returns>The headers, keyed case-insensitively.</returns>
    public static IDictionary<string, string> BuildHandshakeHeaders(
        IEnumerable<KeyValuePair<string, string>>? authHeaders,
        IEnumerable<KeyValuePair<string, string>>? extraHeaders,
        string? apiRevision = DefaultApiRevision)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (apiRevision != null)
            headers[ApiRevisionHeader] = apiRevision;

        string? apiKey = null;
        foreach (var header in authHeaders ?? Enumerable.Empty<KeyValuePair<string, string>>())
        {
            SetHeader(headers, header.Key, header.Value);
            if (string.Equals(header.Key, ApiKeyHeader, StringComparison.OrdinalIgnoreCase))
                apiKey = header.Value;
        }

        foreach (var header in extraHeaders ?? Enumerable.Empty<KeyValuePair<string, string>>())
            SetHeader(headers, header.Key, header.Value);

        if (apiKey != null && apiKey.StartsWith(AuthTokenPrefix, StringComparison.Ordinal))
        {
            headers.Remove(AuthorizationHeader);
            SetHeader(headers, ApiKeyHeader, apiKey);
        }

        return headers;
    }

    private static void SetHeader(Dictionary<string, string> headers, string name, string value)
    {
        headers.Remove(name);
        headers.Add(name, value);
    }

    /// <summary>
    /// Serializes a client payload (a single event or an <c>{"events": [...]}</c> list) to compact JSON.
    /// </summary>
    /// <param name="payload">The payload.</param>
    /// <param name="options">JSON options; defaults to <see cref="SerializerOptions"/>.</param>
    /// <returns>The JSON text of the frame.</returns>
    public static string SerializeClientPayload(InteractionClientEventBatch payload,
        JsonSerializerOptions? options = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(payload);
#else
        if (payload == null) throw new ArgumentNullException(nameof(payload));
#endif
        if (payload.Event == null && payload.EventList?.Events is not { Count: > 0 })
            throw new ArgumentException("Empty client event batch.", nameof(payload));
        if (payload.EventList?.Events != null && payload.EventList.Events.Any(e => e == null))
            throw new ArgumentException("Client event batches cannot contain null events.", nameof(payload));

        return JsonSerializer.Serialize(payload,
            InteractionJson.GetTypeInfo<InteractionClientEventBatch>(options ?? SerializerOptions));
    }

    /// <summary>
    /// Returns whether <paramref name="payload"/> contains an <c>interaction.cancel</c> event, which makes the
    /// connection terminal once sent.
    /// </summary>
    /// <param name="payload">The payload.</param>
    /// <returns><c>true</c> when the payload cancels the interaction.</returns>
    public static bool PayloadHasCancel(InteractionClientEventBatch payload)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(payload);
#else
        if (payload == null) throw new ArgumentNullException(nameof(payload));
#endif
        static bool IsCancel(InteractionClientEvent? e) =>
            e != null && string.Equals(e.EventType, InteractionClientEventTypes.InteractionCancel, StringComparison.Ordinal);

        return IsCancel(payload.Event) || (payload.EventList?.Events?.Any(IsCancel) ?? false);
    }

    /// <summary>
    /// Decodes a server frame (strict UTF-8) and parses its events in wire order. A frame is either one event or an
    /// <c>{"events": [...]}</c> list; every event must be an object with a non-empty <c>event_type</c> string.
    /// </summary>
    /// <param name="utf8Frame">The frame bytes.</param>
    /// <param name="options">JSON options; defaults to <see cref="SerializerOptions"/>.</param>
    /// <returns>The events (empty for an empty list).</returns>
    /// <exception cref="InteractionWebSocketException">The frame is not UTF-8 JSON or an event has no <c>event_type</c>.</exception>
    public static IReadOnlyList<InteractionSseEvent> ParseServerFrame(byte[] utf8Frame,
        JsonSerializerOptions? options = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(utf8Frame);
#else
        if (utf8Frame == null) throw new ArgumentNullException(nameof(utf8Frame));
#endif
        string text;
        try
        {
            text = StrictUtf8.GetString(utf8Frame);
        }
        catch (DecoderFallbackException)
        {
            throw new InteractionWebSocketException("Live interaction server frames must contain UTF-8 JSON text.");
        }

        return ParseServerFrame(text, options);
    }

    /// <summary>
    /// Parses the events of a server text frame in wire order. A frame is either one event or an
    /// <c>{"events": [...]}</c> list; every event must be an object with a non-empty <c>event_type</c> string.
    /// Events that do not match their schema are returned as <see cref="UnknownInteractionSseEvent"/>.
    /// </summary>
    /// <param name="frame">The frame text.</param>
    /// <param name="options">JSON options; defaults to <see cref="SerializerOptions"/>.</param>
    /// <returns>The events (empty for an empty list).</returns>
    /// <exception cref="InteractionWebSocketException">The frame is not JSON or an event has no <c>event_type</c>.</exception>
    public static IReadOnlyList<InteractionSseEvent> ParseServerFrame(string frame,
        JsonSerializerOptions? options = null)
    {
        JsonNode? payload;
        try
        {
            payload = JsonNode.Parse(frame ?? string.Empty);
        }
        catch (JsonException)
        {
            throw new InteractionWebSocketException("Live interaction server frames must contain valid JSON.");
        }

        var candidates = payload is JsonObject obj && obj.TryGetPropertyValue("events", out var list) &&
                         list is JsonArray array
            ? array.ToList()
            : new List<JsonNode?> { payload };

        foreach (var candidate in candidates)
        {
            if (string.IsNullOrEmpty(InteractionJson.GetString(candidate as JsonObject, "event_type")))
                throw new InteractionWebSocketException("Live interaction server events must have an event_type.");
        }

        var typeInfo = InteractionJson.GetTypeInfo<InteractionSseEvent>(options ?? SerializerOptions);
        var events = new List<InteractionSseEvent>(candidates.Count);
        foreach (var candidate in candidates)
        {
            var node = candidate!;
            InteractionSseEvent? item;
            try
            {
                item = node.Deserialize(typeInfo);
            }
            catch (JsonException)
            {
                // Like python-genai, events that do not validate are passed through rather than failing the stream.
                item = null;
            }

            events.Add(item ?? new UnknownInteractionSseEvent(InteractionJson.GetString(node, "event_type"),
                node.DeepClone()));
        }

        return events;
    }
}
