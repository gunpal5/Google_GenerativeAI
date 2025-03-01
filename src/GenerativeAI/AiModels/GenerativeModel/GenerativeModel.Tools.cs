using System.Text.Json;
using System.Text.Json.Nodes;
using GenerativeAI.Core;
using GenerativeAI.Exceptions;
using GenerativeAI.Types;
using GenerativeAI.Types.RagEngine;

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
    
    public Tool? RetrievalTool { get; private set; }


    /// <summary>
    /// Specifies the configuration settings for function calling behavior in the generative model.
    /// </summary>
    /// <remarks>
    /// This property determines how the model interacts with functions, including enabling or disabling function calls,
    /// automatically calling functions, replying to functions, and handling incorrect function calls.
    /// </remarks>
    public FunctionCallingBehaviour FunctionCallingBehaviour { get; set; } = new FunctionCallingBehaviour()
    {
        AutoCallFunction = true,
        AutoReplyFunction = true,
        AutoHandleBadFunctionCalls = false,
        FunctionEnabled = true
    };

    /// <summary>
    /// Represents the default tool configured for search-related operations in the generative model.
    /// By default, it utilizes a Google Search Tool for retrieving information to assist in content generation tasks.
    /// </summary>
    /// <remarks>
    /// This tool is automatically added to the request tools if no other search tool is explicitly defined.
    /// </remarks>
    public Tool DefaultSearchTool = new Tool() { GoogleSearch = new GoogleSearchTool() };

    /// <summary>
    /// Represents the default Google Search Retrieval tool configuration used within the generative model.
    /// This tool leverages dynamic retrieval capabilities to fetch real-time data and enhance model responses.
    /// </summary>
    /// <remarks>
    /// Configured with a dynamic retrieval mode and threshold. Primarily used when no specific retrieval tool
    /// is provided in the request. Ensures the integration of up-to-date search results.
    /// </remarks>
    public Tool DefaultGoogleSearchRetrieval = new Tool()
    {
        GoogleSearchRetrieval = new GoogleSearchRetrievalTool()
        {
            DynamicRetrievalConfig = new DynamicRetrievalConfig()
                { DynamicThreshold = 1, Mode = DynamicRetrievalMode.MODE_DYNAMIC }
        }
    };

    /// <summary>
    /// Represents the default tool used for executing code in a generative AI context.
    /// This tool allows for dynamic code execution as part of the generative model’s operations.
    /// </summary>
    /// <remarks>
    /// The tool is automatically added to the available tools if no other code execution tool is specified.
    /// </remarks>
    public Tool DefaultCodeExecutionTool = new Tool() { CodeExecution = new CodeExecutionTool() };

    /// <summary>
    /// Represents a collection of function tools that can be utilized within the generative model.
    /// Function tools enable additional functionalities by allowing external or custom tools to be
    /// integrated into the model's processing pipeline.
    /// </summary>
    /// <remarks>
    /// The tools provided via this property are managed and invoked based on the specified configurations
    /// and behaviours within the generative model. Tools are used during content generation to enhance
    /// or extend the model's capabilities.
    /// </remarks>
    public List<IFunctionTool> FunctionTools { get; set; } = new List<IFunctionTool>();


    /// <summary>
    /// Represents the configuration settings for a specific tool.
    /// Provides options to define the behavior and setup of tools, such as
    /// function calling configurations, to align with the requirements
    /// of the generative model.
    /// </summary>
    public ToolConfig? ToolConfig { get; set; }
    
    #region Public Methods Related to Tools

    /// <summary>
    /// Adds a new function tool to the list of available function tools.
    /// </summary>
    /// <param name="tool">The implementation of the IFunctionTool interface to be added.</param>
    /// <param name="toolConfig">Optional configuration for the tool.</param>
    /// <param name="functionCallingBehaviour">Optional behavior configuration for function calling.</param>
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

    /// <summary>
    /// Adds necessary tools to the provided GenerateContentRequest instance based on configuration settings.
    /// </summary>
    /// <param name="request">The GenerateContentRequest instance that will be updated with the appropriate tools.</param>
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

        if (this.RetrievalTool != null)
        {
            request.Tools ??= new List<Tool>();
            request.Tools.Add(this.RetrievalTool);
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
            functionResponse = await tool.CallAsync(functionCall).ConfigureAwait(false);
        }

        // If enabled, pass the function result back into the model
        if (FunctionCallingBehaviour.AutoReplyFunction)
        {
            var content = functionResponse.ToFunctionCallContent();

            var contents = BeforeRegeneration(originalRequest, response);
            
            // Add our function result
            contents.Add(content);

            // Re-call the model with appended result
            var nextReq = new GenerateContentRequest { Contents = contents };

            
            response = await GenerateContentAsync(nextReq, cancellationToken).ConfigureAwait(false);
        }

        return response;
    }

    /// <summary>
    /// Process the request before regenerating with a function call response
    /// </summary>
    /// <param name="originalRequest">The original content generation request containing existing contents.</param>
    /// <param name="response">The content generation response containing candidates from the model.</param>
    /// <returns>A list of contents combining the original request contents and updated content from the response.</returns>
    protected virtual List<Content> BeforeRegeneration(GenerateContentRequest originalRequest, GenerateContentResponse response)
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

    /// <summary>
    /// Configures and assigns a Vertex Retrieval Tool with the specified corpus ID and retrieval configuration.
    /// </summary>
    /// <param name="corpusId">The identifier for the corpus to be associated with the Vertex Retrieval Tool.</param>
    /// <param name="retrievalConfig">An optional retrieval configuration for customizing the behavior of the Vertex Retrieval Tool.</param>
    /// <exception cref="NotSupportedException">Thrown when the platform does not support Retrieval Augmentation Generation on Vertex AI.</exception>
    public void UseVertexRetrievalTool(string corpusId, RagRetrievalConfig? retrievalConfig = null)
    {
        if(!this._platform.GetBaseUrl().Contains("aiplatform"))
            throw new NotSupportedException("Retrival Augmentation Generation is only supported on Vertex AI");

        this.RetrievalTool = new Tool()
        {
            Retrieval = new VertexRetrievalTool()
            {
                VertexRagStore = new VertexRagStore()
                {
                    RagResources = new List<VertexRagStoreRagResource>([
                        new VertexRagStoreRagResource()
                        {
                            RagCorpus = corpusId
                        }
                    ]),
                    RagRetrievalConfig = retrievalConfig
                }
            }
        };
    }
   
}