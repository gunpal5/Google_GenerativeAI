using GenerativeAI.Constants;
using GenerativeAI.Core;
using GenerativeAI.Extensions;

namespace GenerativeAI;

public class GoogleAIPlatformAdapter : IPlatformAdapter
{
    public ApiKeyCredentials Credentials { get; }
    public string BaseUrl { get; set; } = BaseUrls.GoogleGenerativeAI;
    public string ApiVersion { get; set; } = ApiVersions.v1Beta;

    public GoogleAIPlatformAdapter(string googleApiKey,string apiVersion = ApiVersions.v1Beta)
    {
        Credentials = new ApiKeyCredentials(googleApiKey);
        this.ApiVersion = apiVersion;
    }

    public void AddAuthorization(HttpRequestMessage request)
    {
        if (request.RequestUri == null)
            throw new InvalidOperationException("Request URI cannot be null.");

        var uriBuilder = new UriBuilder(request.RequestUri);
        var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
        query["key"] = Credentials.ApiKey;
        uriBuilder.Query = query.ToString();
        request.RequestUri = uriBuilder.Uri;
    }

    public void ValidateCredentials()
    {
        Credentials.ValidateCredentials();
    }

    public Task AuthorizeAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public string GetBaseUrl(bool appendVesion = true)
    {
        if(appendVesion)
            return $"{BaseUrl}/{ApiVersion}";
        return BaseUrl;
    }

    public string CreateUrlForModel(string modelId, string task)
    {
        return $"{GetBaseUrl()}/{modelId.ToModelId()}:{task}";
    }

    public string CreateUrlForTunedModel(string modelId, string task)
    {
        return $"{GetBaseUrl()}/{modelId.ToTunedModelId()}:{task}";
    }
    public string GetApiVersion()
    {
        return ApiVersion;
    }
}