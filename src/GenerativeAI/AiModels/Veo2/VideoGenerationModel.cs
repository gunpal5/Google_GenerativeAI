using System.Diagnostics;
using GenerativeAI.Clients;
using GenerativeAI.Core;
using GenerativeAI.Exceptions;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI
{
    /// <summary>
    /// A client for interacting with the Gemini API for video generation tasks.
    /// Note: Video generation capabilities might be specific to certain API versions or platforms (e.g., Vertex AI).
    /// </summary>
    public class VideoGenerationModel : BaseClient
    {
        /// <summary>
        /// Gets or sets the name of the video generation model.
        /// </summary>
        public string ModelName { get; set; }

        private readonly OperationsClient _operationsClient;
        private const int LongRunningOperationTimeout = 60 * 1000 * 10;

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoGenerationModel"/> class.
        /// </summary>
        /// <param name="model">The name of the video generation model.</param>
        /// <param name="platform">The platform adapter for integrating with the API.</param>
        /// <param name="httpClient">Optional HTTP client instance for making API calls.</param>
        /// <param name="logger">Optional logger instance for logging operations.</param>
        public VideoGenerationModel(IPlatformAdapter platform, string model = VertexAIModels.Video.Veo2Generate001,
            HttpClient? httpClient = null,
            ILogger? logger = null)
            : base(platform, httpClient, logger)
        {
            this.ModelName = model;
            _operationsClient = new OperationsClient(platform, httpClient, logger);
        }

        /// <summary>
        /// Generates videos based on a text prompt and optional image input and configuration.
        /// This typically initiates a long-running operation.
        /// </summary>
        /// <param name="request">The request containing the prompt, optional image, and configuration for video generation.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="GenerateVideosOperation"/> representing the long-running video generation task.</returns>
        public async Task<GenerateVideosOperation?> GenerateVideosAsync(GenerateVideosRequest request,
            CancellationToken cancellationToken = default)
        {
            var modelId = this.ModelName.ToModelId();
            var url = $"{_platform.GetBaseUrl()}/{modelId}:predictLongRunning";

            var payload = new VertexGenerateVideosPayload()
            {
                Instances = new List<VideoInstance>
                {
                    new VideoInstance
                    {
                        Prompt = request.Prompt,
                        Image = request.Image
                    }
                },
                Parameters = request.Config != null
                    ? new VideoParameters
                    {
                        SampleCount = request.Config.NumberOfVideos,
                        StorageUri = request.Config.OutputGcsUri,
                        Fps = request.Config.Fps,
                        DurationSeconds = request.Config.DurationSeconds,
                        Seed = request.Config.Seed,
                        AspectRatio = request.Config.AspectRatio,
                        Resolution = request.Config.Resolution,
                        PersonGeneration = request.Config.PersonGeneration,
                        NegativePrompt = request.Config.NegativePrompt,
                        EnhancePrompt = request.Config.EnhancePrompt,
                        PubSubTopic = request.Config.PubsubTopic
                    }
                    : null
            };

            return await SendAsync<VertexGenerateVideosPayload, GenerateVideosOperation>(url, payload, HttpMethod.Post,
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Generates videos based on a specified text prompt, an optional reference image,
        /// and configuration settings. This operation typically initiates a long-running task.
        /// </summary>
        /// <param name="prompt">The text prompt to guide the video generation process.</param>
        /// <param name="referenceImage">An optional reference image to influence the video's style or content.</param>
        /// <param name="config">Optional configuration settings for customizing the video generation.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the operation to complete, allowing it to be cancelled.</param>
        /// <returns>A <see cref="GenerateVideosOperation"/> representing the long-running operation for video generation.</returns>
        public async Task<GenerateVideosOperation?> GenerateVideosAsync(string prompt,
            ImageSample? referenceImage = null, GenerateVideosConfig? config = null,
            CancellationToken cancellationToken = default)
        {
            var request = new GenerateVideosRequest()
            {
                Prompt = prompt,
                Image = referenceImage,
                Config = config
            };

            return await GenerateVideosAsync(request, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Awaits the completion of a long-running operation specified by its operation ID.
        /// This method repeatedly checks the operation's progress until it's complete or until the specified timeout is reached.
        /// </summary>
        /// <param name="operationId">The unique identifier of the long-running operation to monitor.</param>
        /// <param name="timeOut">Optional timeout (in milliseconds) for awaiting the operation. If not provided, the default timeout specified by LongRunningOperationTimeout is used.</param>
        /// <param name="cancellationToken">An optional token to observe and respond to cancellation requests.</param>
        /// <returns>
        /// Returns a <see cref="GoogleLongRunningOperation"/> instance representing the final state of the long-running operation upon completion.
        /// Throws an exception if the operation encounters an error.
        /// </returns>
        public async Task<GenerateVideosOperation?> AwaitForLongRunningOperation(string operationId,
            int? timeOut = null,
            CancellationToken cancellationToken = default)
        {
            GenerateVideosOperation? longRunningOperation = null;
            timeOut ??= LongRunningOperationTimeout;
            var sw = new Stopwatch();
            sw.Start();
            do
            {
                longRunningOperation =
                    await GetVideoGenerationStatusAsync(operationId, timeOut, cancellationToken).ConfigureAwait(false);

                if(longRunningOperation.Done == false)
                    await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
            } while (longRunningOperation.Done != true && sw.ElapsedMilliseconds < LongRunningOperationTimeout);

            if (longRunningOperation.Done == true && longRunningOperation.Error != null)
            {
                throw new VertexAIException(longRunningOperation.Error.Message, longRunningOperation.Error);
            }

            return longRunningOperation;
        }

        /// <summary>
        /// Awaits the completion of a long-running operation specified by its operation ID.
        /// This method repeatedly checks the operation's progress until it's complete or until the specified timeout is reached.
        /// </summary>
        /// <param name="operationId">The unique identifier of the long-running operation to monitor.</param>
        /// <param name="timeOut">Optional timeout (in milliseconds) for awaiting the operation. If not provided, the default timeout specified by LongRunningOperationTimeout is used.</param>
        /// <param name="cancellationToken">An optional token to observe and respond to cancellation requests.</param>
        /// <returns>
        /// Returns a <see cref="GoogleLongRunningOperation"/> instance representing the final state of the long-running operation upon completion.
        /// Throws an exception if the operation encounters an error.
        /// </returns>
        public async Task<GenerateVideosOperation?> GetVideoGenerationStatusAsync(string operationId,
            int? timeOut = null,
            CancellationToken cancellationToken = default)
        {
            var longRunningOperation =
                await _operationsClient.FetchOperationStatusAsync(operationId, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            return new GenerateVideosOperation(longRunningOperation);
        }
    }
}