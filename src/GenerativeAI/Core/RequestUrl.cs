namespace GenerativeAI.Core;

/// <summary>
/// Represents a request URL for interacting with a generative AI service.
/// </summary>
public class RequestUrl
{
    /// <summary>
    /// Gets or sets the model identifier for the request.
    /// </summary>
    public string Model { get; set; }

    /// <summary>
    /// Gets or sets the task associated with the request.
    /// </summary>
    public string Task { get; set; }

    /// <summary>
    /// Gets or sets the API key used for authentication.
    /// </summary>
    public string ApiKey { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the request should use streaming.
    /// </summary>
    public bool Stream { get; set; }

    /// <summary>
    /// Gets or sets the base URL for the API endpoint.
    /// </summary>
    public string BaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the version of the API to be used.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestUrl"/> class with the specified parameters.
    /// </summary>
    /// <param name="model">The model identifier for the request.</param>
    /// <param name="task">The task associated with the request.</param>
    /// <param name="apiKey">The API key used for authentication.</param>
    /// <param name="stream">A value indicating whether the request should use streaming.</param>
    /// <param name="baseUrl">The base URL for the API endpoint.</param>
    /// <param name="version">The version of the API to be used. Defaults to "v1".</param>
    public RequestUrl(string model, string task, string apiKey, bool stream, string baseUrl, string version = "v1")
    {
        Model = model;
        Task = task;
        ApiKey = apiKey;
        Stream = stream;
        BaseUrl = baseUrl;
        Version = version;
    }

    /// <summary>
    /// Converts the request data to its string representation, with a placeholder for the API key.
    /// </summary>
    /// <returns>The string representation of the request URL.</returns>
    public override string ToString()
    {
        return ToString("__API_Key__");
    }

    /// <summary>
    /// Converts the request data to its string representation, using the specified API key.
    /// </summary>
    /// <param name="apiKey">The API key to include in the URL.</param>
    /// <returns>The string representation of the request URL.</returns>
    public string ToString(string apiKey)
    {
        var url = $"{BaseUrl}/{Version}/models/{this.Model}:{this.Task}?key={apiKey}";
        if (this.Stream)
        {
            url += "&alt=sse";
        }

        return url;
    }

    /// <summary>
    /// Defines an implicit conversion from a RequestUrl instance to its string representation.
    /// </summary>
    /// <param name="d">The RequestUrl instance.</param>
    public static implicit operator string(RequestUrl d) => d.ToString(d.ApiKey);
}