namespace GenerativeAI.Core;

public interface IPlatformAdapter
{
    Task AddAuthorizationAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);
    Task ValidateCredentialsAsync(CancellationToken cancellationToken = default);
    Task AuthorizeAsync(CancellationToken cancellationToken = default);
    string GetBaseUrl(bool appendVesion = true);
    string GetBaseUrlForFile();
    string CreateUrlForModel(string modelId,string task);
    string CreateUrlForTunedModel(string modelId, string task);
    string GetApiVersion();
    object GetApiVersionForFile();
    
    
}