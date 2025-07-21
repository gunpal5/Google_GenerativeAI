using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using GenerativeAI.Exceptions;
using GenerativeAI.Logging;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Core
{
    /// <summary>
    /// Base class for handling API requests and responses.
    /// </summary>
    public abstract class ApiBase
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger? _logger;
        /// <summary>
        /// Gets the logger instance for diagnostic and error logging.
        /// </summary>
        protected ILogger? Logger => _logger;

        /// <summary>
        /// Provides an HTTP client for sending and receiving HTTP requests and responses.
        /// This property is used to facilitate communication with APIs or services.
        /// </summary>
        protected HttpClient HttpClient => _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiBase"/> class.
        /// </summary>
        /// <param name="httpClient">HTTP client used for API requests.</param>
        /// <param name="logger">Optional. The logger instance for logging API interactions.</param>
        protected ApiBase(HttpClient? httpClient, ILogger? logger = null)
        {
            _httpClient = httpClient ?? new HttpClient()
            {
                Timeout = new TimeSpan(0,5,0),
                DefaultRequestHeaders =
                {
                    { "User-Agent", "Google_GenerativeAI SDK" }
                }
            };
            _logger = logger;

            SerializerOptions = DefaultSerializerOptions.Options;
        }

        /// <summary>
        /// JSON serialization options used for API requests and responses.
        /// </summary>
        protected JsonSerializerOptions? SerializerOptions { get; } 

        /// <summary>
        /// Adds authorization headers to an HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request where headers will be added.</param>
        /// <param name="requireAccessToken">Whether an access token is required for the request.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <remarks>
        /// Override this method in derived classes to dynamically add authorization headers.
        /// By default, this implementation does nothing.
        /// </remarks>
        protected virtual Task AddAuthorizationHeader(HttpRequestMessage request, bool requireAccessToken = false, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
            // No action in the base class; override in derived classes to add specific headers.
        }

        /// <summary>
        /// Sends a GET request to the specified URL.
        /// </summary>
        /// <param name="url">The full URL of the API endpoint.</param>
        /// <param name="cancellationToken">Token to propagate notification that the operation should be canceled.</param>
        /// <typeparam name="T">The type to deserialize the response into.</typeparam>
        /// <returns>The deserialized object from the response.</returns>
        /// <exception cref="HttpRequestException">Throws upon a non-success status code.</exception>
        /// <exception cref="InvalidOperationException">Throws if deserialization fails.</exception>
        protected async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogGetRequest(url);

                var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));

                await AddAuthorizationHeader(request, false, cancellationToken).ConfigureAwait(false);

                // Send GET request
                var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

                await CheckAndHandleErrors(response, url).ConfigureAwait(false);
#if NET5_0_OR_GREATER
                var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif
                _logger?.LogSuccessfulGetResponse(url, content);

                // Deserialize and return the response
                if (SerializerOptions == null)
                    throw new InvalidOperationException("SerializerOptions is not initialized");
                var typeInfo = SerializerOptions.GetTypeInfo(typeof(T));
                if (typeInfo == null)
                    throw new InvalidOperationException($"Could not get type info for {typeof(T)}");
                return JsonSerializer.Deserialize(content, (JsonTypeInfo<T>) typeInfo) ??
                       throw new InvalidOperationException("Deserialized response is null.");
            }
            catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
            {
                _logger?.LogGetRequestCanceled();
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogException(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Sends an HTTP request (e.g., POST, PUT, or PATCH) to the specified URL.
        /// </summary>
        /// <param name="url">The full URL of the API endpoint.</param>
        /// <param name="payload">The request payload to send in the body.</param>
        /// <param name="method">The HTTP method (e.g., POST, PUT, PATCH).</param>
        /// <param name="cancellationToken">Token to propagate notification that the operation should be canceled.</param>
        /// <typeparam name="TRequest">The type of the request payload.</typeparam>
        /// <typeparam name="TResponse">The type to deserialize the response into.</typeparam>
        /// <returns>The deserialized object from the response.</returns>
        /// <exception cref="HttpRequestException">Throws upon a non-success status code.</exception>
        /// <exception cref="InvalidOperationException">Throws if deserialization fails.</exception>
        protected async Task<TResponse> SendAsync<TRequest, TResponse>(string url, TRequest payload, HttpMethod method,
            CancellationToken cancellationToken = default)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(method);
#else
            if (method == null) throw new ArgumentNullException(nameof(method));
#endif
            try
            {
                _logger?.LogHttpRequest(method.Method, url, payload);

                // Serialize payload and create request
                if (SerializerOptions == null)
                    throw new InvalidOperationException("SerializerOptions is not initialized");
                var typeInfo = SerializerOptions.GetTypeInfo(typeof(TRequest));
                if (typeInfo == null)
                    throw new InvalidOperationException($"Could not get type info for {typeof(TRequest)}");
                var jsonPayload = JsonSerializer.Serialize(payload, typeInfo);
                using var request = new HttpRequestMessage(method, url)
                {
                    Content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json")
                };

                await AddAuthorizationHeader(request, false, cancellationToken).ConfigureAwait(false);

                // Send HTTP request
                var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

                await CheckAndHandleErrors(response, url).ConfigureAwait(false);
                var result = await Deserialize<TResponse>(response).ConfigureAwait(false);
                if (result == null)
                    throw new InvalidOperationException($"Failed to deserialize response from {url}. The server returned null or invalid data.");
                return result;
            }
            catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
            {
                _logger?.LogHttpRequestCanceled();
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogException(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Checks if the HTTP response indicates a successful status, and throws an exception with detailed error information otherwise.
        /// </summary>
        /// <param name="response">The HTTP response to evaluate.</param>
        /// <param name="url">The URL associated with the HTTP request, used for logging or exception messages.</param>
        /// <exception cref="ApiException">
        /// Thrown when the API returns an error with a valid "error" object containing status, code, or message details.
        /// </exception>
        /// <exception cref="HttpRequestException">
        /// Thrown when the HTTP response indicates a failure, but no detailed "error" object is present in the response body.
        /// </exception>
        protected async Task CheckAndHandleErrors(HttpResponseMessage response, string url)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(response);
#else
            if (response == null) throw new ArgumentNullException(nameof(response));
#endif
            if (!response.IsSuccessStatusCode)
            {
                _logger?.LogNonSuccessStatusCode((int)response.StatusCode, url.MaskApiKey());

                try
                {
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    var errorDocument = JsonDocument.Parse(content);
                    var error = errorDocument.RootElement.GetProperty("error");
                    if (error.ValueKind == JsonValueKind.Null)
                        throw new Exception();
                    error.TryGetProperty("status", out var status);
                    error.TryGetProperty("message", out var message);
                    error.TryGetProperty("code", out var code);
                    if (message.ValueKind == JsonValueKind.Null)
                    {
                        throw new Exception();
                    }

                    throw new ApiException(code.GetInt32(), message.GetString() ?? "Unknown error", status.GetString() ?? "Unknown status");
                }
                catch (ApiException)
                {
                    throw;
                }
                catch
                {
                    throw new HttpRequestException(
                        $"Request to {url.MaskApiKey()} failed with status code {response.StatusCode}");
                }
            }
        }

        /// <summary>
        /// Deserializes a JSON string into an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize into.</typeparam>
        /// <param name="response">The response to deserialize.</param>
        /// <returns>The deserialized object of type T, or null if deserialization fails.</returns>
        protected async Task<T?> Deserialize<T>(HttpResponseMessage response)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(response);
#else
            if (response == null) throw new ArgumentNullException(nameof(response));
#endif
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return Deserialize<T>(responseContent);
        }

        /// <summary>
        /// Deserializes a JSON string into an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize into.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized object of type T, or null if deserialization fails.</returns>
        protected T? Deserialize<T>(string json)
        {
            if (SerializerOptions == null)
                throw new InvalidOperationException("SerializerOptions is not initialized");
            var typeInfo = SerializerOptions.GetTypeInfo(typeof(T));
            if (typeInfo == null)
                throw new InvalidOperationException($"Could not get type info for {typeof(T)}");
            return (T?) JsonSerializer.Deserialize(json, typeInfo);
        }

        /// <summary>
        /// Sends a DELETE request to the specified URL.
        /// </summary>
        /// <param name="url">The full URL of the API endpoint.</param>
        /// <param name="cancellationToken">Token to propagate notification that the operation should be canceled.</param>
        /// <returns>A boolean indicating success of the operation.</returns>
        /// <exception cref="HttpRequestException">Throws upon a non-success status code.</exception>
        protected async Task<bool> DeleteAsync(string url, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogHttpRequest("DELETE", url, null);

                using var request = new HttpRequestMessage(HttpMethod.Delete, url);

                await AddAuthorizationHeader(request, false, cancellationToken).ConfigureAwait(false);

                // Send DELETE request
                var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

                await CheckAndHandleErrors(response, url).ConfigureAwait(false);

                _logger?.LogSuccessfulHttpResponse(url, null);
                return true; // DELETE requests typically do not return a response body
            }
            catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
            {
                _logger?.LogHttpRequestCanceled();
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogException(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Uploads a file to the specified URL with progress reporting.
        /// </summary>
        /// <param name="url">The full URL of the API endpoint.</param>
        /// <param name="filePath">The local path to the file to be uploaded.</param>
        /// <param name="progress">A callback function to report upload progress as a percentage.</param>
        /// <param name="additionalHeaders">Optional. Additional headers to add to the request.</param>
        /// <param name="cancellationToken">Token to propagate notification that the operation should be canceled.</param>
        /// <returns>The server's response as a string.</returns>
        protected async Task<string> UploadFileWithProgressAsync(
            string url,
            string filePath,
            Action<double> progress,
            Dictionary<string, string>? additionalHeaders = null,
            CancellationToken cancellationToken = default)
        {
            var mimeType = MimeTypeMap.GetMimeType(filePath);
            var fileName = System.IO.Path.GetFileName(filePath);
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return await UploadFileWithProgressAsync(fileStream, fileName, mimeType, url, progress, additionalHeaders,
                cancellationToken).ConfigureAwait(false);
        }


        /// <summary>
        /// Uploads a file asynchronously to the specified URL with progress tracking.
        /// </summary>
        /// <param name="stream">The stream containing the file data to upload.</param>
        /// <param name="url">The destination URL to upload the file.</param>
        /// <param name="filePath">The full path to the file to upload.</param>
        /// <param name="mimeType">The MIME type of the file being uploaded.</param>
        /// <param name="progress">An action to report progress as a percentage between 0 and 100.</param>
        /// <param name="additionalHeaders">Optional. A dictionary of additional headers to include with the upload request.</param>
        /// <param name="cancellationToken">Optional. A token to cancel the file upload operation.</param>
        /// <returns>A task that represents the asynchronous upload operation. The task result contains the response string from the server.</returns>
        protected async Task<string> UploadFileWithProgressAsync(Stream stream,
            string filePath,
            string mimeType,
            string url,
            Action<double> progress,
            Dictionary<string, string>? additionalHeaders = null,
            CancellationToken cancellationToken = default)
        {
            var content = new ProgressStreamContent(stream, progress);

            using var form = new MultipartFormDataContent();
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            form.Add(content, "file", filePath);

            if (additionalHeaders != null)
            {
                foreach (var header in additionalHeaders)
                {
                    form.Headers.Add(header.Key, header.Value);
                }
            }

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = form
                };

                await AddAuthorizationHeader(request, false, cancellationToken).ConfigureAwait(false);

                var response = await _httpClient
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                    .ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    _logger?.LogNonSuccessStatusCode((int)response.StatusCode, url.MaskApiKey());
                    throw new HttpRequestException(
                        $"File upload to {url.MaskApiKey()} failed with status code {response.StatusCode}");
                }

#if NET5_0_OR_GREATER
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
                var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif
                _logger?.LogSuccessfulHttpResponse(url, responseContent);

                return responseContent;
            }
            catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
            {
                _logger?.LogHttpRequestCanceled();
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogException(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Streams responses by sending an HTTP request with a JSON payload and
        /// yielding the deserialized items as they arrive.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request payload to serialize.</typeparam>
        /// <typeparam name="TResponse">The type of the response items to deserialize.</typeparam>
        /// <param name="payload">The request payload.</param>
        /// <param name="url">The target endpoint URL to which the request is sent.</param>
        /// <param name="cancellationToken">A token for cancelling the operation.</param>
        /// <returns>An async stream of response items.</returns>
        protected async IAsyncEnumerable<TResponse> StreamAsync<TRequest, TResponse>(
            string url,
            TRequest payload,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Serialize the request payload into a MemoryStream
            using var ms = new MemoryStream();
            if (SerializerOptions == null)
                throw new InvalidOperationException("SerializerOptions is not initialized");
            var typeInfo = SerializerOptions.GetTypeInfo(typeof(TRequest));
            if (typeInfo == null)
                throw new InvalidOperationException($"Could not get type info for {typeof(TRequest)}");
            await JsonSerializer.SerializeAsync(ms, payload, typeInfo, cancellationToken).ConfigureAwait(false);
            ms.Seek(0, SeekOrigin.Begin);

            // Prepare an HTTP request message
            using var request = new HttpRequestMessage(HttpMethod.Post, url);

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            // Attach the serialized payload as StreamContent
            using var requestContent = new StreamContent(ms);
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Content = requestContent;
            await AddAuthorizationHeader(request, false, cancellationToken).ConfigureAwait(false);
            // Call your existing SendAsync method (assumed to handle HttpCompletionOption, etc.)
            using var response = await HttpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            // Ensure success (or handle the status code if needed)
            response.EnsureSuccessStatusCode();

            // Read and deserialize the response stream asynchronously
            if (response.Content != null)
            {
                #if NETSTANDARD2_0 || NET462_OR_GREATER
                using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

                #else
                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                #endif
                
                if (SerializerOptions == null)
                    throw new InvalidOperationException("SerializerOptions is not initialized");
                var responseTypeInfo = SerializerOptions.GetTypeInfo(typeof(TResponse));
                if (responseTypeInfo == null)
                    throw new InvalidOperationException($"Could not get type info for {typeof(TResponse)}");
                
                await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable(
                                   stream,
                                   (JsonTypeInfo<TResponse>)responseTypeInfo,
                                   cancellationToken).ConfigureAwait(false)
                              )
                {
                    if (cancellationToken.IsCancellationRequested)
                        yield break;

                    if (item != null)
                        yield return item;
                }
            }
        }
    }
}