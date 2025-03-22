using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using GenerativeAI.Core;
using GenerativeAI.Exceptions;
using GenerativeAI.Microsoft.Extensions;
using GenerativeAI.Types;
using Microsoft.Extensions.AI;

namespace GenerativeAI.Microsoft;

/// <inheritdoc/>
public class GenerativeAIChatClient : IChatClient
{
    public GenerativeModel model { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the function calls in the generative AI chat client
    /// are automatically invoked without requiring explicit user input.
    /// Default: false
    /// </summary>
    /// <remarks>
    /// When set to false, the function calls are handled manually, providing flexibility for scenarios
    /// where explicit user control over function invocation is required or when using M.E.A.I.'s 
    /// FunctionInvokingChatClient.
    /// </remarks>
    public bool AutoCallFunction { get; set; } = true;

    /// <inheritdoc/>
    public GenerativeAIChatClient(string apiKey, string modelName = GoogleAIModels.DefaultGeminiModel, bool autoCallFunction = true)
    {
        model = new GenerativeModel(apiKey, modelName)
        {
            FunctionCallingBehaviour = new FunctionCallingBehaviour()
            {
                FunctionEnabled = true,
                AutoCallFunction = false,
                AutoHandleBadFunctionCalls = false,
                AutoReplyFunction = false
            }
        };
        AutoCallFunction = autoCallFunction;
    }

    /// <inheritdoc/>
    public GenerativeAIChatClient(IPlatformAdapter adapter, string modelName = GoogleAIModels.DefaultGeminiModel)
    {
        model = new GenerativeModel(adapter, modelName);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
    }

    /// <inheritdoc/>
    public async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (messages == null)
            throw new ArgumentNullException(nameof(messages));
        var request = messages.ToGenerateContentRequest(options);
        var response = await model.GenerateContentAsync(request, cancellationToken).ConfigureAwait(false);

        return await CallFunctionAsync(request, response,
            options, cancellationToken).ConfigureAwait(false);
    }

    private async Task<ChatResponse> CallFunctionAsync(GenerateContentRequest request,  GenerateContentResponse response, ChatOptions? options, CancellationToken cancellationToken)
    {
        var chatResponse = response.ToChatResponse() ?? throw new GenerativeAIException("Failed to generate content",
            "The generative model response was null or could not be processed. Verify the API key, model name, input messages, and options for any issues.");
        
        if(!AutoCallFunction)
            return chatResponse;
        
        var functionCall = chatResponse.GetFunction();
        if (functionCall == null)
            return chatResponse;

        var tool = (AIFunction?) options.Tools.Where(s=>s is AIFunction).FirstOrDefault(s=>s.Name == functionCall.Name);
        if (tool != null)
        {
            var result = await tool.InvokeAsync(functionCall.Arguments,cancellationToken).ConfigureAwait(false);
            if (result != null)
            {
                var contents = request.Contents;
                var content = response.Candidates?.FirstOrDefault()?.Content;
                if (content != null)
                    contents.Add(content);
                var responseObject = new JsonObject();
                responseObject["name"] = functionCall.Name;
                responseObject["content"] = ((JsonElement)result).AsNode().DeepClone();
                //responseObject["content"] = result as JsonNode;
                var functionResponse = new FunctionResponse()
                {
                    Name = tool.Name,
                    Id = functionCall.CallId,
                    Response = responseObject
                };
                var funcContent = new Content() { Role = Roles.Function };
                funcContent.AddPart(new Part()
                {
                    FunctionResponse = functionResponse
                });
                contents.Add(funcContent);
                
                return await GetResponseAsync(contents.ToChatMessages().ToList(), options, cancellationToken).ConfigureAwait(false);
            }
        }
        return chatResponse;
    }
    
     private async IAsyncEnumerable<ChatResponseUpdate> CallFunctionStreamingAsync(GenerateContentRequest request,  GenerateContentResponse response, ChatOptions? options, CancellationToken cancellationToken)
    {
        var chatResponse = response.ToChatResponse() ?? throw new GenerativeAIException("Failed to generate content",
            "The generative model response was null or could not be processed. Verify the API key, model name, input messages, and options for any issues.");
        
        if(!AutoCallFunction)
            yield break;
        
        var functionCall = chatResponse.GetFunction();
        if (functionCall == null)
            yield break;

        var tool = (AIFunction?) options.Tools.Where(s=>s is AIFunction).FirstOrDefault(s=>s.Name == functionCall.Name);
        if (tool != null)
        {
            var result = await tool.InvokeAsync(functionCall.Arguments,cancellationToken).ConfigureAwait(false);
            if (result != null)
            {
                var contents = request.Contents;
                var content = response.Candidates?.FirstOrDefault()?.Content;
                if (content != null)
                    contents.Add(content);
                var responseObject = new JsonObject();
                responseObject["name"] = functionCall.Name;
                responseObject["content"] = ((JsonElement)result).AsNode().DeepClone();
                //responseObject["content"] = result as JsonNode;
                var functionResponse = new FunctionResponse()
                {
                    Name = tool.Name,
                    Id = functionCall.CallId,
                    Response = responseObject
                };
                var funcContent = new Content() { Role = Roles.Function };
                funcContent.AddPart(new Part()
                {
                    FunctionResponse = functionResponse
                });
                contents.Add(funcContent);

                await foreach (var res in GetStreamingResponseAsync(contents.ToChatMessages().ToList(), options,
                                   cancellationToken).ConfigureAwait(false))
                {
                    yield return res;
                }
                //yield return GetResponseAsync(contents.ToChatMessages().ToList(), options, cancellationToken);
            }
        }
        yield break;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (messages == null)
            throw new ArgumentNullException(nameof(messages));
        var request = messages.ToGenerateContentRequest(options);
        GenerateContentResponse lastResponse = null;
        await foreach (var response in model.StreamContentAsync(request, cancellationToken).ConfigureAwait(false))
        {
            lastResponse = response;
            yield return lastResponse.ToChatResponseUpdate();
        }

        if (lastResponse != null && lastResponse.GetFunction()!=null)
        {
            await foreach (var resp in CallFunctionStreamingAsync(request, lastResponse, options, cancellationToken).ConfigureAwait(false))
            {
                yield return resp;
            }
        }
    }

    /// <inheritdoc/>
    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        if (serviceKey == null && (bool)serviceType?.IsInstanceOfType(this))
        {
            return this;
        }
        else return null;
    }
}