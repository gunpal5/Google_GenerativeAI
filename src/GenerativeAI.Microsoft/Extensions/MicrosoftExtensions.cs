using GenerativeAI.Core;
using GenerativeAI.Types;
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
            let p = c.ToPart(options)
            where p is not null
            select p).ToArray();
        if (systemParts.Length > 0 || !string.IsNullOrWhiteSpace(options?.Instructions))
        {
            // Only add instructions part if it's not null or empty to avoid creating empty parts
            var allSystemParts = !string.IsNullOrWhiteSpace(options?.Instructions)
                ? systemParts.Concat([new Part { Text = options!.Instructions }])
                : systemParts;
            request.SystemInstruction = new Content(allSystemParts, Roles.System);
        }

        request.Contents = (from m in chatMessages
            where m.Role != ChatRole.System
            select new Content((from c in m.Contents
                let p = c.ToPart(options)
                where p is not null
                select p).ToArray(), m.Role == ChatRole.Assistant ? Roles.Model : Roles.User)).ToList();

        if (options?.Tools is not null)
        {
            List<FunctionDeclaration>? functionDeclarations = null;
            List<Tool>? tools = null;
            foreach (var tool in options.Tools)
            {
                switch (tool)
                {
                    case AIFunctionDeclaration f:
                        (functionDeclarations ??= []).Add(new()
                        {
                            Name = f.Name,
                            Description = f.Description,
                            Parameters = ParseFunctionParameters(f.JsonSchema),
                        });
                        break;

                    case HostedWebSearchTool ws:
                        (tools ??= []).Add(new Tool
                        {
                            GoogleSearch = new GoogleSearchTool()
                        });
                        break;

                    case HostedCodeInterpreterTool ci:
                        (tools ??= []).Add(new Tool
                        {
                            CodeExecution = new CodeExecutionTool(),
                        });
                        break;
                }
            }

            if (functionDeclarations is not null)
            {
                (tools ??= []).Add(new Tool
                {
                    FunctionDeclarations = functionDeclarations.ToList()
                });
            }

            if (tools is not null)
            {
                request.Tools = tools;
            }
        }

        return request;
    }

    private static Schema? ParseFunctionParameters(JsonElement schema)
    {
        if (schema.ValueKind == JsonValueKind.Null)
        {
            return null;
        }
        else
        {
            var properties = schema.GetProperty("properties");
            if (properties.GetPropertyCount() == 0)
                return null;
            else
            {
                // Convert to JsonNode, clone it, and apply transformations to the clone
                var node = schema.AsNode();
                if (node != null)
                {
                    // Clone the node to avoid modifying the original schema
                    var clonedNode = node.DeepClone();
                    ApplySchemaTransformations(clonedNode);
                    return GoogleSchemaHelper.ConvertToCompatibleSchemaSubset(clonedNode);
                }
                return null;
            }
        }
    }


    /// <summary>
    /// Converts a <see cref="JsonElement"/> instance into a <see cref="Schema"/> object.
    /// </summary>
    /// <param name="schema">The JSON schema represented as a <see cref="JsonElement"/>.</param>
    /// <returns>A <see cref="Schema"/> object constructed from the provided JSON schema, or null if deserialization fails.</returns>
    private static Schema? ToSchema(this JsonElement schema)
    {
        var node = schema.AsNode();
        return node != null ? GoogleSchemaHelper.ConvertToCompatibleSchemaSubset(node) : null;
    }

    /// <summary>
    /// Converts an <see cref="AIContent"/> instance into a <see cref="Part"/> object.
    /// </summary>
    /// <param name="content">The AI content to transform into a part object.</param>
    /// <param name="options">Optional chat options for context-aware transformations.</param>
    /// <returns>A <see cref="Part"/> object representing the given AI content, or null if the content type is unsupported.</returns>
    public static Part? ToPart(this AIContent content, ChatOptions? options = null)
    {
        return content switch
        {
            TextContent text => new Part { Text = text.Text },
            DataContent image => new Part
            {
                InlineData = new Blob()
                {
                    MimeType = image.MediaType,
                    Data = Convert.ToBase64String(image.Data.ToArray()),
                }
            },
            FunctionCallContent fcc => new Part
            {
                FunctionCall = new FunctionCall()
                {
                    Name = fcc.Name,
                    Args = fcc.Arguments.ToJsonNode(),
                }
            },
            FunctionResultContent frc => new Part
            {
                FunctionResponse = new FunctionResponse()
                {
                    Name = frc.CallId,
                    Response = frc.ToJsonNodeResponse()
                }
            },
            _ => null,
        };
    }

    /// <summary>
    /// Converts a dictionary of string keys and nullable object values into a <see cref="JsonNode"/> representation.
    /// </summary>
    /// <param name="args">The dictionary containing the arguments to be transformed.</param>
    /// <returns>A <see cref="JsonNode"/> instance representing the provided dictionary.</returns>
    #pragma warning disable CA1859 // Use concrete types when possible for improved performance
    private static JsonNode ToJsonNode(this IDictionary<string, object?>? args)
    #pragma warning restore CA1859
    {
        var node = new JsonObject();
        foreach (var arg in args!)
        {
            if (arg.Value is JsonNode nd)
                node.Add(arg.Key, nd.DeepClone());
            else
            {
                var p = arg.Value switch
                {
                    string s => s,
                    int i => i,
                    float f => f,
                    double d => d,
                    bool b => b,
                    null => null,
                    JsonElement e => e.AsNode()?.AsObject(),
                    JsonNode n => n switch
                    {
                        JsonObject o => o,
                        JsonArray a => a,
                        JsonValue v => v.GetValue<JsonElement>().AsNode(),
                        _ => n
                    },
                    _ => throw new ArgumentException("Unsupported argument type")
                };
                node.Add(arg.Key, p);
            }
        }

        return node; //JsonSerializer.Deserialize(node.ToJsonString(), TypesSerializerContext.Default.JsonElement)!;
    }

    /// <summary>
    /// Converts a given response object into a <see cref="JsonNode"/> representation.
    /// </summary>
    /// <param name="response">The response object to convert. It can be of type <see cref="FunctionResultContent"/>, <see cref="JsonNode"/>, or other compatible types.</param>
    /// <returns>A <see cref="JsonNode"/> instance representing the response. Throws an exception if the response is not a valid JSON node.</returns>
    private static JsonNode ToJsonNodeResponse(this object? response)
    {
        if (response is FunctionResultContent content)
        {
            if (content.Result is JsonObject obj)
                return obj;
            if (content.Result is JsonNode arr)
                return arr;
            if (content.Result is JsonElement el)
            {
                if (el.ValueKind != JsonValueKind.Object && el.ValueKind != JsonValueKind.Array)
                {
                    var jObj = new JsonObject();
                    var node = el.AsNode();
                    if (node != null)
                    {
                        jObj.Add("content", node.DeepClone());
                    }
                    return jObj;
                }
                else
                {
                    var newNode = el.AsNode();
                    if (newNode != null)
                        return newNode;
                }
            }
        }

        if (response is JsonNode node2)
        {
            return node2;
        }
        else
        {
#pragma warning disable IL2026, IL3050 // Reflection is used to deserialize the response
            if (JsonSerializer.IsReflectionEnabledByDefault)
            {
                return JsonSerializer.Deserialize<JsonNode>(
                    JsonSerializer.Serialize(response, DefaultSerializerOptions.GenerateObjectJsonOptions),
                    TypesSerializerContext.Default.JsonNode)!;
            }
#pragma warning restore IL2026, IL3050 // Reflection is used to deserialize the response
        }

        throw new ArgumentException("Response is not a json node");
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

        var config = new GenerationConfig
        {
            Temperature = options.Temperature,
            TopP = options.TopP,
            TopK = options.TopK,
            MaxOutputTokens = options.MaxOutputTokens,
            StopSequences = options.StopSequences?.ToList(),
            Seed = (int?)options.Seed,
            ResponseMimeType = options.ResponseFormat is ChatResponseFormatJson ? "application/json" : null
        };

        if (options.ResponseFormat is ChatResponseFormatJson jsonFormat)
        {
            // see also: https://github.com/dotnet/extensions/blob/f775ed6bd07c0dd94ac422dc6098162eef0b48e5/src/Libraries/Microsoft.Extensions.AI/ChatCompletion/ChatClientStructuredOutputExtensions.cs#L186-L192
            if (jsonFormat.Schema is { ValueKind: JsonValueKind.Object } je)
            {
                // Workaround to convert our real json schema to the format Google's api expects
                var node = je.AsNode();
                if (node != null)
                {
                    var forGoogleApi = GoogleSchemaHelper.ConvertToCompatibleSchemaSubset(node);
                    config.ResponseSchema = forGoogleApi;
                }
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

        if (options.AdditionalProperties.TryGetValue(AdditionalPropertiesKeys.ThinkingBudget, out int? thinkingBudget))
        {
            if (config.ThinkingConfig == null)
                config.ThinkingConfig = new ThinkingConfig();
            config.ThinkingConfig.ThinkingBudget = thinkingBudget;
        }

        if (options.AdditionalProperties.TryGetValue(AdditionalPropertiesKeys.ThinkingConfigIncludeThoughts,
                out bool? includeThoughts))
        {
            if (config.ThinkingConfig == null)
                config.ThinkingConfig = new ThinkingConfig();
            config.ThinkingConfig.IncludeThoughts = includeThoughts;
        }

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
    public static ChatResponse? ToChatResponse(this GenerateContentResponse? response, ChatOptions? options = null)
    {
        if (response is null) return null;

        var chatMessage = ToChatMessage(response, options);

        return new ChatResponse(chatMessage)
        {
            FinishReason = ToFinishReason(response.Candidates?.FirstOrDefault()?.FinishReason),
            AdditionalProperties = null,
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
    public static ChatResponseUpdate ToChatResponseUpdate(this GenerateContentResponse? response, ChatOptions? options = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(response);
#else
        if (response == null) throw new ArgumentNullException(nameof(response));
#endif

        if (response.Candidates != null)
        {
            return new ChatResponseUpdate
            {
                Contents = response.Candidates.Select(s => s.Content).SelectMany(s => s?.Parts ?? new List<Part>()).ToList().ToAiContents(options),
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

        throw new ArgumentException("Response is invalid with no candidates");
    }

    /// <summary>
    /// Converts an <see cref="EmbedContentRequest"/> and an <see cref="EmbedContentResponse"/>
    /// into a <see cref="GeneratedEmbeddings{T}"/> instance containing embeddings of type Embedding&lt;float&gt;.
    /// </summary>
    /// <param name="request">The request containing the embedding parameters and metadata.</param>
    /// <param name="response">The response containing the embedding result.</param>
    /// <returns>A <see cref="GeneratedEmbeddings{T}"/> instance containing the generated embeddings and associated metadata.</returns>
    public static GeneratedEmbeddings<Embedding<float>> ToGeneratedEmbeddings(EmbedContentRequest request,
        EmbedContentResponse response)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(request);
#else
        if (request == null) throw new ArgumentNullException(nameof(request));
#endif
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(response);
#else
        if (response == null) throw new ArgumentNullException(nameof(response));
#endif

        AdditionalPropertiesDictionary? responseProps = null;
        UsageDetails? usage = null;

        return new GeneratedEmbeddings<Embedding<float>>([
            new Embedding<float>(response.Embedding?.Values?.ToArray())
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
    private static ChatMessage ToChatMessage(GenerateContentResponse response, ChatOptions? options = null)
    {
        var generatedContent = response.Candidates?.FirstOrDefault()?.Content;
        var contents = generatedContent?.Parts.ToAiContents(options);

        return new ChatMessage(ToChatRole(generatedContent?.Role), contents)
        {
            RawRepresentation = response
        };
    }

    /// <summary>
    /// Converts a <see cref="GenerateContentResponse"/> instance into a <see cref="ChatMessage"/>.
    /// </summary>
    /// <param name="contents">The <see cref="Content"/> to be transformed into a <see cref="ChatMessage"/>.</param>
    /// <returns>A <see cref="ChatMessage"/> representing the data contained in the <see cref="GenerateContentResponse"/>.</returns>
    public static IEnumerable<ChatMessage> ToChatMessages(this List<Content>? contents, ChatOptions? options = null)
    {
        return (from content in contents
            let aiContents = content.Parts.ToAiContents(options)
            select new ChatMessage(ToChatRole(content.Role), aiContents)).ToList();
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
            _ => new ChatRole(role!)
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

        return new UsageDetails
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
    /// <param name="options">Optional chat options for context-aware transformations.</param>
    /// <returns>A collection of <see cref="AIContent"/> instances derived from the provided <see cref="Part"/> objects, or null if the input list is null.</returns>
    public static IList<AIContent> ToAiContents(this List<Part>? parts, ChatOptions? options = null)
    {
        List<AIContent>? contents = null;
        if (parts is null) return new List<AIContent>();

        foreach (var part in parts)
        {
            if (part.Text is not null)
            {
                (contents ??= new()).Add(new TextContent(part.Text));
            }

            if (part.FunctionCall is not null)
            {
                (contents ??= new()).Add(new FunctionCallContent(part.FunctionCall.Name, part.FunctionCall.Name,
                    ConvertFunctionCallArg(part.FunctionCall.Args, part.FunctionCall.Name, options)));
            }

            if (part.FunctionResponse is not null)
            {
                (contents ??= new()).Add(new FunctionResultContent(part.FunctionResponse.Name,
                    (object?)part.FunctionResponse.Response));
            }

            if (part.InlineData is not null)
            {
                byte[] data = Convert.FromBase64String(part.InlineData.Data!);
                (contents ??= new()).Add(new DataContent(data, part.InlineData.MimeType ?? "application/octet-stream"));
            }
        }

        return contents ?? new List<AIContent>();
    }

    /// <summary>
    /// Recursively transforms date/time values from Gemini's format to a format compatible with DateOnly/TimeOnly.
    /// </summary>
    /// <param name="value">The JSON value to transform</param>
    /// <param name="parameterName">The parameter path for schema lookup</param>
    /// <param name="functionName">The name of the function being called</param>
    /// <param name="options">Chat options containing tool definitions</param>
    /// <param name="arrayItemSchema">Optional schema for array items when processing nested objects within arrays</param>
    private static object? TransformDateTimeValue(JsonNode value, string? parameterName, string? functionName, ChatOptions? options, JsonNode? arrayItemSchema = null)
    {
        // Handle nested objects recursively
        if (value is JsonObject nestedObj)
        {
            var transformedObj = new JsonObject();
            
            // If we're not in an array context, we might need to get the schema for this object parameter
            JsonNode? objectSchema = arrayItemSchema;
            if (objectSchema == null && !string.IsNullOrEmpty(parameterName) && !string.IsNullOrEmpty(functionName) && options?.Tools != null)
            {
                // Try to get the schema for this object parameter
                var function = options.Tools.OfType<AIFunctionDeclaration>().FirstOrDefault(f => f.Name == functionName);
                if (function?.JsonSchema != null)
                {
                    var schemaNode = JsonSerializer.SerializeToNode(function.JsonSchema);
                    var pathParts = parameterName.Split('.');
                    var currentSchema = schemaNode?["properties"];
                    
                    foreach (var part in pathParts)
                    {
                        if (currentSchema == null) break;
                        var propSchema = currentSchema[part];
                        if (propSchema == null) break;
                        
                        // Check the type of this property
                        if (propSchema["type"] is JsonValue tv && tv.TryGetValue<string>(out var ts))
                        {
                            if (ts == "object")
                            {
                                objectSchema = propSchema;
                                currentSchema = propSchema["properties"];
                            }
                            else if (ts == "array")
                            {
                                // For arrays, navigate through items
                                objectSchema = propSchema["items"];
                                currentSchema = propSchema["items"]?["properties"];
                            }
                            else
                            {
                                // For primitive types, this is the end of navigation
                                objectSchema = propSchema;
                                currentSchema = null;
                            }
                        }
                        else
                        {
                            currentSchema = propSchema["properties"];
                        }
                    }
                }
            }
            
            foreach (var kvp in nestedObj)
            {
                // For nested objects, we need to check each property's schema
                JsonNode? propSchemaForNested = null;
                string pathForLookup;
                
                if (arrayItemSchema != null)
                {
                    // We're inside an array item, use relative path and array item schema
                    pathForLookup = kvp.Key;
                    
                    // Get the schema for this property from the array item schema
                    var propSchema = objectSchema?["properties"]?[kvp.Key];
                    if (propSchema != null)
                    {
                        // Pass appropriate schema based on property type
                        if (propSchema["type"] is JsonValue tv && tv.TryGetValue<string>(out var ts))
                        {
                            if (ts == "object")
                            {
                                propSchemaForNested = propSchema;
                            }
                            else if (ts == "array")
                            {
                                propSchemaForNested = propSchema["items"];
                            }
                            else
                            {
                                propSchemaForNested = propSchema;
                            }
                        }
                    }
                }
                else
                {
                    // Not in array context, use full path for lookup
                    pathForLookup = string.IsNullOrEmpty(parameterName) ? kvp.Key : $"{parameterName}.{kvp.Key}";
                    
                    // Get the schema for this property when not in array context
                    if (objectSchema != null)
                    {
                        var propSchema = objectSchema["properties"]?[kvp.Key];
                        if (propSchema != null)
                        {
                            // Pass appropriate schema based on property type
                            if (propSchema["type"] is JsonValue tv && tv.TryGetValue<string>(out var ts))
                            {
                                if (ts == "object")
                                {
                                    propSchemaForNested = propSchema;
                                }
                                else if (ts == "array")
                                {
                                    propSchemaForNested = propSchema["items"];
                                }
                                else
                                {
                                    propSchemaForNested = propSchema;
                                }
                            }
                        }
                    }
                }
                
                // Pass the appropriate schema context for nested transformation
                var transformed = kvp.Value != null ? TransformDateTimeValue(kvp.Value, pathForLookup, functionName, options, propSchemaForNested) : null;
                if (transformed is JsonNode node)
                {
                    transformedObj[kvp.Key] = node;
                }
                else if (transformed != null)
                {
                    transformedObj[kvp.Key] = JsonValue.Create(transformed);
                }
                else
                {
                    transformedObj[kvp.Key] = null;
                }
            }
            return transformedObj;
        }
        
        // Handle arrays recursively
        if (value is JsonArray array)
        {
            var transformedArray = new JsonArray();
            
            // Get the array item schema for nested objects within array items
            JsonNode? itemSchema = null;
            if (!string.IsNullOrEmpty(parameterName) && !string.IsNullOrEmpty(functionName) && options?.Tools != null)
            {
                var function = options.Tools.OfType<AIFunctionDeclaration>().FirstOrDefault(f => f.Name == functionName);
                if (function?.JsonSchema != null)
                {
                    var schemaNode = JsonSerializer.SerializeToNode(function.JsonSchema);
                    var pathParts = parameterName.Split('.');
                    var currentSchema = schemaNode?["properties"];
                    
                    foreach (var part in pathParts)
                    {
                        if (currentSchema == null) break;
                        var propSchema = currentSchema[part];
                        if (propSchema == null) break;
                        
                        // If this is an array property, get its items schema
                        if (propSchema["type"] is JsonValue tv && tv.TryGetValue<string>(out var ts) && ts == "array")
                        {
                            itemSchema = propSchema["items"];
                            break;
                        }
                        
                        currentSchema = propSchema["properties"];
                    }
                }
            }
            
            foreach (var item in array)
            {
                // For arrays, each item should be transformed with the array parameter name
                // so that schema lookup can work properly
                var transformed = item != null ? TransformDateTimeValue(item, parameterName, functionName, options, itemSchema) : null;
                if (transformed is JsonNode node)
                {
                    transformedArray.Add(node);
                }
                else if (transformed != null)
                {
                    transformedArray.Add(JsonValue.Create(transformed));
                }
                else
                {
                    transformedArray.Add(null);
                }
            }
            return transformedArray;
        }
        
        // Check if this parameter should be transformed based on the function's parameter types
        if (!string.IsNullOrEmpty(parameterName) && !string.IsNullOrEmpty(functionName) && options?.Tools != null)
        {
            // Find the function in the tools
            var function = options.Tools.OfType<AIFunctionDeclaration>().FirstOrDefault(f => f.Name == functionName);
            if (function?.JsonSchema != null)
            {
                // Parse the schema to check the parameter's format
                // Since we're cloning before transformation, the original schema is intact
                var schemaNode = arrayItemSchema ?? JsonSerializer.SerializeToNode(function.JsonSchema);
                
                // Navigate to the correct schema path for nested properties
                JsonObject? paramSchema = null;
                
                // Handle the special case when we're inside an array item
                if (arrayItemSchema != null)
                {
                    // Check if the arrayItemSchema itself is a primitive type (like DateOnly in an array)
                    if (arrayItemSchema["type"] is JsonValue typeVal && arrayItemSchema["format"] is JsonValue)
                    {
                        // This is a primitive array item (e.g., DateOnly[])
                        paramSchema = arrayItemSchema as JsonObject;
                    }
                    else if (!string.IsNullOrEmpty(parameterName))
                    {
                        // For complex objects in arrays, look up the property in the item schema
                        var pathParts = parameterName.Split('.');
                        var currentSchema = arrayItemSchema["properties"];
                        
                        foreach (var part in pathParts)
                        {
                            if (currentSchema == null) break;
                            
                            var propSchema = currentSchema[part];
                            if (propSchema == null) break;
                            
                            // If this is the last part, this is our target schema
                            if (part == pathParts[pathParts.Length - 1])
                            {
                                paramSchema = propSchema as JsonObject;
                                
                                // For arrays, check the items schema
                                if (paramSchema?["type"] is JsonValue typeValue && typeValue.TryGetValue<string>(out var typeStr) && typeStr == "array")
                                {
                                    paramSchema = paramSchema["items"] as JsonObject;
                                }
                            }
                            else
                            {
                                // Navigate deeper into nested properties
                                if (propSchema["type"] is JsonValue tv && tv.TryGetValue<string>(out var ts) && ts == "array")
                                {
                                    currentSchema = propSchema["items"]?["properties"];
                                }
                                else
                                {
                                    currentSchema = propSchema["properties"];
                                }
                            }
                        }
                    }
                    else
                    {
                        // If no parameter name, the item schema itself might be a date/time value
                        paramSchema = arrayItemSchema as JsonObject;
                    }
                }
                else
                {
                    // Split the parameter name to handle nested properties like "appointment.date" or "events.schedule.startDate"
                    var pathParts = parameterName.Split('.');
                    var currentSchema = schemaNode?["properties"];
                    
                    foreach (var part in pathParts)
                {
                    if (currentSchema == null) break;
                    
                    var propSchema = currentSchema[part];
                    if (propSchema == null) break;
                    
                    // If this is the last part, this is our target schema
                    if (part == pathParts[pathParts.Length - 1])
                    {
                        paramSchema = propSchema as JsonObject;
                        
                        // For arrays, check the items schema
                        if (paramSchema?["type"] is JsonValue typeValue && typeValue.TryGetValue<string>(out var typeStr) && typeStr == "array")
                        {
                            paramSchema = paramSchema["items"] as JsonObject;
                        }
                    }
                    else
                    {
                        // Navigate deeper into nested properties
                        // If current property is an array, navigate through items
                        if (propSchema["type"] is JsonValue tv && tv.TryGetValue<string>(out var ts) && ts == "array")
                        {
                            currentSchema = propSchema["items"]?["properties"];
                        }
                        // If it's an object, navigate through properties
                        else
                        {
                            currentSchema = propSchema["properties"];
                        }
                    }
                    }
                }
                
                if (paramSchema != null)
                {
                    string? format = null;
                    if (paramSchema["format"] is JsonValue formatValue)
                    {
                        formatValue.TryGetValue<string>(out format);
                    }
                    
                    // Check if it's a DateOnly (format: "date") or TimeOnly (format: "time")
                    var isDateOnly = format == "date";
                    var isTimeOnly = format == "time";
                    
                    if (isDateOnly || isTimeOnly)
                    {
                        // Transform the value for DateOnly/TimeOnly parameters
                        if (value is JsonValue jsonValue && jsonValue.TryGetValue<string>(out var stringValue))
                        {
                            if (!string.IsNullOrEmpty(stringValue))
                            {
                                if (isDateOnly)
                                {
                                    // Try to parse various date formats and convert to yyyy-MM-dd
                                    // Use RoundtripKind to preserve UTC times and avoid timezone conversion
                                    if (DateTime.TryParse(stringValue, System.Globalization.CultureInfo.InvariantCulture,
                                        System.Globalization.DateTimeStyles.RoundtripKind, out var dateTime))
                                    {
                                        // Return date part for DateOnly as JsonValue
                                        return JsonValue.Create(dateTime.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture));
                                    }
                                    // If already in correct format, return as-is
                                    else if (stringValue.Length == 10 && stringValue[4] == '-' && stringValue[7] == '-')
                                    {
                                        return JsonValue.Create(stringValue);
                                    }
                                }
                                else if (isTimeOnly)
                                {
                                    // Try to parse various time formats and convert to HH:mm:ss
                                    if (DateTime.TryParse(stringValue, System.Globalization.CultureInfo.InvariantCulture,
                                        System.Globalization.DateTimeStyles.None, out var dateTime))
                                    {
                                        // Return time part for TimeOnly as JsonValue
                                        return JsonValue.Create(dateTime.ToString("HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture));
                                    }
                                    // Parse time-only formats like "2:30 PM"
                                    else if (DateTime.TryParse("2000-01-01 " + stringValue, System.Globalization.CultureInfo.InvariantCulture,
                                        System.Globalization.DateTimeStyles.None, out dateTime))
                                    {
                                        return JsonValue.Create(dateTime.ToString("HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture));
                                    }
                                    // If already in correct format, return as-is
                                    else if (stringValue.Contains(':'))
                                    {
                                        return JsonValue.Create(stringValue);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        
        // For non-date values or when we can't determine the type, return as-is
        return value?.DeepClone();
    }

    /// <summary>
    /// Converts the arguments of a function call into a dictionary representation.
    /// </summary>
    /// <param name="functionCallArgs">The arguments of the function call, potentially in a serialized JSON format.</param>
    /// <param name="functionName">The name of the function being called (optional, used for context).</param>
    /// <param name="options">Chat options containing tool definitions for parameter type checking.</param>
    /// <returns>A dictionary where the keys represent argument names and values represent their corresponding data, or null if conversion is not possible.</returns>
    #pragma warning disable CA1859 // Use concrete types when possible for improved performance
    private static IDictionary<string, object?>? ConvertFunctionCallArg(JsonNode? functionCallArgs, string? functionName = null, ChatOptions? options = null)
    #pragma warning restore CA1859
    {
        // Use the comprehensive transformation logic that handles all cases
        return ComprehensiveDateTimeTransformer.TransformFunctionCallArguments(functionCallArgs, functionName, options);

        #region Unused codes for future reference

        // if (functionCallArgs is JsonElement jsonElement)
        // {
        //     var obj = jsonElement.AsNode().AsObject();
        //     return obj?.ToDictionary(s => s.Key, s => (object?)s.Value?.DeepClone());
        // }

        // if (functionCallArgs is JsonNode jsonElement2)
        // {
        // var obj = functionCallArgs.AsObject();
        // return obj?.ToDictionary(s => s.Key, s => (object?)s.Value?.DeepClone());
        // }
        // else if (functionCallArgs != null && functionCallArgs is not JsonNode)
        // {
        //     //This code is a fail safe, it will never execute as of  
        //     #pragma warning disable IL2026 IL3050
        //     try
        //     {
        //         // Attempt to serialize and deserialize the object to a JsonNode
        //         var serializedArg = JsonSerializer.Serialize(functionCallArgs);
        //         var deserializedNode = JsonSerializer.Deserialize(serializedArg, TypesSerializerContext.Default.JsonNode);
        //
        //         if (deserializedNode != null && deserializedNode is JsonObject obj)
        //         {
        //             return obj.ToDictionary(s => s.Key, s => (object?)s.Value?.DeepClone());
        //         }
        //     }
        //     catch (JsonException)
        //     {
        //         throw new ArgumentException("Cannot convert function call arguments to a supported type.");
        //     }
        // #pragma warning restore IL2026 IL3050
        //}

        //return null;

        #endregion
    }

    /// <summary>
    /// Retrieves the first occurrence of a function call content from the provided <see cref="ChatResponse"/> message contents.
    /// </summary>
    /// <param name="response">The <see cref="ChatResponse"/> object containing the messages and their associated contents.</param>
    /// <returns>A <see cref="FunctionCallContent"/> object if a function call is present; otherwise, null.</returns>
    #pragma warning disable CA1002 // Do not expose generic lists
    public static List<FunctionCallContent>? GetFunctions(this ChatResponse response)
    #pragma warning restore CA1002
    {
        if (response == null)
            return null;
        var aiFunction = response.Messages
            .SelectMany(s => s.Contents).OfType<FunctionCallContent>().ToList();
        return aiFunction.Count > 0 ? aiFunction : null;
    }

    /// <summary>
    /// Applies schema transformations to make schemas compatible with Gemini API restrictions.
    /// </summary>
    private static void ApplySchemaTransformations(JsonNode node)
    {
        if (node is JsonObject obj)
        {
            // Convert DateOnly/TimeOnly formats from "date"/"time" to "date-time" for Google API compatibility
            if (obj["format"] is { } formatNode && formatNode.GetValueKind() is JsonValueKind.String)
            {
                var formatValue = formatNode.GetValue<string>();
                if (formatValue is "date" or "time")
                {
                    obj["format"] = "date-time";
                }
                else if (formatValue is not ("enum" or "date-time"))
                {
                    // For unsupported formats like "email", remove the format and add it to description
                    _ = obj.Remove("format");

                    obj["description"] = obj["description"] is { } descriptionNode && descriptionNode.GetValueKind() is JsonValueKind.String ?
                        $"{descriptionNode.GetValue<string>()} (format: {formatValue})" :
                        $"format: {formatValue}";
                }
            }

            // Recursively apply transformations to nested properties
            if (obj["properties"] is JsonObject properties)
            {
                foreach (var property in properties)
                {
                    if (property.Value != null)
                    {
                        ApplySchemaTransformations(property.Value);
                    }
                }
            }

            // Recursively apply transformations to items (for arrays)
            if (obj["items"] is JsonNode items)
            {
                ApplySchemaTransformations(items);
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var item in array)
            {
                if (item != null)
                {
                    ApplySchemaTransformations(item);
                }
            }
        }
    }

    /// <summary>
    /// Gets a JSON schema transformer cache conforming to known Gemini restrictions.
    /// </summary>
    private static AIJsonSchemaTransformCache s_schemaTransformerCache { get; } = new(new()
    {
        TransformSchemaNode = (ctx, node) =>
        {
            // Move content from common but unsupported properties to description. In particular, we focus on properties that
            // the AIJsonUtilities schema generator might produce and that we know to be unsupported by Gemini.

            if (node is JsonObject schemaObj)
            {
                // Convert DateOnly/TimeOnly formats from "date"/"time" to "date-time" for Google API compatibility
                if (schemaObj["format"] is { } formatNode && formatNode.GetValueKind() is JsonValueKind.String)
                {
                    var formatValue = formatNode.GetValue<string>();
                    if (formatValue is "date" or "time")
                    {
                        schemaObj["format"] = "date-time";
                    }
                    else if (formatValue is not ("enum" or "date-time"))
                    {
                        // For unsupported formats like "email", remove the format and add it to description
                        _ = schemaObj.Remove("format");

                        schemaObj["description"] = schemaObj["description"] is { } descriptionNode && descriptionNode.GetValueKind() is JsonValueKind.String ?
                            $"{descriptionNode.GetValue<string>()} (format: {formatValue})" :
                            $"format: {formatValue}";
                    }
                }
            }

            return node;
        },
    });

    /// <summary>
    /// Creates a GenerativeAIEmbeddingGenerator from an API key and optional model name.
    /// </summary>
    /// <param name="apiKey">The API key for authenticating with Google AI.</param>
    /// <param name="modelName">The name of the embedding model to use. Defaults to "text-embedding-004".</param>
    /// <returns>A new instance of GenerativeAIEmbeddingGenerator.</returns>
    public static GenerativeAIEmbeddingGenerator AsEmbeddingGenerator(this string apiKey, string modelName = "text-embedding-004")
    {
        return new GenerativeAIEmbeddingGenerator(apiKey, modelName);
    }

    /// <summary>
    /// Creates a GenerativeAIEmbeddingGenerator from a platform adapter and optional model name.
    /// </summary>
    /// <param name="adapter">The platform adapter to use for API calls.</param>
    /// <param name="modelName">The name of the embedding model to use. Defaults to "text-embedding-004".</param>
    /// <returns>A new instance of GenerativeAIEmbeddingGenerator.</returns>
    public static GenerativeAIEmbeddingGenerator AsEmbeddingGenerator(this IPlatformAdapter adapter, string modelName = "text-embedding-004")
    {
        return new GenerativeAIEmbeddingGenerator(adapter, modelName);
    }

    /// <summary>
    /// Sets the task type for embedding generation.
    /// </summary>
    /// <param name="options">The embedding generation options to modify.</param>
    /// <param name="taskType">The task type for which the embedding will be optimized.</param>
    /// <returns>The modified embedding generation options.</returns>
    public static EmbeddingGenerationOptions WithTaskType(this EmbeddingGenerationOptions options, TaskType taskType)
    {
        options.AdditionalProperties ??= new AdditionalPropertiesDictionary();
        options.AdditionalProperties["TaskType"] = taskType;
        return options;
    }
}