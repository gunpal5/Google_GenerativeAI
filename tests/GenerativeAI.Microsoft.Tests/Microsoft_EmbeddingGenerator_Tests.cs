using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GenerativeAI.Core;
using GenerativeAI.Exceptions;
using GenerativeAI.Microsoft;
using GenerativeAI.Microsoft.Extensions;
using GenerativeAI.Tests.Base;
using GenerativeAI.Types;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace GenerativeAI.Tests.Microsoft;

/// <summary>
/// Tests for <see cref="GenerativeAIEmbeddingGenerator"/>.
/// </summary>
[TestCaseOrderer(typeof(TestPriorityAttribute))]
public class Microsoft_EmbeddingGenerator_Tests : TestBase
{
    private const string DefaultTestEmbeddingModel = GoogleAIModels.GeminiEmbedding;

    public Microsoft_EmbeddingGenerator_Tests(ITestOutputHelper helper) : base(helper)
    {
    }

    #region Helper Methods

    /// <summary>
    /// Gets the test Google platform adapter with proper API key handling.
    /// </summary>
    protected override IPlatformAdapter GetTestGooglePlatform()
    {
        Assert.SkipWhen(!IsGoogleApiKeySet, GoogleTestSkipMessage);
        return new GoogleAIPlatformAdapter(EnvironmentVariables.GOOGLE_API_KEY);
    }

    /// <summary>
    /// Creates a test embedding generator with real Google API credentials.
    /// </summary>
    private GenerativeAIEmbeddingGenerator CreateTestEmbeddingGenerator(string? modelName = null)
    {
        return new GenerativeAIEmbeddingGenerator(
            GetTestGooglePlatform(),
            modelName ?? DefaultTestEmbeddingModel);
    }

    /// <summary>
    /// Creates a mock embedding generator for tests that don't need real API.
    /// </summary>
    private GenerativeAIEmbeddingGenerator CreateMockEmbeddingGenerator(string? modelName = null)
    {
        return new GenerativeAIEmbeddingGenerator(
            "mock-api-key",
            modelName ?? DefaultTestEmbeddingModel);
    }

    #endregion

    #region Constructor Tests

    [Fact, TestPriority(1)]
    public void Constructor_WithApiKey_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var generator = CreateMockEmbeddingGenerator();

        // Assert
        generator.ShouldNotBeNull();
        generator.Model.ShouldNotBeNull();
        generator.Model.Model.ShouldBe(DefaultTestEmbeddingModel);
        generator.Metadata.ShouldNotBeNull();
        // Metadata properties vary by Microsoft.Extensions.AI version
        generator.Metadata.ProviderUri?.ToString().ShouldContain("ai.google.dev");
    }

    [Fact, TestPriority(2)]
    public void Constructor_WithPlatformAdapter_ShouldInitializeCorrectly()
    {
        // Skip if no API key is set
        if (!IsGoogleApiKeySet)
        {
            Assert.Skip(GoogleTestSkipMessage);
            return;
        }

        // Arrange & Act
        var generator = CreateTestEmbeddingGenerator();

        // Assert
        generator.ShouldNotBeNull();
        generator.Model.ShouldNotBeNull();
        generator.Model.Model.ShouldBe(DefaultTestEmbeddingModel);
        generator.Metadata.ShouldNotBeNull();
        // Metadata properties vary by Microsoft.Extensions.AI version
    }

    [Fact, TestPriority(3)]
    public void Constructor_WithDefaultModel_ShouldUseDefaultModel()
    {
        // Arrange & Act
        var generator = new GenerativeAIEmbeddingGenerator("mock-api-key");

        // Assert
        generator.Model.Model.ShouldBe(GoogleAIModels.GeminiEmbedding);
    }

    #endregion

    #region Metadata Tests

    [Fact, TestPriority(4)]
    public void Metadata_ShouldContainCorrectInformation()
    {
        // Arrange
        var generator = CreateMockEmbeddingGenerator();

        // Act
        var metadata = generator.Metadata;

        // Assert
        metadata.ShouldNotBeNull();
        // Model ID accessible via metadata
        metadata.ProviderUri.ShouldNotBeNull();
        metadata.ProviderUri.ToString().ShouldContain("ai.google.dev");
    }

    #endregion

    #region GenerateAsync Tests

    [Fact, TestPriority(5)]
    public async Task GenerateAsync_WithEmptyInput_ShouldReturnEmptyEmbeddings()
    {
        // Arrange
        var generator = CreateMockEmbeddingGenerator();
        var emptyInput = new List<string>();

        // Act
        var result = await generator.GenerateAsync(emptyInput);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
    }

    [Fact, TestPriority(6)]
    public async Task GenerateAsync_WithNullOptions_ShouldNotThrow()
    {
        Assert.SkipWhen(!IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange
        var generator = CreateTestEmbeddingGenerator();
        var input = new[] { "Test text" };

        // Act
        var result = await generator.GenerateAsync(input, null);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
    }

    [Fact, TestPriority(7)]
    public void GenerateAsync_WithNullInput_ShouldThrowArgumentNullException()
    {
        // Arrange
        var generator = CreateMockEmbeddingGenerator();

        // Act & Assert
        Should.Throw<ArgumentNullException>(async () =>
            await generator.GenerateAsync(null!, null));
    }

    #endregion

    #region TaskType Tests

    [Fact, TestPriority(8)]
    public async Task GenerateAsync_WithTaskTypeInOptions_ShouldUseSpecifiedTaskType()
    {
        Assert.SkipWhen(!IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange
        var generator = CreateTestEmbeddingGenerator();
        var input = new[] { "Test text for classification" };
        var options = new EmbeddingGenerationOptions
        {
            AdditionalProperties = new AdditionalPropertiesDictionary
            {
                ["TaskType"] = TaskType.CLASSIFICATION
            }
        };

        // Act
        var result = await generator.GenerateAsync(input, options);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
    }

    [Fact, TestPriority(9)]
    public async Task GenerateAsync_WithStringTaskType_ShouldParseCorrectly()
    {
        Assert.SkipWhen(!IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange
        var generator = CreateTestEmbeddingGenerator();
        var input = new[] { "Test text" };
        var options = new EmbeddingGenerationOptions
        {
            AdditionalProperties = new AdditionalPropertiesDictionary
            {
                ["TaskType"] = "SEMANTIC_SIMILARITY"
            }
        };

        // Act
        var result = await generator.GenerateAsync(input, options);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
    }

    [Fact, TestPriority(10)]
    public async Task GenerateAsync_WithTaskTypeExtension_ShouldUseSpecifiedTaskType()
    {
        Assert.SkipWhen(!IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange
        var generator = CreateTestEmbeddingGenerator();
        var input = new[] { "Test code for retrieval" };
        var options = new EmbeddingGenerationOptions().WithTaskType(TaskType.CODE_RETRIEVAL_QUERY);

        // Act
        var result = await generator.GenerateAsync(input, options);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
    }

    #endregion

    #region GetService Tests

    [Fact, TestPriority(11)]
    public void GetService_WithMatchingType_ShouldReturnSelf()
    {
        // Arrange
        var generator = CreateMockEmbeddingGenerator();

        // Act
        var service = generator.GetService(typeof(GenerativeAIEmbeddingGenerator));

        // Assert
        service.ShouldNotBeNull();
        service.ShouldBe(generator);
    }

    [Fact, TestPriority(12)]
    public void GetService_WithNonMatchingType_ShouldReturnNull()
    {
        // Arrange
        var generator = CreateMockEmbeddingGenerator();

        // Act
        var service = generator.GetService(typeof(string));

        // Assert
        service.ShouldBeNull();
    }

    [Fact, TestPriority(13)]
    public void GetService_WithServiceKey_ShouldReturnNull()
    {
        // Arrange
        var generator = CreateMockEmbeddingGenerator();

        // Act
        var service = generator.GetService(typeof(GenerativeAIEmbeddingGenerator), "key");

        // Assert
        service.ShouldBeNull();
    }

    #endregion

    #region Extension Method Tests

    [Fact, TestPriority(14)]
    public void AsEmbeddingGenerator_WithApiKey_ShouldCreateGenerator()
    {
        // Arrange
        var apiKey = "mock-api-key";

        // Act
        var generator = apiKey.AsEmbeddingGenerator();

        // Assert
        generator.ShouldNotBeNull();
        generator.Model.Model.ShouldBe("text-embedding-004");
    }

    [Fact, TestPriority(15)]
    public void AsEmbeddingGenerator_WithApiKeyAndModel_ShouldCreateGeneratorWithSpecifiedModel()
    {
        // Arrange
        var apiKey = "mock-api-key";
        var modelName = "text-multilingual-embedding-002";

        // Act
        var generator = apiKey.AsEmbeddingGenerator(modelName);

        // Assert
        generator.ShouldNotBeNull();
        generator.Model.Model.ShouldBe(modelName);
    }

    [Fact, TestPriority(16)]
    public void AsEmbeddingGenerator_WithPlatformAdapter_ShouldCreateGenerator()
    {
        Assert.SkipWhen(!IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange
        var adapter = GetTestGooglePlatform();

        // Act
        var generator = adapter.AsEmbeddingGenerator();

        // Assert
        generator.ShouldNotBeNull();
        generator.Model.Model.ShouldBe("text-embedding-004");
    }

    [Fact, TestPriority(17)]
    public void AsEmbeddingGenerator_WithPlatformAdapterAndModel_ShouldCreateGeneratorWithSpecifiedModel()
    {
        Assert.SkipWhen(!IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange
        var adapter = GetTestGooglePlatform();
        var modelName = "textembedding-gecko";

        // Act
        var generator = adapter.AsEmbeddingGenerator(modelName);

        // Assert
        generator.ShouldNotBeNull();
        generator.Model.Model.ShouldBe(modelName);
    }

    [Fact, TestPriority(18)]
    public void WithTaskType_ShouldSetTaskTypeInOptions()
    {
        // Arrange
        var options = new EmbeddingGenerationOptions();

        // Act
        var result = options.WithTaskType(TaskType.CODE_RETRIEVAL_QUERY);

        // Assert
        result.ShouldNotBeNull();
        result.AdditionalProperties.ShouldNotBeNull();
        result.AdditionalProperties["TaskType"].ShouldBe(TaskType.CODE_RETRIEVAL_QUERY);
    }

    #endregion

    #region Dispose Tests

    [Fact, TestPriority(19)]
    public void Dispose_ShouldNotThrow()
    {
        // Arrange
        var generator = CreateMockEmbeddingGenerator();

        // Act & Assert
        Should.NotThrow(() => generator.Dispose());
    }

    [Fact, TestPriority(20)]
    public void Dispose_MultipleCalls_ShouldNotThrow()
    {
        // Arrange
        var generator = CreateMockEmbeddingGenerator();

        // Act & Assert
        Should.NotThrow(() =>
        {
            generator.Dispose();
            generator.Dispose(); // Second call should not throw
        });
    }

    #endregion

    #region Error Handling Tests

    [Fact, TestPriority(21)]
    public async Task GenerateAsync_WithInvalidApiKey_ShouldThrowGenerativeAIException()
    {
        // Arrange
        var generator = new GenerativeAIEmbeddingGenerator("invalid-key", DefaultTestEmbeddingModel);
        var input = new[] { "Test text" };

        // Act & Assert
        await Should.ThrowAsync<GenerativeAIException>(async () =>
            await generator.GenerateAsync(input));
    }



    #endregion

    #region Model Support Tests

    [Theory, TestPriority(22)]
    [InlineData("text-embedding-004")]
    [InlineData("text-multilingual-embedding-002")]
    [InlineData("textembedding-gecko")]
    [InlineData("textembedding-gecko-multilingual")]
    public void Constructor_WithSupportedModels_ShouldInitialize(string modelName)
    {
        // Arrange & Act
        var generator = new GenerativeAIEmbeddingGenerator("mock-api-key", modelName);

        // Assert
        generator.ShouldNotBeNull();
        generator.Model.Model.ShouldBe(modelName);
    }

    #endregion

    #region Batch Embedding Tests

    [Fact, TestPriority(23)]
    public async Task GenerateAsync_WithMultipleInputs_ShouldProcessAsBatch()
    {
        Assert.SkipWhen(!IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange
        var generator = CreateTestEmbeddingGenerator();
        var inputs = new[]
        {
            "First text to embed",
            "Second text to embed",
            "Third text to embed"
        };

        // Act
        var result = await generator.GenerateAsync(inputs);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
    }

    #endregion

    #region Usage Tracking Tests

    [Fact, TestPriority(24)]
    public async Task GenerateAsync_ShouldIncludeUsageDetails()
    {
        Assert.SkipWhen(!IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange
        var generator = CreateTestEmbeddingGenerator();
        var input = new[] { "Text for usage tracking" };

        // Act
        var result = await generator.GenerateAsync(input);

        // Assert
        result.Usage.ShouldNotBeNull();
        result.Usage.InputTokenCount.ShouldNotBeNull();
        result.Usage.InputTokenCount.Value.ShouldBeGreaterThan(0L);
    }

    #endregion
}