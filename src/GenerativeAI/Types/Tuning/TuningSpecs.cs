using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using GenerativeAI.Types.RagEngine;

namespace GenerativeAI.Types;

/// <summary>
/// Tuning spec for preference optimization.
/// </summary>
public class PreferenceOptimizationSpec
{
    /// <summary>
    /// Optional. If set to true, disable intermediate checkpoints and only export the last checkpoint.
    /// </summary>
    [JsonPropertyName("exportLastCheckpointOnly")]
    public bool? ExportLastCheckpointOnly { get; set; }

    /// <summary>
    /// Optional. Hyperparameters for preference optimization.
    /// </summary>
    [JsonPropertyName("hyperParameters")]
    public PreferenceOptimizationHyperParameters? HyperParameters { get; set; }

    /// <summary>
    /// Required. Cloud Storage path to the JSONL training dataset.
    /// </summary>
    [JsonPropertyName("trainingDatasetUri")]
    public string? TrainingDatasetUri { get; set; }

    /// <summary>
    /// Optional. Cloud Storage path to the JSONL validation dataset.
    /// </summary>
    [JsonPropertyName("validationDatasetUri")]
    public string? ValidationDatasetUri { get; set; }

    /// <summary>
    /// Optional. Evaluation config for the tuning job.
    /// </summary>
    [JsonPropertyName("evaluationConfig")]
    public EvaluationConfig? EvaluationConfig { get; set; }
}

/// <summary>
/// Hyperparameters for preference optimization.
/// </summary>
[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
public class PreferenceOptimizationHyperParameters
{
    /// <summary>
    /// Optional. Adapter size for preference optimization.
    /// </summary>
    [JsonPropertyName("adapterSize")]
    public AdapterSize? AdapterSize { get; set; }

    /// <summary>
    /// Optional. Weight for KL Divergence regularization.
    /// </summary>
    [JsonPropertyName("beta")]
    public double? Beta { get; set; }

    /// <summary>
    /// Optional. Number of complete passes the model makes over the entire training dataset during training.
    /// </summary>
    [JsonPropertyName("epochCount")]
    public int? EpochCount { get; set; }

    /// <summary>
    /// Optional. Multiplier for adjusting the default learning rate.
    /// </summary>
    [JsonPropertyName("learningRateMultiplier")]
    public double? LearningRateMultiplier { get; set; }
}

/// <summary>
/// Tuning spec for distillation.
/// </summary>
public class DistillationSpec
{
    /// <summary>
    /// Optional. Cloud Storage path to the JSONL prompt dataset for distillation.
    /// </summary>
    [JsonPropertyName("promptDatasetUri")]
    public string? PromptDatasetUri { get; set; }

    /// <summary>
    /// The base teacher model that is being distilled.
    /// </summary>
    [JsonPropertyName("baseTeacherModel")]
    public string? BaseTeacherModel { get; set; }

    /// <summary>
    /// Optional. Hyperparameters for distillation.
    /// </summary>
    [JsonPropertyName("hyperParameters")]
    public DistillationHyperParameters? HyperParameters { get; set; }

    /// <summary>
    /// Deprecated. A path in a Cloud Storage bucket treated as the root output directory of the distillation pipeline.
    /// </summary>
    [JsonPropertyName("pipelineRootDirectory")]
    public string? PipelineRootDirectory { get; set; }

    /// <summary>
    /// Deprecated. The student model that is being tuned. Use the tuning job base model instead.
    /// </summary>
    [JsonPropertyName("studentModel")]
    public string? StudentModel { get; set; }

    /// <summary>
    /// Deprecated. Cloud Storage path to the JSONL training dataset.
    /// </summary>
    [JsonPropertyName("trainingDatasetUri")]
    public string? TrainingDatasetUri { get; set; }

    /// <summary>
    /// The resource name of the tuned teacher model.
    /// Format: <c>projects/{project}/locations/{location}/models/{model}</c>.
    /// </summary>
    [JsonPropertyName("tunedTeacherModelSource")]
    public string? TunedTeacherModelSource { get; set; }

    /// <summary>
    /// Optional. Cloud Storage path to the JSONL validation dataset.
    /// </summary>
    [JsonPropertyName("validationDatasetUri")]
    public string? ValidationDatasetUri { get; set; }

    /// <summary>
    /// Optional. The tuning mode for the SFT part of distillation (open source models only).
    /// </summary>
    [JsonPropertyName("tuningMode")]
    public TuningMode? TuningMode { get; set; }

    /// <summary>
    /// Optional. If set to true, disable intermediate checkpoints and only export the last checkpoint.
    /// </summary>
    [JsonPropertyName("exportLastCheckpointOnly")]
    public bool? ExportLastCheckpointOnly { get; set; }

    /// <summary>
    /// Optional. Evaluation config for the tuning job.
    /// </summary>
    [JsonPropertyName("evaluationConfig")]
    public EvaluationConfig? EvaluationConfig { get; set; }
}

/// <summary>
/// Hyperparameters for distillation.
/// </summary>
[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
public class DistillationHyperParameters
{
    /// <summary>
    /// The size of the adapter.
    /// </summary>
    [JsonPropertyName("adapterSize")]
    public AdapterSize? AdapterSize { get; set; }

    /// <summary>
    /// Number of complete passes the model makes over the entire training dataset during training.
    /// </summary>
    [JsonPropertyName("epochCount")]
    public int? EpochCount { get; set; }

    /// <summary>
    /// Multiplier for adjusting the default learning rate.
    /// </summary>
    [JsonPropertyName("learningRateMultiplier")]
    public double? LearningRateMultiplier { get; set; }

    /// <summary>
    /// Generation config for teacher model sampling.
    /// </summary>
    [JsonPropertyName("generationConfig")]
    public GenerationConfig? GenerationConfig { get; set; }

    /// <summary>
    /// The learning rate for distillation tuning.
    /// </summary>
    [JsonPropertyName("learningRate")]
    public double? LearningRate { get; set; }

    /// <summary>
    /// Batch size for tuning (open source models only).
    /// </summary>
    [JsonPropertyName("batchSize")]
    public int? BatchSize { get; set; }

    /// <summary>
    /// Multiplier for adjusting the weight of the SFT loss.
    /// </summary>
    [JsonPropertyName("sftLossWeightMultiplier")]
    public double? SftLossWeightMultiplier { get; set; }
}

/// <summary>
/// Spec for the teacher model sampling of a distillation job.
/// </summary>
public class DistillationSamplingSpec
{
    /// <summary>
    /// The base teacher model that is being distilled.
    /// </summary>
    [JsonPropertyName("baseTeacherModel")]
    public string? BaseTeacherModel { get; set; }

    /// <summary>
    /// The resource name of the tuned teacher model.
    /// </summary>
    [JsonPropertyName("tunedTeacherModelSource")]
    public string? TunedTeacherModelSource { get; set; }

    /// <summary>
    /// Cloud Storage path to the JSONL validation dataset.
    /// </summary>
    [JsonPropertyName("validationDatasetUri")]
    public string? ValidationDatasetUri { get; set; }

    /// <summary>
    /// Cloud Storage path to the JSONL prompt dataset.
    /// </summary>
    [JsonPropertyName("promptDatasetUri")]
    public string? PromptDatasetUri { get; set; }

    /// <summary>
    /// Hyperparameters for distillation tuning.
    /// </summary>
    [JsonPropertyName("hyperparameters")]
    public DistillationHyperParameters? Hyperparameters { get; set; }
}

/// <summary>
/// Tuning spec for full fine tuning.
/// </summary>
public class FullFineTuningSpec
{
    /// <summary>
    /// Optional. Hyperparameters for full fine tuning.
    /// </summary>
    [JsonPropertyName("hyperParameters")]
    public SupervisedHyperParameters? HyperParameters { get; set; }

    /// <summary>
    /// Required. Training dataset (Cloud Storage path or Multimodal Dataset resource).
    /// </summary>
    [JsonPropertyName("trainingDatasetUri")]
    public string? TrainingDatasetUri { get; set; }

    /// <summary>
    /// Optional. Validation dataset (Cloud Storage path or Multimodal Dataset resource).
    /// </summary>
    [JsonPropertyName("validationDatasetUri")]
    public string? ValidationDatasetUri { get; set; }
}

/// <summary>
/// Tuning spec for Veo LoRA tuning.
/// </summary>
public class VeoLoraTuningSpec
{
    /// <summary>
    /// Optional. Hyperparameters for Veo LoRA.
    /// </summary>
    [JsonPropertyName("hyperParameters")]
    public VeoHyperParameters? HyperParameters { get; set; }

    /// <summary>
    /// Required. Training dataset (Cloud Storage path or Multimodal Dataset resource).
    /// </summary>
    [JsonPropertyName("trainingDatasetUri")]
    public string? TrainingDatasetUri { get; set; }

    /// <summary>
    /// Optional. Validation dataset (Cloud Storage path or Multimodal Dataset resource).
    /// </summary>
    [JsonPropertyName("validationDatasetUri")]
    public string? ValidationDatasetUri { get; set; }

    /// <summary>
    /// Optional. The orientation of the video. Defaults to LANDSCAPE.
    /// </summary>
    [JsonPropertyName("videoOrientation")]
    public VideoOrientation? VideoOrientation { get; set; }
}

/// <summary>
/// Tuning job metadata.
/// </summary>
public class TuningJobMetadata
{
    /// <summary>
    /// Output only. The number of epochs that have been completed.
    /// </summary>
    [JsonPropertyName("completedEpochCount")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? CompletedEpochCount { get; set; }

    /// <summary>
    /// Output only. The number of steps that have been completed (multi-step reinforcement tuning).
    /// </summary>
    [JsonPropertyName("completedStepCount")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? CompletedStepCount { get; set; }
}

/// <summary>
/// An evaluation run of a tuning job.
/// </summary>
public class EvaluateDatasetRun
{
    /// <summary>
    /// Output only. The checkpoint id used in the evaluation run (only when evaluating checkpoints).
    /// </summary>
    [JsonPropertyName("checkpointId")]
    public string? CheckpointId { get; set; }

    /// <summary>
    /// Output only. The error of the evaluation run, if any.
    /// </summary>
    [JsonPropertyName("error")]
    public GoogleRpcStatus? Error { get; set; }

    /// <summary>
    /// Output only. Results of the evaluation (raw JSON).
    /// </summary>
    [JsonPropertyName("evaluateDatasetResponse")]
    public JsonObject? EvaluateDatasetResponse { get; set; }

    /// <summary>
    /// Output only. The resource name of the evaluation run.
    /// </summary>
    [JsonPropertyName("evaluationRun")]
    public string? EvaluationRun { get; set; }

    /// <summary>
    /// Output only. Deprecated: the operation name of the evaluation run.
    /// </summary>
    [JsonPropertyName("operationName")]
    public string? OperationName { get; set; }
}
