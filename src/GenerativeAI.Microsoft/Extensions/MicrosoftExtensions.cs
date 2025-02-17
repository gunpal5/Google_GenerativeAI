using GenerativeAI.Types;
using Microsoft.Extensions.AI;

namespace GenerativeAI.Microsoft.Extensions;

/// <summary>
/// Provides extension methods to transform between Google_GenerativeAI and Microsoft.Extensions.AI models.
/// </summary>
public static class AbstractionMapper
{
    /// <summary>
    /// Transforms a single <see cref="ChatMessage"/> into a <see cref="GenerateContentRequest"/>.
    /// </summary>
    /// <param name="message">The chat message to be transformed.</param>
    /// <param name="options">Optional settings for generation.</param>
    /// <returns>A <see cref="GenerateContentRequest"/> object.</returns>
    public static GenerateContentRequest ToContentRequest(this ChatMessage message, ChatOptions? options = null)
    {
        return CreateContentRequest(new[] { message }, options);
    }

    /// <summary>
    /// Turns a group of <see cref="ChatMessage"/> objects into a <see cref="GenerateContentRequest"/>.
    /// </summary>
    /// <param name="messages">The set of chat messages to convert.</param>
    /// <param name="options">Optional settings for the generation process.</param>
    /// <returns>A <see cref="GenerateContentRequest"/> object.</returns>
    public static GenerateContentRequest ToContentRequest(this IEnumerable<ChatMessage> messages,
        ChatOptions? options = null)
    {
        return CreateContentRequest(messages, options);
    }

    /// <summary>
    /// Constructs a <see cref="GenerateContentRequest"/> using a set of chat messages and optional configuration.
    /// </summary>
    /// <param name="messages">The input chat messages.</param>
    /// <param name="options">Optional parameters to configure the generation process.</param>
    /// <returns>A configured <see cref="GenerateContentRequest"/> object.</returns>
    private static GenerateContentRequest CreateContentRequest(IEnumerable<ChatMessage> messages,
        ChatOptions? options)
    {
        var request = new GenerateContentRequest { GenerationConfig = MapGenerationConfig(options) };

        foreach (var message in messages)
        {
            var content = new Content { Role = MapRole(message.Role) };
            content.Parts.AddRange(message.Contents.Select(MapContentPart));
            request.Contents.Add(content);
        }

        return request;
    }

    /// <summary>
    /// Maps <see cref="ChatOptions"/> into a <see cref="GenerationConfig"/> object used by GenerativeAI.
    /// </summary>
    /// <param name="options">The chat options defining parameters for content generation.</param>
    /// <returns>A <see cref="GenerationConfig"/> instance or null, depending on the input.</returns>
    private static GenerationConfig? MapGenerationConfig(this ChatOptions? options)
    {
        if (options?.AdditionalProperties == null)
        {
            return null;
        }

        var config = new GenerationConfig();
        TryAddOption<double?>(options, "Temperature", v => config.Temperature = v);
        TryAddOption<double?>(options, "TopP", v => config.TopP = v);
        TryAddOption<int?>(options, "TopK", v => config.TopK = v);
        TryAddOption<int>(options, "MaxOutputTokens", v => config.MaxOutputTokens = v);
        TryAddOption<string>(options, "ResponseMimeType", v => config.ResponseMimeType = v);
        TryAddOption<double>(options, "PresencePenalty", v => config.PresencePenalty = v);
        TryAddOption<double>(options, "FrequencyPenalty", v => config.FrequencyPenalty = v);
        TryAddOption<bool?>(options, "ResponseLogprobs", v => config.ResponseLogprobs = v);
        TryAddOption<int?>(options, "Logprobs", v => config.Logprobs = v);
        return config;
    }

    /// <summary>
    /// Converts an <see cref="AIContent"/> object to a <see cref="Part"/> specifically formulated for GenerativeAI.
    /// </summary>
    /// <param name="content">The content to be converted.</param>
    /// <returns>A <see cref="Part"/> representative object.</returns>
    /// <exception cref="NotSupportedException">If the content type is unsupported.</exception>
    private static Part MapContentPart(AIContent content)
    {
        if (content is TextContent text)
        {
            return new Part { Text = text.Text };
        }

        throw new NotSupportedException($"Unsupported AIContent type: {content.GetType()}");
    }

    /// <summary>
    /// Maps a <see cref="ChatRole"/> from Microsoft.Extensions.AI to a GenerativeAI role string.
    /// </summary>
    /// <param name="role">The source role to convert.</param>
    /// <returns>A string representing the equivalent GenerativeAI role.</returns>
    private static string MapRole(ChatRole role)
    {
        switch (role.Value)
        {
            case "user" :
                return Roles.User;
            case "assistant":
                return Roles.Model;
            case "system":
                return Roles.System;
            case "tool":
                return Roles.Function;
            default:
                return role.ToString();
        }
    }

    

    /// <summary>
    /// Converts strings and embedding options into an <see cref="EmbedContentRequest"/>.
    /// </summary>
    /// <param name="values">The strings to embed.</param>
    /// <param name="options">Optional properties for generation settings.</param>
    /// <returns>An <see cref="EmbedContentRequest"/> object populated with the inputs.</returns>
    public static EmbedContentRequest ToGeminiEmbedContentRequest(IEnumerable<string> values,
        EmbeddingGenerationOptions? options)
    {
        return new EmbedContentRequest()
        {
            Content = RequestExtensions.FormatGenerateContentInput(values),
            Model = options?.ModelId ?? null
        };
    }

    /// <summary>
    /// Transforms a <see cref="GenerateContentResponse"/> into a <see cref="ChatResponse"/>.
    /// </summary>
    /// <param name="response">The response to process.</param>
    /// <returns>A <see cref="ChatCompletion"/> object, or null if the response is invalid.</returns>
    public static ChatResponse? ToChatCompletion(this GenerateContentResponse? response)
    {
        if (response is null) return null;

        var chatMessage = ToChatMessage(response);

        return new ChatResponse(chatMessage)
        {
            FinishReason = ToFinishReason(response.Candidates?.FirstOrDefault()?.FinishReason),
            AdditionalProperties = null,
            Choices = new[] {chatMessage}.ToList(),
            CreatedAt = null,
            ModelId = null,
            RawRepresentation = response,
            ResponseId = null,
            Usage = ParseContentResponseUsage(response)
        };
    }

    /// <summary>
    /// Converts a <see cref="GenerateContentResponse"/> into a <see cref="StreamingChatCompletionUpdate"/>.
    /// </summary>
    /// <param name="response">The response to convert.</param>
    /// <returns>A configured <see cref="StreamingChatCompletionUpdate"/>.</returns>
    public static ChatResponseUpdate ToStreamingChatCompletionUpdate(this GenerateContentResponse? response)
    {
        return new ChatResponseUpdate
        {
            ChoiceIndex = 0, // Default to 0 as GenerativeAI doesn't support multiple choices
            CreatedAt = null,
            AdditionalProperties = null,
            FinishReason = response?.Candidates?.FirstOrDefault()?.FinishReason == FinishReason.OTHER
                ? ChatFinishReason.Stop
                : null,
            RawRepresentation = response,
            ResponseId = null,
            Role = ToAbstractionRole(response?.Candidates?.FirstOrDefault()?.Content?.Role),
            Text = response?.Text(),
        };
    }

    public static GeneratedEmbeddings<Embedding<float>> ToGeneratedEmbeddings(EmbedContentRequest request,
        EmbedContentResponse response)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (response == null) throw new ArgumentNullException(nameof(response));

        AdditionalPropertiesDictionary? responseProps = null;
        UsageDetails? usage = null;

        return new GeneratedEmbeddings<Embedding<float>>([
            new Embedding<float>(response.Embedding?.Values.ToArray() )
            {
                CreatedAt = DateTimeOffset.Now,
                ModelId = request.Model
            }
        ])
        {
            AdditionalProperties = responseProps,
            Usage = usage
        };
    }

    /// <summary>
    /// Creates a <see cref="ChatMessage"/> from a <see cref="GenerateContentResponse"/>.
    /// </summary>
    /// <param name="response">The source response.</param>
    /// <returns>A <see cref="ChatMessage"/> derived from the response.</returns>
    private static ChatMessage ToChatMessage(GenerateContentResponse response)
    {
        var contents = new List<AIContent>();
        if (response.Text()?.Length > 0)
            contents.Insert(0, new TextContent(response.Text()));

        return new ChatMessage(ToAbstractionRole(response.Candidates?.FirstOrDefault()?.Content?.Role), contents)
        {
            RawRepresentation = response
        };
    }

    /// <summary>
    /// Converts a role string from GenerativeAI into a <see cref="ChatRole"/>.
    /// </summary>
    /// <param name="role">The string representing the role.</param>
    /// <returns>The equivalent <see cref="ChatRole"/>.</returns>
    private static ChatRole ToAbstractionRole(string? role)
    {
        if (string.IsNullOrEmpty(role)) return new ChatRole("unknown");

        return role switch
        {
            Roles.User => ChatRole.User,
            Roles.Model => ChatRole.Assistant,
            Roles.System => ChatRole.System,
            Roles.Function => ChatRole.Tool,
            _ => new ChatRole(role)
        };
    }

    /// <summary>
    /// Converts a <see cref="FinishReason"/> from GenerativeAI into a <see cref="ChatFinishReason"/>.
    /// </summary>
    /// <param name="finishReason">The finish reason to be mapped.</param>
    /// <returns>A <see cref="ChatFinishReason"/>, or null if it cannot be mapped.</returns>
    private static ChatFinishReason? ToFinishReason(FinishReason? finishReason)
    {
        return finishReason switch
        {
            null => null,
            FinishReason.MAX_TOKENS => ChatFinishReason.Length,
            FinishReason.STOP => ChatFinishReason.Stop,
            FinishReason.SAFETY => ChatFinishReason.ContentFilter,
            FinishReason.PROHIBITED_CONTENT => ChatFinishReason.ContentFilter,
            FinishReason.RECITATION => ChatFinishReason.ContentFilter,
            FinishReason.SPII => ChatFinishReason.ContentFilter,
            FinishReason.BLOCKLIST => ChatFinishReason.ContentFilter,
            FinishReason.MALFORMED_FUNCTION_CALL => ChatFinishReason.ToolCalls,
            _ => new ChatFinishReason(Enum.GetName(typeof(FinishReason), finishReason)!)
        };
    }

    /// <summary>
    /// Extracts detailed usage information from a <see cref="GenerateContentResponse"/>.
    /// </summary>
    /// <param name="response">The response containing usage metadata.</param>
    /// <returns>A <see cref="UsageDetails"/> object, or null if metadata is unavailable.</returns>
    private static UsageDetails? ParseContentResponseUsage(GenerateContentResponse response)
    {
        if (response.UsageMetadata is null) return null;

        return new()
        {
            InputTokenCount = response.UsageMetadata.PromptTokenCount,
            OutputTokenCount = response.UsageMetadata.CandidatesTokenCount,
            TotalTokenCount = response.UsageMetadata.TotalTokenCount
        };
    }

    /// <summary>
    /// Tries to retrieve GenerativeAI options from additional properties in <see cref="ChatOptions"/> and set them.
    /// </summary>
    /// <typeparam name="T">The type of the option value.</typeparam>
    /// <param name="chatOptions">The options containing additional configuration properties.</param>
    /// <param name="option">The name of the option to retrieve.</param>
    /// <param name="optionSetter">The action to set the retrieved option value.</param>
    private static void TryAddOption<T>(ChatOptions chatOptions, string option, Action<T> optionSetter)
    {
        if (chatOptions.AdditionalProperties?.TryGetValue(option, out var value) ?? false)
            optionSetter((T)value);
    }
}