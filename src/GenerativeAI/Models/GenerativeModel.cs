using GenerativeAI.Helpers;
using GenerativeAI.Methods;
using GenerativeAI.Types;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using GenerativeAI.Tools;
using System.Threading;
using GenerativeAI.Exceptions;

namespace GenerativeAI.Models
{
    /// <summary>
    /// Google Gemini Pro
    /// </summary>
    public class GenerativeModel : ModelBase
    {
        #region Properties
        public string Model { get; set; }
        public GenerationConfig Config { get; set; }
        public SafetySetting[] SafetySettings { get; set; }
        private string ApiKey { get; set; }
        public bool AutoCallFunction { get; set; } = true;
        public bool AutoReplyFunction { get; set; } = true;
        public List<ChatCompletionFunction>? Functions { get; set; }
        public bool FunctionEnabled { get; set; } = true;
        public bool AutoHandleBadFunctionCalls { get; set; } = false;
        public IDictionary<string, Func<string, CancellationToken, Task<string>>> Calls { get; set; } =
            new Dictionary<string, Func<string, CancellationToken, Task<string>>>();

        #endregion

        #region Contructors
        public GenerativeModel(string apiKey, ModelParams modelParams, HttpClient? client = null, ICollection<ChatCompletionFunction>? functions = null, IReadOnlyDictionary<string, Func<string, CancellationToken, Task<string>>>? calls = null)
        {
            if (modelParams.Model != null && modelParams.Model.StartsWith("models/"))
            {
                this.Model = modelParams.Model.Split(new[] { "model/" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }
            else
            {
                this.Model = modelParams.Model ?? "gemini-pro";
            }
            this.ApiKey = apiKey;
            

            InitClient(client,modelParams,functions,calls);
        }

        private void InitClient(HttpClient client, ModelParams? modelParams, ICollection<ChatCompletionFunction> functions, IReadOnlyDictionary<string, Func<string, CancellationToken, Task<string>>> calls)
        {
            if (modelParams != null)
            {
                this.Config = modelParams.GenerationConfig ?? new GenerationConfig() { Temperature = 0.8, MaxOutputTokens = 2048 };
                this.SafetySettings = modelParams.SafetySettings ?? new List<SafetySetting>().ToArray();
            }
            else
            {
                this.Config = new GenerationConfig(){Temperature = 0.8,MaxOutputTokens = 2048};
                this.SafetySettings =  new List<SafetySetting>().ToArray();

            }

            if (functions != null)
                this.Functions = functions.ToList();
            else FunctionEnabled = false;

            if (calls != null)
            {
                foreach (var call in calls)
                {
                    this.Calls.Add(call.Key, call.Value);
                }
            }
            if (client == null)
            {
                this.Client = new HttpClient() { Timeout = new TimeSpan(0, 10, 0) };
            }
            else
                Client = client;
        }

        //private void InitClient(HttpClient? client)
        //{
        //    if (client == null)
        //    {
        //        this.Client = new HttpClient() { Timeout = new TimeSpan(0, 10, 0) };
        //    }
        //    else
        //        Client = client;
        //}

        public GenerativeModel(string apiKey, string model = "gemini-pro", HttpClient? client = null, ICollection<ChatCompletionFunction>? functions = null, IReadOnlyDictionary<string, Func<string, CancellationToken, Task<string>>>? calls = null)
        {
            this.ApiKey = apiKey;
            this.Model = model;
            this.Config = new GenerationConfig();
            this.SafetySettings = new List<SafetySetting>().ToArray();
            if (functions != null)
                this.Functions = functions.ToList();
            if (calls != null)
            {
                foreach (var call in calls)
                {
                    this.Calls.Add(call.Key, call.Value);
                }
            }

            InitClient(client,null,functions,calls);
        }
        #endregion

        #region public Methods
        /// <summary>
        /// Add Global Extension Functions
        /// </summary>
        /// <param name="functions">Extension Functions</param>
        /// <param name="calls">Function Call Map</param>
        public void AddGlobalFunctions(ICollection<ChatCompletionFunction>? functions,
            IReadOnlyDictionary<string, Func<string, CancellationToken, Task<string>>>? calls)
        {
            if (functions == null)
                return;
            this.Functions ??= new List<ChatCompletionFunction>();
            this.Functions.AddRange(functions);
            this.FunctionEnabled = true;
            calls ??= new Dictionary<string, Func<string, CancellationToken, Task<string>>>();

            foreach (var call in calls)
            {
                this.Calls.Add(call.Key, call.Value);
            }
        }
        /// <summary>
        /// Generate Content
        /// </summary>
        /// <param name="request">Generate Content Request</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns></returns>
        public async Task<EnhancedGenerateContentResponse> GenerateContentAsync(GenerateContentRequest request, CancellationToken cancellationToken = default)
        {
            if(request.GenerationConfig == null)
                request.GenerationConfig = this.Config;
            if(request.SafetySettings == null)
                request.SafetySettings = this.SafetySettings;
            if (FunctionEnabled && this.Functions != null)
            {
                request.Tools = new List<GenerativeAITool>(new[]{new GenerativeAITool()
                {
                    FunctionDeclaration = this.Functions
                }});
            }
            var res = await GenerateContent(this.ApiKey, this.Model, request).ConfigureAwait(false);
            return await CallFunction(request, res, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Generate Content with Parts
        /// </summary>
        /// <param name="parts">Request Content Parts</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<EnhancedGenerateContentResponse> GenerateContentAsync(IEnumerable<Part> parts, CancellationToken cancellationToken = default)
        {
            var request = new GenerateContentRequest()
            {
                Contents = new[] { RequestExtensions.FormatGenerateContentInput(parts) }
            };
            
            return await GenerateContentAsync(request, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Generate Content with Text Message
        /// </summary>
        /// <param name="message">Text Message to Send to Model</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns></returns>
        public async Task<string?> GenerateContentAsync(string message, CancellationToken cancellationToken = default)
        {
            var content = RequestExtensions.FormatGenerateContentInput(message);
            var req = new GenerateContentRequest()
            {
                Contents = new[] { content },
                GenerationConfig = this.Config,
                SafetySettings = this.SafetySettings
            };

            if (FunctionEnabled && this.Functions != null)
            {
                req.Tools = new List<GenerativeAITool>(new[]{new GenerativeAITool()
                    {
                        FunctionDeclaration = this.Functions
                    }});
            }

            var res = await GenerateContent(this.ApiKey, this.Model, req);

            res = await CallFunction(req, res, cancellationToken).ConfigureAwait(false);
            return res.Text();
        }

        private async Task<EnhancedGenerateContentResponse> CallFunction(GenerateContentRequest req, EnhancedGenerateContentResponse res, CancellationToken cancellationToken = default)
        {
            if (AutoCallFunction && res.GetFunction() != null)
            {
                var function = res.GetFunction();
                var name = function.Name ?? string.Empty;
                var jsonResult = "";
                if (Calls.ContainsKey(name))
                {
                    var func = Calls[name];
                    var args = function.Arguments != null
                        ? JsonSerializer.Serialize(function.Arguments, SerializerOptions)
                        : string.Empty;
                    jsonResult = await func(args, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    if (!AutoHandleBadFunctionCalls)
                        throw new GenerativeAIException($"AI Model called an invalid function. function_name: {name}",$"Invalid function_name: {name}");
                    res.Candidates[0].Content.Parts[0].FunctionCall.Name = "InvalidName";
                    name = "InvalidName";
                    jsonResult = "{\"error\":\"Invalid Function name or function doesn't exist. Please provide proper function name.\"}";
                }
                if (AutoReplyFunction)
                {
                    var responseContent = JsonSerializer.Deserialize<JsonNode>(jsonResult, SerializerOptions);

                    var content = new Content() { Role = Roles.Function };
                    content.Parts = new[]
                    {
                            new Part()
                            {
                                FunctionResponse = new ChatFunctionResponse()
                                {
                                    Name = name,
                                    Response = new FunctionResponse() { Name = name, Content = responseContent }
                                }
                            }
                        };

                    var contents = new List<Content>();
                    if (req.Contents != null)
                        contents.AddRange(req.Contents);

                    contents.Add(new Content(res.Candidates[0].Content.Parts, res.Candidates[0].Content.Role));
                    contents.Add(content);
                    res = await GenerateContentAsync(new GenerateContentRequest() { Contents = contents.ToArray() },
                        cancellationToken);
                }
                //}
                //else
                //{

                //    throw new Exception("InvalidFunction: Model called a non existing function. Please try again");
                //}
            }

            return res;
        }

        /// <summary>
        /// Start a Chat Session
        /// </summary>
        /// <param name="startSessionParams">Session Params with Chat History</param>
        /// <returns>ChatSession Object</returns>
        public ChatSession StartChat(StartChatParams startSessionParams)
        {
            return new ChatSession(this, startSessionParams);
        }
        /// <summary>
        /// Count Content Tokens
        /// </summary>
        /// <param name="request">CountTokenRequest with Contents array</param>
        /// <returns>Number of token in content</returns>

        public async Task<CountTokensResponse> CountTokens(CountTokensRequest request)
        {
            return await CountTokens(this.ApiKey, this.Model, request).ConfigureAwait(false);
        }
        #endregion

        /// <summary>
        /// Disable Global Functions
        /// </summary>
        public void DisableFunctions()
        {
            FunctionEnabled = false;
        }

        /// <summary>
        /// Enable Global Functions
        /// </summary>
        public void EnableFunctions()
        {
            FunctionEnabled = true;
        }
    }
}
