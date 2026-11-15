using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// A sandbox environment (<c>/environments</c>) used by agent interactions, with its mounted sources and
/// workspace files. Named <c>AgentEnvironment</c> to avoid clashing with <see cref="System.Environment"/>.
/// </summary>
public sealed class AgentEnvironment
{
    /// <summary>The environment ID.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>The environment status (see <see cref="EnvironmentStatuses"/>).</summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>
    /// Network configuration: the string <c>"disabled"</c> or an egress allowlist object
    /// <c>{"allowlist": [...]}</c>.
    /// </summary>
    [JsonPropertyName("network")]
    public JsonNode? Network { get; set; }

    /// <summary>The sources mounted into the environment.</summary>
    [JsonPropertyName("sources")]
    public List<InteractionEnvironmentSource>? Sources { get; set; }

    /// <summary>The number of files in the workspace.</summary>
    [JsonPropertyName("file_count")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public long? FileCount { get; set; }

    /// <summary>The total size of the workspace in bytes.</summary>
    [JsonPropertyName("size_bytes")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public long? SizeBytes { get; set; }

    /// <summary>Creation time (ISO 8601).</summary>
    [JsonPropertyName("created")]
    public string? Created { get; set; }

    /// <summary>Last update time (ISO 8601).</summary>
    [JsonPropertyName("updated")]
    public string? Updated { get; set; }

    /// <summary>Last access time (ISO 8601).</summary>
    [JsonPropertyName("last_accessed")]
    public string? LastAccessed { get; set; }

    /// <summary>Fields this SDK does not model yet.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>Request body of <c>POST /environments</c>.</summary>
public sealed class CreateEnvironmentRequest
{
    /// <summary>
    /// Clone an existing environment (<c>environments/{id}</c> or <c>{id}</c>). When set, <see cref="Sources"/> must
    /// be empty.
    /// </summary>
    [JsonPropertyName("from_environment")]
    public string? FromEnvironment { get; set; }

    /// <summary>
    /// Network configuration: <c>"disabled"</c> (see <see cref="EnvironmentNetworkModes"/>) or an egress allowlist
    /// object <c>{"allowlist": [{"domain": "...", "credential": "..."}]}</c>.
    /// </summary>
    [JsonPropertyName("network")]
    public JsonNode? Network { get; set; }

    /// <summary>Sources to mount (GCS buckets, inline files, repositories, skill registries).</summary>
    [JsonPropertyName("sources")]
    public List<InteractionEnvironmentSource>? Sources { get; set; }

    /// <summary>Additional body fields to send (for API features this SDK does not model yet).</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>Response of <c>GET /environments</c>.</summary>
public sealed class ListEnvironmentsResponse
{
    /// <summary>The environments.</summary>
    [JsonPropertyName("environments")]
    public List<AgentEnvironment>? Environments { get; set; }

    /// <summary>The token of the next page, if any.</summary>
    [JsonPropertyName("next_page_token")]
    public string? NextPageToken { get; set; }
}

/// <summary>A file or directory in an environment workspace.</summary>
public sealed class EnvironmentFile
{
    /// <summary>The file name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>The path relative to the workspace root.</summary>
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    /// <summary>The entry type (see <see cref="EnvironmentFileTypes"/>).</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>The MIME type of the file.</summary>
    [JsonPropertyName("mime_type")]
    public string? MimeType { get; set; }

    /// <summary>The size in bytes.</summary>
    [JsonPropertyName("size_bytes")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public long? SizeBytes { get; set; }

    /// <summary>Creation time (RFC 3339).</summary>
    [JsonPropertyName("created")]
    public string? Created { get; set; }

    /// <summary>Last modification time (RFC 3339).</summary>
    [JsonPropertyName("modified")]
    public string? Modified { get; set; }

    /// <summary>Fields this SDK does not model yet.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>Response of <c>GET /environments/{environment}/files/{path}</c> (and of a file upload).</summary>
public sealed class ListEnvironmentFilesResponse
{
    /// <summary>The files.</summary>
    [JsonPropertyName("files")]
    public List<EnvironmentFile>? Files { get; set; }

    /// <summary>The token of the next page, if any.</summary>
    [JsonPropertyName("next_page_token")]
    public string? NextPageToken { get; set; }
}

/// <summary>Known environment statuses.</summary>
public static class EnvironmentStatuses
{
    /// <summary>The environment is active.</summary>
    public const string Active = "active";

    /// <summary>The environment has expired.</summary>
    public const string Expired = "expired";
}

/// <summary>Known environment network modes.</summary>
public static class EnvironmentNetworkModes
{
    /// <summary>Network access is disabled.</summary>
    public const string Disabled = "disabled";
}

/// <summary>Known environment file entry types.</summary>
public static class EnvironmentFileTypes
{
    /// <summary>A file.</summary>
    public const string File = "file";

    /// <summary>A directory.</summary>
    public const string Directory = "directory";
}

/// <summary>Known environment source types (<see cref="InteractionEnvironmentSource.Type"/>).</summary>
public static class EnvironmentSourceTypes
{
    /// <summary>A Google Cloud Storage location (bring your own storage).</summary>
    public const string Gcs = "gcs";

    /// <summary>Inline file content.</summary>
    public const string Inline = "inline";

    /// <summary>A source repository.</summary>
    public const string Repository = "repository";

    /// <summary>A skill registry.</summary>
    public const string SkillRegistry = "skill_registry";
}
