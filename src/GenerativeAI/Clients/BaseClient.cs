using GenerativeAI.Core;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Clients;

public class BaseClient:ApiBase
{
    protected readonly IPlatformAdapter _platform;
    protected IPlatformAdapter Platform => _platform;
    public BaseClient(IPlatformAdapter platform, HttpClient? httpClient, ILogger? logger = null) : base(httpClient, logger)
    {
        _platform = platform;
    }

    protected override void AddAuthorizationHeader(HttpRequestMessage request)
    {
        _platform.AddAuthorization(request);
    }
}