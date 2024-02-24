using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GenerativeAI.Exceptions;
using GenerativeAI.Requests;
using GenerativeAI.Services.Classes;
using GenerativeAI.Types;
using static System.Net.WebRequestMethods;
using System.Net.Http;

namespace GenerativeAI.Services
{
    /// <summary>
    /// Service to Query Generative AI Models
    /// </summary>
    public class ModelInfoService
    {
        public HttpClient Client { get; protected set; }
        public string ApiKey { get; private set; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="apiKey">Google Generative AI API key</param>
        /// <param name="client">Http Client</param>
        public ModelInfoService(string apiKey, HttpClient? client = null)
        {
            this.ApiKey = apiKey;
            this.Client = client ?? new HttpClient();
        }
        /// <summary>
        /// Get All Generative AI Models
        /// </summary>
        /// <returns></returns>
        /// <exception cref="GenerativeAIException"></exception>
        public async Task<List<ModelInfo>> GetModelsAsync()
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models?key=" + this.ApiKey;

            var response = await Client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var result = await JsonSerializer.DeserializeAsync<GetModelsListResponse>(await response.Content.ReadAsStreamAsync());
                foreach (var resultModel in result.Models)
                {
                    resultModel.ModelId = resultModel.Name.Replace("models/", "");
                }
                return result.Models;
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();

                throw new GenerativeAIException($"Error while requesting https://generativelanguage.googleapis.com/v1beta/models?key=__API_Key__:\r\n\r\n{content}", content);
            }
        }
        /// <summary>
        /// Get Generative AI Model Info
        /// </summary>
        /// <param name="modelId">Model Id</param>
        /// <returns></returns>
        /// <exception cref="GenerativeAIException"></exception>
        public async Task<ModelInfo> GetModelInfoAsync(string modelId)
        {
            if (modelId.StartsWith("models/"))
                modelId = modelId.Substring("models/".Length);

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelId}?key=" + this.ApiKey;

            var response = await Client.GetAsync(new Uri(url)).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var result = await JsonSerializer.DeserializeAsync<ModelInfo>(await response.Content.ReadAsStreamAsync().ConfigureAwait(false)).ConfigureAwait(false);
                result.ModelId = result.Name.Replace("models/", "");
                return result;
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                throw new GenerativeAIException($"Error while requesting https://generativelanguage.googleapis.com/v1beta/models/{modelId}?key=__API_Key__:\r\n\r\n{content}", content);
            }
        }
    }
}
