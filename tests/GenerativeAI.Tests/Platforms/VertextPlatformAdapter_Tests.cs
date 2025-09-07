using GenerativeAI.Core;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace GenerativeAI.Tests.Platforms;

public class VertextPlatformAdapter_Tests
{
    [Fact]
    public void GetBaseUrl_WithGlobalRegion_ShouldUseCorrectBaseUri()
    {
        // Arrange
        const string projectId = "test-project";
        const string globalRegion = "global";
        const string apiVersion = "v1beta1";
        var mockAuthenticator = new Mock<IGoogleAuthenticator>();
        
        var adapter = new VertextPlatformAdapter(
            projectId: projectId,
            region: globalRegion,
            expressMode: false,
            apiVersion: apiVersion,
            authenticator: mockAuthenticator.Object
        );

        // Act
        var baseUrl = adapter.GetBaseUrl(appendPublisher: false);

        // Assert
        baseUrl.ShouldBe($"https://aiplatform.googleapis.com/{apiVersion}/projects/{projectId}/locations/{globalRegion}");
    }

    [Fact]
    public void GetBaseUrl_WithGlobalRegionAndPublisher_ShouldUseCorrectBaseUri()
    {
        // Arrange
        const string projectId = "test-project";
        const string globalRegion = "global";
        const string apiVersion = "v1beta1";
        const string publisher = "google";
        var mockAuthenticator = new Mock<IGoogleAuthenticator>();
        
        var adapter = new VertextPlatformAdapter(
            projectId: projectId,
            region: globalRegion,
            expressMode: false,
            apiVersion: apiVersion,
            authenticator: mockAuthenticator.Object
        );

        // Act
        var baseUrl = adapter.GetBaseUrl(appendPublisher: true);

        // Assert
        baseUrl.ShouldBe($"https://aiplatform.googleapis.com/{apiVersion}/projects/{projectId}/locations/{globalRegion}/publishers/{publisher}");
    }

    [Fact]
    public void GetBaseUrl_WithRegionalRegion_ShouldUseRegionalBaseUri()
    {
        // Arrange
        const string projectId = "test-project";
        const string region = "us-central1";
        const string apiVersion = "v1beta1";
        var mockAuthenticator = new Mock<IGoogleAuthenticator>();
        
        var adapter = new VertextPlatformAdapter(
            projectId: projectId,
            region: region,
            expressMode: false,
            apiVersion: apiVersion,
            authenticator: mockAuthenticator.Object
        );

        // Act
        var baseUrl = adapter.GetBaseUrl(appendPublisher: false);

        // Assert
        baseUrl.ShouldBe($"https://{region}-aiplatform.googleapis.com/{apiVersion}/projects/{projectId}/locations/{region}");
    }

    [Fact]
    public void GetBaseUrl_WithRegionalRegionAndPublisher_ShouldUseRegionalBaseUri()
    {
        // Arrange
        const string projectId = "test-project";
        const string region = "us-east1";
        const string apiVersion = "v1beta1";
        const string publisher = "google";
        var mockAuthenticator = new Mock<IGoogleAuthenticator>();
        
        var adapter = new VertextPlatformAdapter(
            projectId: projectId,
            region: region,
            expressMode: false,
            apiVersion: apiVersion,
            authenticator: mockAuthenticator.Object
        );

        // Act
        var baseUrl = adapter.GetBaseUrl(appendPublisher: true);

        // Assert
        baseUrl.ShouldBe($"https://{region}-aiplatform.googleapis.com/{apiVersion}/projects/{projectId}/locations/{region}/publishers/{publisher}");
    }

    [Theory]
    [InlineData("GLOBAL")]
    [InlineData("Global")]
    [InlineData("global")]
    public void GetBaseUrl_WithGlobalRegionCaseInsensitive_ShouldUseCorrectBaseUri(string globalRegion)
    {
        // Arrange
        const string projectId = "test-project";
        const string apiVersion = "v1beta1";
        var mockAuthenticator = new Mock<IGoogleAuthenticator>();
        
        var adapter = new VertextPlatformAdapter(
            projectId: projectId,
            region: globalRegion,
            expressMode: false,
            apiVersion: apiVersion,
            authenticator: mockAuthenticator.Object
        );

        // Act
        var baseUrl = adapter.GetBaseUrl(appendPublisher: false);

        // Assert
        baseUrl.ShouldBe($"https://aiplatform.googleapis.com/{apiVersion}/projects/{projectId}/locations/{globalRegion}");
    }

    [Fact]
    public void GetBaseUrl_WithExpressModeEnabled_ShouldUseExpressBaseUri()
    {
        // Arrange
        const string projectId = "test-project";
        const string region = "global";
        const string apiVersion = "v1beta1";
        const string apiKey = "test-api-key";
        const string publisher = "google";
        
        var adapter = new VertextPlatformAdapter(
            projectId: projectId,
            region: region,
            expressMode: true,
            apiKey: apiKey,
            apiVersion: apiVersion
        );

        // Act
        var baseUrl = adapter.GetBaseUrl(appendPublisher: true);

        // Assert
        baseUrl.ShouldBe($"https://aiplatform.googleapis.com/{apiVersion}/publishers/{publisher}");
    }

    [Fact]
    public void VertexAIModels_ShouldContain_Gemini25FlashImagePreview()
    {
        // Act & Assert
        VertexAIModels.Gemini.Gemini25FlashImagePreview.ShouldBe("gemini-2.5-flash-image-preview");
    }
}