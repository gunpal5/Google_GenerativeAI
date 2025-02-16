using GenerativeAI.Core;
using GenerativeAI.Microsoft.Extensions;
using Microsoft.Extensions.AI;

namespace GenerativeAI.Microsoft;

public class GenerativeAIChatClient : IChatClient
{
    public GenerativeModel model { get; }

    public GenerativeAIChatClient(IPlatformAdapter adapter, string modelName = GoogleAIModels.DefaultGeminiModel)
    {
        model = new GenerativeModel(adapter, modelName);
    }


    public void Dispose()
    {
        
    }

    public async Task<ChatCompletion> CompleteAsync(IList<ChatMessage> chatMessages, ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (chatMessages == null)
            throw new ArgumentNullException(nameof(chatMessages));
        var request = chatMessages.ToContentRequest(options);
        var response = await model.GenerateContentAsync(request, cancellationToken);
        return response.ToChatCompletion() ?? throw new Exception("Failed to generate content");
    }

    public async IAsyncEnumerable<StreamingChatCompletionUpdate> CompleteStreamingAsync(IList<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = new CancellationToken())
    {
        if (chatMessages == null)
            throw new ArgumentNullException(nameof(chatMessages));
        var request = chatMessages.ToContentRequest(options);
        await foreach (var response in model.StreamContentAsync(request, cancellationToken))
        {
            yield return response.ToStreamingChatCompletionUpdate();
        }
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        if (serviceKey == null && (serviceType is GenerativeAIChatClient))
        {
            return this;
        }
        else return null;
    }

    public ChatClientMetadata Metadata { get; }
}