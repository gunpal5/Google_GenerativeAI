using System.Text.Json;
using GenerativeAI.Core;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Clients;

/// <summary>
/// A client for interacting with the Gemini API Batch endpoint.
/// Provides methods for creating, managing, and monitoring batch processing jobs.
/// </summary>
/// <seealso href="https://ai.google.dev/api/batch">See Official API Documentation</seealso>
public class BatchClient : BaseClient
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BatchClient"/> class.
    /// </summary>
    /// <param name="platform">The platform adapter used to manage API endpoints and authentication.</param>
    /// <param name="httpClient">The HTTP client instance used for making API requests.</param>
    /// <param name="logger">An optional logger instance for logging operations.</param>
    /// <remarks>
    /// The <see cref="BatchClient"/> provides functionality for batch processing operations,
    /// including creating batch jobs for content generation and embeddings, monitoring job status,
    /// and managing batch job lifecycle.
    /// </remarks>
    public BatchClient(IPlatformAdapter platform, HttpClient? httpClient = null, ILogger? logger = null)
        : base(platform, httpClient, logger)
    {
    }

    /// <summary>
    /// Creates a batch job for content generation asynchronously.
    /// </summary>
    /// <param name="model">The name of the model to use for batch generation.</param>
    /// <param name="source">The source configuration specifying input data location (GCS, BigQuery, or inlined requests).</param>
    /// <param name="destination">Optional destination configuration for output data.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the created <see cref="BatchJob"/>.</returns>
    /// <remarks>
    /// Batch jobs are asynchronous operations that process multiple requests efficiently.
    /// The job status can be monitored using <see cref="GetAsync"/> with the returned job name.
    /// </remarks>
    /// <seealso href="https://ai.google.dev/api/batch#method:-models.batchgeneratecontent">See Official API Documentation</seealso>
    public async Task<BatchJob> CreateAsync(
        string model,
        BatchJobSource source,
        BatchJobDestination? destination = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Model name cannot be null or empty.", nameof(model));
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var modelId = model;

        // Google AI and Vertex AI use different request structures
        if (Platform is GoogleAIPlatformAdapter)
        {
            modelId = model.ToModelId();
            // Google AI (MLDev) format
            var url = $"{Platform.GetBaseUrl()}/{modelId}:batchGenerateContent";

            var inputConfig = new BatchInputConfig();
            if (source.InlinedRequests != null)
            {
                // Wrap each InlinedRequest with the required "request" field
                var wrappedRequests = source.InlinedRequests.Select(req => new InlinedRequestWrapper
                {
                    Request = new InlinedRequestBody
                    {
                        Model = req.Model,
                        Contents = req.Contents,
                        GenerationConfig = req.GenerationConfig,
                        SafetySettings = req.SafetySettings,
                        Tools = req.Tools,
                        ToolConfig = req.ToolConfig,
                        SystemInstruction = req.SystemInstruction,
                        CachedContent = req.CachedContent
                    },
                    Metadata = req.Metadata
                }).ToList();

                inputConfig.Requests = new RequestsWrapper { Requests = wrappedRequests };
            }
            else if (source.FileName != null)
            {
                inputConfig.FileName = source.FileName;
            }
            else
            {
                throw new ArgumentException("For Google AI, source must have either InlinedRequests or FileName.", nameof(source));
            }

            var requestBody = new BatchWrapper
            {
                Batch = new BatchConfig { InputConfig = inputConfig }
            };

            var response = await SendAsync<BatchWrapper, BatchJobResponse>(url, requestBody, HttpMethod.Post, cancellationToken).ConfigureAwait(false);

            // Extract metadata and copy top-level name
            var batchJob = response.Metadata ?? throw new InvalidOperationException("API response missing metadata");
            batchJob.Name = response.Name;
            return batchJob;
        }
        else
        {
            // Vertex AI format
            var url = $"{Platform.GetBaseUrl(true,false)}/batchPredictionJobs";

            // Validate unsupported parameters for Vertex AI
            if (source.FileName != null)
                throw new NotSupportedException("FileName is not supported in Vertex AI. Use GcsUri or BigqueryUri instead.");

            if (source.InlinedRequests != null && source.InlinedRequests.Count > 0)
                throw new NotSupportedException("InlinedRequests are not supported in Vertex AI. Use GcsUri or BigqueryUri instead.");

            var vertexInputConfig = new VertexInputConfig();
            if (source.GcsUri != null && source.GcsUri.Count > 0)
            {
                vertexInputConfig.InstancesFormat = source.Format ?? "jsonl";
                vertexInputConfig.GcsSource = new BatchGcsSource { Uris = source.GcsUri };
            }
            else if (source.BigqueryUri != null)
            {
                vertexInputConfig.InstancesFormat = source.Format ?? "bigquery";
                vertexInputConfig.BigQuerySource = new BatchBigQuerySource { InputUri = source.BigqueryUri };
            }
            else
            {
                throw new ArgumentException("For Vertex AI, source must have either GcsUri or BigqueryUri.", nameof(source));
            }

            var vertexRequest = new VertexBatchRequest
            {
                Model = Platform.GetFullyQualifiedModelName(modelId,false),
                DisplayName = $"Batch job - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}",
                InputConfig = vertexInputConfig
            };

            if (destination != null)
            {
                var vertexOutputConfig = new VertexOutputConfig();
                if (destination.GcsUri != null)
                {
                    vertexOutputConfig.PredictionsFormat = destination.Format ?? "jsonl";
                    vertexOutputConfig.GcsDestination = new BatchGcsDestination { OutputUriPrefix = destination.GcsUri };
                }
                else if (destination.BigqueryUri != null)
                {
                    vertexOutputConfig.PredictionsFormat = destination.Format ?? "bigquery";
                    vertexOutputConfig.BigQueryDestination = new BatchBigQueryDestination { OutputUri = destination.BigqueryUri };
                }
                vertexRequest.OutputConfig = vertexOutputConfig;
            }

            return await SendAsync<VertexBatchRequest, BatchJob>(url, vertexRequest, HttpMethod.Post, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Creates a batch job for embedding generation asynchronously.
    /// </summary>
    /// <param name="model">The name of the embedding model to use.</param>
    /// <param name="source">The source configuration specifying input data for embeddings.</param>
    /// <param name="destination">Optional destination configuration for embedding output.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the created <see cref="BatchJob"/>.</returns>
    /// <remarks>
    /// This method creates a batch job specifically for generating embeddings from text inputs.
    /// It's optimized for processing large volumes of embedding requests efficiently.
    /// </remarks>
    /// <seealso href="https://ai.google.dev/api/batch#method:-models.asyncbatchembedcontent">See Official API Documentation</seealso>
    public async Task<BatchJob> CreateEmbeddingsAsync(
        string model,
        BatchJobSource source,
        BatchJobDestination? destination = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Model name cannot be null or empty.", nameof(model));
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        // Embeddings batch is only supported in Google AI (MLDev)
        if (Platform is not GoogleAIPlatformAdapter)
        {
            throw new NotSupportedException("Batch embeddings are only supported in Google AI, not Vertex AI.");
        }

        var modelId = model.ToModelId();
        var url = $"{Platform.GetBaseUrl()}/{modelId}:asyncBatchEmbedContent";

        var embeddingInputConfig = new EmbeddingBatchInputConfig();
        if (source.InlinedRequests != null)
        {
            // For embeddings, each request has a single "content" field, not "contents"
            // Structure: requests.requests[] (same nested structure as content generation)
            var embedRequests = source.InlinedRequests.Select(req => new EmbedRequestWrapper
            {
                Request = new EmbedRequestBody
                {
                    Model = req.Model,
                    // Take the first content item for embeddings (embeddings use single content)
                    Content = req.Contents?.FirstOrDefault(),
                    // Note: TaskType, Title, OutputDimensionality would need to be added to InlinedRequest if needed
                }
            }).ToList();

            embeddingInputConfig.Requests = new EmbedRequestsWrapper { Requests = embedRequests };
        }
        else if (source.FileName != null)
        {
            embeddingInputConfig.FileName = source.FileName;
        }
        else
        {
            throw new ArgumentException("For Google AI, source must have either InlinedRequests or FileName.", nameof(source));
        }

        var requestBody = new EmbeddingBatchWrapper
        {
            Batch = new EmbeddingBatchConfig
            {
                InputConfig = embeddingInputConfig
            }
        };

        var response = await SendAsync<EmbeddingBatchWrapper, BatchJobResponse>(url, requestBody, HttpMethod.Post, cancellationToken).ConfigureAwait(false);

        // Extract metadata and copy top-level name
        var batchJob = response.Metadata ?? throw new InvalidOperationException("API response missing metadata");
        batchJob.Name = response.Name;
        return batchJob;
    }

    /// <summary>
    /// Retrieves a batch job by its name asynchronously.
    /// </summary>
    /// <param name="name">The resource name of the batch job to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the <see cref="BatchJob"/> details.</returns>
    /// <remarks>
    /// Use this method to check the status and progress of a batch job.
    /// The name should be in the format returned when the job was created.
    /// </remarks>
    /// <seealso href="https://ai.google.dev/api/batch#method:-batchjobs.get">See Official API Documentation</seealso>
    public async Task<BatchJobResponse> GetAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Batch job name cannot be null or empty.", nameof(name));

        

        // Google AI returns wrapped response, Vertex AI returns flat
        if (Platform is GoogleAIPlatformAdapter)
        {
            var url = $"{Platform.GetBaseUrl()}/{name}";
            return await GetAsync<BatchJobResponse>(url, cancellationToken).ConfigureAwait(false);
           
        }
        else
        {
            var url = $"{Platform.GetBaseUrl(true,false,false,false)}/{name}";
            return await GetAsync<BatchJobResponse>(url, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Lists batch jobs with pagination support.
    /// </summary>
    /// <param name="pageSize">The maximum number of batch jobs to return per page. If not specified, uses the server default.</param>
    /// <param name="pageToken">A page token from a previous call to retrieve the next page of results.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing a <see cref="ListBatchJobsResponse"/> with batch jobs and pagination details.</returns>
    /// <remarks>
    /// Use the <see cref="ListBatchJobsResponse.NextPageToken"/> from the response to retrieve subsequent pages.
    /// </remarks>
    /// <seealso href="https://ai.google.dev/api/batch#method:-batchjobs.list">See Official API Documentation</seealso>
    public async Task<ListBatchJobsResponse> ListAsync(
        int? pageSize = null,
        string? pageToken = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();

        if (pageSize.HasValue)
        {
            queryParams.Add($"pageSize={pageSize.Value}");
        }

        if (!string.IsNullOrEmpty(pageToken))
        {
            queryParams.Add($"pageToken={pageToken}");
        }

        var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;

        // Google AI and Vertex AI use different response structures
        if (Platform is GoogleAIPlatformAdapter)
        {
            var url = $"{Platform.GetBaseUrl()}/batches{queryString}";
            var googleResponse = await GetAsync<GoogleAIListBatchJobsResponse>(url, cancellationToken).ConfigureAwait(false);

            // Transform operations to batch jobs
            var batchJobs = googleResponse.Operations?.Select(op =>
            {
                var job = op.Metadata ?? throw new InvalidOperationException("Operation missing metadata");
                job.Name = op.Name;
                return job;
            }).ToList();

            return new ListBatchJobsResponse
            {
                NextPageToken = googleResponse.NextPageToken,
                BatchJobs = batchJobs
            };
        }
        else
        {
            var url = $"{Platform.GetBaseUrl(true,false)}/batchPredictionJobs{queryString}";
            var vertexResponse = await GetAsync<VertexAIListBatchJobsResponse>(url, cancellationToken).ConfigureAwait(false);

            return new ListBatchJobsResponse
            {
                NextPageToken = vertexResponse.NextPageToken,
                BatchJobs = vertexResponse.BatchPredictionJobs
            };
        }
    }

    /// <summary>
    /// Cancels a running batch job asynchronously.
    /// </summary>
    /// <param name="name">The resource name of the batch job to cancel.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous cancel operation.</returns>
    /// <remarks>
    /// Once cancelled, a batch job cannot be resumed. Partial results may be available
    /// depending on how much processing was completed before cancellation.
    /// </remarks>
    /// <seealso href="https://ai.google.dev/api/batch#method:-batchjobs.cancel">See Official API Documentation</seealso>
    public async Task CancelAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Batch job name cannot be null or empty.", nameof(name));

        var url = "";
        
        if(Platform is GoogleAIPlatformAdapter)
             url = $"{Platform.GetBaseUrl()}/{name}:cancel";
        else
        {
            url =  $"{Platform.GetBaseUrl(true, false, false,false)}/{name}";
        }

        // Cancel doesn't return any meaningful response, just need to send the POST request
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        await Platform.AddAuthorizationAsync(request, false, cancellationToken).ConfigureAwait(false);
        var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await CheckAndHandleErrors(response, url).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes a batch job asynchronously.
    /// </summary>
    /// <param name="name">The resource name of the batch job to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    /// <remarks>
    /// Deleting a batch job removes its metadata but does not affect output files
    /// that have already been written to the destination.
    /// </remarks>
    /// <seealso href="https://ai.google.dev/api/batch#method:-batchjobs.delete">See Official API Documentation</seealso>
    public async Task DeleteBatchJobAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Batch job name cannot be null or empty.", nameof(name));

        string url;
        if(Platform is GoogleAIPlatformAdapter)
            url = $"{Platform.GetBaseUrl()}/{name}";
        else
        {
            url = $"{Platform.GetBaseUrl(true,false,false,false)}/{name}";
        }
        await DeleteAsync(url, cancellationToken).ConfigureAwait(false);
    }
}
