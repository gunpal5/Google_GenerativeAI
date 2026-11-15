using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using GenerativeAI.Core;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Clients;

/// <summary>
/// A client for model tuning, mirroring <c>client.tunings</c> of the google-genai SDK.
/// </summary>
/// <remarks>
/// <para>
/// On Vertex AI it uses the <c>projects/{project}/locations/{location}/tuningJobs</c> resource and supports
/// supervised fine tuning, preference tuning, distillation and reinforcement tuning, listing, cancellation
/// and reward validation.
/// </para>
/// <para>
/// On the Gemini Developer API it uses the <c>tunedModels</c> resource: <see cref="TuneAsync"/>,
/// <see cref="GetAsync(string, CancellationToken)"/>, <see cref="CancelAsync"/> and
/// <see cref="WaitForCompletionAsync"/> are supported, and tuned models are reported as <see cref="TuningJob"/>s.
/// </para>
/// </remarks>
/// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/tuning">See Official API Documentation</seealso>
public class TuningClient : BaseClient
{
    private static readonly string[] DefaultAggregationMetrics = { "AVERAGE", "STANDARD_DEVIATION" };

    /// <summary>
    /// Initializes a new instance of the <see cref="TuningClient"/> class.
    /// </summary>
    /// <param name="platform">The platform adapter used to manage API endpoints and authentication.</param>
    /// <param name="httpClient">The HTTP client instance used for making API requests.</param>
    /// <param name="logger">An optional logger instance for logging operations.</param>
    public TuningClient(IPlatformAdapter platform, HttpClient? httpClient = null, ILogger? logger = null)
        : base(platform, httpClient, logger)
    {
        // Make the tuning types resolvable (also under Native AOT, where reflection is unavailable).
        if (SerializerOptions != null && !SerializerOptions.IsReadOnly &&
            !SerializerOptions.TypeInfoResolverChain.Contains(TuningSerializerContext.Default))
        {
            SerializerOptions.TypeInfoResolverChain.Insert(0, TuningSerializerContext.Default);
        }
    }

    private bool IsGoogleAI => Platform is GoogleAIPlatformAdapter;

    #region Tune

    /// <summary>
    /// Creates a tuning job (Vertex AI) or a tuned model (Gemini API) and returns it as a <see cref="TuningJob"/>.
    /// </summary>
    /// <param name="baseModel">
    /// The model to tune. On Vertex AI, a tuned model resource name (<c>projects/.../models/...</c>) continues
    /// tuning from that model (with <see cref="CreateTuningJobConfig.PreTunedModelCheckpointId"/> as checkpoint).
    /// </param>
    /// <param name="trainingDataset">The training dataset.</param>
    /// <param name="config">Optional tuning configuration.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// The created <see cref="TuningJob"/>. On the Gemini API only <see cref="TuningJob.Name"/> (the tuned model name)
    /// and <see cref="TuningJob.State"/> (<see cref="JobState.JOB_STATE_QUEUED"/>) are set.
    /// </returns>
    /// <exception cref="NotSupportedException">An option is not supported on the current platform.</exception>
    public async Task<TuningJob> TuneAsync(string baseModel, TuningDataset trainingDataset,
        CreateTuningJobConfig? config = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(baseModel))
            throw new ArgumentException("Base model cannot be null or empty.", nameof(baseModel));
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(trainingDataset);
#else
        if (trainingDataset == null) throw new ArgumentNullException(nameof(trainingDataset));
#endif

        if (IsGoogleAI)
            return await TuneGoogleAIAsync(baseModel, trainingDataset, config, cancellationToken).ConfigureAwait(false);

        var request = BuildVertexTuningJob(baseModel, trainingDataset, config);
        var url = $"{Platform.GetBaseUrl(true, false)}/tuningJobs";
        var job = await SendAsync<TuningJob, TuningJob>(url, request, HttpMethod.Post, cancellationToken)
            .ConfigureAwait(false);
        if (job.EvaluationConfig == null && config?.EvaluationConfig != null)
            job.EvaluationConfig = config.EvaluationConfig;
        return job;
    }

    private async Task<TuningJob> TuneGoogleAIAsync(string baseModel, TuningDataset trainingDataset,
        CreateTuningJobConfig? config, CancellationToken cancellationToken)
    {
        if (trainingDataset.GcsUri != null)
            throw VertexOnly("gcsUri");
        if (trainingDataset.VertexDatasetResource != null)
            throw VertexOnly("vertexDatasetResource");

        var request = new GoogleAITunedModel { BaseModel = baseModel };
        var task = new TuningTask();
        if (trainingDataset.Examples != null)
        {
            task.TrainingData = new Dataset { Examples = new TuningExamples { Examples = trainingDataset.Examples } };
        }

        if (config != null)
        {
            ThrowIfVertexOnlyOptionsSet(config);
            request.DisplayName = config.TunedModelDisplayName;
            if (config.EpochCount != null || config.LearningRateMultiplier != null || config.BatchSize != null ||
                config.LearningRate != null)
            {
                task.Hyperparameters = new Hyperparameters
                {
                    EpochCount = config.EpochCount,
                    LearningRateMultiplier = config.LearningRateMultiplier,
                    BatchSize = config.BatchSize,
                    LearningRate = config.LearningRate
                };
            }
        }

        request.TuningTask = task;

        var url = $"{Platform.GetBaseUrl()}/tunedModels";
        var operation = await SendAsync<GoogleAITunedModel, GoogleLongRunningOperation>(url, request,
            HttpMethod.Post, cancellationToken).ConfigureAwait(false);

        string? tunedModelName = null;
        if (operation.Metadata != null && operation.Metadata.TryGetValue("tunedModel", out var value))
        {
            tunedModelName = value is JsonElement { ValueKind: JsonValueKind.String } element
                ? element.GetString()
                : value?.ToString();
        }

        if (string.IsNullOrEmpty(tunedModelName))
        {
            if (operation.Name == null)
                throw new InvalidOperationException("The tuning operation did not return a name.");
            var index = operation.Name.IndexOf("/operations/", StringComparison.Ordinal);
            tunedModelName = index >= 0 ? operation.Name.Substring(0, index) : operation.Name;
        }

        return new TuningJob { Name = tunedModelName, State = JobState.JOB_STATE_QUEUED };
    }

    private static void ThrowIfVertexOnlyOptionsSet(CreateTuningJobConfig config)
    {
        if (config.ValidationDataset != null) throw VertexOnly("validationDataset");
        if (config.Description != null) throw VertexOnly("description");
        if (config.ExportLastCheckpointOnly != null) throw VertexOnly("exportLastCheckpointOnly");
        if (config.PreTunedModelCheckpointId != null) throw VertexOnly("preTunedModelCheckpointId");
        if (config.AdapterSize != null) throw VertexOnly("adapterSize");
        if (config.TuningMode != null) throw VertexOnly("tuningMode");
        if (config.CustomBaseModel != null) throw VertexOnly("customBaseModel");
        if (config.EvaluationConfig != null) throw VertexOnly("evaluationConfig");
        if (config.Labels != null) throw VertexOnly("labels");
        if (config.Beta != null) throw VertexOnly("beta");
        if (config.BaseTeacherModel != null) throw VertexOnly("baseTeacherModel");
        if (config.TunedTeacherModelSource != null) throw VertexOnly("tunedTeacherModelSource");
        if (config.SftLossWeightMultiplier != null) throw VertexOnly("sftLossWeightMultiplier");
        if (config.OutputUri != null) throw VertexOnly("outputUri");
        if (config.EncryptionSpec != null) throw VertexOnly("encryptionSpec");
        if (config.RewardConfig != null) throw VertexOnly("rewardConfig");
        if (config.CompositeRewardConfig != null) throw VertexOnly("compositeRewardConfig");
        if (config.SamplesPerPrompt != null) throw VertexOnly("samplesPerPrompt");
        if (config.EvaluateInterval != null) throw VertexOnly("evaluateInterval");
        if (config.CheckpointInterval != null) throw VertexOnly("checkpointInterval");
        if (config.MaxOutputTokens != null) throw VertexOnly("maxOutputTokens");
        if (config.ThinkingLevel != null) throw VertexOnly("thinkingLevel");
        if (config.ValidationDatasetUri != null) throw VertexOnly("validationDatasetUri");
    }

    private static NotSupportedException VertexOnly(string parameter) =>
        new($"{parameter} parameter is only supported on Vertex AI, not on the Gemini Developer API.");

    private static NotSupportedException GoogleAIOnly(string parameter) =>
        new($"{parameter} parameter is only supported on the Gemini Developer API, not on Vertex AI.");

    /// <summary>
    /// Builds the Vertex AI tuning job request the same way as <c>_CreateTuningJobParametersPrivate_to_vertex</c>
    /// of the google-genai SDK.
    /// </summary>
    private static TuningJob BuildVertexTuningJob(string baseModel, TuningDataset dataset, CreateTuningJobConfig? config)
    {
        var job = new TuningJob();
        if (baseModel.StartsWith("projects/", StringComparison.Ordinal))
        {
            job.PreTunedModel = new PreTunedModel
            {
                TunedModelName = baseModel,
                CheckpointId = config?.PreTunedModelCheckpointId
            };
        }
        else
        {
            if (config?.EvaluationConfig is { } evaluationConfig &&
                (evaluationConfig.Metrics is not { Count: > 0 } || evaluationConfig.OutputConfig == null))
            {
                throw new ArgumentException("Evaluation config must have at least one metric and an output config.",
                    nameof(config));
            }

            job.BaseModel = baseModel;
        }

        var method = config?.Method ?? TuningMethod.SUPERVISED_FINE_TUNING;

        // Training dataset.
        if (dataset.Examples != null)
            throw GoogleAIOnly("examples");
        foreach (var uri in new[] { dataset.GcsUri, dataset.VertexDatasetResource })
        {
            if (uri == null) continue;
            switch (method)
            {
                case TuningMethod.PREFERENCE_TUNING: Preference(job).TrainingDatasetUri = uri; break;
                case TuningMethod.DISTILLATION: Distillation(job).PromptDatasetUri = uri; break;
                case TuningMethod.REINFORCEMENT_TUNING: Reinforcement(job).TrainingDatasetUri = uri; break;
                default: Supervised(job).TrainingDatasetUri = uri; break;
            }
        }

        if (config == null)
            return job;

        // Validation dataset.
        if (config.ValidationDataset != null)
        {
            foreach (var uri in new[] { config.ValidationDataset.GcsUri, config.ValidationDataset.VertexDatasetResource })
            {
                if (uri == null) continue;
                switch (method)
                {
                    case TuningMethod.PREFERENCE_TUNING: Preference(job).ValidationDatasetUri = uri; break;
                    case TuningMethod.DISTILLATION: Distillation(job).ValidationDatasetUri = uri; break;
                    case TuningMethod.REINFORCEMENT_TUNING: Reinforcement(job).ValidationDatasetUri = uri; break;
                    default: Supervised(job).ValidationDatasetUri = uri; break;
                }
            }
        }

        job.TunedModelDisplayName = config.TunedModelDisplayName;
        job.Description = config.Description;

        if (config.EpochCount != null)
        {
            switch (method)
            {
                case TuningMethod.PREFERENCE_TUNING: PreferenceHp(job).EpochCount = config.EpochCount; break;
                case TuningMethod.DISTILLATION: DistillationHp(job).EpochCount = config.EpochCount; break;
                case TuningMethod.REINFORCEMENT_TUNING: ReinforcementHp(job).EpochCount = config.EpochCount; break;
                default: SupervisedHp(job).EpochCount = config.EpochCount; break;
            }
        }

        if (config.LearningRateMultiplier != null)
        {
            var value = config.LearningRateMultiplier;
            switch (method)
            {
                case TuningMethod.PREFERENCE_TUNING: PreferenceHp(job).LearningRateMultiplier = value; break;
                case TuningMethod.DISTILLATION: DistillationHp(job).LearningRateMultiplier = value; break;
                case TuningMethod.REINFORCEMENT_TUNING: ReinforcementHp(job).LearningRateMultiplier = value; break;
                default: SupervisedHp(job).LearningRateMultiplier = value; break;
            }
        }

        if (config.ExportLastCheckpointOnly != null)
        {
            var value = config.ExportLastCheckpointOnly;
            switch (method)
            {
                case TuningMethod.PREFERENCE_TUNING: Preference(job).ExportLastCheckpointOnly = value; break;
                case TuningMethod.DISTILLATION: Distillation(job).ExportLastCheckpointOnly = value; break;
                case TuningMethod.SUPERVISED_FINE_TUNING: Supervised(job).ExportLastCheckpointOnly = value; break;
            }
        }

        if (config.AdapterSize != null)
        {
            var value = config.AdapterSize;
            switch (method)
            {
                case TuningMethod.PREFERENCE_TUNING: PreferenceHp(job).AdapterSize = value; break;
                case TuningMethod.DISTILLATION: DistillationHp(job).AdapterSize = value; break;
                case TuningMethod.REINFORCEMENT_TUNING: ReinforcementHp(job).AdapterSize = value; break;
                default: SupervisedHp(job).AdapterSize = value; break;
            }
        }

        if (config.TuningMode != null)
        {
            switch (method)
            {
                case TuningMethod.SUPERVISED_FINE_TUNING: Supervised(job).TuningMode = config.TuningMode; break;
                case TuningMethod.DISTILLATION: Distillation(job).TuningMode = config.TuningMode; break;
            }
        }

        job.CustomBaseModel = config.CustomBaseModel;

        if (config.BatchSize != null)
        {
            switch (method)
            {
                case TuningMethod.SUPERVISED_FINE_TUNING: SupervisedHp(job).BatchSize = config.BatchSize; break;
                case TuningMethod.DISTILLATION: DistillationHp(job).BatchSize = config.BatchSize; break;
                case TuningMethod.REINFORCEMENT_TUNING: ReinforcementHp(job).BatchSize = config.BatchSize; break;
            }
        }

        if (config.LearningRate != null)
        {
            switch (method)
            {
                case TuningMethod.SUPERVISED_FINE_TUNING: SupervisedHp(job).LearningRate = config.LearningRate; break;
                case TuningMethod.DISTILLATION: DistillationHp(job).LearningRate = config.LearningRate; break;
            }
        }

        if (config.EvaluationConfig != null)
        {
            var evaluation = ToWireEvaluationConfig(config.EvaluationConfig);
            switch (method)
            {
                case TuningMethod.SUPERVISED_FINE_TUNING: Supervised(job).EvaluationConfig = evaluation; break;
                case TuningMethod.PREFERENCE_TUNING: Preference(job).EvaluationConfig = evaluation; break;
                case TuningMethod.DISTILLATION: Distillation(job).EvaluationConfig = evaluation; break;
            }
        }

        job.Labels = config.Labels;

        if (config.Beta != null) PreferenceHp(job).Beta = config.Beta;
        if (config.BaseTeacherModel != null) Distillation(job).BaseTeacherModel = config.BaseTeacherModel;
        if (config.TunedTeacherModelSource != null)
            Distillation(job).TunedTeacherModelSource = config.TunedTeacherModelSource;
        if (config.SftLossWeightMultiplier != null)
            DistillationHp(job).SftLossWeightMultiplier = config.SftLossWeightMultiplier;

        job.OutputUri = config.OutputUri;
        job.EncryptionSpec = config.EncryptionSpec;

        if (config.RewardConfig != null) Reinforcement(job).SingleRewardConfig = config.RewardConfig;
        if (config.CompositeRewardConfig != null) Reinforcement(job).CompositeRewardConfig = config.CompositeRewardConfig;
        if (config.SamplesPerPrompt != null) ReinforcementHp(job).SamplesPerPrompt = config.SamplesPerPrompt;
        if (config.EvaluateInterval != null) ReinforcementHp(job).EvaluateInterval = config.EvaluateInterval;
        if (config.CheckpointInterval != null) ReinforcementHp(job).CheckpointInterval = config.CheckpointInterval;
        if (config.MaxOutputTokens != null) ReinforcementHp(job).MaxOutputTokens = config.MaxOutputTokens;
        if (config.ThinkingLevel != null) ReinforcementHp(job).ThinkingLevel = config.ThinkingLevel;
        if (config.ValidationDatasetUri != null) Reinforcement(job).ValidationDatasetUri = config.ValidationDatasetUri;

        return job;
    }

    private static SupervisedTuningSpec Supervised(TuningJob job) =>
        job.SupervisedTuningSpec ??= new SupervisedTuningSpec();

    private static SupervisedHyperParameters SupervisedHp(TuningJob job) =>
        Supervised(job).HyperParameters ??= new SupervisedHyperParameters();

    private static PreferenceOptimizationSpec Preference(TuningJob job) =>
        job.PreferenceOptimizationSpec ??= new PreferenceOptimizationSpec();

    private static PreferenceOptimizationHyperParameters PreferenceHp(TuningJob job) =>
        Preference(job).HyperParameters ??= new PreferenceOptimizationHyperParameters();

    private static DistillationSpec Distillation(TuningJob job) =>
        job.DistillationSpec ??= new DistillationSpec();

    private static DistillationHyperParameters DistillationHp(TuningJob job) =>
        Distillation(job).HyperParameters ??= new DistillationHyperParameters();

    private static ReinforcementTuningSpec Reinforcement(TuningJob job) =>
        job.ReinforcementTuningSpec ??= new ReinforcementTuningSpec();

    private static ReinforcementTuningHyperParameters ReinforcementHp(TuningJob job) =>
        Reinforcement(job).HyperParameters ??= new ReinforcementTuningHyperParameters();

    private static EvaluationConfig ToWireEvaluationConfig(EvaluationConfig config)
    {
        return new EvaluationConfig
        {
            Metrics = config.Metrics?.Select(ToWireMetric).ToList(),
            OutputConfig = config.OutputConfig,
            AutoraterConfig = config.AutoraterConfig,
            InferenceGenerationConfig = config.InferenceGenerationConfig
        };
    }

    /// <summary>
    /// Converts a metric to its wire format, as <c>t_metrics</c> of the google-genai SDK does.
    /// </summary>
    private static Metric ToWireMetric(Metric metric)
    {
        var wire = new Metric
        {
            AggregationMetrics = metric.AggregationMetrics ?? DefaultAggregationMetrics.ToList(),
            ExactMatchSpec = metric.ExactMatchSpec,
            BleuSpec = metric.BleuSpec,
            RougeSpec = metric.RougeSpec,
            PointwiseMetricSpec = metric.PointwiseMetricSpec
        };
        if (wire.ExactMatchSpec != null || wire.BleuSpec != null || wire.RougeSpec != null ||
            wire.PointwiseMetricSpec != null)
        {
            return wire;
        }

        // The google-genai SDK lower-cases metric names before mapping them.
#pragma warning disable CA1308
        var name = (metric.Name ?? string.Empty).ToLowerInvariant();
#pragma warning restore CA1308
        if (name == "exact_match")
        {
            wire.ExactMatchSpec = new JsonObject();
        }
        else if (name == "bleu")
        {
            wire.BleuSpec = new JsonObject();
        }
        else if (name.StartsWith("rouge", StringComparison.Ordinal))
        {
            wire.RougeSpec = new JsonObject { ["rougeType"] = new string(name.Where(c => c != '_').ToArray()) };
        }
        else if (!string.IsNullOrEmpty(metric.PromptTemplate))
        {
            var pointwise = new JsonObject { ["metricPromptTemplate"] = metric.PromptTemplate };
            if (!string.IsNullOrEmpty(metric.JudgeModelSystemInstruction))
                pointwise["systemInstruction"] = metric.JudgeModelSystemInstruction;
            if (metric.ReturnRawOutput == true)
                pointwise["customOutputFormatConfig"] = new JsonObject { ["returnRawOutput"] = true };
            wire.PointwiseMetricSpec = pointwise;
        }
        else
        {
            throw new ArgumentException($"Unsupported metric type or invalid metric name: {name}", nameof(metric));
        }

        return wire;
    }

    #endregion

    #region Get / List / Cancel

    /// <summary>
    /// Gets a tuning job (Vertex AI) or a tuned model reported as a tuning job (Gemini API).
    /// </summary>
    /// <param name="name">
    /// The tuning job resource name (<c>projects/.../tuningJobs/...</c>, <c>tuningJobs/...</c> or the job id) on
    /// Vertex AI, or the tuned model name (<c>tunedModels/...</c> or the id) on the Gemini API.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The <see cref="TuningJob"/>.</returns>
    public async Task<TuningJob> GetAsync(string name, CancellationToken cancellationToken = default)
    {
        var url = ResolveJobUrl(name);
        if (IsGoogleAI)
        {
            var tunedModel = await GetAsync<GoogleAITunedModel>(url, cancellationToken).ConfigureAwait(false);
            return tunedModel.ToTuningJob();
        }

        return await GetAsync<TuningJob>(url, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Lists tuning jobs (Vertex AI only).
    /// </summary>
    /// <param name="pageSize">The maximum number of tuning jobs to return.</param>
    /// <param name="pageToken">A page token from a previous call.</param>
    /// <param name="filter">An optional list filter.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A page of tuning jobs.</returns>
    /// <exception cref="NotSupportedException">Called on the Gemini Developer API.</exception>
    public async Task<ListTuningJobsResponse> ListAsync(int? pageSize = null, string? pageToken = null,
        string? filter = null, CancellationToken cancellationToken = default)
    {
        if (IsGoogleAI)
            throw new NotSupportedException(
                "Listing tuning jobs is only supported on Vertex AI, not on the Gemini Developer API.");

        var query = new List<string>();
        if (pageSize.HasValue) query.Add($"pageSize={pageSize.Value}");
        if (!string.IsNullOrEmpty(pageToken)) query.Add($"pageToken={Uri.EscapeDataString(pageToken)}");
        if (!string.IsNullOrEmpty(filter)) query.Add($"filter={Uri.EscapeDataString(filter)}");
        var queryString = query.Count > 0 ? "?" + string.Join("&", query) : string.Empty;

        var url = $"{Platform.GetBaseUrl(true, false)}/tuningJobs{queryString}";
        return await GetAsync<ListTuningJobsResponse>(url, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Lists all tuning jobs, fetching further pages as needed (Vertex AI only).
    /// </summary>
    /// <param name="pageSize">The page size used for each request.</param>
    /// <param name="filter">An optional list filter.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An async sequence of tuning jobs.</returns>
    public async IAsyncEnumerable<TuningJob> ListAllAsync(int? pageSize = null, string? filter = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string? pageToken = null;
        do
        {
            var page = await ListAsync(pageSize, pageToken, filter, cancellationToken).ConfigureAwait(false);
            if (page.TuningJobs != null)
            {
                foreach (var job in page.TuningJobs)
                    yield return job;
            }

            pageToken = page.NextPageToken;
        } while (!string.IsNullOrEmpty(pageToken));
    }

    /// <summary>
    /// Cancels a tuning job.
    /// </summary>
    /// <param name="name">The tuning job (Vertex AI) or tuned model (Gemini API) name.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that completes when the cancellation request was accepted.</returns>
    public async Task CancelAsync(string name, CancellationToken cancellationToken = default)
    {
        var url = $"{ResolveJobUrl(name)}:cancel";
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        await Platform.AddAuthorizationAsync(request, false, cancellationToken).ConfigureAwait(false);
        using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await CheckAndHandleErrors(response, url).ConfigureAwait(false);
    }

    /// <summary>
    /// Polls a tuning job until it reaches a terminal state (succeeded, partially succeeded, failed, cancelled
    /// or expired).
    /// </summary>
    /// <param name="name">The tuning job (Vertex AI) or tuned model (Gemini API) name.</param>
    /// <param name="pollInterval">The delay between polls. Defaults to 30 seconds.</param>
    /// <param name="timeout">The maximum time to wait. Defaults to no limit.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The completed <see cref="TuningJob"/>.</returns>
    /// <exception cref="TimeoutException">The job did not complete within <paramref name="timeout"/>.</exception>
    public async Task<TuningJob> WaitForCompletionAsync(string name, TimeSpan? pollInterval = null,
        TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        var interval = pollInterval ?? TimeSpan.FromSeconds(30);
        var stopwatch = Stopwatch.StartNew();
        while (true)
        {
            var job = await GetAsync(name, cancellationToken).ConfigureAwait(false);
            if (job.IsCompleted)
                return job;

            if (timeout.HasValue && stopwatch.Elapsed + interval > timeout.Value)
                throw new TimeoutException(
                    $"Tuning job '{name}' did not complete within {timeout.Value}. Last state: {job.State}.");

            await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
        }
    }

    private string ResolveJobUrl(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tuning job name cannot be null or empty.", nameof(name));

        if (IsGoogleAI)
            return $"{Platform.GetBaseUrl()}/{name.ToTunedModelId()}";

        if (name.StartsWith("projects/", StringComparison.Ordinal))
            return $"{Platform.GetBaseUrl(true, false, false, false)}/{name}";
        if (name.StartsWith("tuningJobs/", StringComparison.Ordinal))
            return $"{Platform.GetBaseUrl(true, false)}/{name}";
        return $"{Platform.GetBaseUrl(true, false)}/tuningJobs/{name}";
    }

    #endregion

    #region Reward validation

    /// <summary>
    /// Validates a reinforcement tuning reward configuration against a sample response and example, without
    /// creating a tuning job (Vertex AI only).
    /// </summary>
    /// <param name="sampleResponse">The sample model response to score.</param>
    /// <param name="example">The example (prompt and references) the response belongs to.</param>
    /// <param name="singleRewardConfig">A single reward configuration. Mutually exclusive with <paramref name="compositeRewardConfig"/>.</param>
    /// <param name="compositeRewardConfig">A composite reward configuration. Mutually exclusive with <paramref name="singleRewardConfig"/>.</param>
    /// <param name="parent">
    /// The location to validate in, <c>projects/{project}/locations/{location}</c>. Defaults to the platform's
    /// project and location.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The computed reward(s).</returns>
    /// <exception cref="NotSupportedException">Called on the Gemini Developer API.</exception>
    public async Task<ValidateRewardResponse> ValidateRewardAsync(Content sampleResponse,
        ReinforcementTuningExample example,
        SingleReinforcementTuningRewardConfig? singleRewardConfig = null,
        CompositeReinforcementTuningRewardConfig? compositeRewardConfig = null,
        string? parent = null,
        CancellationToken cancellationToken = default)
    {
        if (IsGoogleAI)
            throw new NotSupportedException(
                "Reward validation is only supported on Vertex AI, not on the Gemini Developer API.");
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(sampleResponse);
        ArgumentNullException.ThrowIfNull(example);
#else
        if (sampleResponse == null) throw new ArgumentNullException(nameof(sampleResponse));
        if (example == null) throw new ArgumentNullException(nameof(example));
#endif

        var parentUrl = string.IsNullOrEmpty(parent)
            ? Platform.GetBaseUrl(true, false)
            : $"{Platform.GetBaseUrl(true, false, false, false)}/{parent}";
        var url = $"{parentUrl}/tuningJobs:validateReinforcementTuningReward";

        var request = new ValidateRewardRequest
        {
            SampleResponse = sampleResponse,
            Example = example,
            SingleRewardConfig = singleRewardConfig,
            CompositeRewardConfig = compositeRewardConfig
        };
        return await SendAsync<ValidateRewardRequest, ValidateRewardResponse>(url, request, HttpMethod.Post,
            cancellationToken).ConfigureAwait(false);
    }

    #endregion
}
