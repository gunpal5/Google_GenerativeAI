using System.Text.Json.Serialization;

namespace GenerativeAI.Types
{
    [JsonSerializable(typeof(BatchEmbedContentsResponse))]
    [JsonSerializable(typeof(EnhancedGenerateContentResponse))]
    [JsonSerializable(typeof(GenerateContentResult))]
    [JsonSerializable(typeof(GenerateContentRequest))]
    [JsonSerializable(typeof(StartChatParams))]
    [JsonSerializable(typeof(GenerateContentResponse))]
    [JsonSerializable(typeof(CountTokensRequest))]
    [JsonSerializable(typeof(CountTokensResponse))]
    [JsonSourceGenerationOptions(
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        UseStringEnumConverter = true
        )]
    internal partial class GoogleSerializerContext : JsonSerializerContext
    {
    }
}
