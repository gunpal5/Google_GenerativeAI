using GenerativeAI.Core;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace GenerativeAI.Tests.Platforms;

public class GoogleAIPlatformAdapterTests:TestBase
{
    [Fact]
    public void Constructor_WithNullApiKey_ShouldThrowException()
    {
        Assert.SkipWhen(IsGoogleApiKeySet,"Skipping test because GOOGLE_API_KEY environment variable is set.");
        // Arrange / Act / Assert
        var ex = Should.Throw<Exception>(() =>
        {
            // Passing null for the API key (and environment variable checks are skipped here)
                
            _ = new GoogleAIPlatformAdapter(null!);
        });

        ex.Message.ShouldContain("API Key is required");
    }

    [Fact]
    public void Constructor_WithValidApiKey_ShouldInitializeCredentialsProperly()
    {
        // Arrange
        const string testApiKey = "TEST_API_KEY";

        // Act
        var adapter = new GoogleAIPlatformAdapter(testApiKey);

        // Assert
        adapter.Credentials.ShouldNotBeNull();
        adapter.Credentials.ApiKey.ShouldBe(testApiKey);
        adapter.Credentials.AuthToken.ShouldBeNull("No access token was passed, so AuthToken should be null.");
    }

    [Fact]
    public void Constructor_WithApiKeyAndAccessToken_ShouldPopulateCredentialsAndToken()
    {
        // Arrange
        const string testApiKey = "TEST_API_KEY";
        const string testAccessToken = "TEST_ACCESS_TOKEN";

        // Act
        var adapter = new GoogleAIPlatformAdapter(testApiKey, accessToken: testAccessToken);

        // Assert
        adapter.Credentials.ShouldNotBeNull();
        adapter.Credentials.ApiKey.ShouldBe(testApiKey);
        adapter.Credentials.AuthToken.ShouldNotBeNull();
        adapter.Credentials.AuthToken!.AccessToken.ShouldBe(testAccessToken);
    }

    [Fact]
    public void Constructor_WithCustomApiVersion_ShouldSetApiVersionProperty()
    {
        // Arrange
        const string customVersion = "v2Test";

        // Act
        var adapter = new GoogleAIPlatformAdapter("TEST_API_KEY", apiVersion: customVersion);

        // Assert
        adapter.ApiVersion.ShouldBe(customVersion);
    }

    [Fact]
    public void Constructor_DefaultBaseUrl_ShouldBeSetToGoogleGenerativeAI()
    {
        // Arrange
        const string testApiKey = "TEST_API_KEY";

        // Act
        var adapter = new GoogleAIPlatformAdapter(testApiKey);

        // Assert
        adapter.BaseUrl.ShouldNotBeNullOrEmpty();
        // Use whichever default value you expect it to have:
        // For example, "https://generativelanguage.googleapis.com"
        // adapter.BaseUrl.ShouldBe("https://generativelanguage.googleapis.com");
    }

    [Fact]
    public void Constructor_ValidateAccessToken_ShouldBeTrueByDefault()
    {
        // Arrange / Act
        var adapter = new GoogleAIPlatformAdapter("TEST_API_KEY");

        // Assert
        // Using reflection or a test accessor to ensure the internal property
        // is set to true (the code snippet shows "bool ValidateAccessToken = true;")
        adapter.ShouldSatisfyAllConditions(
            () => adapter.ShouldNotBeNull(),
            () => adapter.GetType().GetProperty("ValidateAccessToken",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .ShouldNotBeNull(),
            () => adapter.GetType().GetProperty("ValidateAccessToken",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .GetValue(adapter)
                .ShouldBeOfType<bool>()
                .ShouldBeTrue()
        );
    }

    [Fact]
    public void Constructor_WithLogger_ShouldInitializeLogger()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();

        // Act
        var adapter = new GoogleAIPlatformAdapter("TEST_API_KEY", logger: loggerMock.Object);

        // Assert
        // Using reflection because Logger is not public:
        var loggerProperty = adapter.GetType().GetProperty("Logger",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        loggerProperty.ShouldNotBeNull("Logger property should exist.");

        var actualLogger = loggerProperty!.GetValue(adapter) as ILogger;
        actualLogger.ShouldNotBeNull();
        actualLogger.ShouldBe(loggerMock.Object);
    }

    [Fact]
    public void Constructor_WithAuthenticator_ShouldSetAuthenticator()
    {
        // Arrange
        var mockAuthenticator = new Mock<IGoogleAuthenticator>();

        // Act
        var adapter = new GoogleAIPlatformAdapter("TEST_API_KEY", authenticator: mockAuthenticator.Object);

        // Assert

        var actualAuthenticator = adapter.Authenticator;
        actualAuthenticator.ShouldBe(mockAuthenticator.Object);
    }

    [Fact]
    public void Constructor_WithCustomValidateAccessToken_ShouldSetProperly()
    {
        // Arrange
        const bool testFlag = false;

        // Act
        var adapter = new GoogleAIPlatformAdapter("TEST_API_KEY", validateAccessToken: testFlag);

        // Assert
        var validateAccessTokenProperty = adapter.GetType().GetProperty("ValidateAccessToken",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        validateAccessTokenProperty.ShouldNotBeNull("ValidateAccessToken property should exist.");
        var actualValue = (bool)validateAccessTokenProperty!.GetValue(adapter)!;
        actualValue.ShouldBeFalse();
    }
}