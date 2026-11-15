using GenerativeAI.Core;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Clients;

/// <summary>
/// A client for the managed Agents API (<c>/agents</c>): create reusable agent definitions (base agent, agent
/// configuration, system instruction, tools and base environment) and reference them from interactions. Follows
/// python-genai's <c>client.agents</c> (experimental).
/// </summary>
public class AgentsClient : AgentApiClientBase
{
    private const string Collection = "agents";

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentsClient"/> class.
    /// </summary>
    /// <param name="platform">The platform adapter (Google AI or Vertex AI).</param>
    /// <param name="httpClient">Optional HTTP client.</param>
    /// <param name="logger">Optional logger.</param>
    public AgentsClient(IPlatformAdapter platform, HttpClient? httpClient = null, ILogger? logger = null)
        : base(platform, httpClient, logger)
    {
    }

    /// <summary>
    /// Creates an agent (<c>POST /agents</c>).
    /// </summary>
    /// <param name="agent">The agent definition.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The created agent.</returns>
    public Task<Agent> CreateAsync(Agent agent, CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(agent);
#else
        if (agent == null) throw new ArgumentNullException(nameof(agent));
#endif
        return SendRequestAsync<Agent, Agent>(HttpMethod.Post, GetCollectionUrl(Collection), agent, cancellationToken);
    }

    /// <summary>
    /// Gets an agent (<c>GET /agents/{id}</c>).
    /// </summary>
    /// <param name="id">The agent ID (an <c>agents/</c> prefix is accepted).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The agent.</returns>
    public Task<Agent> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        return SendRequestAsync<Agent>(HttpMethod.Get, GetResourceUrl(Collection, id), cancellationToken);
    }

    /// <summary>
    /// Deletes an agent (<c>DELETE /agents/{id}</c>).
    /// </summary>
    /// <param name="id">The agent ID.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes when the agent has been deleted.</returns>
    public new async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await base.DeleteAsync(GetResourceUrl(Collection, id), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Lists one page of agents (<c>GET /agents</c>).
    /// </summary>
    /// <param name="pageSize">The maximum number of agents per page.</param>
    /// <param name="pageToken">The page token of a previous call.</param>
    /// <param name="parent">Optional parent filter (sent as the <c>parent</c> query parameter).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The page.</returns>
    public Task<ListAgentsResponse> ListAsync(int? pageSize = null, string? pageToken = null, string? parent = null,
        CancellationToken cancellationToken = default)
    {
        var query = PageParameters(pageSize, pageToken);
        query.Add(new("parent", parent));
        return SendRequestAsync<ListAgentsResponse>(HttpMethod.Get, GetCollectionUrl(Collection) + BuildQuery(query),
            cancellationToken);
    }

    /// <summary>
    /// Lists all agents, following page tokens.
    /// </summary>
    /// <param name="pageSize">The page size.</param>
    /// <param name="parent">Optional parent filter.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The agents.</returns>
    public IAsyncEnumerable<Agent> ListAllAsync(int? pageSize = null, string? parent = null,
        CancellationToken cancellationToken = default)
    {
        return PaginateAsync<ListAgentsResponse, Agent>((token, ct) => ListAsync(pageSize, token, parent, ct),
            page => page.Agents, page => page.NextPageToken, cancellationToken);
    }
}
