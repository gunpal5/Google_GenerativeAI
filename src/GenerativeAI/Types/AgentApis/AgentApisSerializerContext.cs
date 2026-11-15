using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Source-generated JSON metadata for the agent-related API types (Voices, Agents, Environments, Triggers,
/// Credentials and Decisions). The clients of these APIs put it first in their resolver chain (followed by
/// <see cref="InteractionsSerializerContext"/> for the shared Interactions types), so they work with Native AOT and
/// trimming.
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, UseStringEnumConverter = true)]
// Voices
[JsonSerializable(typeof(Voice))]
[JsonSerializable(typeof(PromptedVoice))]
[JsonSerializable(typeof(ReplicatedVoice))]
[JsonSerializable(typeof(VoiceAudioData))]
[JsonSerializable(typeof(CreateVoiceRequest))]
[JsonSerializable(typeof(ListVoicesResponse))]
[JsonSerializable(typeof(DeleteVoiceResponse))]
// Agents
[JsonSerializable(typeof(Agent))]
[JsonSerializable(typeof(ListAgentsResponse))]
// Environments
[JsonSerializable(typeof(AgentEnvironment))]
[JsonSerializable(typeof(CreateEnvironmentRequest))]
[JsonSerializable(typeof(ListEnvironmentsResponse))]
[JsonSerializable(typeof(EnvironmentFile))]
[JsonSerializable(typeof(ListEnvironmentFilesResponse))]
// Triggers
[JsonSerializable(typeof(Trigger))]
[JsonSerializable(typeof(CreateTriggerRequest))]
[JsonSerializable(typeof(UpdateTriggerRequest))]
[JsonSerializable(typeof(TriggerExecution))]
[JsonSerializable(typeof(ListTriggersResponse))]
[JsonSerializable(typeof(ListTriggerExecutionsResponse))]
// Credentials
[JsonSerializable(typeof(Credential))]
[JsonSerializable(typeof(ListCredentialsResponse))]
[JsonSerializable(typeof(CredentialConfig))]
[JsonSerializable(typeof(EnvironmentVariableCredentialConfig))]
[JsonSerializable(typeof(BearerTokenCredentialConfig))]
[JsonSerializable(typeof(OAuth2CredentialConfig))]
// Decisions
[JsonSerializable(typeof(CreateDecisionRequest))]
[JsonSerializable(typeof(CreateDecisionResponse))]
[JsonSerializable(typeof(DecisionQuestion))]
[JsonSerializable(typeof(NoulDecisionQuestion))]
[JsonSerializable(typeof(NoulCriteria))]
[JsonSerializable(typeof(ChoiceDecisionQuestion))]
[JsonSerializable(typeof(ScoreDecisionQuestion))]
[JsonSerializable(typeof(DecisionAnswer))]
[JsonSerializable(typeof(NoulDecisionAnswer))]
[JsonSerializable(typeof(ChoiceDecisionAnswer))]
[JsonSerializable(typeof(ScoreDecisionAnswer))]
[JsonSerializable(typeof(DecisionPromptFeedback))]
// Shared
[JsonSerializable(typeof(Dictionary<string, double>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(Dictionary<string, JsonElement>))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(JsonNode))]
[JsonSerializable(typeof(JsonElement))]
public partial class AgentApisSerializerContext : JsonSerializerContext
{
}
