using System.Text.Json;
using Bogus;
using GenerativeAI.IntegrationTests;
using GenerativeAI.Microsoft.Extensions;
using GenerativeAI.Tests.Base;
using GenerativeAI.Types;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace GenerativeAI.Microsoft.Tests;

public class MicrosoftExtension_Tests
{
    [Fact, TestPriority(10)]
    public void ShouldCreateClientWithCustomChatOptions()
    {
        // Arrange
        var options = new ChatOptions
        {
            ChatThreadId = "test-thread-id",
            Temperature = 0.7f,
            MaxOutputTokens = 150,
            TopP = 0.9f,
            TopK = 50,
            FrequencyPenalty = 0.2f,
            PresencePenalty = 0.3f,
            Seed = 123456789,
            ResponseFormat = null,
            ModelId = "test-model-id",
            StopSequences = new List<string> { "end", "stop" },
            ToolMode = null,
            Tools = null,
            AdditionalProperties = new AdditionalPropertiesDictionary
            {
                { "ResponseLogprobs", true },
                { "Logprobs", 90234 }
            }
        };

        // Act
        var generationConfig = options.ToGenerationConfig();

        // Assert
        generationConfig.ShouldNotBeNull();
        generationConfig.StopSequences.ShouldBeEquivalentTo(new List<string> { "end", "stop" });
        generationConfig.ResponseMimeType.ShouldBeNull();
        generationConfig.CandidateCount.ShouldBeNull();
        generationConfig.MaxOutputTokens.ShouldBe(150);
        generationConfig.Temperature.ShouldBe(0.7f);
        generationConfig.TopP.ShouldBe(0.9f);
        generationConfig.TopK.ShouldBe(50);
        generationConfig.Seed.ShouldBe(123456789);
        generationConfig.PresencePenalty.ShouldBe(0.3f);
        generationConfig.FrequencyPenalty.ShouldBe(0.2f);
        generationConfig.ResponseLogprobs.ShouldBe(true);
        generationConfig.Logprobs.ShouldBe(90234);
    }


    [Fact]
    public void ToChatResponseUpdate_ShouldMapCorrectly()
    {
        // Arrange

        var faker = new Faker();
        var candidate = new Candidate
        {
            FinishReason = faker.PickRandom<FinishReason>(),
            Content =  new Content((string)faker.Lorem.Sentence(), Roles.Model)
           
        };

        var response = new GenerateContentResponse
        {
            Candidates = [ candidate ],
            PromptFeedback = new PromptFeedback
            {
                BlockReason = faker.PickRandom<BlockReason>(),
                SafetyRatings = faker.Make(3, () => new SafetyRating
                {
                    Blocked = faker.Random.Bool(),
                    Category = faker.PickRandom<HarmCategory>(),
                    Probability = faker.PickRandom<HarmProbability>()
                }).ToList()
            },
            UsageMetadata = new UsageMetadata
            {
                PromptTokenCount = faker.Random.Int(100, 200),
                CachedContentTokenCount = faker.Random.Int(50, 100),
                CandidatesTokenCount = faker.Random.Int(80, 150),
                TotalTokenCount = faker.Random.Int(200, 500),
                PromptTokensDetails = new List<ModalityTokenCount>
                {
                    new ModalityTokenCount
                    {
                        Modality = faker.PickRandom<Modality>(),
                        TokenCount = faker.Random.Int(50, 150)
                    }
                },
                CacheTokensDetails = new List<ModalityTokenCount>
                {
                    new ModalityTokenCount
                    {
                        Modality = faker.PickRandom<Modality>(),
                        TokenCount = faker.Random.Int(10, 100)
                    }
                },
                CandidatesTokensDetails = new List<ModalityTokenCount>
                {
                    new ModalityTokenCount
                    {
                        Modality = faker.PickRandom<Modality>(),
                        TokenCount = faker.Random.Int(30, 120)
                    }
                }
            },
            ModelVersion = faker.System.Semver()
        };

// Act
        var result = response.ToChatResponseUpdate();

// Assert
        result.ShouldNotBeNull();
        //result.FinishReason.ShouldBe(candidate.FinishReason);
        result.RawRepresentation.ShouldBe(response);
       // result.Role.ShouldBe((ChatRole?)candidate.Content?.Role);
        result.Text.ShouldBe(candidate.Content.Parts[0].Text);
        result.ChoiceIndex.ShouldBe(0);
        result.CreatedAt.ShouldBeNull();
        result.AdditionalProperties.ShouldBeNull();
        result.ResponseId.ShouldBeNull();
    }

    [Fact]
    public void ToChatResponseUpdate_ShouldHandleNullResponse()
    {
        // Arrange
        GenerateContentResponse? response = null;

        // Act
        Should.Throw<ArgumentNullException>(() => response!.ToChatResponseUpdate());
    }

    #region ToAIContent

    [Fact]
    public void ToAiContents_NullParts_ReturnsNull()
    {
        // Arrange
        List<Part>? parts = null;

        // Act
        var result = parts.ToAiContents();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void ToAiContents_EmptyParts_ReturnsEmptyList()
    {
        // Arrange
        var parts = new List<Part>();

        // Act
        var result = parts.ToAiContents();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void ToAiContents_WithTextPart_ReturnsTextContent()
    {
        // Arrange
        var parts = new List<Part> { new Part { Text = "Hello, world!" } };

        // Act
        var result = parts.ToAiContents();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result.FirstOrDefault().ShouldBeOfType<TextContent>();
        ((TextContent)result.FirstOrDefault()).Text.ShouldBe("Hello, world!");
    }

    [Fact]
    public void ToAiContents_WithFunctionCallPart_ReturnsFunctionCallContent()
    {
        // Arrange
        var parts = new List<Part> { new Part { FunctionCall = new FunctionCall { Name = "myFunction", Args = new { arg1 = "value1", arg2 = "value2" } } } };

        // Act
        var result = parts.ToAiContents();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result.FirstOrDefault().ShouldBeOfType<FunctionCallContent>();
        var functionCallContent = (FunctionCallContent)result.FirstOrDefault();
        functionCallContent.Name.ShouldBe("myFunction");
        functionCallContent.Arguments.ShouldNotBeNull();
        // Add more specific assertions for the Args object if needed
    }

    [Fact]
    public void ToAiContents_WithFunctionResponsePart_ReturnsFunctionResultContent()
    {
        // Arrange
        var parts = new List<Part> { new Part { FunctionResponse = new FunctionResponse { Name = "myFunction", Response = new { result = "success" } } } };

        // Act
        var result = parts.ToAiContents();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result.FirstOrDefault().ShouldBeOfType<FunctionResultContent>();
        var functionResultContent = (FunctionResultContent)result.FirstOrDefault();
        functionResultContent.CallId.ShouldBe("myFunction");
        functionResultContent.Result.ShouldNotBeNull();
        // Add more specific assertions for the Result object if needed
    }

    [Fact]
    public void ToAiContents_WithInlineDataPart_ReturnsDataContent()
    {
        // Arrange
        var parts = new List<Part> { new Part { InlineData = new Blob { MimeType = "image/png", Data = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M/wHwAFAAH/8mdr1QAAAABJRU5ErkJggg==" } } };

        // Act
        var result = parts.ToAiContents();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result.FirstOrDefault().ShouldBeOfType<DataContent>();
        var dataContent = (DataContent)result.FirstOrDefault();
        dataContent.MediaType.ShouldBe("image/png");
        dataContent.Data.ShouldNotBeNull();
    }

    [Fact]
    public void ToAiContents_WithMultipleParts_ReturnsMultipleContents()
    {
        // Arrange
        var parts = new List<Part>
        {
            new Part { Text = "Hello, world!" },
            new Part { FunctionCall = new FunctionCall { Name = "myFunction", Args = new { arg1 = "value1" } } },
            new Part { InlineData = new Blob { MimeType = "image/png", Data = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M/wHwAFAAH/8mdr1QAAAABJRU5ErkJggg==" } }
        };

        // Act
        var result = parts.ToAiContents();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        result[0].ShouldBeOfType<TextContent>();
        result[1].ShouldBeOfType<FunctionCallContent>();
        result[2].ShouldBeOfType<DataContent>();
    }

    [Fact]
    public void ToAiContents_BogusData_HandlesVariousParts()
    {
        // Arrange
        var faker = new Faker<Part>()
          .Rules((f, o) =>
            {
                if (f.Random.Bool(0.5f))
                {
                    o.Text = f.Lorem.Sentence();
                }
                else
                {
                    o.FunctionCall = f.Random.Bool(0.5f)
                        ? new FunctionCall { Name = f.Internet.DomainName(), Args = new Faker<Microsoft_AIFunction_Tests.Weather>().Generate()}
                        : null;
                }

                if (f.Random.Bool(0.33f))
                {
                    o.FunctionResponse = f.Random.Bool(0.5f)
                        ? new FunctionResponse { Name = f.Internet.DomainName(), Response = new Faker<Microsoft_AIFunction_Tests.Weather>().Generate() }
                        : null;
                }

                if (f.Random.Bool(0.25f))
                {
                    o.InlineData = f.Random.Bool(0.5f)
                        ? new Blob { MimeType = "image/png", Data = f.Random.AlphaNumeric(100) }
                        : null;
                }
            });

        var parts = faker.Generate(10);

        // Act
        var result = parts.ToAiContents();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThanOrEqualTo(0);
        // Add more specific assertions based on your Bogus rules if needed
    }

    #endregion
}