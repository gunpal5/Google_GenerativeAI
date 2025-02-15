using GenerativeAI.Core;

namespace GenerativeAI.Web;

public interface IGenerativeAIOptions
{
    public string? ProjectId { get; set; }
    public string? Region { get; set; }
    public IGoogleAuthenticator? Authenticator { get; set; }
    public GoogleAICredentials? Credentials { get; set; }
    public bool? IsVertex { get; set; }
    public string? Model { get; set; }
    public bool? ExpressMode { get; set; }
    public string? ApiVersion { get; set; }
}

public class GenerativeAIOptions:IGenerativeAIOptions
{
    public string? ProjectId { get; set; }
    public string? Region { get; set; }
    public IGoogleAuthenticator? Authenticator { get; set; }
    public GoogleAICredentials? Credentials { get; set; }
    public bool? IsVertex { get; set; }
    public string? Model { get; set; }
    public bool? ExpressMode { get; set; }
    public string? ApiVersion { get; set; }
}