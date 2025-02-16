namespace GenerativeAI.Core;

public class RequestUrl
{
    public string Model { get; set; }
    public string Task { get; set; }
    public string ApiKey { get; set; }
    public bool Stream { get; set; }

    public string BaseUrl { get; set; }
    public string Version { get; set; }
    public RequestUrl(string model, string task, string apiKey, bool stream, string baseUrl, string version = "v1")
    {
        Model = model;
        Task = task;
        ApiKey = apiKey;
        Stream = stream;
        BaseUrl = baseUrl;
        Version = version;
    }

    public override string ToString()
    {
        return ToString("__API_Key__");
    }

    public string ToString(string apiKey)
    {
        var url = $"{BaseUrl}/{Version}/models/{this.Model}:{this.Task}?key={apiKey}";
        if (this.Stream)
        {
            url += "&alt=sse";
        }

        return url;
    }

    public static implicit operator string(RequestUrl d) => d.ToString(d.ApiKey);
}