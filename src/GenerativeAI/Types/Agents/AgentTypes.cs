using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// A managed agent (<c>/agents</c>): a reusable agent definition (base agent, configuration, system instruction,
/// tools and base environment) that interactions can reference by ID.
/// </summary>
public sealed class Agent
{
    /// <summary>The agent ID. Optional on creation (the server assigns one when omitted).</summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>The agent this agent is based on (see <see cref="InteractionAgents"/>).</summary>
    [JsonPropertyName("base_agent")]
    public string? BaseAgent { get; set; }

    /// <summary>A description of the agent.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>The system instruction of the agent.</summary>
    [JsonPropertyName("system_instruction")]
    public string? SystemInstruction { get; set; }

    /// <summary>
    /// The agent configuration (python-genai types it as the Antigravity configuration; any
    /// <see cref="InteractionAgentConfig"/> is accepted and unknown types are kept as raw JSON).
    /// </summary>
    [JsonPropertyName("agent_config")]
    public InteractionAgentConfig? AgentConfig { get; set; }

    /// <summary>The base environment: an environment ID or a remote environment configuration.</summary>
    [JsonPropertyName("base_environment")]
    public InteractionEnvironmentReference? BaseEnvironment { get; set; }

    /// <summary>
    /// The tools of the agent (code execution, function, Google Search, MCP server, URL context). Unknown tool types
    /// are read as <see cref="UnknownInteractionTool"/>.
    /// </summary>
    [JsonPropertyName("tools")]
    public List<InteractionTool>? Tools { get; set; }

    /// <summary>Fields this SDK does not model yet.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>Response of <c>GET /agents</c>.</summary>
public sealed class ListAgentsResponse
{
    /// <summary>The agents.</summary>
    [JsonPropertyName("agents")]
    public List<Agent>? Agents { get; set; }

    /// <summary>The token of the next page, if any.</summary>
    [JsonPropertyName("next_page_token")]
    public string? NextPageToken { get; set; }
}
