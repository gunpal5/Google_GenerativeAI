namespace GenerativeAI.Types;

/// <summary>
/// The training dataset of a tuning job.
/// </summary>
/// <remarks>
/// Vertex AI accepts <see cref="GcsUri"/> or <see cref="VertexDatasetResource"/>; the Gemini API accepts
/// only inline <see cref="Examples"/>.
/// </remarks>
public class TuningDataset
{
    /// <summary>
    /// Vertex AI only. Cloud Storage URI of the JSONL training dataset.
    /// </summary>
    public string? GcsUri { get; set; }

    /// <summary>
    /// Vertex AI only. The resource name of a Vertex AI Multimodal Dataset used as the training dataset.
    /// </summary>
    public string? VertexDatasetResource { get; set; }

    /// <summary>
    /// Gemini API only. Inline examples with simple text input and output.
    /// </summary>
    public List<TuningExample>? Examples { get; set; }
}

/// <summary>
/// The validation dataset of a tuning job (Vertex AI only).
/// </summary>
public class TuningValidationDataset
{
    /// <summary>
    /// Cloud Storage URI of the JSONL validation dataset.
    /// </summary>
    public string? GcsUri { get; set; }

    /// <summary>
    /// The resource name of a Vertex AI Multimodal Dataset used as the validation dataset.
    /// </summary>
    public string? VertexDatasetResource { get; set; }
}

/// <summary>
/// Optional configuration for <see cref="GenerativeAI.Clients.TuningClient.TuneAsync"/>, mirroring
/// <c>CreateTuningJobConfig</c> of the google-genai SDK.
/// </summary>
/// <remarks>
/// Most options are Vertex AI only. On the Gemini API only <see cref="TunedModelDisplayName"/>,
/// <see cref="EpochCount"/>, <see cref="LearningRateMultiplier"/>, <see cref="BatchSize"/> and
/// <see cref="LearningRate"/> are supported; setting any other option throws <see cref="NotSupportedException"/>.
/// </remarks>
public class CreateTuningJobConfig
{
    /// <summary>
    /// The tuning method. Defaults to <see cref="TuningMethod.SUPERVISED_FINE_TUNING"/>.
    /// </summary>
    public TuningMethod? Method { get; set; }

    /// <summary>
    /// Validation dataset for tuning (JSONL).
    /// </summary>
    public TuningValidationDataset? ValidationDataset { get; set; }

    /// <summary>
    /// The display name of the tuned model (up to 128 UTF-8 characters).
    /// </summary>
    public string? TunedModelDisplayName { get; set; }

    /// <summary>
    /// The description of the tuning job.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Number of complete passes over the training dataset.
    /// </summary>
    public int? EpochCount { get; set; }

    /// <summary>
    /// Multiplier for adjusting the default learning rate. Mutually exclusive with <see cref="LearningRate"/>.
    /// </summary>
    public double? LearningRateMultiplier { get; set; }

    /// <summary>
    /// If true, disable intermediate checkpoints and only export the last checkpoint.
    /// </summary>
    public bool? ExportLastCheckpointOnly { get; set; }

    /// <summary>
    /// The checkpoint id of the pre-tuned model to continue tuning from (when the base model is a tuned model).
    /// </summary>
    public string? PreTunedModelCheckpointId { get; set; }

    /// <summary>
    /// Adapter size for tuning.
    /// </summary>
    public AdapterSize? AdapterSize { get; set; }

    /// <summary>
    /// Tuning mode (supervised fine tuning and distillation only).
    /// </summary>
    public TuningMode? TuningMode { get; set; }

    /// <summary>
    /// Custom base model weights (Cloud Storage path). Open source models only.
    /// </summary>
    public string? CustomBaseModel { get; set; }

    /// <summary>
    /// The batch size hyperparameter.
    /// </summary>
    public int? BatchSize { get; set; }

    /// <summary>
    /// The learning rate. Mutually exclusive with <see cref="LearningRateMultiplier"/>.
    /// </summary>
    public double? LearningRate { get; set; }

    /// <summary>
    /// Evaluation config for the tuning job. Requires at least one metric and an output config.
    /// </summary>
    public EvaluationConfig? EvaluationConfig { get; set; }

    /// <summary>
    /// User-defined labels for the tuning job and the resources it creates.
    /// </summary>
    public Dictionary<string, string>? Labels { get; set; }

    /// <summary>
    /// Weight for KL Divergence regularization. Preference tuning only.
    /// </summary>
    public double? Beta { get; set; }

    /// <summary>
    /// The base teacher model that is being distilled. Distillation only.
    /// </summary>
    public string? BaseTeacherModel { get; set; }

    /// <summary>
    /// The resource name of the tuned teacher model. Distillation only.
    /// </summary>
    public string? TunedTeacherModelSource { get; set; }

    /// <summary>
    /// Multiplier for adjusting the weight of the SFT loss. Distillation only.
    /// </summary>
    public double? SftLossWeightMultiplier { get; set; }

    /// <summary>
    /// The Cloud Storage location where the tuning job outputs are written.
    /// </summary>
    public string? OutputUri { get; set; }

    /// <summary>
    /// Customer-managed encryption key options for the tuning job.
    /// </summary>
    public EncryptionSpec? EncryptionSpec { get; set; }

    /// <summary>
    /// Single reward function configuration. Reinforcement tuning only.
    /// </summary>
    public SingleReinforcementTuningRewardConfig? RewardConfig { get; set; }

    /// <summary>
    /// Composite reward function configuration. Reinforcement tuning only.
    /// </summary>
    public CompositeReinforcementTuningRewardConfig? CompositeRewardConfig { get; set; }

    /// <summary>
    /// Number of different responses to generate per prompt. Reinforcement tuning only.
    /// </summary>
    public int? SamplesPerPrompt { get; set; }

    /// <summary>
    /// How often, in steps, to evaluate during training. Reinforcement tuning only.
    /// </summary>
    public int? EvaluateInterval { get; set; }

    /// <summary>
    /// How often, in steps, to save checkpoints. Reinforcement tuning only.
    /// </summary>
    public int? CheckpointInterval { get; set; }

    /// <summary>
    /// The maximum number of tokens to generate per prompt. Reinforcement tuning only.
    /// </summary>
    public int? MaxOutputTokens { get; set; }

    /// <summary>
    /// The maximum thinking depth. Reinforcement tuning only.
    /// </summary>
    public ReinforcementTuningThinkingLevel? ThinkingLevel { get; set; }

    /// <summary>
    /// Cloud Storage path of the JSONL validation dataset. Reinforcement tuning only.
    /// </summary>
    public string? ValidationDatasetUri { get; set; }
}
