using GenerativeAI.Authenticators;
using GenerativeAI.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GenerativeAI.Web;

public static class ServiceCollectionExtension
{
    /// <summary>
    /// Adds the Google Generative AI features to the <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the services to.</param>
    /// <returns>The updated <see cref="IServiceCollection" /> for chaining additional calls.</returns>
    public static IServiceCollection AddGenerativeAI(this IServiceCollection services)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        bool isVertex = !string.IsNullOrEmpty(EnvironmentVariables.GOOGLE_PROJECT_ID);
        services.AddOptions<GenerativeAIOptions>().Configure(s =>
        {
            s.Authenticator = s.Authenticator?? null;
            s.Credentials = s.Credentials?? new GoogleAICredentials(EnvironmentVariables.GOOGLE_API_KEY);
            s.IsVertex = s.IsVertex?? isVertex;
            s.Model = s.Model?? EnvironmentVariables.GOOGLE_AI_MODEL?? GoogleAIModels.DefaultGeminiModel;
            s.ProjectId = s.ProjectId?? EnvironmentVariables.GOOGLE_PROJECT_ID;
            s.Region = s.Region?? EnvironmentVariables.GOOGLE_REGION ?? "us-central1";
            s.ExpressMode =s.ExpressMode?? false;
            
        });
        services.AddTransient<IGenerativeAiService, GenerativeAIService>();

        return services;
    }

    /// <summary>
    /// Adds Google Generative AI features to the <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="namedConfigurationSection">The <see cref="IConfiguration"/> section with service options.</param>
    /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
    public static IServiceCollection AddGenerativeAI(this IServiceCollection services,
        IConfiguration namedConfigurationSection)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (namedConfigurationSection == null) throw new ArgumentNullException(nameof(namedConfigurationSection));

        services.Configure<GenerativeAIOptions>(namedConfigurationSection);
        services.AddGenerativeAI();

        return services;
    }

    /// <summary>
    /// Adds the Google Generative AI features to the <see cref="IServiceCollection" /> using the specified options.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the services to.</param>
    /// <param name="options">The configuration options for the Google Generative AI services.</param>
    /// <returns>The updated <see cref="IServiceCollection" /> for chaining additional calls.</returns>
    public static IServiceCollection AddGenerativeAI(this IServiceCollection services, GenerativeAIOptions options)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (options == null) throw new ArgumentNullException(nameof(options));

        services.AddOptions<GenerativeAIOptions>()
            .Configure(o =>
            {
                o.Credentials = options.Credentials;
                o.ProjectId = options.ProjectId;
                o.Region = options.Region;
                o.Model = options.Model;
                o.IsVertex = options.IsVertex;
                o.ExpressMode = options.ExpressMode;
                o.ApiVersion = options.ApiVersion;
                o.Authenticator = options.Authenticator;
            });
        services.AddGenerativeAI();

        return services;
    }

    /// <summary>
    /// Adds the Google Generative AI features to the <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the services to.</param>
    /// <param name="configSectionPath">The configuration section path containing Generative AI options.</param>
    /// <returns>The updated <see cref="IServiceCollection" /> for chaining additional calls.</returns>
    public static IServiceCollection AddGenerativeAI(this IServiceCollection services, string configSectionPath)
    {
        services.AddOptions<GenerativeAIOptions>()
            .BindConfiguration(configSectionPath)
            .ValidateOnStart();
        services.AddGenerativeAI();

        return services;
    }


    /// <summary>
    /// Configures the Google ADC (Application Default Credentials) authentication for Generative AI services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to configure the authentication for.</param>
    public static void WithAdc(this IServiceCollection services)
    {
        services.Configure<GenerativeAIOptions>(s =>
        {
            s.Authenticator = new GoogleCloudAdcAuthenticator();
        });
    }

    /// <summary>
    /// Configures Google Service Account authentication for Generative AI features in the <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the authentication configuration to.</param>
    /// <param name="jsonFilePath">The file path to the JSON credential file for Google Service Account authentication.</param>
    public static void WithServiceAccount(this IServiceCollection services, string jsonFilePath)
    {
        services.Configure<GenerativeAIOptions>(s =>
        {
            s.Authenticator = new GoogleServiceAccountAuthenticator(jsonFilePath);
        });
    }

    /// <summary>
    /// Configures Google Service Account authentication for the Generative AI services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to configure the authentication for.</param>
    /// <param name="serviceAccountEmail">The email address associated with the Google Service Account.</param>
    /// <param name="certificateFilePath">The file path to the service account's certificate.</param>
    /// <param name="passPhrase">The passphrase for the certificate file.</param>
    public static void WithServiceAccount(this IServiceCollection services, string serviceAccountEmail, string certificateFilePath, string passPhrase)
    {
        services.Configure<GenerativeAIOptions>(s =>
        {
            s.Authenticator =
                new GoogleServiceAccountAuthenticator(serviceAccountEmail, certificateFilePath, passPhrase);
        });
    }

    /// <summary>
    /// Configures the <see cref="IServiceCollection" /> to use Google OAuth authentication for Generative AI services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to configure.</param>
    /// <param name="clientSecretFilePath">The file path to the client secret JSON file used for Google OAuth authentication.</param>
    public static void WithOAuth(this IServiceCollection services, string clientSecretFilePath)
    {
        services.Configure<GenerativeAIOptions>(s =>
        {
            s.Authenticator = new GoogleOAuthAuthenticator(clientSecretFilePath);
        });
    }
    
    public static void ConfigureGenerativeAI(this IServiceCollection services, Action<GenerativeAIOptions> setupAction)
    {
        services.Configure(setupAction);
    }

    /// <summary>
    /// Adds the Google Generative AI features to the <see cref="IServiceCollection" /> with optional configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the services to.</param>
    /// <param name="setupAction">An optional action to configure the <see cref="GenerativeAIOptions" />.</param>
    /// <returns>The updated <see cref="IServiceCollection" /> for chaining additional calls.</returns>
    public static IServiceCollection AddGenerativeAI(this IServiceCollection services,
        Action<GenerativeAIOptions> setupAction)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (setupAction == null) throw new ArgumentNullException(nameof(setupAction));

        services.AddGenerativeAI();
        services.ConfigureGenerativeAI(setupAction);

        return services;
    }
}