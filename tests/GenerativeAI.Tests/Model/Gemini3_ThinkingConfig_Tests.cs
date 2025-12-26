using System.Text.Json;
using GenerativeAI.Types;
using Shouldly;
using Xunit;

namespace GenerativeAI.Tests.Model;

/// <summary>
/// Unit tests for Gemini 3 thinking features including ThinkingConfig and thought signatures.
/// </summary>
public class Gemini3_ThinkingConfig_Tests
{
    #region ThinkingConfig Tests

    [Fact]
    public void ThinkingConfig_ShouldSerialize_WithAllProperties()
    {
        // Arrange
        var config = new ThinkingConfig
        {
            IncludeThoughts = true,
            ThinkingBudget = 1024,
            ThinkingLevel = ThinkingLevel.HIGH
        };

        // Act
        var json = JsonSerializer.Serialize(config);
        var deserialized = JsonSerializer.Deserialize<ThinkingConfig>(json);

        // Assert
        json.ShouldContain("\"includeThoughts\":true");
        json.ShouldContain("\"thinkingBudget\":1024");
        json.ShouldContain("\"thinkingLevel\"");

        deserialized.ShouldNotBeNull();
        deserialized.IncludeThoughts.ShouldBe(true);
        deserialized.ThinkingBudget.ShouldBe(1024);
        deserialized.ThinkingLevel.ShouldBe(ThinkingLevel.HIGH);
    }

    [Fact]
    public void ThinkingConfig_ShouldSerialize_WithLowThinkingLevel()
    {
        // Arrange
        var config = new ThinkingConfig
        {
            IncludeThoughts = true,
            ThinkingLevel = ThinkingLevel.LOW
        };

        // Act
        var json = JsonSerializer.Serialize(config);
        var deserialized = JsonSerializer.Deserialize<ThinkingConfig>(json);

        // Assert
        deserialized.ShouldNotBeNull();
        deserialized.ThinkingLevel.ShouldBe(ThinkingLevel.LOW);
    }

    [Fact]
    public void ThinkingConfig_ShouldSerialize_WithUnspecifiedThinkingLevel()
    {
        // Arrange
        var config = new ThinkingConfig
        {
            ThinkingLevel = ThinkingLevel.THINKING_LEVEL_UNSPECIFIED
        };

        // Act
        var json = JsonSerializer.Serialize(config);
        var deserialized = JsonSerializer.Deserialize<ThinkingConfig>(json);

        // Assert
        deserialized.ShouldNotBeNull();
        deserialized.ThinkingLevel.ShouldBe(ThinkingLevel.THINKING_LEVEL_UNSPECIFIED);
    }

    [Fact]
    public void ThinkingConfig_ShouldSerialize_WithOnlyThinkingBudget()
    {
        // Arrange
        var config = new ThinkingConfig
        {
            ThinkingBudget = 2048
        };

        // Act
        var json = JsonSerializer.Serialize(config);
        var deserialized = JsonSerializer.Deserialize<ThinkingConfig>(json);

        // Assert
        deserialized.ShouldNotBeNull();
        deserialized.ThinkingBudget.ShouldBe(2048);
        deserialized.IncludeThoughts.ShouldBeNull();
        deserialized.ThinkingLevel.ShouldBeNull();
    }

    #endregion

    #region GenerationConfig with ThinkingConfig Tests

    [Fact]
    public void GenerationConfig_ShouldIncludeThinkingConfig()
    {
        // Arrange
        var generationConfig = new GenerationConfig
        {
            Temperature = 0.7,
            MaxOutputTokens = 8192,
            ThinkingConfig = new ThinkingConfig
            {
                IncludeThoughts = true,
                ThinkingBudget = 4096,
                ThinkingLevel = ThinkingLevel.HIGH
            }
        };

        // Act
        var json = JsonSerializer.Serialize(generationConfig);
        var deserialized = JsonSerializer.Deserialize<GenerationConfig>(json);

        // Assert
        json.ShouldContain("\"thinkingConfig\"");
        json.ShouldContain("\"thinkingLevel\"");

        deserialized.ShouldNotBeNull();
        deserialized.ThinkingConfig.ShouldNotBeNull();
        deserialized.ThinkingConfig!.IncludeThoughts.ShouldBe(true);
        deserialized.ThinkingConfig.ThinkingBudget.ShouldBe(4096);
        deserialized.ThinkingConfig.ThinkingLevel.ShouldBe(ThinkingLevel.HIGH);
    }

    #endregion

    #region Part Thought Properties Tests

    [Fact]
    public void Part_ShouldSerialize_WithThoughtProperty()
    {
        // Arrange
        var part = new Part
        {
            Text = "This is a thought from the model",
            Thought = true
        };

        // Act
        var json = JsonSerializer.Serialize(part);
        var deserialized = JsonSerializer.Deserialize<Part>(json);

        // Assert
        json.ShouldContain("\"thought\":true");
        deserialized.ShouldNotBeNull();
        deserialized.Thought.ShouldBe(true);
        deserialized.Text.ShouldBe("This is a thought from the model");
    }

    [Fact]
    public void Part_ShouldSerialize_WithThoughtSignature()
    {
        // Arrange
        var signature = "dGhpcyBpcyBhIGJhc2U2NCBlbmNvZGVkIHNpZ25hdHVyZQ=="; // base64 encoded string
        var part = new Part
        {
            FunctionCall = new FunctionCall("get_weather")
            {
                Args = System.Text.Json.Nodes.JsonNode.Parse("{\"city\": \"Paris\"}")
            },
            ThoughtSignature = signature
        };

        // Act
        var json = JsonSerializer.Serialize(part);
        var deserialized = JsonSerializer.Deserialize<Part>(json);

        // Assert
        json.ShouldContain("\"thoughtSignature\"");
        json.ShouldContain(signature);
        deserialized.ShouldNotBeNull();
        deserialized.ThoughtSignature.ShouldBe(signature);
        deserialized.FunctionCall.ShouldNotBeNull();
        deserialized.FunctionCall!.Name.ShouldBe("get_weather");
    }

    [Fact]
    public void Part_ShouldSerialize_WithBothThoughtAndSignature()
    {
        // Arrange
        var signature = "c2lnbmF0dXJlX2Zvcl90aG91Z2h0";
        var part = new Part
        {
            Text = "Model's internal reasoning",
            Thought = true,
            ThoughtSignature = signature
        };

        // Act
        var json = JsonSerializer.Serialize(part);
        var deserialized = JsonSerializer.Deserialize<Part>(json);

        // Assert
        deserialized.ShouldNotBeNull();
        deserialized.Thought.ShouldBe(true);
        deserialized.ThoughtSignature.ShouldBe(signature);
    }

    #endregion

    #region Content Preservation Tests

    [Fact]
    public void Content_ShouldPreserve_ThoughtSignature_InParts()
    {
        // Arrange - Simulating a response from Gemini 3 with function call and thought signature
        var signature = "ZnVuY3Rpb25fY2FsbF9zaWduYXR1cmU=";
        var parts = new List<Part>
        {
            new Part
            {
                FunctionCall = new FunctionCall("search_books")
                {
                    Args = System.Text.Json.Nodes.JsonNode.Parse("{\"genre\": \"science fiction\"}")
                },
                ThoughtSignature = signature
            }
        };

        var content = new Content(parts, "model");

        // Act
        var json = JsonSerializer.Serialize(content);
        var deserialized = JsonSerializer.Deserialize<Content>(json);

        // Assert
        deserialized.ShouldNotBeNull();
        deserialized.Parts.ShouldNotBeNull();
        deserialized.Parts.Count.ShouldBe(1);
        deserialized.Parts[0].ThoughtSignature.ShouldBe(signature);
        deserialized.Parts[0].FunctionCall.ShouldNotBeNull();
    }

    [Fact]
    public void Content_Constructor_ShouldPreserve_PartReferences()
    {
        // Arrange
        var signature = "b3JpZ2luYWxfc2lnbmF0dXJl";
        var originalPart = new Part
        {
            FunctionCall = new FunctionCall("get_weather"),
            ThoughtSignature = signature
        };
        var parts = new List<Part> { originalPart };

        // Act
        var content = new Content(parts, "model");

        // Assert - Content should reference the same parts, preserving ThoughtSignature
        content.Parts[0].ThoughtSignature.ShouldBe(signature);
        content.Parts[0].ShouldBe(originalPart); // Same reference
    }

    #endregion

    #region ThinkingLevel Enum Tests

    [Theory]
    [InlineData(ThinkingLevel.THINKING_LEVEL_UNSPECIFIED)]
    [InlineData(ThinkingLevel.LOW)]
    [InlineData(ThinkingLevel.HIGH)]
    public void ThinkingLevel_ShouldRoundTrip_AllValues(ThinkingLevel level)
    {
        // Arrange
        var config = new ThinkingConfig { ThinkingLevel = level };

        // Act
        var json = JsonSerializer.Serialize(config);
        var deserialized = JsonSerializer.Deserialize<ThinkingConfig>(json);

        // Assert
        deserialized.ShouldNotBeNull();
        deserialized.ThinkingLevel.ShouldBe(level);
    }

    #endregion

    #region Model Constants Tests

    [Fact]
    public void GoogleAIModels_ShouldContain_Gemini3Models()
    {
        // Assert
        GoogleAIModels.Gemini3ProPreview.ShouldBe("models/gemini-3-pro-preview");
        GoogleAIModels.Gemini3FlashPreview.ShouldBe("models/gemini-3-flash-preview");
    }

    #endregion
}
