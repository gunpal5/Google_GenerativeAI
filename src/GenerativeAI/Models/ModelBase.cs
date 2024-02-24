using GenerativeAI.Requests;
using GenerativeAI.Types;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GenerativeAI.Exceptions;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Reflection;
using System.Net;

namespace GenerativeAI.Models
{
    public abstract class ModelBase
    {
        public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com";
        public string Version { get; set; } = "v1beta";
        
        public HttpClient Client { get; protected set; }
        public TimeSpan Timeout
        {
            get => Client.Timeout;
            set => Client.Timeout = value;
        }

        protected JsonSerializerOptions SerializerOptions => new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, Converters = { new JsonStringEnumConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        protected virtual async Task<EnhancedGenerateContentResponse> GenerateContent(string apiKey, string model, GenerateContentRequest request)
        {
            var url = new RequestUrl(model, Tasks.GenerateContent, apiKey, false, BaseUrl, Version);

            var json = JsonSerializer.Serialize(request, SerializerOptions);

            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await Client.PostAsync(url, stringContent);
            if (response.IsSuccessStatusCode)
            {
                var result = await JsonSerializer.DeserializeAsync<EnhancedGenerateContentResponse>(await response.Content.ReadAsStreamAsync(), SerializerOptions);

                if (!(result.Candidates is { Length: > 0 }))
                {
                    var blockErrorMessage = ResponseHelper.FormatBlockErrorMessage(result);
                    if (!string.IsNullOrEmpty(blockErrorMessage))
                    {
                        throw new GenerativeAIException($"Error while requesting {url.ToString("__API_Key__")}:\r\n\r\n{blockErrorMessage}",blockErrorMessage);
                    }
                }

                return result;
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();

                throw new GenerativeAIException($"Error while requesting {url.ToString("__API_Key__")}:\r\n\r\n{content}",content);
            }
        }

        protected virtual async IAsyncEnumerable<string> StreamContentAsync(string apiKey, string model, GenerateContentRequest request2,CancellationToken cancellationToken = default)
        {
            
            var url = new RequestUrl(model, Tasks.StreamGenerateContent, apiKey, false, BaseUrl, Version);

            var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, request2, SerializerOptions, cancellationToken);
            ms.Seek(0, SeekOrigin.Begin);

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            using var requestContent = new StreamContent(ms);
            request.Content = requestContent;
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            using var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                using (var responseStream = await response.Content.ReadAsStreamAsync())
                using (var streamReader = new StreamReader(responseStream))
                {
                    string line = string.Empty;
                    while ((line = await streamReader.ReadLineAsync()) != null)
                    {
                        if (line.Contains(@"""text"""))
                        {
                            var jsonString = "{" + line + "}";
                            var jsonObject = JsonSerializer.Deserialize<JsonObject>(jsonString);
                            yield return jsonObject?["text"]?.ToString();
                        }
                    }
                }
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new GenerativeAIException($"Error while requesting {url.ToString("__API_Key__")}:\r\n\r\n{content}", content);
            }
        }
        protected virtual async Task<CountTokensResponse> CountTokens(string apiKey, string model,
            CountTokensRequest request)
        {
            var url = new RequestUrl(model, Tasks.CountTokens, apiKey, false, this.BaseUrl, this.Version);

            var json = JsonSerializer.Serialize(request, GoogleSerializerContext.Default.CountTokensRequest);

            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await Client.PostAsync(url, stringContent);
            if (response.IsSuccessStatusCode)
            {
                var result = await JsonSerializer.DeserializeAsync(await response.Content.ReadAsStreamAsync(),
                    GoogleSerializerContext.Default.CountTokensResponse);
                return result;
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new GenerativeAIException($"Error while requesting {url.ToString("__API_Key__")}: \r\n\r\n{content}",content);
            }
        }
    }
}
