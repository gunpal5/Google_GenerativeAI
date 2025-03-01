using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Document AI Layout Parser config.
/// </summary>
public class RagFileParsingConfigLayoutParser
{
    /// <summary>
    /// The maximum number of requests the job is allowed to make to the Document AI processor per minute. Consult https://cloud.google.com/document-ai/quotas and the Quota page for your project to set an appropriate value here. If unspecified, a default value of 120 QPM would be used.
    /// </summary>
    [JsonPropertyName("maxParsingRequestsPerMin")]
    public int? MaxParsingRequestsPerMin { get; set; }

    /// <summary>
    /// The full resource name of a Document AI processor or processor version. The processor must have type `LAYOUT_PARSER_PROCESSOR`. If specified, the `additional_config.parse_as_scanned_pdf` field must be false. Format: * `projects/{project_id}/locations/{location}/processors/{processor_id}` * `projects/{project_id}/locations/{location}/processors/{processor_id}/processorVersions/{processor_version_id}`
    /// </summary>
    [JsonPropertyName("processorName")]
    public string? ProcessorName { get; set; }
}