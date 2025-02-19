using GenerativeAI.Core;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Clients;

/// <summary>
/// Represents the base implementation for API clients that interact with an underlying platform.
/// This class provides shared functionality such as HTTP communication and authorization handling
/// to its derived client classes.
/// </summary>
public class BaseClient : ApiBase
{
    /// <summary>
    /// 
    /// </summary>
    protected readonly IPlatformAdapter _platform;

    /// <summary>
    /// Gets the platform adapter associated with the client.
    /// </summary>
    /// <remarks>
    /// Provides access to the platform-specific functionality including authorization handling,
    /// URL generation, API versioning, and other platform-dependent operations.
    /// The platform is initialized when the client is instantiated and can be used by derived classes
    /// to interact with underlying platform APIs.
    /// </remarks>
    public IPlatformAdapter Platform => _platform;

    /// <summary>
    /// Represents the base client class that provides foundational functionality for API clients.
    /// </summary>
    /// <remarks>
    /// This class serves as an abstraction layer for managing HTTP requests and interactions with
    /// platform-specific implementations. It encapsulates functionality for handling authorization,
    /// logging, and platform-specific operations through the use of dependency injection for
    /// <see cref="IPlatformAdapter"/>.
    /// </remarks>
    public BaseClient(IPlatformAdapter platform, HttpClient? httpClient, ILogger? logger = null) : base(httpClient,
        logger)
    {
        _platform = platform;
    }

   
    /// <inheritdoc/>
    protected override async Task AddAuthorizationHeader(HttpRequestMessage request, bool requiredAccessToken = false,
        CancellationToken cancellationToken = default)
    {
        await _platform.AddAuthorizationAsync(request, requiredAccessToken, cancellationToken).ConfigureAwait(false);
    }
}