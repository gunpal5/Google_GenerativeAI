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


    public FunctionCallingBehaviour FunctionCallingBehaviour { get; set; } = new FunctionCallingBehaviour()
    {
        AutoCallFunction = true,
        AutoReplyFunction = true,
        AutoHandleBadFunctionCalls = false,
        FunctionEnabled = true
    };

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

    public void AddFunctionTool(IFunctionTool tool, ToolConfig? toolConfig = null,FunctionCallingBehaviour? functionCallingBehaviour=null)
    {
        this.FunctionTools.Add(tool);
        this.ToolConfig = toolConfig;
        if (functionCallingBehaviour != null)
        {
            this.FunctionCallingBehaviour = functionCallingBehaviour;
        }
    }

    /// <summary>
    /// Disable Global Functions
    /// </summary>
    public void DisableFunctions()
    {
        FunctionCallingBehaviour.FunctionEnabled = false;
    }

    /// <summary>
    /// Enable Global Functions
    /// </summary>
    public void EnableFunctions()
    {
        FunctionCallingBehaviour.FunctionEnabled = true;
    }

    #endregion

    #region Private Helpers

    private void AddTools(GenerateContentRequest request)
    {
        if (FunctionCallingBehaviour.FunctionEnabled && FunctionTools.Count > 0)
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
    protected virtual async Task<GenerateContentResponse> CallFunctionAsync(
        GenerateContentRequest originalRequest,
        GenerateContentResponse response,
        CancellationToken cancellationToken)
    {
        var functionCall = response.GetFunction();
        if (!FunctionCallingBehaviour.AutoCallFunction || functionCall == null)
            return response;

        var name = functionCall.Name ?? string.Empty;
        string jsonResult;

        var tool = FunctionTools.FirstOrDefault(s => s.IsContainFunction(name));
        FunctionResponse functionResponse;
        if (tool == null)
        {
            if (!FunctionCallingBehaviour.AutoHandleBadFunctionCalls)
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
        if (FunctionCallingBehaviour.AutoReplyFunction)
        {
            var content = functionResponse.ToFunctionCallContent();

            var contents = BeforeRegenration(originalRequest, response);
            
            // Add our function result
            contents.Add(content);

            // Re-call the model with appended result
            var nextReq = new GenerateContentRequest { Contents = contents };

            
            response = await GenerateContentAsync(nextReq, cancellationToken).ConfigureAwait(false);
        }

        return response;
    }

    protected virtual List<Content> BeforeRegenration(GenerateContentRequest originalRequest, GenerateContentResponse response)
    {
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
        return contents;
    }

    #endregion
}