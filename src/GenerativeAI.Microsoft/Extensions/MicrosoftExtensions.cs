using GenerativeAI.Types;
using Json.More;
using Microsoft.Extensions.AI;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace GenerativeAI.Microsoft.Extensions;

/// <summary>
/// Provides extension methods for interoperability between
/// Google_GenerativeAI and Microsoft.Extensions.AI models.
/// </summary>
public static class MicrosoftExtensions
{
    /// <summary>
    /// Converts a collection of <see cref="ChatMessage"/> objects into a <see cref="GenerateContentRequest"/> instance.
    /// </summary>
    /// <param name="chatMessages">The collection of chat messages to transform.</param>
    /// <param name="options">Optional settings to customize the generation process.</param>
    /// <returns>A <see cref="GenerateContentRequest"/> instance derived from the provided chat messages.</returns>
    public static GenerateContentRequest ToGenerateContentRequest(this IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null)
    {
        GenerateContentRequest request = new()
        {
            GenerationConfig = options.ToGenerationConfig()
        };

        Part[] systemParts = (from m in chatMessages
            where m.Role == ChatRole.System
            from c in m.Contents
            let p = c.ToPart()
            where p is not null
            select p).ToArray();
        if (systemParts.Length > 0)
        {
            request.SystemInstruction = new Content(systemParts, Roles.System);
        }

        request.Contents = (from m in chatMessages
            where m.Role != ChatRole.System
            select new Content((from c in m.Contents
                let p = c.ToPart()
                where p is not null
                select p).ToArray(), m.Role == ChatRole.Assistant ? Roles.Model : Roles.User)).ToList();

        request.Tools = options?.Tools?.OfType<AIFunction>().Select(f => new Tool()
        {

            FunctionDeclarations = new()
            {
                new FunctionDeclaration()
                {
                    Name = f.Name,
                    Description = f.Description,
                    Parameters = f.JsonSchema.ToSchema(),
                }
            }
        }).ToList()!;

        return request;
    }


    /// <summary>
    /// Converts a <see cref="JsonElement"/> instance into a <see cref="Schema"/> object.
    /// </summary>
    /// <param name="schema">The JSON schema represented as a <see cref="JsonElement"/>.</param>
    /// <returns>A <see cref="Schema"/> object constructed from the provided JSON schema, or null if deserialization fails.</returns>
    public static Schema? ToSchema(this JsonElement schema)
    {
       
        var serialized = JsonSerializer.Serialize(schema);
        return JsonSerializer.Deserialize(serialized,SchemaSourceGenerationContext.Default.Schema);
    }

    /// <summary>
    /// Converts an <see cref="AIContent"/> instance into a <see cref="Part"/> object.
    /// </summary>
    /// <param name="content">The AI content to transform into a part object.</param>
    /// <returns>A <see cref="Part"/> object representing the given AI content, or null if the content type is unsupported.</returns>
    public static Part? ToPart(this AIContent content)
    {
        return content switch
        {
            TextContent text => new Part { Text = text.Text },
            DataContent image when image.Data != null => new Part
            {
                InlineData = new Blob()
                {
                    MimeType = image.MediaType,
                    Data = Convert.ToBase64String(image.Data!.Value.ToArray()),
                }
            },
            FunctionCallContent fcc => new Part
            {
                FunctionCall = new FunctionCall()
                {
                    Name = fcc.Name,
                    Args = fcc.Arguments!,
                }
            },
            FunctionResultContent frc => new Part
            {
                FunctionResponse = new FunctionResponse()
                {
                    Name = frc.CallId, 
                    Response = new
                    {
                        Name = frc.CallId,
                        Content = JsonSerializer.SerializeToNode(frc.Result)!,
                    }
                }
            },
            _ => null,
        };
    }

    

    /// <summary>
    /// Maps <see cref="ChatOptions"/> into a <see cref="GenerationConfig"/> object used by GenerativeAI.
    /// </summary>
    /// <param name="options">The chat options defining parameters for content generation.</param>
    /// <returns>A <see cref="GenerationConfig"/> instance or null, depending on the input.</returns>
    public static GenerationConfig? ToGenerationConfig(this ChatOptions? options)
    {
        if (options is null)
        {
            return null;
        }

        var config = new GenerationConfig();
        config.Temperature = options.Temperature;
        config.TopP = options.TopP;
        config.TopK = options.TopK;
        config.MaxOutputTokens = options.MaxOutputTokens;
        config.StopSequences = options.StopSequences?.ToList();
        config.Seed = (int) options.Seed!;
        config.ResponseMimeType = options.ResponseFormat is ChatResponseFormatJson ? "application/json" : null;
        if (options.ResponseFormat is ChatResponseFormatJson jsonFormat)
        {
            // see also: https://github.com/dotnet/extensions/blob/f775ed6bd07c0dd94ac422dc6098162eef0b48e5/src/Libraries/Microsoft.Extensions.AI/ChatCompletion/ChatClientStructuredOutputExtensions.cs#L186-L192
            if (jsonFormat.Schema is JsonElement je && je.ValueKind == JsonValueKind.Object)
            {
                // Workaround to convert our real json schema to the format Google's api expects
                var forGoogleApi = GoogleSchemaHelper.ConvertToCompatibleSchemaSubset(je.ToJsonDocument());
                config.ResponseSchema = forGoogleApi;
            }
        }

        config.PresencePenalty = options.PresencePenalty;
        config.FrequencyPenalty = options.FrequencyPenalty;

        if (options.AdditionalProperties is null)
        {
            return config;
        }

        if (options.AdditionalProperties.TryGetValue("ResponseLogprobs", out bool? responseProbs))
            config.ResponseLogprobs = responseProbs;
        if (options.AdditionalProperties.TryGetValue("Logprobs", out int? logProbs))
            config.Logprobs = logProbs;
        return config;
    }

    /// <summary>
    /// Converts a collection of strings and optional embedding generation settings into an <see cref="EmbedContentRequest"/>.
    /// </summary>
    /// <param name="values">The collection of strings to be embedded.</param>
    /// <param name="options">Optional embedding generation settings, such as model identification.</param>
    /// <returns>An <see cref="EmbedContentRequest"/> instance created from the provided values and options.</returns>
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
    /// <param name="response">The <see cref="GenerateContentResponse"/> instance to convert.</param>
    /// <returns>A <see cref="ChatResponse"/> object if the transformation is successful; otherwise, null.</returns>
    public static ChatResponse? ToChatResponse(this GenerateContentResponse? response)
    {
        if (response is null) return null;

        var chatMessage = ToChatMessage(response);

        return new ChatResponse(chatMessage)
        {
            FinishReason = ToFinishReason(response.Candidates?.FirstOrDefault()?.FinishReason),
            AdditionalProperties = null,
            Choices = new[] { chatMessage }.ToList(),
            CreatedAt = null,
            ModelId = null,
            RawRepresentation = response,
            ResponseId = null,
            Usage = ParseContentResponseUsage(response)
        };
    }

    /// <summary>
    /// Converts a <see cref="GenerateContentResponse"/> instance into a <see cref="ChatResponseUpdate"/> object.
    /// </summary>
    /// <param name="response">The input <see cref="GenerateContentResponse"/> to transform into a chat response update.</param>
    /// <returns>A new <see cref="ChatResponseUpdate"/> object reflecting the data in the provided <see cref="GenerateContentResponse"/>.</returns>
    public static ChatResponseUpdate ToChatResponseUpdate(this GenerateContentResponse? response)
    {
        if(response == null) throw new ArgumentNullException(nameof(response));
        
        return new ChatResponseUpdate
        {
            Contents = response.Candidates.Select(s => s.Content).SelectMany(s => s.Parts).ToList().ToAiContents(),
            ChoiceIndex = 0,
            AdditionalProperties = null,
            FinishReason = response?.Candidates?.FirstOrDefault()?.FinishReason == FinishReason.OTHER
                ? ChatFinishReason.Stop
                : null,
            RawRepresentation = response,
            //ResponseId = response.id,
            Role = ToChatRole(response?.Candidates?.FirstOrDefault()?.Content?.Role),
            
           // Text = response?.Candidates?.FirstOrDefault()?.Content?.Parts?.Select(p => p.Text).FirstOrDefault() // Assuming extracting text from parts
        };
    }

    /// <summary>
    /// Converts an <see cref="EmbedContentRequest"/> and an <see cref="EmbedContentResponse"/>
    /// into a <see cref="GeneratedEmbeddings{T}"/> instance containing embeddings of type <see cref="Embedding{float}"/>.
    /// </summary>
    /// <param name="request">The request containing the embedding parameters and metadata.</param>
    /// <param name="response">The response containing the embedding result.</param>
    /// <returns>A <see cref="GeneratedEmbeddings{T}"/> instance containing the generated embeddings and associated metadata.</returns>
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
    /// Converts a <see cref="GenerateContentResponse"/> instance into a <see cref="ChatMessage"/>.
    /// </summary>
    /// <param name="response">The <see cref="GenerateContentResponse"/> to be transformed into a <see cref="ChatMessage"/>.</param>
    /// <returns>A <see cref="ChatMessage"/> representing the data contained in the <see cref="GenerateContentResponse"/>.</returns>
    private static ChatMessage ToChatMessage(GenerateContentResponse response)
    {
        var generatedContent = response.Candidates?.FirstOrDefault().Content;
        var contents = generatedContent.Parts.ToAiContents();
        
        return new ChatMessage(ToChatRole(generatedContent.Role), contents)
        {
            RawRepresentation = response
        };
    }
    
    /// <summary>
    /// Converts a <see cref="GenerateContentResponse"/> instance into a <see cref="ChatMessage"/>.
    /// </summary>
    /// <param name="contents">The <see cref="Content"/> to be transformed into a <see cref="ChatMessage"/>.</param>
    /// <returns>A <see cref="ChatMessage"/> representing the data contained in the <see cref="GenerateContentResponse"/>.</returns>
    public static IEnumerable<ChatMessage> ToChatMessages(this List<Content>? contents)
    {
        List<ChatMessage>? chatMessages = new List<ChatMessage>();
        foreach (var content in contents)
        {
            var aiContents = content.Parts.ToAiContents();
            chatMessages.Add(new ChatMessage(ToChatRole(content.Role), aiContents));
            
        }

        return chatMessages;
    }

    /// <summary>
    /// Converts a role string from GenerativeAI into a <see cref="ChatRole"/>.
    /// </summary>
    /// <param name="role">The string representing the role.</param>
    /// <returns>The equivalent <see cref="ChatRole"/>.</returns>
    private static ChatRole ToChatRole(string? role)
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
    /// Converts a <see cref="FinishReason"/> value into a corresponding <see cref="ChatFinishReason"/> instance.
    /// </summary>
    /// <param name="finishReason">The <see cref="FinishReason"/> value to convert.</param>
    /// <returns>A corresponding <see cref="ChatFinishReason"/> instance, or null if the mapping cannot be performed.</returns>
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
    /// Transforms a list of <see cref="Part"/> objects into a collection of <see cref="AIContent"/> instances.
    /// </summary>
    /// <param name="parts">The list of <see cref="Part"/> objects to be converted.</param>
    /// <returns>A collection of <see cref="AIContent"/> instances derived from the provided <see cref="Part"/> objects, or null if the input list is null.</returns>
    public static IList<AIContent> ToAiContents(this List<Part>? parts)
    {
        List<AIContent>? contents = null;
        if (parts is not null)
        {
            foreach (Part part in parts)
            {
                if (part.Text is not null)
                {
                    (contents ??= new()).Add(new TextContent(part.Text));
                }

                if (part.FunctionCall is not null)
                {
                    (contents ??= new()).Add(new FunctionCallContent(part.FunctionCall.Name, part.FunctionCall.Name, ConvertFunctionCallArg(part.FunctionCall.Args)));
                }

                if (part.FunctionResponse is not null)
                {
                    (contents ??= new()).Add(new FunctionResultContent(part.FunctionResponse.Name, (object?) part.FunctionResponse.Response));
                }

                if (part.InlineData is not null)
                {
                    byte[] data = Convert.FromBase64String(part.InlineData.Data!);
                    (contents ??= new()).Add(new DataContent(data, part.InlineData.MimeType));
                }
            }
        }

        return contents;
    }

    /// <summary>
    /// Converts the arguments of a function call into a dictionary representation.
    /// </summary>
    /// <param name="functionCallArgs">The arguments of the function call, potentially in a serialized JSON format.</param>
    /// <returns>A dictionary where the keys represent argument names and values represent their corresponding data, or null if conversion is not possible.</returns>
    private static IDictionary<string, object?>? ConvertFunctionCallArg(object? functionCallArgs)
    {
        if (functionCallArgs != null && functionCallArgs is not JsonElement)
        {
            functionCallArgs = JsonSerializer.Deserialize<dynamic>(JsonSerializer.Serialize(functionCallArgs));
        }
        if (functionCallArgs is JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.Object)
            {
                var obj = JsonObject.Create(jsonElement);
                return obj?.ToDictionary(s=>s.Key,s=>(object?)s.Value);
            }
        }

        return null;
    }

    public static FunctionCallContent? GetFunction(this ChatResponse response)
    {
        var aiFunction = (FunctionCallContent?) response.Choices.SelectMany(s=>s.Contents).FirstOrDefault(s=>s is FunctionCallContent);
        return aiFunction;
    }
}