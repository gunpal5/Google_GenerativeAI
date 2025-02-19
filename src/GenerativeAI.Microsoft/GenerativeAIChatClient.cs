using System.Runtime.CompilerServices;
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

    /// <inheritdoc/>
    public GenerativeAIChatClient(string apiKey, string modelName = GoogleAIModels.DefaultGeminiModel)
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
    public async Task<ChatResponse> GetResponseAsync(IList<ChatMessage> chatMessages, ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (chatMessages == null)
            throw new ArgumentNullException(nameof(chatMessages));
        var request = chatMessages.ToGenerateContentRequest(options);
        var response = await model.GenerateContentAsync(request, cancellationToken).ConfigureAwait(false);

        return await CallFunctionAsync(request, response,
            options, cancellationToken).ConfigureAwait(false);
    }

    private async Task<ChatResponse> CallFunctionAsync(GenerateContentRequest request,  GenerateContentResponse response, ChatOptions? options, CancellationToken cancellationToken)
    {
        var chatResponse = response.ToChatResponse() ?? throw new GenerativeAIException("Failed to generate content",
            "The generative model response was null or could not be processed. Verify the API key, model name, input messages, and options for any issues.");
        
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
                var functionResponse = new FunctionResponse()
                {
                    Name = tool.Name,
                    Id = functionCall.CallId,
                    Response = new
                    {
                        Name = tool.Name,
                        Content = result
                    }
                };
                var funcContent = new Content() { Role = Roles.Function };
                funcContent.AddPart(new Part()
                {
                    FunctionResponse = functionResponse
                });
                contents.Add(funcContent);
                
                return GetResponseAsync(contents.ToChatMessages().ToList(), options, cancellationToken).Result;
            }
        }
        return chatResponse;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IList<ChatMessage> chatMessages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (chatMessages == null)
            throw new ArgumentNullException(nameof(chatMessages));
        var request = chatMessages.ToGenerateContentRequest(options);
        await foreach (var response in model.StreamContentAsync(request, cancellationToken).ConfigureAwait(false))
        {
            yield return response.ToChatResponseUpdate();
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