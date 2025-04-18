using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents a long-running operation, specifically for video generation tasks.
/// Poll this resource to check the status and retrieve results upon completion.
/// </summary>
public class GenerateVideosOperation : GoogleLongRunningOperation
{
    public GenerateVideosOperation()
    {
    }

    public GenerateVideosOperation(GoogleLongRunningOperation operation)
    {
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
                if (operation.Response.ContainsKey("generatedVideos"))
                    Result.GeneratedVideos = (operation.Response["generatedVideos"] as JsonElement?)
                        ?.Deserialize<List<Video>>();
                if (operation.Response.ContainsKey("raiMediaFilteredCount"))
                    Result.RaiMediaFilteredCount =
                        (operation.Response["raiMediaFilteredCount"] as JsonElement?)?.GetInt32();
                if (operation.Response.ContainsKey("raiMediaFilteredReasons"))
                    Result.RaiMediaFilteredReasons =
                        (operation.Response["raiMediaFilteredReasons"] as JsonElement?)?.Deserialize<List<string>>();
                
                if (operation.Response.ContainsKey("videos"))
                    Result.GeneratedVideos = (operation.Response["videos"] as JsonElement?)
                        ?.Deserialize<List<Video>>();
                
                if (operation.Response.ContainsKey("generated_videos"))
                    Result.GeneratedVideos = (operation.Response["generated_videos"] as JsonElement?)
                        ?.Deserialize<List<Video>>();
                if (operation.Response.ContainsKey("rai_media_filtered_count"))
                    Result.RaiMediaFilteredCount =
                        (operation.Response["rai_media_filtered_count"] as JsonElement?)?.GetInt32();
                if (operation.Response.ContainsKey("rai_media_filtered_reasons"))
                    Result.RaiMediaFilteredReasons =
                        (operation.Response["rai_media_filtered_reasons"] as JsonElement?)?.Deserialize<List<string>>();
            }
        }
    }

    /// <summary>
    /// Convenience property potentially containing the typed result of the video generation if the operation succeeded.
    /// This field might be populated by SDK logic by parsing the <see cref="Response"/> dictionary.
    /// </summary>
    [JsonPropertyName("result")]
    public GenerateVideosResponse? Result { get; set; }
}