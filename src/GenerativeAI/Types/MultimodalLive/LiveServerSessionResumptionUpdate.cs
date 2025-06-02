using System.Text.Json.Serialization;

namespace GenerativeAI.Types;
  /// <summary>
    /// Message sent by the server to inform the client about session resumption updates.
    /// This typically indicates that the server has processed some information related to session resumption.
    /// </summary>
    public class LiveServerSessionResumptionUpdate
    {
        /// <summary>
        /// A token that can be used by the client to resume the session.
        /// This token encapsulates the state of the session on the server side.
        /// Optional: This field might be present if the server successfully captured a resumption point.
        /// </summary>
        [JsonPropertyName("resumptionToken")]
        public string? ResumptionToken { get; set; }

        /// <summary>
        /// Optional. A message from the server regarding the session resumption status (e.g., success, error, pending).
        /// </summary>
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        /// <summary>
        /// Optional. Indicates the status of the session resumption process.
        /// </summary>
        [JsonPropertyName("status")]
        public SessionResumptionStatus? Status { get; set; }
    }