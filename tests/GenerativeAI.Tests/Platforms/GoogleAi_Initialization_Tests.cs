using GenerativeAI.Core;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;
using Moq;
// or your preferred mocking library
using Shouldly;

namespace GenerativeAI.Tests.Platforms
{
    public class GoogleAiTests
    {
        [Fact]
        public void Constructor_WithApiKeyOnly_ShouldInitializeDefaultPlatformAdapter()
        {
            // Arrange
            const string apiKey = "TestApiKey";
        
            // Act
            var googleAi = new GoogleAi(apiKey);
        
            // Assert
            googleAi.ShouldNotBeNull();
            googleAi.GetPlatformAdapter().ShouldNotBeNull();
        }
        
        [Fact]
        public void Constructor_WithApiKeyAndAccessToken_ShouldInitializePlatformAdapter()
        {
            // Arrange
            const string apiKey = "TestApiKey";
            const string accessToken = "TestAccessToken";
        
            // Act
            var googleAi = new GoogleAi(apiKey, accessToken);
        
            // Assert
            googleAi.ShouldNotBeNull();
            googleAi.GetPlatformAdapter().ShouldNotBeNull();
        }
        
        [Fact]
        public void Constructor_WithApiKeyAccessTokenAndHttpClient_ShouldInitializePlatformAdapter()
        {
            // Arrange
            const string apiKey = "TestApiKey";
            const string accessToken = "TestAccessToken";
            var client = new HttpClient();
        
            // Act
            var googleAi = new GoogleAi(apiKey, accessToken, client);
        
            // Assert
            googleAi.ShouldNotBeNull();
            googleAi.GetPlatformAdapter().ShouldNotBeNull();
        }
        
        [Fact]
        public void Constructor_WithAllParameters_ShouldInitializePlatformAdapter()
        {
            // Arrange
            const string apiKey = "TestApiKey";
            const string accessToken = "TestAccessToken";
            var client = new HttpClient();
            var logger = new Mock<ILogger>().Object;
        
            // Act
            var googleAi = new GoogleAi(apiKey, accessToken, client, logger);
        
            // Assert
            googleAi.ShouldNotBeNull();
            googleAi.GetPlatformAdapter().ShouldNotBeNull();
            var platform = googleAi.GetPlatformAdapter();
            var version = platform.GetApiVersion();
        }

        [Fact]
        public void Constructor_WithAdapter_ShouldUseProvidedAdapter()
        {
            // Arrange
            var mockAdapter = new Mock<IPlatformAdapter>();
            var mockClient = new HttpClient();
            var mockLogger = new Mock<ILogger>().Object;

            // Act
            var googleAi = new GoogleAi(mockAdapter.Object, mockClient, mockLogger);

            // Assert
            googleAi.ShouldNotBeNull();
            var platform = googleAi.GetPlatformAdapter();
            platform.ShouldBe(mockAdapter.Object);
            // mockClient and mockLogger verification can be uncommented if needed
            // googleAi.HttpClient.ShouldBe(mockClient);
            // googleAi.Logger.ShouldBe(mockLogger);
        }

        [Fact]
        public void CreateGenerativeModel_GivenParameters_ShouldReturnGeminiModelInstance()
        {
            // Arrange
            var mockAdapter = new Mock<IPlatformAdapter>();
            var googleAi = new GoogleAi(mockAdapter.Object);
            var modelName = "test-model";
            var config = new GenerationConfig { Temperature = 0.7f };
            var safetySettings = new SafetySetting[]
            {
                new SafetySetting
                {
                    Threshold = HarmBlockThreshold.BLOCK_MEDIUM_AND_ABOVE,
                    Category = HarmCategory.HARM_CATEGORY_HATE_SPEECH
                }
            };
            const string systemInstruction = "Test instruction";

            // Act
            var result = googleAi.CreateGenerativeModel(
                modelName, 
                config, 
                safetySettings, 
                systemInstruction);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBeOfType<GeminiModel>();

            var gemini = (GeminiModel)result;
            gemini.Model.ShouldBe(modelName);
            gemini.Config.ShouldBe(config);
            gemini.SafetySettings.ShouldBe(safetySettings);
            gemini.SystemInstruction.ShouldBe(systemInstruction);
        }

        [Fact]
        public void CreateGeminiModel_GivenParameters_ShouldReturnConfiguredGeminiModel()
        {
            // Arrange
            var mockAdapter = new Mock<IPlatformAdapter>();
            var googleAi = new GoogleAi(mockAdapter.Object);
            var modelName = "gemini-test-model";
            var config = new GenerationConfig { Temperature = 0.9f, MaxOutputTokens = 500 };
            var safetySettings = new SafetySetting[]
            {
                new SafetySetting()
                {
                    Category = HarmCategory.HARM_CATEGORY_SEXUAL,
                    Threshold = HarmBlockThreshold.BLOCK_LOW_AND_ABOVE
                }
            };
            const string systemInstruction = "System-level guidance";

            // Act
            var geminiModel = googleAi.CreateGeminiModel(
                modelName, 
                config, 
                safetySettings, 
                systemInstruction);

            // Assert
            geminiModel.ShouldNotBeNull();
            geminiModel.Model.ShouldBe(modelName);
            geminiModel.Config.ShouldBe(config);
            geminiModel.SafetySettings.ShouldBe(safetySettings);
            geminiModel.SystemInstruction.ShouldBe(systemInstruction);
        }
        
    }
}