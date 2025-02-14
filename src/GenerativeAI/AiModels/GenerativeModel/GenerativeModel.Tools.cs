using System.Text.Json;
using System.Text.Json.Nodes;
using GenerativeAI.Core;
using GenerativeAI.Exceptions;
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

    // Used to show or hide function-call capabilities
    /// <summary>
    /// Determines whether function-call capabilities are automatically invoked in the generative process.
    /// This property enables or disables the auto-execution of predefined functions as part of the model's response generation.
    /// </summary>
    /// <remarks>
    /// Can be used to enhance workflow automation by allowing predefined functions to operate without manual invocation.
    /// </remarks>
    public bool AutoCallFunction { get; set; } = true;

    /// <summary>
    /// Enables or disables automatic function replies. When enabled, the model automatically generates responses
    /// for functions that are invoked during an interaction without requiring explicit prompts.
    /// </summary>
    /// <remarks>
    /// Useful for streamlining automated workflows involving generative AI functions.
    /// </remarks>
    public bool AutoReplyFunction { get; set; } = true;

    /// <summary>
    /// Determines whether function-based tools and capabilities are enabled within the generative model.
    /// These tools can include processing mechanisms that enhance content generation or facilitate specific operations.
    /// </summary>
    /// <remarks>
    /// Disabling this property prevents the model from utilizing any function-based features.
    /// </remarks>
    public bool FunctionEnabled { get; set; } = true;

    /// <summary>
    /// Determines whether the system automatically resolves issues caused by improper or unrecognized function calls.
    /// When enabled, it helps ensure smoother operation by handling invalid or malformed function requests.
    /// </summary>
    /// <remarks>
    /// Useful in scenarios where user-defined functions or external integrations might encounter unexpected failures.
    /// </remarks>
    public bool AutoHandleBadFunctionCalls { get; set; } = false;

    public Tool DefaultSearchTool = new Tool() { GoogleSearch = new GoogleSearchTool() };

    public Tool DefaultGoogleSearchRetrieval = new Tool()
    {
        GoogleSearchRetrieval = new GoogleSearchRetrievalTool()
        {
            DynamicRetrievalConfig = new DynamicRetrievalConfig()
                { DynamicThreshold = 1, Mode = DynamicRetrievalMode.MODE_DYNAMIC }
        }
    };

    public Tool DefaultCodeExecutionTool = new Tool() { CodeExecution = new CodeExecutionTool() };

    public List<IFunctionTool> FunctionTools { get; set; } = new List<IFunctionTool>();


    public ToolConfig? ToolConfig { get; set; }
    
    #region Public Methods Related to Tools

    public void AddFunctionTool(IFunctionTool tool, ToolConfig? toolConfig = null)
    {
        this.FunctionTools.Add(tool);
        this.ToolConfig = toolConfig;
    }

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
        if (FunctionEnabled && FunctionTools.Count > 0)
        {
            foreach (var tool in FunctionTools)
            {
                request.AddTool(tool.AsTool(), this.ToolConfig);
            }
        }

        if (UseGrounding)
        {
            request.Tools ??= new List<Tool>();
            if (request.Tools.All(s => s.GoogleSearchRetrieval == null))
                request.Tools.Add(DefaultGoogleSearchRetrieval);
        }

        if (UseGoogleSearch)
        {
            request.Tools ??= new List<Tool>();
            if (request.Tools.All(s => s.GoogleSearch == null))
                request.Tools.Add(DefaultSearchTool);
        }

        if (UseCodeExecutionTool)
        {
            request.Tools ??= new List<Tool>();
            if (request.Tools.All(s => s.CodeExecution == null))
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
        var functionCall = response.GetFunction();
        if (!AutoCallFunction || functionCall == null)
            return response;

        var name = functionCall.Name ?? string.Empty;
        string jsonResult;

        var tool = FunctionTools.FirstOrDefault(s => s.IsContainFunction(name));
        FunctionResponse functionResponse;
        if (tool == null)
        {
            if (!AutoHandleBadFunctionCalls)
            {
                throw new GenerativeAIException(
                    $"AI Model called an invalid function: {name}",
                    $"Invalid function_name: {name}");
            }

            // Marking the function name as invalid in the response
            if (response.Candidates.Length > 0)
            {
                response.Candidates[0].Content.Parts[0].FunctionCall!.Name = "InvalidName";
            }

            name = "InvalidName";
            jsonResult = "{\"error\":\"Invalid function name or function doesn't exist.\"}";
            functionResponse = new FunctionResponse()
            {
                Name = name,
                Response = jsonResult
            };
        }
        else
        {
            functionResponse = await tool.CallAsync(functionCall);
        }

        // If enabled, pass the function result back into the model
        if (AutoReplyFunction)
        {
            var content = functionResponse.ToFunctionCallContent();

            var contents = new List<Content>();
            if (originalRequest.Contents != null)
            {
                contents.AddRange(originalRequest.Contents);
            }

            // Add the AI's function-call message
            if (response.Candidates.Length > 0)
            {
                contents.Add(new Content(response.Candidates[0].Content.Parts, response.Candidates[0].Content.Role));
            }

            // Add our function result
            contents.Add(content);

            // Re-call the model with appended result
            var nextReq = new GenerateContentRequest { Contents = contents };
            response = await GenerateContentAsync(nextReq, cancellationToken).ConfigureAwait(false);
        }

        return response;
    }

    #endregion
}