using GenerativeAI.Core;
using GenerativeAI.Microsoft.Extensions;
using Microsoft.Extensions.AI;

namespace GenerativeAI.Microsoft;


/// <inheritdoc/>
public class GenerativeAIChatClient : IChatClient
{
    public GenerativeModel model { get; }

    /// <inheritdoc/>
    public GenerativeAIChatClient(string apiKey,string modelName = GoogleAIModels.DefaultGeminiModel)
    {
        model = new GenerativeModel(apiKey, modelName);
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
    public async Task<ChatCompletion> CompleteAsync(IList<ChatMessage> chatMessages, ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (chatMessages == null)
            throw new ArgumentNullException(nameof(chatMessages));
        var request = chatMessages.ToContentRequest(options);
        var response = await model.GenerateContentAsync(request, cancellationToken);
        return response.ToChatCompletion() ?? throw new Exception("Failed to generate content");
    }
    /// <inheritdoc/>
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
    /// <inheritdoc/>
    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        if (serviceKey == null && (bool)serviceType?.IsInstanceOfType(this))
        {
            return this;
        }
        else return null;
    }
    /// <inheritdoc/>
    public ChatClientMetadata Metadata { get; }
}