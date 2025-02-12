namespace GenerativeAI.Core;

public interface IPlatformAdapter
{
    void AddAuthorization(HttpRequestMessage request);
    void ValidateCredentials();
    Task AuthorizeAsync(CancellationToken cancellationToken = default);
    string GetBaseUrl(bool appendVesion = true);
    string CreateUrlForModel(string modelId,string task);
    string CreateUrlForTunedModel(string modelId, string task);
    string GetApiVersion();
}