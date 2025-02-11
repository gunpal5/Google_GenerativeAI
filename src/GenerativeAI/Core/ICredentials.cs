using System.Net.Http;

namespace GenerativeAI.Core;

public interface ICredentials
{
    void ValidateCredentials();
}