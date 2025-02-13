using GenerativeAI.Types;

namespace GenerativeAI;

public partial class GenerativeModel
{
    /// <summary>
    /// Indicates whether grounding is enabled. Grounding integrates external tools, such as Google Search Retrieval,
    /// to support specific generative model requests for enhanced content generation.
    /// </summary>
    /// <remarks>
    /// Use Google Search Tool for Latest Models
    /// </remarks>
    public bool UseGrounding { get; set; } = false;

    /// <summary>
    /// Specifies whether the Google Search integration is enabled.
    /// This feature allows utilizing the Google Search Tool within the generative model
    /// for enhanced search and retrieval capabilities.
    /// </summary>
    /// <remarks>
    /// Enabling this property incorporates Google Search as a tool for the generative model,
    /// providing dynamic search support based on the requested content needs.
    /// </remarks>
    public bool UseGoogleSearch { get; set; } = false;

    /// <summary>
    /// Indicates whether the code execution tool is enabled. The code execution tool facilitates the integration
    /// of code execution capabilities for processing specific requests and enhancing model functionalities.
    /// </summary>
    /// <remarks>
    /// Use the Code Execution Tool for tasks requiring direct code evaluation or execution within the generative model.
    /// This feature is incompatible with JSON mode or cached content mode.
    /// </remarks>
    public bool UseCodeExecutionTool { get; set; } = false;

    /// <summary>
    /// Determines whether JSON mode is enabled. JSON mode adjusts the content generation response
    /// to specifically produce outputs in JSON format as defined in generation configurations.
    /// </summary>
    /// <remarks>
    /// JSON mode is incompatible with grounding, Google Search, and code execution tools.
    /// Enabling this mode will override other response formats with "application/json".
    /// </remarks>
    public bool UseJsonMode { get; set; } = false;

    public Tool DefaultSearchTool = new Tool() { GoogleSearch = new GoogleSearchTool() };
    public Tool DefaultGoogleSearchRetrieval = new Tool() { GoogleSearchRetrieval = new GoogleSearchRetrievalTool(){DynamicRetrievalConfig = new DynamicRetrievalConfig(){DynamicThreshold = 1,Mode = DynamicRetrievalMode.MODE_DYNAMIC}} };
    public Tool DefaultCodeExecutionTool = new Tool() { CodeExecution = new CodeExecutionTool() };
    
    
    #region Public Methods Related to Tools
    // /// <summary>
    // /// Add Global Extension Functions
    // /// </summary>
    // /// <param name="functions">Extension Functions</param>
    // /// <param name="calls">Function Call Map</param>
    // public void AddGlobalFunctions(
    //     ICollection<ChatCompletionFunction>? functions,
    //     IReadOnlyDictionary<string, Func<string, CancellationToken, Task<string>>>? calls)
    // {
    //     if (functions == null) return;
    //     Functions ??= new List<ChatCompletionFunction>();
    //     Functions.AddRange(functions);
    //     FunctionEnabled = true;
    //
    //     calls ??= new Dictionary<string, Func<string, CancellationToken, Task<string>>>();
    //     foreach (var call in calls)
    //     {
    //         Calls[call.Key] = call.Value;
    //     }
    // }
    
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

    #endregion
    
    #region Private Helpers

    private void AddTools(GenerateContentRequest request)
    {
        //ToDo Implement Function Calling
        // if (FunctionEnabled && Functions != null)
        // {
        //     request.Tools = new List<GenerativeAITool>
        //     {
        //         new GenerativeAITool { FunctionDeclaration = Functions }
        //     };
        // }

        if (UseGrounding)
        {
            request.Tools ??= new List<Tool>();
            if(request.Tools.All(s => s.GoogleSearchRetrieval == null))
                request.Tools.Add(DefaultGoogleSearchRetrieval);
        }
        
        if (UseGoogleSearch)
        {
            request.Tools ??= new List<Tool>();
            if(request.Tools.All(s => s.GoogleSearch == null))
                request.Tools.Add(DefaultSearchTool);
        }
        if (UseCodeExecutionTool)
        {
            request.Tools ??= new List<Tool>();
            if(request.Tools.All(s => s.CodeExecution == null))
                request.Tools.Add(DefaultCodeExecutionTool);
        }
    }

    /// <summary>
    /// Handles invoking a function if the response requests one
    /// and optionally feeding the response back into the model
    /// </summary>
    private async Task<GenerateContentResponse> CallFunctionAsync(
        GenerateContentRequest originalRequest,
        GenerateContentResponse response,
        CancellationToken cancellationToken)
    {
        //ToDo implement Function Calling
        // var functionCall = response.GetFunction();
        // if (!AutoCallFunction || functionCall == null)
        //     return response;
        //
        // var name = functionCall.Name ?? string.Empty;
        // string jsonResult;
        //
        // if (!Calls.ContainsKey(name))
        // {
        //     if (!AutoHandleBadFunctionCalls)
        //     {
        //         throw new GenerativeAIException(
        //             $"AI Model called an invalid function: {name}",
        //             $"Invalid function_name: {name}");
        //     }
        //
        //     // Marking the function name as invalid in the response
        //     if (response.Candidates.Count > 0)
        //     {
        //         response.Candidates[0].Content.Parts[0].FunctionCall!.Name = "InvalidName";
        //     }
        //
        //     name = "InvalidName";
        //     jsonResult = "{\"error\":\"Invalid function name or function doesn't exist.\"}";
        // }
        // else
        // {
        //     var callFunc = Calls[name];
        //     var args = functionCall.Arguments != null
        //         ? JsonSerializer.Serialize(functionCall.Arguments, SerializerOptions)
        //         : string.Empty;
        //     jsonResult = await callFunc(args, cancellationToken).ConfigureAwait(false);
        // }
        //
        // // If enabled, pass the function result back into the model
        // if (AutoReplyFunction)
        // {
        //     var responseNode = JsonSerializer.Deserialize<JsonNode>(jsonResult, SerializerOptions);
        //     var content = responseNode.ToFunctionCallContent(name);
        //
        //     var contents = new List<Content>();
        //     if (originalRequest.Contents != null)
        //     {
        //         contents.AddRange(originalRequest.Contents);
        //     }
        //
        //     // Add the AI's function-call message
        //     if (response.Candidates.Count > 0)
        //     {
        //         contents.Add(new Content(response.Candidates[0].Content.Parts, response.Candidates[0].Content.Role));
        //     }
        //
        //     // Add our function result
        //     contents.Add(content);
        //
        //     // Re-call the model with appended result
        //     var nextReq = new GenerateContentRequest { Contents = contents.ToArray() };
        //     response = await GenerateContentAsync(nextReq, cancellationToken).ConfigureAwait(false);
        // }

        //return response;
        return response;
    }

    #endregion
}