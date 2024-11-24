using GenerativeAI.Tools;
using GenerativeAI.Types;
using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable CA1033 // Interface methods should be callable by child types
#pragma warning disable CA1063 // Implement IDisposable Correctly
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize

namespace GenerativeAI.Models
{
    public partial class GenerativeModel : ModelBase, IChatClient
    {
        private ChatClientMetadata? _metadata;

        ChatClientMetadata IChatClient.Metadata => _metadata ??= new(GetType().Name, new(BaseUrl), this.Model);

        object? IChatClient.GetService(Type serviceType, object? key)
        {
            return key is null && serviceType?.IsInstanceOfType(this) is true ? this : null;
        }

        void IDisposable.Dispose() { }

        async Task<ChatCompletion> IChatClient.CompleteAsync(
            IList<ChatMessage> chatMessages, ChatOptions? options, CancellationToken cancellationToken)
        {
            // TODO: GenerateContent doesn't accept a cancellation token, so cancellationToken is currently ignored.

            EnhancedGenerateContentResponse result = await GenerateContent(this.ApiKey, options?.ModelId ?? this.Model, CreateRequest(chatMessages, options)).ConfigureAwait(false);

            List<ChatMessage> messages = new();
            if (result.Candidates is not null)
            {
                foreach (GenerateContentCandidate candidate in result.Candidates)
                {
                    if (candidate.Content is Content choice)
                    {
                        messages.Add(new()
                        {
                            RawRepresentation = choice,
                            Role = choice.Role is "model" ? ChatRole.Assistant : ChatRole.User,
                            Contents = GetContentFromParts(choice.Parts),
                        });
                    }
                }
            }

            return new(messages)
            {
                RawRepresentation = result,
            };
        }

        async IAsyncEnumerable<StreamingChatCompletionUpdate> IChatClient.CompleteStreamingAsync(
            IList<ChatMessage> chatMessages, ChatOptions? options, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            // TODO: StreamContentAsync only yields text. As such, this implementation only supports creating TextContent and
            // won't create FunctionCallContent. This is a limitation of the underlying implementation. If/when that's improved,
            // this should be improved as well.

            await foreach (string text in StreamContentAsync(this.ApiKey, options?.ModelId ?? this.Model, CreateRequest(chatMessages, options), cancellationToken).ConfigureAwait(false))
            {
                yield return new StreamingChatCompletionUpdate() { Text = text };
            }
        }

        private GenerateContentRequest CreateRequest(IList<ChatMessage> chatMessages, ChatOptions? options)
        {
            GenerateContentRequest request = new()
            {
                GenerationConfig = new()
                {
                    Temperature = options?.Temperature ?? this.Config?.Temperature,
                    MaxOutputTokens = options?.MaxOutputTokens ?? this.Config?.MaxOutputTokens,
                    TopP = options?.TopP ?? this.Config?.TopP,
                    TopK = options?.TopK ?? this.Config?.TopK,
                    StopSequences = options?.StopSequences?.ToArray() ?? this.Config?.StopSequences,
                    CandidateCount = options?.AdditionalProperties?.TryGetValue(nameof(GenerationConfig.CandidateCount), out int candidateCount) is true ? candidateCount : this.Config?.CandidateCount,
                },
                SafetySettings = this.SafetySettings,
            };

            Part[] systemParts = (from m in chatMessages
                                  where m.Role == ChatRole.System
                                  from c in m.Contents
                                  let p = CreatePart(c)
                                  where p is not null
                                  select p).ToArray();
            if (systemParts.Length > 0)
            {
                request.SystemInstruction = new Content(systemParts, "system");
            }

            request.Contents = (from m in chatMessages
                                where m.Role != ChatRole.System
                                select new Content((from c in m.Contents
                                                    let p = CreatePart(c)
                                                    where p is not null
                                                    select p).ToArray(), m.Role == ChatRole.Assistant ? "model" : "user")).ToArray();

            request.Tools = options?.Tools?.OfType<AIFunction>().Select(f => new Tools.GenerativeAITool()
            {
                FunctionDeclaration = new()
                {
                    new ChatCompletionFunction()
                    {
                        Name = f.Metadata.Name,
                        Description = f.Metadata.Description,
                        Parameters = FunctionParameters.CreateSchema(f),
                    }
                }
            }).ToList()!;

            return request;
        }

        private static Part? CreatePart(AIContent content)
        {
            return content switch
            {
                TextContent text => new Part { Text = text.Text },
                ImageContent image when image.ContainsData => new Part
                {
                    InlineData = new GenerativeContentBlob()
                    {
                        MimeType = image.MediaType,
                        Data = Convert.ToBase64String(image.Data!.Value.ToArray()),
                    }
                },
                FunctionCallContent fcc => new Part
                {
                    FunctionCall = new Tools.ChatFunctionCall()
                    {
                        Name = fcc.Name,
                        Arguments = fcc.Arguments!,
                    }
                },
                FunctionResultContent frc => new Part
                {
                    FunctionResponse = new ChatFunctionResponse()
                    {
                        Name = frc.CallId, // TODO: frc.Name?
                        Response = new FunctionResponse()
                        {
                            Name = frc.Name,
                            Content = JsonSerializer.SerializeToNode(frc.Result)!,
                        }
                    }
                },
                _ => null,
            };
        }

        private static List<AIContent>? GetContentFromParts(Part[]? parts)
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
                        (contents ??= new()).Add(new FunctionCallContent(part.FunctionCall.Name, part.FunctionCall.Name, part.FunctionCall.Arguments!));
                    }

                    if (part.InlineData is not null)
                    {
                        byte[] data = Convert.FromBase64String(part.InlineData.Data!);
                        (contents ??= new()).Add(part.InlineData.MimeType is "image/png" or "image/jpeg" or "image/gif" or "image/webp" ?
                            new ImageContent(data, part.InlineData.MimeType) :
                            new DataContent(data, part.InlineData.MimeType));
                    }
                }
            }

            return contents;
        }

        private sealed class FunctionParameters
        {
            private static readonly JsonElement s_defaultParameterSchema = JsonDocument.Parse("{}").RootElement;

            [JsonPropertyName("type")]
            public string Type { get; set; } = "object";

            [JsonPropertyName("required")]
            public List<string> Required { get; set; } = new();

            [JsonPropertyName("properties")]
            public Dictionary<string, JsonElement> Properties { get; set; } = new();

            public static ChatCompletionFunctionParameters CreateSchema(AIFunction f)
            {
                IReadOnlyList<AIFunctionParameterMetadata> parameters = f.Metadata.Parameters;

                FunctionParameters schema = new();

                foreach (AIFunctionParameterMetadata parameter in parameters)
                {
                    schema.Properties.Add(parameter.Name, parameter.Schema is JsonElement e ? e : s_defaultParameterSchema);

                    if (parameter.IsRequired)
                    {
                        schema.Required.Add(parameter.Name);
                    }
                }

                return JsonSerializer.Deserialize<ChatCompletionFunctionParameters>(JsonSerializer.Serialize(schema))!;
            }
        }
    }
}
