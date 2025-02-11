using System.Net.Http;
using GenerativeAI.Core;

namespace GenerativeAI.Platforms.Platforms;

public class ApiKeyCredentials : ICredentials
{
    public string ApiKey { get; }

    public ApiKeyCredentials(string apiKey)
    {
        this.ApiKey = apiKey;
    }

    public void ValidateCredentials()
    {
        if(string.IsNullOrEmpty(ApiKey))
            throw new ArgumentNullException(nameof(ApiKey));
    }
}