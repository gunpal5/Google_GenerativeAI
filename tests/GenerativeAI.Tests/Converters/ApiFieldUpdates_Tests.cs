using System.Text.Json;
using System.Text.Json.Nodes;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Converters;

/// <summary>
/// Wire-format tests for fields and enum values added to match google-genai v3.3.0.
/// </summary>
public class ApiFieldUpdates_Tests
{
    private static T Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, DefaultSerializerOptions.Options)!;

    private static JsonNode Serialize<T>(T value) =>
        JsonNode.Parse(JsonSerializer.Serialize(value, DefaultSerializerOptions.Options))!;

    [Fact]
    public void Response_WithNewEnumValuesAndFields_Deserializes()
    {
        const string json = """
        {
          "candidates": [{
            "content": {"role": "model", "parts": [
              {"text": "partial", "mediaResolution": {"level": "MEDIA_RESOLUTION_ULTRA_HIGH"}},
              {"toolCall": {"id": "c1", "toolType": "GOOGLE_SEARCH_WEB", "args": {"query": "weather"}}},
              {"toolResponse": {"id": "c1", "toolType": "GOOGLE_SEARCH_WEB", "response": {"ok": true}}},
              {"executableCode": {"id": "e1", "language": "PYTHON", "code": "print(1)"}},
              {"codeExecutionResult": {"id": "e1", "outcome": "OUTCOME_OK", "output": "1"}}
            ]},
            "finishReason": "CONTINUATION",
            "continuationToken": "dG9rZW4=",
            "safetyRatings": [{"category": "HARM_CATEGORY_JAILBREAK", "probability": "NEGLIGIBLE"}],
            "groundingMetadata": {
              "imageSearchQueries": ["cats"],
              "groundingChunks": [
                {"image": {"sourceUri": "https://a.example", "imageUri": "https://a.example/cat.png", "title": "Cat", "domain": "a.example"}},
                {"retrievedContext": {"fileSearchStore": "fileSearchStores/s1", "pageNumber": 3,
                  "customMetadata": [{"key": "author", "stringValue": "Ann"}, {"key": "tags", "stringListValue": {"values": ["x", "y"]}}]}}
              ],
              "groundingSupports": [{"renderedParts": [0, 2]}]
            }
          }],
          "promptFeedback": {"blockReason": "JAILBREAK"},
          "modelStatus": {"message": "Deprecated", "modelStage": "DEPRECATED", "retirementTime": "2026-12-01T00:00:00Z"}
        }
        """;

        var response = Deserialize<GenerateContentResponse>(json);

        var candidate = response.Candidates![0];
        candidate.FinishReason.ShouldBe(FinishReason.CONTINUATION);
        candidate.ContinuationToken.ShouldBe("dG9rZW4=");
        candidate.SafetyRatings![0].Category.ShouldBe(HarmCategory.HARM_CATEGORY_JAILBREAK);

        var parts = candidate.Content!.Parts;
        parts[0].MediaResolution!.Level.ShouldBe(PartMediaResolutionLevel.MEDIA_RESOLUTION_ULTRA_HIGH);
        parts[1].ToolCall!.ToolType.ShouldBe(ToolType.GOOGLE_SEARCH_WEB);
        parts[1].ToolCall!.Args!["query"]!.GetValue<string>().ShouldBe("weather");
        parts[2].ToolResponse!.Id.ShouldBe("c1");
        parts[3].ExecutableCode!.Id.ShouldBe("e1");
        parts[4].CodeExecutionResult!.Id.ShouldBe("e1");

        var grounding = candidate.GroundingMetadata!;
        grounding.ImageSearchQueries.ShouldBe(new[] { "cats" });
        grounding.GroundingChunks![0].Image!.ImageUri.ShouldBe("https://a.example/cat.png");
        var retrieved = grounding.GroundingChunks[1].RetrievedContext!;
        retrieved.FileSearchStore.ShouldBe("fileSearchStores/s1");
        retrieved.PageNumber.ShouldBe(3);
        retrieved.CustomMetadata![1].StringListValue!.Values.ShouldBe(new[] { "x", "y" });
        grounding.GroundingSupports![0].RenderedParts.ShouldBe(new[] { 0, 2 });

        response.PromptFeedback!.BlockReason.ShouldBe(BlockReason.JAILBREAK);
        response.ModelStatus!.ModelStage.ShouldBe(ModelStage.DEPRECATED);
    }

    [Fact]
    public void Request_NewFields_SerializeWithWireNames()
    {
        var request = new GenerateContentRequest
        {
            ServiceTier = ServiceTiers.Flex,
            Store = false,
            Labels = new Dictionary<string, string> { ["team"] = "ml" },
            ContinuationToken = "dG9rZW4=",
            Tools = new List<Tool>
            {
                new()
                {
                    FileSearch = new FileSearch { FileSearchStoreNames = new List<string> { "fileSearchStores/s1" }, TopK = 5 },
                    McpServers = new List<McpServer>
                    {
                        new() { Name = "docs", StreamableHttpTransport = new StreamableHttpTransport { Url = "https://mcp.example/mcp" } }
                    }
                },
                new()
                {
                    GoogleSearch = new GoogleSearchTool { SearchTypes = new SearchTypes { ImageSearch = new ImageSearch() } }
                }
            },
            ToolConfig = new ToolConfig { IncludeServerSideToolInvocations = true },
            GenerationConfig = new GenerationConfig { ThinkingConfig = new ThinkingConfig { ThinkingLevel = ThinkingLevel.XHIGH } }
        };

        var json = Serialize(request);

        json["serviceTier"]!.GetValue<string>().ShouldBe("flex");
        json["store"]!.GetValue<bool>().ShouldBeFalse();
        json["labels"]!["team"]!.GetValue<string>().ShouldBe("ml");
        json["continuationToken"]!.GetValue<string>().ShouldBe("dG9rZW4=");
        json["tools"]![0]!["fileSearch"]!["fileSearchStoreNames"]![0]!.GetValue<string>().ShouldBe("fileSearchStores/s1");
        json["tools"]![0]!["mcpServers"]![0]!["streamableHttpTransport"]!["url"]!.GetValue<string>().ShouldBe("https://mcp.example/mcp");
        json["tools"]![1]!["googleSearch"]!["searchTypes"]!["imageSearch"].ShouldNotBeNull();
        json["toolConfig"]!["includeServerSideToolInvocations"]!.GetValue<bool>().ShouldBeTrue();
        json["generationConfig"]!["thinkingConfig"]!["thinkingLevel"]!.GetValue<string>().ShouldBe("XHIGH");
    }

    [Theory]
    [InlineData("\"MINIMAL\"", ThinkingLevel.MINIMAL)]
    [InlineData("\"MEDIUM\"", ThinkingLevel.MEDIUM)]
    [InlineData("\"XHIGH\"", ThinkingLevel.XHIGH)]
    public void ThinkingLevel_NewValues_Deserialize(string json, ThinkingLevel expected)
    {
        Deserialize<ThinkingLevel>(json).ShouldBe(expected);
    }

    [Fact]
    public void CrossReferencedLiveTypes_SerializeAndDeserialize()
    {
        var config = new GenerationConfig
        {
            AudioTranscriptionConfig = new AudioTranscriptionConfig(),
            TranslationConfig = new TranslationConfig { TargetLanguageCode = "fr-FR" }
        };
        var json = Serialize(config);
        json["audioTranscriptionConfig"].ShouldNotBeNull();
        json["translationConfig"]!["targetLanguageCode"]!.GetValue<string>().ShouldBe("fr-FR");

        var part = Deserialize<Part>("""{"audioTranscription": {"text": "hello", "languageCode": "en-US"}}""");
        part.AudioTranscription!.Text.ShouldBe("hello");
        part.AudioTranscription.LanguageCode.ShouldBe("en-US");

        var setupComplete = Deserialize<BidiGenerateContentSetupComplete>(
            """{"sessionId": "s1", "voiceConsentSignature": {"signature": "sig"}}""");
        setupComplete.VoiceConsentSignature!.Signature.ShouldBe("sig");
    }

    [Fact]
    public void FunctionCallingMode_Validated_RoundTrips()
    {
        var json = JsonSerializer.Serialize(FunctionCallingMode.VALIDATED, DefaultSerializerOptions.Options);
        json.ShouldBe("\"VALIDATED\"");
        Deserialize<FunctionCallingMode>(json).ShouldBe(FunctionCallingMode.VALIDATED);
    }
}
