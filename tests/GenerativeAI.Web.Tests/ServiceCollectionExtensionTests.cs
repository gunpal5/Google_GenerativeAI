using GenerativeAI.Authenticators;
using GenerativeAI.Tests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;

namespace GenerativeAI.Web.Tests;

public class ServiceCollectionExtensionTests
{
    [Fact]
    public void ShouldConfigureServices()
    {
        var services = new ServiceCollection();
        
        services.AddGenerativeAI(new GenerativeAIOptions()
        {
            Credentials = new GoogleAICredentials("lakdhflksdahlkf")
        });
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IGenerativeAiService>();
        service.Platform.ShouldNotBeNull();
        var model = service.Platform.CreateGenerativeModel(GoogleAIModels.DefaultGeminiModel);
        model.Model.ShouldBe(GoogleAIModels.DefaultGeminiModel);
        var adapter = service.Platform.GetPlatformAdapter();
        adapter.ShouldBeOfType<GoogleAIPlatformAdapter>();
    }


    [Fact]
    public void AddGenerativeAI_ShouldThrowArgumentNullException_WhenServicesIsNull()
    {
        // Arrange & Act
        var exception = Should.Throw<ArgumentNullException>(() =>
        {
            IServiceCollection services = null;
            services.AddGenerativeAI();
        });

        // Assert
        exception.ParamName.ShouldBe("services");
    }

    [Fact]
    public void AddGenerativeAI_ShouldRegisterIGenerativeAiService_AndBindOptions()
    {
        // Arrange
        var services = new ServiceCollection();

         // Act
        services.AddGenerativeAI(new GenerativeAIOptions()
        {
            Credentials = new GoogleAICredentials("lakdhflksdahlkf")
        });
        var provider = services.BuildServiceProvider();

        // Assert
        var aiService = provider.GetService<IGenerativeAiService>();
        aiService.ShouldNotBeNull();

        var options = provider.GetService<IOptions<GenerativeAIOptions>>()?.Value;
        options.ShouldNotBeNull();
        options.ExpressMode.Value.ShouldBeFalse();
    }

    [Fact]
    public void AddGenerativeAI_WithConfiguration_ShouldThrowArgumentNullException_WhenConfigurationIsNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var exception = Should.Throw<ArgumentNullException>(() =>
            services.AddGenerativeAI((IConfiguration)null));

        // Assert
        exception.ParamName.ShouldBe("namedConfigurationSection");
    }

    [Fact]
    public void AddGenerativeAI_WithConfiguration_ShouldBindOptionsCorrectly()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string>("Credentials:ApiKey", "asdfasdfasdfsadf"),
            new KeyValuePair<string, string>("Region", "us-east1"),
            new KeyValuePair<string, string>("ProjectId", "MyProject")
        });
        var config = configBuilder.Build();
        var services = new ServiceCollection();

        // Act
        services.AddGenerativeAI(config);
        var provider = services.BuildServiceProvider();

        // Assert
        var aiService = provider.GetService<IGenerativeAiService>();
        aiService.ShouldNotBeNull();

        var options = provider.GetService<IOptions<GenerativeAIOptions>>()?.Value;
        options.ShouldNotBeNull();
        options.Region.ShouldBe("us-east1");
        options.ProjectId.ShouldBe("MyProject");
    }

    [Fact]
    public void AddGenerativeAI_WithOptions_ShouldThrowArgumentNullException_WhenOptionsAreNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var exception = Should.Throw<ArgumentNullException>(() =>
            services.AddGenerativeAI((GenerativeAIOptions)null));

        // Assert
        exception.ParamName.ShouldBe("options");
    }

    [Fact]
    public void AddGenerativeAI_WithOptions_ShouldApplyProvidedOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var customOptions = new GenerativeAIOptions
        {
            ProjectId = "ladkfhlkh",
            Region = "us-west2",
            IsVertex = true
        };

        // Act
        services.AddGenerativeAI(customOptions);
        var provider = services.BuildServiceProvider();

        // Assert
        var aiService = provider.GetService<IGenerativeAiService>();
        aiService.ShouldNotBeNull();

        var options = provider.GetService<IOptions<GenerativeAIOptions>>()?.Value;
        options.ShouldNotBeNull();
        options.Region.ShouldBe("us-west2");
        options.IsVertex.Value.ShouldBeTrue();
    }

    [Fact]
    public void AddGenerativeAI_WithConfigSectionPath_ShouldBindOptions()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string>("GenerativeAI:Credentials:ApiKey", "asdfasdfasdfsadf"),
            new KeyValuePair<string, string>("GenerativeAI:ApiVersion", "v2"),
            new KeyValuePair<string, string>("GenerativeAI:ExpressMode", "true")
        });
        var config = configBuilder.Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);

        // Act
        services.AddGenerativeAI("GenerativeAI");
        var provider = services.BuildServiceProvider();

        // Assert
        var aiService = provider.GetService<IGenerativeAiService>();
        aiService.ShouldNotBeNull();

        var options = provider.GetService<IOptions<GenerativeAIOptions>>()?.Value;
        options.ShouldNotBeNull();
        options.ApiVersion.ShouldBe("v2");
        options.ExpressMode.Value.ShouldBeTrue();
    }

    [Fact]
    public void WithGoogleAdcAuthentication_ShouldSetAuthenticatorToGoogleCloudAdc()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddGenerativeAI();

        // Act
        services.WithAdc();
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetService<IOptions<GenerativeAIOptions>>()?.Value;
        options.ShouldNotBeNull();
        options.Authenticator.ShouldBeOfType<GoogleCloudAdcAuthenticator>();
    }

   
    [Fact(Skip = "This test requires a service account to be configured in the environment.",Explicit = true)]
    public void WithGoogleServiceAuthentication_ShouldSetJsonFileAuthenticator()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddGenerativeAI();

        // Act
        services.WithServiceAccount(Environment.GetEnvironmentVariable("Google_Service_Account_Json",EnvironmentVariableTarget.User));
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetService<IOptions<GenerativeAIOptions>>()?.Value;
        options.ShouldNotBeNull();
        options.Authenticator.ShouldBeOfType<GoogleServiceAccountAuthenticator>();
    }

    [Fact(Skip = "This test requires a service account to be configured in the environment.",Explicit = true)]

    public void WithGoogleServiceAuthentication_ShouldSetServiceAccountAuthenticatorWithCertPath()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddGenerativeAI();

        // Act
        services.WithServiceAccount("service@account.com", Environment.GetEnvironmentVariable("Google_Service_Account_Key",EnvironmentVariableTarget.User), Environment.GetEnvironmentVariable("Google_key_password",EnvironmentVariableTarget.User));
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetService<IOptions<GenerativeAIOptions>>()?.Value;
        options.ShouldNotBeNull();
        options.Authenticator.ShouldBeOfType<GoogleServiceAccountAuthenticator>();
    }

    [RunnableInDebugOnlyAttribute]
    public void WithGoogleOAuthAuthentication_ShouldSetAuthenticatorToGoogleOAuth()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddGenerativeAI();

        // Act
        services.WithOAuth(Environment.GetEnvironmentVariable("Google_Client_Secret"));
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetService<IOptions<GenerativeAIOptions>>()?.Value;
        options.ShouldNotBeNull();
        options.Authenticator.ShouldBeOfType<GoogleOAuthAuthenticator>();
    }

    [Fact]
    public void ConfigureGenerativeAI_ShouldUpdateOptionsUsingPassedAction()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddGenerativeAI();

        // Act
        services.ConfigureGenerativeAI(o =>
        {
            o.ApiVersion = "v3";
            o.Model = "customModel";
        });
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetService<IOptions<GenerativeAIOptions>>()?.Value;
        options.ShouldNotBeNull();
        options.ApiVersion.ShouldBe("v3");
        options.Model.ShouldBe("customModel");
    }

    // [Fact]
    // public void AddGenerativeAI_WithSetupAction_ShouldThrowArgumentNullException_WhenNullActionProvided()
    // {
    //     // Arrange
    //     var services = new ServiceCollection();
    //
    //     // Act
    //     var exception = Should.Throw<ArgumentNullException>(() =>
    //         services.AddGenerativeAI(setupAction: null));
    //
    //     // Assert
    //     exception.ParamName.ShouldBe("setupAction");
    // }

    [Fact]
    public void AddGenerativeAI_WithSetupAction_ShouldApplyActionAndRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddGenerativeAI(options =>
        {
            options.ApiVersion = "v3";
            options.ProjectId = "TestProject";
            options.Credentials = new GoogleAICredentials("hlkadhflkdashfl");
        });
        var provider = services.BuildServiceProvider();

        // Assert
        var aiService = provider.GetService<IGenerativeAiService>();
        aiService.ShouldNotBeNull();

        var options = provider.GetService<IOptions<GenerativeAIOptions>>()?.Value;
        options.ShouldNotBeNull();
        options.ApiVersion.ShouldBe("v3");
        options.ProjectId.ShouldBe("TestProject");
    }
}