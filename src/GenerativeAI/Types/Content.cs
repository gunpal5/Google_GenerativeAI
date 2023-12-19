using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using GenerativeAI.Tools;

namespace GenerativeAI.Types
{
    /// <summary>
    /// Content type for both prompts and response candidates.
    /// </summary>
    public class Content
    {
        public Content() { }
        public Content(Part[]? parts, string? role)
        {
            Parts = parts;
            Role = role;
        }

        public Part[]? Parts { get; set; }
        public string? Role { get; set; }
    }

    /// <summary>
    /// Content that can be provided as history input to startChat().
    /// </summary>
    public class InputContent
    {
        public string? Parts { get; set; }
        public string? Role { get; set; }
    }
    /// <summary>
    /// Content part - includes text or image part types.
    /// </summary>
    public class Part
    {
        public string? Text { get; set; }
        public GenerativeContentBlob? InlineData { get; set; }

        public ChatFunctionCall? FunctionCall { get; set; }
        public ChatFunctionResponse? FunctionResponse { get; set; }
    }

    public class ChatFunctionResponse
    {
        public string Name { get; set; }
        public FunctionResponse Response { get; set; }
    }

    public class FunctionResponse
    {
        public string Name { get; set; }
        public JsonNode Content { get; set; }
    }
    /// <summary>
    /// Interface for sending an image.
    /// </summary>
    public class GenerativeContentBlob
    {
        /// <summary>
        /// MimeType of Image
        /// </summary>
        public string? MimeType { get; set; }
        /// <summary>
        ///  Image as a base64 string.
        /// </summary>
        public string? Data { get; set; }
    }
}
