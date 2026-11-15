using System.Globalization;
using GenerativeAI.Core;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Clients;

/// <summary>
/// A client for the Voices API (<c>/voices</c>): design voices from a prompt, remix an existing voice, replicate a
/// voice from recorded audio, and list custom and prebuilt voices for text-to-speech. Follows python-genai's
/// <c>client.voices</c>.
/// </summary>
public class VoicesClient : AgentApiClientBase
{
    private const string Collection = "voices";

    /// <summary>
    /// Initializes a new instance of the <see cref="VoicesClient"/> class.
    /// </summary>
    /// <param name="platform">The platform adapter (Google AI or Vertex AI).</param>
    /// <param name="httpClient">Optional HTTP client.</param>
    /// <param name="logger">Optional logger.</param>
    public VoicesClient(IPlatformAdapter platform, HttpClient? httpClient = null, ILogger? logger = null)
        : base(platform, httpClient, logger)
    {
    }

    /// <summary>
    /// Creates a voice (<c>POST /voices</c>).
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The created voice.</returns>
    public Task<Voice> CreateAsync(CreateVoiceRequest request, CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(request);
#else
        if (request == null) throw new ArgumentNullException(nameof(request));
#endif
        if (request.Voice == null)
            throw new ArgumentException("'Voice' is required.", nameof(request));
        if (string.IsNullOrEmpty(request.Voice.Type))
            throw new ArgumentException("'Voice.Type' is required.", nameof(request));

        return SendRequestAsync<CreateVoiceRequest, Voice>(HttpMethod.Post, GetCollectionUrl(Collection), request,
            cancellationToken);
    }

    /// <summary>
    /// Creates a voice (<c>POST /voices</c>).
    /// </summary>
    /// <param name="voice">The voice to create.</param>
    /// <param name="store">Whether to store the voice for reuse.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The created voice.</returns>
    public Task<Voice> CreateAsync(Voice voice, bool? store = null, CancellationToken cancellationToken = default)
    {
        return CreateAsync(new CreateVoiceRequest { Voice = voice, Store = store }, cancellationToken);
    }

    /// <summary>
    /// Designs a voice from a text description (a <c>prompted</c> voice).
    /// </summary>
    /// <param name="prompt">The description of the voice.</param>
    /// <param name="displayName">Optional display name.</param>
    /// <param name="store">Whether to store the voice for reuse.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The designed voice.</returns>
    public Task<Voice> DesignAsync(string prompt, string? displayName = null, bool? store = null,
        CancellationToken cancellationToken = default)
    {
        return CreateAsync(new Voice
        {
            Type = VoiceTypes.Prompted,
            DisplayName = displayName,
            Prompted = new PromptedVoice { Input = prompt }
        }, store, cancellationToken);
    }

    /// <summary>
    /// Remixes an existing voice according to instructions (a <c>prompted</c> voice with a base voice).
    /// </summary>
    /// <param name="baseVoice">The voice to remix.</param>
    /// <param name="instructions">How to change the voice.</param>
    /// <param name="displayName">Optional display name.</param>
    /// <param name="store">Whether to store the voice for reuse.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The remixed voice.</returns>
    public Task<Voice> RemixAsync(string baseVoice, string instructions, string? displayName = null,
        bool? store = null, CancellationToken cancellationToken = default)
    {
        return CreateAsync(new Voice
        {
            Type = VoiceTypes.Prompted,
            DisplayName = displayName,
            Prompted = new PromptedVoice { Input = instructions, BaseVoice = baseVoice }
        }, store, cancellationToken);
    }

    /// <summary>
    /// Replicates a voice from recorded audio (a <c>replicated</c> voice).
    /// </summary>
    /// <param name="sourceAudio">Audio of the voice to replicate.</param>
    /// <param name="consentAudio">Audio of the speaker consenting to the replication.</param>
    /// <param name="displayName">Optional display name.</param>
    /// <param name="store">Whether to store the voice for reuse.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The replicated voice.</returns>
    public Task<Voice> ReplicateAsync(VoiceAudioData sourceAudio, VoiceAudioData consentAudio,
        string? displayName = null, bool? store = null, CancellationToken cancellationToken = default)
    {
        return CreateAsync(new Voice
        {
            Type = VoiceTypes.Replicated,
            DisplayName = displayName,
            Replicated = new ReplicatedVoice { SourceAudio = sourceAudio, ConsentAudio = consentAudio }
        }, store, cancellationToken);
    }

    /// <summary>
    /// Gets a voice (<c>GET /voices/{id}</c>).
    /// </summary>
    /// <param name="id">The voice ID (a <c>voices/</c> prefix is accepted).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The voice.</returns>
    public Task<Voice> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        return SendRequestAsync<Voice>(HttpMethod.Get, GetResourceUrl(Collection, id), cancellationToken);
    }

    /// <summary>
    /// Deletes a custom voice (<c>DELETE /voices/{id}</c>).
    /// </summary>
    /// <param name="id">The voice ID.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The (empty) delete response.</returns>
    public new Task<DeleteVoiceResponse> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return SendRequestAsync<DeleteVoiceResponse>(HttpMethod.Delete, GetResourceUrl(Collection, id),
            cancellationToken);
    }

    /// <summary>
    /// Lists one page of voices (<c>GET /voices</c>): custom voices (newest first) followed by prebuilt voices.
    /// </summary>
    /// <param name="options">Optional filters.</param>
    /// <param name="pageSize">The maximum number of voices per page (server default 50, max 1000).</param>
    /// <param name="pageToken">The page token of a previous call (filters must match).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The page.</returns>
    public Task<ListVoicesResponse> ListAsync(ListVoicesOptions? options = null, int? pageSize = null,
        string? pageToken = null, CancellationToken cancellationToken = default)
    {
        var query = new List<KeyValuePair<string, string?>>();
        AddAll(query, "accent", options?.Accent);
        AddAll(query, "context", options?.Contexts);
        AddAll(query, "gender", options?.Gender);
        AddAll(query, "language_code", options?.LanguageCode);
        query.Add(new("page_size", pageSize?.ToString(CultureInfo.InvariantCulture)));
        query.Add(new("page_token", string.IsNullOrEmpty(pageToken) ? null : pageToken));
        AddAll(query, "persona", options?.Persona);
        AddAll(query, "pitch", options?.Pitch);
        AddAll(query, "region_code", options?.RegionCode);
        query.Add(new("search", options?.Search));
        AddAll(query, "type", options?.Type);
        return SendRequestAsync<ListVoicesResponse>(HttpMethod.Get, GetCollectionUrl(Collection) + BuildQuery(query),
            cancellationToken);
    }

    /// <summary>
    /// Lists all voices matching <paramref name="options"/>, following page tokens.
    /// </summary>
    /// <param name="options">Optional filters.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The voices.</returns>
    public IAsyncEnumerable<Voice> ListAllAsync(ListVoicesOptions? options = null, int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        return PaginateAsync<ListVoicesResponse, Voice>((token, ct) => ListAsync(options, pageSize, token, ct),
            page => page.Voices, page => page.NextPageToken, cancellationToken);
    }

    private static void AddAll(List<KeyValuePair<string, string?>> query, string name, IList<string>? values)
    {
        if (values == null)
            return;
        foreach (var value in values)
            query.Add(new(name, value));
    }
}
