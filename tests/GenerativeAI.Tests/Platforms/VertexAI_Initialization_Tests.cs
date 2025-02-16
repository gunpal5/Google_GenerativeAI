using GenerativeAI.Core;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace GenerativeAI.Tests.Platforms
{
    public class VertexAi_Initialization_Tests
    {
        [Fact]
        public void Constructor_WithPlatformAdapter_ShouldInitializeBaseClass()
        {
            // Arrange
            var mockAdapter = new Mock<IPlatformAdapter>();
            var httpClient = new HttpClient();
            var mockLogger = new Mock<ILogger>().Object;

            // Act
            var vertexAi = new VertexAI(mockAdapter.Object, httpClient, mockLogger);

            // Assert
            vertexAi.ShouldNotBeNull();
            var platform = vertexAi.GetPlatformAdapter();
            platform.ShouldNotBeNull();
            platform.ShouldBe(mockAdapter.Object);
            // vertexAi.HttpClient.ShouldBe(httpClient);
            // vertexAi.Logger.ShouldBe(mockLogger);
        }

        [Fact]
        public void Constructor_WithAllParameters_ShouldInitializeVertextPlatformAdapter()
        {
            // Arrange
            const string testProjectId = "TestProject";
            const string testRegion = "us-central1";
            const string testAccessToken = "TestAccessToken";
            const bool expressMode = true;
            const string testApiKey = "TestApiKey";
            const string testApiVersion = "v1Custom";
            var mockHttpClient = new HttpClient();
            var mockAuthenticator = new Mock<IGoogleAuthenticator>().Object;
            var mockLogger = new Mock<ILogger>().Object;

            // Act
            var vertexAi = new VertexAI(
                projectId: testProjectId,
                region: testRegion,
                accessToken: testAccessToken,
                expressMode: expressMode,
                apiKey: testApiKey,
                apiVersion: testApiVersion,
                httpClient: mockHttpClient,
                authenticator: mockAuthenticator,
                logger: mockLogger
            );
            
            // Assert
            vertexAi.ShouldNotBeNull();
            var platform = vertexAi.GetPlatformAdapter();
            platform.ShouldNotBeNull();

            // The VertexAI constructor wraps the input params in a VertextPlatformAdapter.
            // We can cast to check if the proper data is set.
            var adapter = platform as VertextPlatformAdapter;
            adapter.ShouldNotBeNull("Platform should be a VertextPlatformAdapter instance.");

            adapter!.ProjectId.ShouldBe(testProjectId);
            adapter.Region.ShouldBe(testRegion);
            adapter.Credentials.AuthToken.AccessToken.ShouldBe(testAccessToken);
            adapter.ExpressMode.ShouldBe(expressMode);
            adapter.Credentials.ApiKey.ShouldBe(testApiKey);
            adapter.ApiVersion.ShouldBe(testApiVersion);
            
            // adapter.Authenticator.ShouldBeSameAs(mockAuthenticator);
            //
            // vertexAi.HttpClient.ShouldBe(mockHttpClient);
            // vertexAi.Logger.ShouldBe(mockLogger);
        }
    }
}