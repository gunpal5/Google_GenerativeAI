using GenerativeAI.Core;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Clients;

/// <summary>
/// A client for the Triggers API (<c>/triggers</c>): run an agent interaction on a cron schedule, pause/resume it,
/// run it on demand and inspect its executions. Follows python-genai's <c>client.triggers</c> (experimental).
/// </summary>
public class TriggersClient : AgentApiClientBase
{
    private const string Collection = "triggers";

    /// <summary>
    /// Initializes a new instance of the <see cref="TriggersClient"/> class.
    /// </summary>
    /// <param name="platform">The platform adapter (Google AI or Vertex AI).</param>
    /// <param name="httpClient">Optional HTTP client.</param>
    /// <param name="logger">Optional logger.</param>
    public TriggersClient(IPlatformAdapter platform, HttpClient? httpClient = null, ILogger? logger = null)
        : base(platform, httpClient, logger)
    {
    }

    /// <summary>
    /// Creates a trigger (<c>POST /triggers</c>). Like python-genai, an interaction input given as a list of content
    /// blocks is sent as a single <c>user_input</c> step.
    /// </summary>
    /// <param name="request">The request (interaction, schedule and time zone are required).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The created trigger.</returns>
    public Task<Trigger> CreateAsync(CreateTriggerRequest request, CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(request);
#else
        if (request == null) throw new ArgumentNullException(nameof(request));
#endif
        if (request.Interaction == null)
            throw new ArgumentException("'Interaction' is required.", nameof(request));
        if (string.IsNullOrEmpty(request.Schedule))
            throw new ArgumentException("'Schedule' is required.", nameof(request));
        if (string.IsNullOrEmpty(request.TimeZone))
            throw new ArgumentException("'TimeZone' is required.", nameof(request));

        var body = request;
        if (request.Interaction.Input?.Contents is { Count: > 0 } contents)
        {
            var interaction = request.Interaction.Clone();
            interaction.Input = InteractionInput.FromSteps(new InteractionStep[]
            {
                new InteractionUserInputStep { Content = contents.ToList() }
            });
            body = new CreateTriggerRequest
            {
                Interaction = interaction,
                Schedule = request.Schedule,
                TimeZone = request.TimeZone,
                DisplayName = request.DisplayName,
                EnvironmentId = request.EnvironmentId,
                ExecutionTimeoutSeconds = request.ExecutionTimeoutSeconds,
                MaxConsecutiveFailures = request.MaxConsecutiveFailures,
                ExtensionData = request.ExtensionData
            };
        }

        return SendRequestAsync<CreateTriggerRequest, Trigger>(HttpMethod.Post, GetCollectionUrl(Collection), body,
            cancellationToken);
    }

    /// <summary>
    /// Gets a trigger (<c>GET /triggers/{id}</c>).
    /// </summary>
    /// <param name="id">The trigger ID (a <c>triggers/</c> prefix is accepted).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The trigger.</returns>
    public Task<Trigger> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        return SendRequestAsync<Trigger>(HttpMethod.Get, GetResourceUrl(Collection, id), cancellationToken);
    }

    /// <summary>
    /// Updates a trigger (<c>PATCH /triggers/{id}</c>), e.g. to rename, pause or resume it.
    /// </summary>
    /// <param name="id">The trigger ID.</param>
    /// <param name="update">The fields to update.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The updated trigger.</returns>
    public Task<Trigger> UpdateAsync(string id, UpdateTriggerRequest update,
        CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(update);
#else
        if (update == null) throw new ArgumentNullException(nameof(update));
#endif
        return SendRequestAsync<UpdateTriggerRequest, Trigger>(new HttpMethod("PATCH"),
            GetResourceUrl(Collection, id), update, cancellationToken);
    }

    /// <summary>
    /// Pauses a trigger (sets its status to <see cref="TriggerStatuses.Paused"/>).
    /// </summary>
    /// <param name="id">The trigger ID.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The updated trigger.</returns>
    public Task<Trigger> PauseAsync(string id, CancellationToken cancellationToken = default) =>
        UpdateAsync(id, new UpdateTriggerRequest { Status = TriggerStatuses.Paused }, cancellationToken);

    /// <summary>
    /// Resumes a trigger (sets its status to <see cref="TriggerStatuses.Active"/>).
    /// </summary>
    /// <param name="id">The trigger ID.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The updated trigger.</returns>
    public Task<Trigger> ResumeAsync(string id, CancellationToken cancellationToken = default) =>
        UpdateAsync(id, new UpdateTriggerRequest { Status = TriggerStatuses.Active }, cancellationToken);

    /// <summary>
    /// Deletes a trigger (<c>DELETE /triggers/{id}</c>).
    /// </summary>
    /// <param name="id">The trigger ID.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes when the trigger has been deleted.</returns>
    public new async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await base.DeleteAsync(GetResourceUrl(Collection, id), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Lists one page of triggers (<c>GET /triggers</c>).
    /// </summary>
    /// <param name="filter">Optional filter expression (sent as <c>filter</c>).</param>
    /// <param name="pageSize">The maximum number of triggers per page.</param>
    /// <param name="pageToken">The page token of a previous call.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The page.</returns>
    public Task<ListTriggersResponse> ListAsync(string? filter = null, int? pageSize = null, string? pageToken = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<KeyValuePair<string, string?>> { new("filter", filter) };
        query.AddRange(PageParameters(pageSize, pageToken));
        return SendRequestAsync<ListTriggersResponse>(HttpMethod.Get, GetCollectionUrl(Collection) + BuildQuery(query),
            cancellationToken);
    }

    /// <summary>
    /// Lists all triggers, following page tokens.
    /// </summary>
    /// <param name="filter">Optional filter expression.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The triggers.</returns>
    public IAsyncEnumerable<Trigger> ListAllAsync(string? filter = null, int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        return PaginateAsync<ListTriggersResponse, Trigger>((token, ct) => ListAsync(filter, pageSize, token, ct),
            page => page.Triggers, page => page.NextPageToken, cancellationToken);
    }

    /// <summary>
    /// Runs a trigger now (<c>POST /triggers/{id}/executions</c>).
    /// </summary>
    /// <param name="triggerId">The trigger ID.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The started execution.</returns>
    public Task<TriggerExecution> RunAsync(string triggerId, CancellationToken cancellationToken = default)
    {
        return SendRequestAsync<TriggerExecution>(HttpMethod.Post, GetResourceUrl(Collection, triggerId) + "/executions",
            cancellationToken);
    }

    /// <summary>
    /// Lists one page of the executions of a trigger (<c>GET /triggers/{id}/executions</c>).
    /// </summary>
    /// <param name="triggerId">The trigger ID.</param>
    /// <param name="pageSize">The maximum number of executions per page.</param>
    /// <param name="pageToken">The page token of a previous call.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The page.</returns>
    public Task<ListTriggerExecutionsResponse> ListExecutionsAsync(string triggerId, int? pageSize = null,
        string? pageToken = null, CancellationToken cancellationToken = default)
    {
        return SendRequestAsync<ListTriggerExecutionsResponse>(HttpMethod.Get,
            GetResourceUrl(Collection, triggerId) + "/executions" + BuildQuery(PageParameters(pageSize, pageToken)),
            cancellationToken);
    }

    /// <summary>
    /// Lists all executions of a trigger, following page tokens.
    /// </summary>
    /// <param name="triggerId">The trigger ID.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The executions.</returns>
    public IAsyncEnumerable<TriggerExecution> ListAllExecutionsAsync(string triggerId, int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        return PaginateAsync<ListTriggerExecutionsResponse, TriggerExecution>(
            (token, ct) => ListExecutionsAsync(triggerId, pageSize, token, ct),
            page => page.TriggerExecutions, page => page.NextPageToken, cancellationToken);
    }

    /// <summary>
    /// Polls the executions of a trigger until the execution <paramref name="executionId"/> is no longer
    /// <see cref="TriggerExecutionStatuses.InProgress"/> (the API has no endpoint to get a single execution).
    /// </summary>
    /// <param name="triggerId">The trigger ID.</param>
    /// <param name="executionId">The execution ID (e.g. returned by <see cref="RunAsync"/>).</param>
    /// <param name="pollInterval">Delay between polls (default 10 seconds).</param>
    /// <param name="timeout">Optional overall timeout; a <see cref="TimeoutException"/> is thrown when exceeded.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The finished execution.</returns>
    public async Task<TriggerExecution> WaitForExecutionAsync(string triggerId, string executionId,
        TimeSpan? pollInterval = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(executionId))
            throw new ArgumentException("The execution ID is required.", nameof(executionId));
        var interval = pollInterval ?? TimeSpan.FromSeconds(10);
        var started = DateTime.UtcNow;
        while (true)
        {
            TriggerExecution? execution = null;
            await foreach (var item in ListAllExecutionsAsync(triggerId, cancellationToken: cancellationToken)
                               .ConfigureAwait(false))
            {
                if (item.Id == executionId)
                {
                    execution = item;
                    break;
                }
            }

            if (execution != null && TriggerExecutionStatuses.IsTerminal(execution.Status))
                return execution;

            if (timeout.HasValue && DateTime.UtcNow - started + interval > timeout.Value)
                throw new TimeoutException(
                    $"Execution '{executionId}' of trigger '{triggerId}' did not finish within {timeout.Value} " +
                    $"(last status: {execution?.Status ?? "not found"}).");

            await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
        }
    }
}
