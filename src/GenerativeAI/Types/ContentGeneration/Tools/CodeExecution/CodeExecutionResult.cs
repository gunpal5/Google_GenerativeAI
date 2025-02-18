using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Result of executing the <see cref="ExecutableCode"/>.
/// Only generated when using the <see cref="CodeExecutionTool"/>, and always follows a <see cref="Part"/>
/// containing the <see cref="ExecutableCode"/>.
/// </summary>
/// <seealso href="https://ai.google.dev/api/caching#CodeExecutionResult">See Official API Documentation</seealso>
public class CodeExecutionResult
{
    /// <summary>
    /// Required. Outcome of the code execution.
    /// </summary>
    [JsonPropertyName("outcome")]
    public Outcome Outcome { get; set; }

    /// <summary>
    /// Optional. Contains stdout when code execution is successful, stderr or other
    /// description otherwise.
    /// </summary>
    [JsonPropertyName("output")]
    public string? Output { get; set; }
}