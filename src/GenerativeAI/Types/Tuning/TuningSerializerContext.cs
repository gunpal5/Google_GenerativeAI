using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Source-generated serialization metadata for the tuning types used by
/// <see cref="GenerativeAI.Clients.TuningClient"/>.
/// </summary>
/// <remarks>
/// This is a separate context rather than a partial of <see cref="TypesSerializerContext"/> because the
/// System.Text.Json source generator shipped for net6.0-net10.0/netstandard2.0 fails when
/// <c>[JsonSerializable]</c> attributes are split across partial declarations (duplicate hint names).
/// <see cref="GenerativeAI.Clients.TuningClient"/> inserts it first in its serializer resolver chain.
/// Options mirror those of <see cref="TypesSerializerContext"/>.
/// </remarks>
[JsonSourceGenerationOptions(WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(TuningJob))]
[JsonSerializable(typeof(ListTuningJobsResponse))]
[JsonSerializable(typeof(GoogleAITunedModel))]
[JsonSerializable(typeof(GoogleLongRunningOperation))]
[JsonSerializable(typeof(ValidateRewardRequest))]
[JsonSerializable(typeof(ValidateRewardResponse))]
[JsonSerializable(typeof(ReinforcementTuningExample))]
[JsonSerializable(typeof(ReinforcementTuningSpec))]
[JsonSerializable(typeof(PreferenceOptimizationSpec))]
[JsonSerializable(typeof(DistillationSpec))]
[JsonSerializable(typeof(DistillationSamplingSpec))]
[JsonSerializable(typeof(FullFineTuningSpec))]
[JsonSerializable(typeof(VeoLoraTuningSpec))]
[JsonSerializable(typeof(TuningJobMetadata))]
[JsonSerializable(typeof(EvaluateDatasetRun))]
[JsonSerializable(typeof(TuningMethod))]
[JsonSerializable(typeof(TuningJobState))]
public partial class TuningSerializerContext : JsonSerializerContext
{
}
