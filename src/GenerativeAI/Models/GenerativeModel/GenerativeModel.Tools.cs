using System.Text.Json;
using System.Text.Json.Nodes;
using GenerativeAI.Exceptions;
using GenerativeAI.Types;

namespace GenerativeAI.Models;

public partial class GenerativeModel
{
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