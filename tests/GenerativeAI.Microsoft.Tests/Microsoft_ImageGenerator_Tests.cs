#pragma warning disable MEAI001
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GenerativeAI.Core;
using GenerativeAI.Microsoft;
using GenerativeAI.Microsoft.Extensions;
using GenerativeAI.Tests;
using GenerativeAI.Tests.Base;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace GenerativeAI.Tests.Microsoft;

#pragma warning disable MEAI001

/// <summary>
/// Tests for <see cref="GenerativeAIImageGenerator"/>.
/// Demonstrates a style similar to other Generative AI tests,
/// using xUnit and Shouldly for assertions.
/// </summary>
[TestCaseOrderer(
    typeof(TestPriorityAttribute))]
public class Microsoft_ImageGenerator_Tests : TestBase
{
    private const string DefaultTestModelName = GoogleAIModels.Gemini25FlashImagePreview;

    public Microsoft_ImageGenerator_Tests(ITestOutputHelper helper) : base(helper)
    {
    }

    #region Helper Methods

    /// <summary>
    /// Creates a minimal mock or fake platform adapter for testing.
    /// This can be replaced by a more feature-complete mock if needed.
    /// </summary>
    private IPlatformAdapter CreateTestPlatformAdapter()
    {
        return new GoogleAIPlatformAdapter("test_api_key");
    }

    #endregion

    #region Constructor Tests

    [Fact, TestPriority(1)]
    public void ShouldCreateWithBasicConstructor()
    {
        // Arrange
        var adapter = CreateTestPlatformAdapter();

        // Act
        var generator = new GenerativeAIImageGenerator(adapter);

        // Assert
        generator.ShouldNotBeNull();
        generator.model.ShouldNotBeNull();
        generator.model.Model.ShouldBe(DefaultTestModelName);
        Console.WriteLine("GenerativeAIImageGenerator created successfully with the basic constructor.");
    }

    [Fact, TestPriority(2)]
    public void ShouldCreateWithCustomModelName()
    {
        // Arrange
        var adapter = CreateTestPlatformAdapter();
        var customModel = "my-custom-model";

        // Act
        var generator = new GenerativeAIImageGenerator(adapter, customModel);

        // Assert
        generator.ShouldNotBeNull();
        generator.model.ShouldNotBeNull();
        generator.model.Model.ShouldBe(customModel);
        Console.WriteLine($"GenerativeAIImageGenerator created with custom model: {customModel}");
    }

    [Fact, TestPriority(3)]
    public void ShouldCreateWithApiKeyConstructor()
    {
        // Arrange
        const string testApiKey = "test_api_key";

        // Act
        var generator = new GenerativeAIImageGenerator(testApiKey);

        // Assert
        generator.ShouldNotBeNull();
        generator.model.ShouldNotBeNull();
        generator.model.Model.ShouldBe(DefaultTestModelName);
        Console.WriteLine("GenerativeAIImageGenerator created successfully with API key constructor.");
    }

    [Fact, TestPriority(4)]
    public void ShouldCreateWithApiKeyAndCustomModel()
    {
        // Arrange
        const string testApiKey = "test_api_key";
        const string customModel = "custom-gemini-model";

        // Act
        var generator = new GenerativeAIImageGenerator(testApiKey, customModel);

        // Assert
        generator.ShouldNotBeNull();
        generator.model.ShouldNotBeNull();
        generator.model.Model.ShouldBe(customModel);
        Console.WriteLine($"GenerativeAIImageGenerator created with API key and custom model: {customModel}");
    }

    #endregion

    #region GenerateAsync Tests

    [Fact, TestPriority(5)]
    public async Task ShouldThrowArgumentNullExceptionWhenRequestIsNull()
    {
        // Arrange
        var adapter = CreateTestPlatformAdapter();
        var generator = new GenerativeAIImageGenerator(adapter);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
        {
            await generator.GenerateAsync(null!, cancellationToken: TestContext.Current.CancellationToken);
        });
        Console.WriteLine("GenerateAsync threw ArgumentNullException as expected when request was null.");
    }

    [Fact, TestPriority(6)]
    public async Task ShouldReturnImageGenerationResponseOnValidInput()
    {
        Assert.SkipWhen(!IsGoogleApiKeySet, GoogleTestSkipMessage);
        
        // Arrange
        var adapter = GetTestGooglePlatform();
        var generator = new GenerativeAIImageGenerator(adapter);

        var request = new ImageGenerationRequest("Generate an image of a beautiful sunset over mountains");
        var options = new ImageGenerationOptions
        {
            Count = 1,
            MediaType = "image/png"
        };

        // Act
        var result = await generator.GenerateAsync(request, options, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        Console.WriteLine("GenerateAsync returned a valid result.");
        
        // Check if the result has content - either check the raw response or the constructed response
        if (result.RawRepresentation != null)
        {
            Console.WriteLine("Raw representation is available.");
        }
    }

    #endregion

    #region GetService Tests

    [Fact, TestPriority(7)]
    public void ShouldReturnSelfFromGetServiceIfTypeMatches()
    {
        // Arrange
        var adapter = CreateTestPlatformAdapter();
        var generator = new GenerativeAIImageGenerator(adapter);

        // Act
        var service = generator.GetService(typeof(GenerativeAIImageGenerator));

        // Assert
        service.ShouldNotBeNull();
        service.ShouldBeOfType<GenerativeAIImageGenerator>();
        service.ShouldBe(generator);
        Console.WriteLine("GetService returned the correct instance when serviceType matches the generator type.");
    }

    [Fact, TestPriority(8)]
    public void ShouldReturnSelfFromGetServiceForIImageGenerator()
    {
        // Arrange
        var adapter = CreateTestPlatformAdapter();
        var generator = new GenerativeAIImageGenerator(adapter);

        // Act
        var service = generator.GetService(typeof(IImageGenerator));

        // Assert
        service.ShouldNotBeNull();
        service.ShouldBeOfType<GenerativeAIImageGenerator>();
        service.ShouldBe(generator);
        Console.WriteLine("GetService returned the correct instance when serviceType is IImageGenerator.");
    }

    [Fact, TestPriority(9)]
    public void ShouldReturnNullFromGetServiceIfTypeDoesNotMatch()
    {
        // Arrange
        var adapter = CreateTestPlatformAdapter();
        var generator = new GenerativeAIImageGenerator(adapter);

        // Act
        var service = generator.GetService(typeof(string));

        // Assert
        service.ShouldBeNull();
        Console.WriteLine("GetService returned null when the requested serviceType did not match.");
    }

    [Fact, TestPriority(10)]
    public void ShouldReturnNullFromGetServiceWithServiceKey()
    {
        // Arrange
        var adapter = CreateTestPlatformAdapter();
        var generator = new GenerativeAIImageGenerator(adapter);

        // Act
        var service = generator.GetService(typeof(GenerativeAIImageGenerator), "some_key");

        // Assert
        service.ShouldBeNull();
        Console.WriteLine("GetService returned null when serviceKey was provided.");
    }

    #endregion

    #region Dispose Tests

    [Fact, TestPriority(11)]
    public void ShouldDisposeWithoutException()
    {
        // Arrange
        var adapter = CreateTestPlatformAdapter();
        var generator = new GenerativeAIImageGenerator(adapter);

        // Act & Assert
        Should.NotThrow(() => generator.Dispose());
        Console.WriteLine("Dispose completed without throwing an exception.");
    }

    #endregion

    protected override IPlatformAdapter GetTestGooglePlatform()
    {
        Assert.SkipWhen(!IsGoogleApiKeySet, GoogleTestSkipMessage);
        return new GoogleAIPlatformAdapter(EnvironmentVariables.GOOGLE_API_KEY);
    }
}
#pragma warning restore MEAI001
