using GenerativeAI.Core;
using Microsoft.Extensions.Logging;

namespace GenerativeAI;

public class VertexAI:GenAI,IGenerativeAI
{
    public VertexAI(IPlatformAdapter platformAdapter, HttpClient? client = null, ILogger? logger = null) : base(platformAdapter, client, logger)
    {
        
    }
    
    public VertexAI(string? projectId = null, 
        string? region = null, 
        string? accessToken = null,
        bool expressMode = false, 
        string? apiKey = null,
        string apiVersion= ApiVersions.v1Beta1, 
        HttpClient? httpClient =null, 
        ILogger? logger =null):this(new VertextPlatformAdapter(projectId,region,expressMode,apiKey,accessToken,apiVersion),httpClient,logger)
    {
        
    }
}