#pragma warning disable MEAI001
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GenerativeAI;
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
/// Tests for <see cref="GenerativeAIImagenGenerator"/>.
/// Demonstrates a style similar to other Generative AI tests,
/// using xUnit and Shouldly for assertions.
/// </summary>
[TestCaseOrderer(
    typeof(TestPriorityAttribute))]
public class Microsoft_ImagenGenerator_Tests : TestBase
{
    private const string DefaultTestModelName = GoogleAIModels.Imagen.Imagen3FastGenerate001;

    public Microsoft_ImagenGenerator_Tests(ITestOutputHelper helper) : base(helper)
    {
    }

    #region Helper Methods

    /// <summary>
    /// Creates a minimal mock GoogleAi instance for testing.
    /// </summary>
    private GoogleAi CreateTestGoogleAi()
    {
        return new GoogleAi("test_api_key");
    }

    #endregion

    #region Constructor Tests

    [Fact, TestPriority(1)]
    public void ShouldCreateWithApiKeyConstructor()
    {
        // Arrange
        const string testApiKey = "test_api_key";

        // Act
        var generator = new GenerativeAIImagenGenerator(testApiKey);

        // Assert
        generator.ShouldNotBeNull();
        generator.model.ShouldNotBeNull();
        Console.WriteLine("GenerativeAIImagenGenerator created successfully with API key constructor.");
    }

    [Fact, TestPriority(2)]
    public void ShouldCreateWithApiKeyAndCustomModel()
    {
        // Arrange
        const string testApiKey = "test_api_key";
        const string customModel = "custom-imagen-model";

        // Act
        var generator = new GenerativeAIImagenGenerator(testApiKey, customModel);

        // Assert
        generator.ShouldNotBeNull();
        generator.model.ShouldNotBeNull();
        Console.WriteLine($"GenerativeAIImagenGenerator created with API key and custom model: {customModel}");
    }

    [Fact, TestPriority(3)]
    public void ShouldCreateWithGenAIConstructor()
    {
        // Arrange
        var genAi = CreateTestGoogleAi();

        // Act
        var generator = new GenerativeAIImagenGenerator(genAi);

        // Assert
        generator.ShouldNotBeNull();
        generator.model.ShouldNotBeNull();
        Console.WriteLine("GenerativeAIImagenGenerator created successfully with GenAI constructor.");
    }

    [Fact, TestPriority(4)]
    public void ShouldCreateWithGenAIAndCustomModel()
    {
        // Arrange
        var genAi = CreateTestGoogleAi();
        const string customModel = "custom-imagen-model";

        // Act
        var generator = new GenerativeAIImagenGenerator(genAi, customModel);

        // Assert
        generator.ShouldNotBeNull();
        generator.model.ShouldNotBeNull();
        Console.WriteLine($"GenerativeAIImagenGenerator created with GenAI and custom model: {customModel}");
    }

    [Fact, TestPriority(5)]
    public void ShouldThrowArgumentNullExceptionWhenGenAIIsNull()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() =>
        {
            var generator = new GenerativeAIImagenGenerator((GenAI)null!);
        });
        Console.WriteLine("Constructor threw ArgumentNullException as expected when GenAI was null.");
    }

    #endregion

    #region GenerateAsync Tests

    [Fact, TestPriority(6)]
    public async Task ShouldThrowArgumentNullExceptionWhenRequestIsNull()
    {
        // Arrange
        const string testApiKey = "test_api_key";
        var generator = new GenerativeAIImagenGenerator(testApiKey);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
        {
            await generator.GenerateAsync(null!, cancellationToken: TestContext.Current.CancellationToken);
        });
        Console.WriteLine("GenerateAsync threw ArgumentNullException as expected when request was null.");
    }

    [Fact, TestPriority(7)]
    public async Task ShouldReturnImageGenerationResponseOnValidInput()
    {
        Assert.SkipWhen(!IsGoogleApiKeySet, GoogleTestSkipMessage);
        
        // Arrange
        var genAi = new GoogleAi(EnvironmentVariables.GOOGLE_API_KEY);
        var generator = new GenerativeAIImagenGenerator(genAi);

        var request = new ImageGenerationRequest("A beautiful landscape with mountains and a lake");
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

    [Fact, TestPriority(8)]
    public async Task ShouldHandleImageGenerationWithoutOptions()
    {
        Assert.SkipWhen(!IsGoogleApiKeySet, GoogleTestSkipMessage);
        
        // Arrange
        var genAi = new GoogleAi(EnvironmentVariables.GOOGLE_API_KEY);
        var generator = new GenerativeAIImagenGenerator(genAi);

        var request = new ImageGenerationRequest("A simple drawing of a cat");

        // Act
        var result = await generator.GenerateAsync(request, null, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        Console.WriteLine("GenerateAsync without options returned a valid result.");
        
        // Check if the result has content - either check the raw response or the constructed response
        if (result.RawRepresentation != null)
        {
            Console.WriteLine("Raw representation is available.");
        }
    }

    #endregion

    #region GetService Tests

    [Fact, TestPriority(9)]
    public void ShouldReturnSelfFromGetServiceIfTypeMatches()
    {
        // Arrange
        const string testApiKey = "test_api_key";
        var generator = new GenerativeAIImagenGenerator(testApiKey);

        // Act
        var service = generator.GetService(typeof(GenerativeAIImagenGenerator));

        // Assert
        service.ShouldNotBeNull();
        service.ShouldBeOfType<GenerativeAIImagenGenerator>();
        service.ShouldBe(generator);
        Console.WriteLine("GetService returned the correct instance when serviceType matches the generator type.");
    }

    [Fact, TestPriority(10)]
    public void ShouldReturnSelfFromGetServiceForIImageGenerator()
    {
        // Arrange
        const string testApiKey = "test_api_key";
        var generator = new GenerativeAIImagenGenerator(testApiKey);

        // Act
        var service = generator.GetService(typeof(IImageGenerator));

        // Assert
        service.ShouldNotBeNull();
        service.ShouldBeOfType<GenerativeAIImagenGenerator>();
        service.ShouldBe(generator);
        Console.WriteLine("GetService returned the correct instance when serviceType is IImageGenerator.");
    }

    [Fact, TestPriority(11)]
    public void ShouldReturnNullFromGetServiceIfTypeDoesNotMatch()
    {
        // Arrange
        const string testApiKey = "test_api_key";
        var generator = new GenerativeAIImagenGenerator(testApiKey);

        // Act
        var service = generator.GetService(typeof(string));

        // Assert
        service.ShouldBeNull();
        Console.WriteLine("GetService returned null when the requested serviceType did not match.");
    }

    [Fact, TestPriority(12)]
    public void ShouldReturnNullFromGetServiceWithServiceKey()
    {
        // Arrange
        const string testApiKey = "test_api_key";
        var generator = new GenerativeAIImagenGenerator(testApiKey);

        // Act
        var service = generator.GetService(typeof(GenerativeAIImagenGenerator), "some_key");

        // Assert
        service.ShouldBeNull();
        Console.WriteLine("GetService returned null when serviceKey was provided.");
    }

    #endregion

    #region Dispose Tests

    [Fact, TestPriority(13)]
    public void ShouldDisposeWithoutException()
    {
        // Arrange
        const string testApiKey = "test_api_key";
        var generator = new GenerativeAIImagenGenerator(testApiKey);

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
