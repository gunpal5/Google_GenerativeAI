using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents a long-running operation, specifically for video generation tasks.
/// Poll this resource to check the status and retrieve results upon completion.
/// </summary>
public class GenerateVideosOperation : GoogleLongRunningOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GenerateVideosOperation"/> class.
    /// </summary>
    public GenerateVideosOperation()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GenerateVideosOperation"/> class from a Google long-running operation.
    /// </summary>
    /// <param name="operation">The base Google long-running operation to convert.</param>
    public GenerateVideosOperation(GoogleLongRunningOperation operation)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(operation);
#else
        if (operation == null) throw new ArgumentNullException(nameof(operation));
#endif
        this.Name = operation.Name;
        this.Metadata = operation.Metadata;
        this.Response = operation.Response;
        this.Done = operation.Done;
        this.Error = operation.Error;

        if (this.Done == true)
        {
            this.Result = new GenerateVideosResponse();

            if (operation.Response != null)
            {
                if (operation.Response.TryGetValue("generatedVideos", out var value))
                    Result.GeneratedVideos = (value as JsonElement?)
                        ?.Deserialize<List<Video>>();
                if (operation.Response.TryGetValue("raiMediaFilteredCount", out var value1))
                    Result.RaiMediaFilteredCount =
                        (value1 as JsonElement?)?.GetInt32();
                if (operation.Response.TryGetValue("raiMediaFilteredReasons", out var value2))
                    Result.RaiMediaFilteredReasons =
                        (value2 as JsonElement?)?.Deserialize<List<string>>();
                
                if (operation.Response.TryGetValue("videos", out var value3))
                    Result.GeneratedVideos = (value3 as JsonElement?)
                        ?.Deserialize<List<Video>>();
                
                if (operation.Response.TryGetValue("generated_videos", out var value4))
                    Result.GeneratedVideos = (value4 as JsonElement?)
                        ?.Deserialize<List<Video>>();
                if (operation.Response.TryGetValue("rai_media_filtered_count", out var value5))
                    Result.RaiMediaFilteredCount =
                        (value5 as JsonElement?)?.GetInt32();
                if (operation.Response.TryGetValue("rai_media_filtered_reasons", out var value6))
                    Result.RaiMediaFilteredReasons =
                        (value6 as JsonElement?)?.Deserialize<List<string>>();
            }
        }
    }

    /// <summary>
    /// Convenience property potentially containing the typed result of the video generation if the operation succeeded.
    /// This field might be populated by SDK logic by parsing the Response dictionary.
    /// </summary>
    [JsonPropertyName("result")]
    public GenerateVideosResponse? Result { get; set; }
}