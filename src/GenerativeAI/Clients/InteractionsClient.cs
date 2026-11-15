using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using GenerativeAI.Core;
using GenerativeAI.Logging;
using GenerativeAI.Types;
using GenerativeAI.Types.Converters;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Clients;

/// <summary>
/// A client for the Gemini Interactions API (<c>/interactions</c>): stateful model and agent interactions
/// (e.g. Deep Research, Antigravity) with server-side state, background execution, Server-Sent Event streaming,
/// stream resumption and continuation of long decodes. Works with the Gemini Developer API and Vertex AI.
/// </summary>
/// <seealso href="https://ai.google.dev/api/interactions">See Official API Documentation</seealso>
public class InteractionsClient : BaseClient
{
    private const string EventStreamMediaType = "text/event-stream";
    private const string DoneSentinel = "[DONE]";

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractionsClient"/> class.
    /// </summary>
    /// <param name="platform">The platform adapter (Google AI or Vertex AI).</param>
    /// <param name="httpClient">Optional HTTP client.</param>
    /// <param name="logger">Optional logger.</param>
    public InteractionsClient(IPlatformAdapter platform, HttpClient? httpClient = null, ILogger? logger = null)
        : base(platform, httpClient, logger)
    {
        // Resolve the Interactions types from their own source-generated context first (Native AOT friendly).
        SerializerOptions?.TypeInfoResolverChain.Insert(0, InteractionsSerializerContext.Default);
    }

    /// <summary>
    /// Gets the base URL of the interactions collection, e.g.
    /// <c>https://generativelanguage.googleapis.com/v1beta/interactions</c> or
    /// <c>https://{location}-aiplatform.googleapis.com/{version}/projects/{project}/locations/{location}/interactions</c>.
    /// </summary>
    /// <returns>The collection URL.</returns>
    protected string GetInteractionsUrl()
    {
        return $"{Platform.GetBaseUrl(appendPublisher: false)}/interactions";
    }

    /// <summary>
    /// Creates an interaction and returns it once the request completes (or, with
    /// <see cref="CreateInteractionRequest.Background"/>, as soon as it has been queued).
    /// </summary>
    /// <param name="request">The request. <see cref="CreateInteractionRequest.Stream"/> is ignored; use <see cref="StreamAsync(CreateInteractionRequest, CancellationToken)"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The interaction.</returns>
    /// <seealso href="https://ai.google.dev/api/interactions">See Official API Documentation</seealso>
    public async Task<Interaction> CreateAsync(CreateInteractionRequest request,
        CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(request);
#else
        if (request == null) throw new ArgumentNullException(nameof(request));
#endif
        var interaction = await CreateOnceAsync(request, cancellationToken).ConfigureAwait(false);

        // Automatic continuation, as in python-genai: keep resuming a long decode while the server asks for it,
        // and return one interaction holding the steps of every hop. Background interactions are left alone.
        var hops = 0;
        while (EnableAutomaticContinuation && request.Background != true && hops < MaxContinuations &&
               string.Equals(interaction.Status, InteractionStatuses.RequiresContinuation, StringComparison.Ordinal) &&
               !string.IsNullOrEmpty(interaction.ContinuationToken))
        {
            hops++;
            var next = await CreateOnceAsync(CreateContinuationRequest(interaction, request), cancellationToken)
                .ConfigureAwait(false);
            if (interaction.Steps is { Count: > 0 })
            {
                var steps = new List<InteractionStep>(interaction.Steps);
                if (next.Steps != null)
                    steps.AddRange(next.Steps);
                next.Steps = steps;
            }

            interaction = next;
        }

        return interaction;
    }

    /// <summary>
    /// Whether <see cref="CreateAsync(CreateInteractionRequest, CancellationToken)"/> automatically resumes an
    /// interaction whose status is <see cref="InteractionStatuses.RequiresContinuation"/> (default true, as in
    /// python-genai). The returned interaction is the last hop, with the steps of all hops.
    /// </summary>
    public bool EnableAutomaticContinuation { get; set; } = true;

    /// <summary>
    /// The maximum number of continuation requests per create call (default 10).
    /// </summary>
    public int MaxContinuations { get; set; } = 10;

    private async Task<Interaction> CreateOnceAsync(CreateInteractionRequest request,
        CancellationToken cancellationToken)
    {
        Validate(request);
        var body = request.Clone();
        body.Stream = null;
        using var response = await SendJsonAsync(HttpMethod.Post, GetInteractionsUrl(), body,
            HttpCompletionOption.ResponseContentRead, false, cancellationToken).ConfigureAwait(false);
        return await ReadInteractionAsync(response).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a model interaction from a model ID and an input.
    /// </summary>
    /// <param name="model">The model ID (see <see cref="InteractionModels"/>).</param>
    /// <param name="input">The input: a string, content blocks or steps.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The interaction.</returns>
    public Task<Interaction> CreateAsync(string model, InteractionInput input,
        CancellationToken cancellationToken = default)
    {
        return CreateAsync(new CreateInteractionRequest { Model = model, Input = input }, cancellationToken);
    }

    /// <summary>
    /// Creates an interaction and streams its Server-Sent Events (<c>interaction.created</c>,
    /// <c>step.start</c>, <c>step.delta</c>, <c>step.stop</c>, <c>interaction.status_update</c>,
    /// <c>interaction.completed</c>, <c>error</c>). Keep the last <see cref="InteractionSseEvent.EventId"/> to
    /// resume a dropped stream with <see cref="ResumeStreamAsync"/>.
    /// </summary>
    /// <param name="request">The request; <see cref="CreateInteractionRequest.Stream"/> is set to <c>true</c>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The stream of events.</returns>
    public async IAsyncEnumerable<InteractionSseEvent> StreamAsync(CreateInteractionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        Validate(request);
        var body = request.Clone();
        body.Stream = true;

        string? interactionId = null;
        string? lastEventId = null;
        var reconnects = 0;
        IAsyncEnumerable<InteractionSseEvent> source = CreateStreamAsync(body, cancellationToken);

        while (true)
        {
            Exception? failure = null;
            var enumerator = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (true)
                {
                    InteractionSseEvent item;
                    try
                    {
                        if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                            break;
                        item = enumerator.Current;
                    }
#pragma warning disable CA1031 // Swallowed only when the stream can be resumed; otherwise rethrown below
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        failure = ex;
                        break;
                    }
#pragma warning restore CA1031

                    if (item is InteractionCreatedEvent { Interaction.Id: { } id })
                        interactionId = id;
                    if (!string.IsNullOrEmpty(item.EventId))
                        lastEventId = item.EventId;
                    yield return item;
                }
            }
            finally
            {
                await enumerator.DisposeAsync().ConfigureAwait(false);
            }

            if (failure == null)
                yield break;

            // Resume a dropped stream from the last event, like the official SDKs' stream reconnection.
            if (interactionId == null || lastEventId == null || reconnects >= MaxStreamReconnects)
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();

            reconnects++;
            Logger?.LogWarning(failure, "Interaction stream for {InteractionId} failed; resuming after event {EventId}",
                interactionId, lastEventId);
            source = ResumeStreamAsync(interactionId!, lastEventId, cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// How many times <see cref="StreamAsync(CreateInteractionRequest, CancellationToken)"/> resumes a stream that
    /// fails after the interaction was created, using the last received event ID (default 3; 0 disables it).
    /// </summary>
    public int MaxStreamReconnects { get; set; } = 3;

    private async IAsyncEnumerable<InteractionSseEvent> CreateStreamAsync(CreateInteractionRequest body,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var response = await SendJsonAsync(HttpMethod.Post, GetInteractionsUrl(), body,
            HttpCompletionOption.ResponseHeadersRead, true, cancellationToken).ConfigureAwait(false);
        await foreach (var item in ReadEventsAsync(response, cancellationToken).ConfigureAwait(false))
            yield return item;
    }

    /// <summary>
    /// Retrieves an interaction by ID.
    /// </summary>
    /// <param name="id">The interaction ID (an <c>interactions/</c> prefix is accepted).</param>
    /// <param name="includeInput">If <c>true</c>, includes the input in the response.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The interaction.</returns>
    public async Task<Interaction> GetAsync(string id, bool? includeInput = null,
        CancellationToken cancellationToken = default)
    {
        var url = GetInteractionUrl(id) + BuildQuery(includeInput, null, null);
        using var response = await SendAsync(HttpMethod.Get, url, null, HttpCompletionOption.ResponseContentRead,
            false, cancellationToken).ConfigureAwait(false);
        return await ReadInteractionAsync(response).ConfigureAwait(false);
    }

    /// <summary>
    /// Streams the events of an existing interaction (<c>GET /interactions/{id}?stream=true</c>), e.g. a
    /// background interaction, or resumes a dropped stream after <paramref name="lastEventId"/>.
    /// </summary>
    /// <param name="id">The interaction ID.</param>
    /// <param name="lastEventId">If set, resumes the stream from the event after this event ID.</param>
    /// <param name="includeInput">If <c>true</c>, includes the input in the interaction.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The stream of events.</returns>
    public async IAsyncEnumerable<InteractionSseEvent> ResumeStreamAsync(string id, string? lastEventId = null,
        bool? includeInput = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var url = GetInteractionUrl(id) + BuildQuery(includeInput, lastEventId, true);
        using var response = await SendAsync(HttpMethod.Get, url, null, HttpCompletionOption.ResponseHeadersRead,
            true, cancellationToken).ConfigureAwait(false);
        await foreach (var item in ReadEventsAsync(response, cancellationToken).ConfigureAwait(false))
            yield return item;
    }

    /// <summary>
    /// Deletes an interaction.
    /// </summary>
    /// <param name="id">The interaction ID.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes when the interaction has been deleted.</returns>
    public new async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await base.DeleteAsync(GetInteractionUrl(id), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Cancels a running (background) interaction.
    /// </summary>
    /// <param name="id">The interaction ID.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The interaction after cancellation.</returns>
    public async Task<Interaction> CancelAsync(string id, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Post, GetInteractionUrl(id) + "/cancel", null,
            HttpCompletionOption.ResponseContentRead, false, cancellationToken).ConfigureAwait(false);
        return await ReadInteractionAsync(response).ConfigureAwait(false);
    }

    /// <summary>
    /// Polls an interaction (typically one created with <see cref="CreateInteractionRequest.Background"/>) until
    /// it is no longer <see cref="InteractionStatuses.InProgress"/> or <see cref="InteractionStatuses.Queued"/>.
    /// </summary>
    /// <param name="id">The interaction ID.</param>
    /// <param name="pollInterval">Delay between polls (default 10 seconds).</param>
    /// <param name="timeout">Optional overall timeout; a <see cref="TimeoutException"/> is thrown when exceeded.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The interaction in its final (or action/continuation-required) state.</returns>
    public async Task<Interaction> WaitForCompletionAsync(string id, TimeSpan? pollInterval = null,
        TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        var interval = pollInterval ?? TimeSpan.FromSeconds(10);
        var started = DateTime.UtcNow;
        while (true)
        {
            var interaction = await GetAsync(id, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (!InteractionStatuses.IsRunning(interaction.Status))
                return interaction;

            if (timeout.HasValue && DateTime.UtcNow - started + interval > timeout.Value)
                throw new TimeoutException(
                    $"Interaction '{id}' did not finish within {timeout.Value} (last status: {interaction.Status}).");

            await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Builds the request that continues an interaction whose status is
    /// <see cref="InteractionStatuses.RequiresContinuation"/>, following python-genai: the
    /// <see cref="Interaction.ContinuationToken"/> is passed back unchanged; when the original request was stored
    /// (the default) the interaction is referenced by <c>previous_interaction_id</c> and no input is sent,
    /// otherwise (<c>store=false</c>) the original input is sent again.
    /// </summary>
    /// <param name="interaction">The interaction to continue.</param>
    /// <param name="originalRequest">The request that created the interaction. When omitted, the model/agent of
    /// <paramref name="interaction"/> are used and the interaction is assumed to be stored.</param>
    /// <returns>The continuation request.</returns>
    public static CreateInteractionRequest CreateContinuationRequest(Interaction interaction,
        CreateInteractionRequest? originalRequest = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(interaction);
#else
        if (interaction == null) throw new ArgumentNullException(nameof(interaction));
#endif
        if (string.IsNullOrEmpty(interaction.ContinuationToken))
            throw new ArgumentException("The interaction has no continuation token.", nameof(interaction));

        var request = originalRequest?.Clone() ?? new CreateInteractionRequest
        {
            Model = interaction.Model,
            Agent = interaction.Agent,
            AgentConfig = interaction.AgentConfig
        };
        request.Stream = null;
        request.ContinuationToken = interaction.ContinuationToken;
        if (request.Store != false && !string.IsNullOrEmpty(interaction.Id))
        {
            request.PreviousInteractionId = interaction.Id;
            request.Input = null;
        }

        return request;
    }

    /// <summary>
    /// Continues an interaction whose status is <see cref="InteractionStatuses.RequiresContinuation"/> (see
    /// <see cref="CreateContinuationRequest"/>). Each continuation re-prefills and bills input tokens; background
    /// interactions are continued by the server.
    /// </summary>
    /// <param name="interaction">The interaction to continue.</param>
    /// <param name="originalRequest">The request that created the interaction, if available.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The new interaction produced by the continuation hop.</returns>
    public Task<Interaction> ContinueAsync(Interaction interaction, CreateInteractionRequest? originalRequest = null,
        CancellationToken cancellationToken = default)
    {
        return CreateAsync(CreateContinuationRequest(interaction, originalRequest), cancellationToken);
    }

    private static void Validate(CreateInteractionRequest request)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(request);
#else
        if (request == null) throw new ArgumentNullException(nameof(request));
#endif
        var hasModel = !string.IsNullOrEmpty(request.Model);
        var hasAgent = !string.IsNullOrEmpty(request.Agent);
        if (hasModel == hasAgent)
            throw new ArgumentException("Specify exactly one of 'Model' or 'Agent'.", nameof(request));
        if (hasAgent && request.GenerationConfig != null)
            throw new ArgumentException("'GenerationConfig' cannot be used with 'Agent'.", nameof(request));
        if (hasAgent && request.CachedContent != null)
            throw new ArgumentException("'CachedContent' cannot be used with 'Agent'.", nameof(request));
        if (hasModel && request.AgentConfig != null)
            throw new ArgumentException("'AgentConfig' cannot be used with 'Model'.", nameof(request));
    }

    private string GetInteractionUrl(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("The interaction ID is required.", nameof(id));
        const string prefix = "interactions/";
        if (id.StartsWith(prefix, StringComparison.Ordinal))
            id = id.Substring(prefix.Length);
        return $"{GetInteractionsUrl()}/{Uri.EscapeDataString(id)}";
    }

    private static string BuildQuery(bool? includeInput, string? lastEventId, bool? stream)
    {
        var query = new List<string>();
        if (includeInput.HasValue)
            query.Add("include_input=" + (includeInput.Value ? "true" : "false"));
        if (!string.IsNullOrEmpty(lastEventId))
            query.Add("last_event_id=" + Uri.EscapeDataString(lastEventId));
        if (stream.HasValue)
            query.Add("stream=" + (stream.Value ? "true" : "false"));
        return query.Count == 0 ? string.Empty : "?" + string.Join("&", query);
    }

    private Task<HttpResponseMessage> SendJsonAsync(HttpMethod method, string url, CreateInteractionRequest body,
        HttpCompletionOption completionOption, bool eventStream, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(body, InteractionJson.GetTypeInfo<CreateInteractionRequest>(Options));
        Logger?.LogHttpRequest(method.Method, url, json);
        return SendAsync(method, url, new StringContent(json, Encoding.UTF8, "application/json"), completionOption,
            eventStream, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, HttpContent? content,
        HttpCompletionOption completionOption, bool eventStream, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, url);
        request.Content = content;
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(eventStream
            ? EventStreamMediaType
            : "application/json"));
        await AddAuthorizationHeader(request, false, cancellationToken).ConfigureAwait(false);

        var response = await HttpClient.SendAsync(request, completionOption, cancellationToken).ConfigureAwait(false);
        try
        {
            await CheckAndHandleErrors(response, url).ConfigureAwait(false);
        }
        catch
        {
            response.Dispose();
            throw;
        }

        return response;
    }

    private JsonSerializerOptions Options =>
        SerializerOptions ?? throw new InvalidOperationException("SerializerOptions is not initialized");

    private async Task<Interaction> ReadInteractionAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var interaction = JsonSerializer.Deserialize(json, InteractionJson.GetTypeInfo<Interaction>(Options))
                          ?? throw new InvalidOperationException("The server returned an empty interaction.");
        NormalizeLegacyOutputs(interaction, Options);
        return interaction;
    }

    /// <summary>
    /// Some models (legacy Lyria) still return the removed <c>outputs</c> field instead of <c>steps</c>; like
    /// python-genai, expose them as a single <c>model_output</c> step.
    /// </summary>
    internal static void NormalizeLegacyOutputs(Interaction interaction, JsonSerializerOptions options)
    {
        if (interaction.Steps != null || interaction.ExtensionData == null ||
            !interaction.ExtensionData.TryGetValue("outputs", out var outputs) ||
            outputs.ValueKind != JsonValueKind.Array)
            return;

        interaction.ExtensionData.Remove("outputs");
        var contents = outputs.Deserialize(InteractionJson.GetTypeInfo<List<InteractionContent>>(options));
        interaction.Steps = contents is { Count: > 0 }
            ? new List<InteractionStep> { new InteractionModelOutputStep { Content = contents } }
            : new List<InteractionStep>();
    }

    private async IAsyncEnumerable<InteractionSseEvent> ReadEventsAsync(HttpResponseMessage response,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
#if NET5_0_OR_GREATER
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#else
        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
#endif
        var typeInfo = InteractionJson.GetTypeInfo<InteractionSseEvent>(Options);
        await foreach (var data in ServerSentEventReader.ReadDataAsync(stream, cancellationToken).ConfigureAwait(false))
        {
            if (data == DoneSentinel)
                yield break;

            var item = JsonSerializer.Deserialize(data, typeInfo);
            if (item is InteractionCreatedEvent { Interaction: { } created })
                NormalizeLegacyOutputs(created, Options);
            else if (item is InteractionCompletedEvent { Interaction: { } completed })
                NormalizeLegacyOutputs(completed, Options);
            if (item != null)
                yield return item;
        }
    }
}

/// <summary>
/// Minimal Server-Sent Events reader: yields the (joined) <c>data</c> of each event.
/// </summary>
internal static class ServerSentEventReader
{
    /// <summary>
    /// Reads the <c>data</c> payload of each event in <paramref name="stream"/>.
    /// </summary>
    internal static async IAsyncEnumerable<string> ReadDataAsync(Stream stream,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var data = new StringBuilder();
        var hasData = false;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
#if NET7_0_OR_GREATER
            var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
#else
            var line = await reader.ReadLineAsync().ConfigureAwait(false);
#endif
            if (line == null)
                break;

            if (line.Length == 0)
            {
                if (hasData)
                    yield return data.ToString();
                data.Clear();
                hasData = false;
                continue;
            }

            if (line[0] == ':')
                continue;

#if NETCOREAPP2_1_OR_GREATER
            var colon = line.IndexOf(':', StringComparison.Ordinal);
#else
            var colon = line.IndexOf(':');
#endif
            var field = colon < 0 ? line : line.Substring(0, colon);
            if (field != "data")
                continue;

            var value = colon < 0 ? string.Empty : line.Substring(colon + 1);
            if (value.Length > 0 && value[0] == ' ')
                value = value.Substring(1);
            if (hasData)
                data.Append('\n');
            data.Append(value);
            hasData = true;
        }

        if (hasData)
            yield return data.ToString();
    }
}
