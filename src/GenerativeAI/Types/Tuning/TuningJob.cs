using System.Text.Json.Serialization;
using GenerativeAI.Types.RagEngine;

namespace GenerativeAI.Types;

/// <summary>
/// Represents a model tuning job.
/// A tuning job is used to fine-tune a base model with custom training data
/// to create a specialized model tailored to specific tasks or domains.
/// </summary>
public class TuningJob
{
    /// <summary>
    /// Output only. Identifier. Resource name of a TuningJob.
    /// Format: `projects/{project}/locations/{location}/tuningJobs/{tuning_job}`
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Output only. The detailed state of the job.
    /// </summary>
    [JsonPropertyName("state")]
    public JobState? State { get; set; }

    /// <summary>
    /// Output only. Time when the TuningJob was created.
    /// </summary>
    [JsonPropertyName("createTime")]
    public Timestamp? CreateTime { get; set; }

    /// <summary>
    /// Output only. Time when the TuningJob for the first time entered the `JOB_STATE_RUNNING` state.
    /// </summary>
    [JsonPropertyName("startTime")]
    public Timestamp? StartTime { get; set; }

    /// <summary>
    /// Output only. Time when the TuningJob entered any of the following JobStates:
    /// `JOB_STATE_SUCCEEDED`, `JOB_STATE_FAILED`, `JOB_STATE_CANCELLED`, `JOB_STATE_EXPIRED`.
    /// </summary>
    [JsonPropertyName("endTime")]
    public Timestamp? EndTime { get; set; }

    /// <summary>
    /// Output only. Time when the TuningJob was most recently updated.
    /// </summary>
    [JsonPropertyName("updateTime")]
    public Timestamp? UpdateTime { get; set; }

    /// <summary>
    /// Output only. Only populated when job's state is `JOB_STATE_FAILED` or `JOB_STATE_CANCELLED`.
    /// </summary>
    [JsonPropertyName("error")]
    public GoogleRpcStatus? Error { get; set; }

    /// <summary>
    /// Optional. The description of the TuningJob.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// The base model that is being tuned.
    /// See <see href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/tuning#supported_models">Supported models</see>.
    /// </summary>
    [JsonPropertyName("baseModel")]
    public string? BaseModel { get; set; }

    /// <summary>
    /// Output only. The tuned model resources associated with this TuningJob.
    /// </summary>
    [JsonPropertyName("tunedModel")]
    public TunedModel? TunedModel { get; set; }

    /// <summary>
    /// The pre-tuned model for continuous tuning.
    /// </summary>
    [JsonPropertyName("preTunedModel")]
    public PreTunedModel? PreTunedModel { get; set; }

    /// <summary>
    /// Tuning Spec for Supervised Fine Tuning.
    /// </summary>
    [JsonPropertyName("supervisedTuningSpec")]
    public SupervisedTuningSpec? SupervisedTuningSpec { get; set; }

    /// <summary>
    /// Output only. The tuning data statistics associated with this TuningJob.
    /// </summary>
    [JsonPropertyName("tuningDataStats")]
    public TuningDataStats? TuningDataStats { get; set; }

    /// <summary>
    /// Customer-managed encryption key options for a TuningJob.
    /// If this is set, then all resources created by the TuningJob will be encrypted with the provided encryption key.
    /// </summary>
    [JsonPropertyName("encryptionSpec")]
    public EncryptionSpec? EncryptionSpec { get; set; }

    /// <summary>
    /// Tuning Spec for open sourced and third party Partner models.
    /// </summary>
    [JsonPropertyName("partnerModelTuningSpec")]
    public PartnerModelTuningSpec? PartnerModelTuningSpec { get; set; }

    /// <summary>
    /// Optional. Configuration for model evaluation during tuning.
    /// </summary>
    [JsonPropertyName("evaluationConfig")]
    public EvaluationConfig? EvaluationConfig { get; set; }

    /// <summary>
    /// Optional. The user-provided path to custom model weights.
    /// Set this field to tune a custom model. The path must be a Cloud Storage directory that contains
    /// the model weights in .safetensors format along with associated model metadata files.
    /// If this field is set, the base_model field must still be set to indicate which base model
    /// the custom model is derived from. This feature is only available for open source models.
    /// </summary>
    [JsonPropertyName("customBaseModel")]
    public string? CustomBaseModel { get; set; }

    /// <summary>
    /// Output only. The Experiment associated with this TuningJob.
    /// </summary>
    [JsonPropertyName("experiment")]
    public string? Experiment { get; set; }

    /// <summary>
    /// Optional. The labels with user-defined metadata to organize TuningJob and generated resources such as Model and Endpoint.
    /// Label keys and values can be no longer than 64 characters (Unicode codepoints), can only contain lowercase letters,
    /// numeric characters, underscores and dashes. International characters are allowed.
    /// See https://goo.gl/xmQnxf for more information and examples of labels.
    /// </summary>
    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }

    /// <summary>
    /// Optional. Cloud Storage path to the directory where tuning job outputs are written to.
    /// This field is only available and required for open source models.
    /// </summary>
    [JsonPropertyName("outputUri")]
    public string? OutputUri { get; set; }

    /// <summary>
    /// Output only. The resource name of the PipelineJob associated with the TuningJob.
    /// Format: `projects/{project}/locations/{location}/pipelineJobs/{pipeline_job}`.
    /// </summary>
    [JsonPropertyName("pipelineJob")]
    public string? PipelineJob { get; set; }

    /// <summary>
    /// The service account that the tuningJob workload runs as.
    /// If not specified, the Vertex AI Secure Fine-Tuned Service Agent in the project will be used.
    /// See https://cloud.google.com/iam/docs/service-agents#vertex-ai-secure-fine-tuning-service-agent
    /// Users starting the pipeline must have the `iam.serviceAccounts.actAs` permission on this service account.
    /// </summary>
    [JsonPropertyName("serviceAccount")]
    public string? ServiceAccount { get; set; }

    /// <summary>
    /// Optional. The display name of the TunedModel.
    /// The name can be up to 128 characters long and can consist of any UTF-8 characters.
    /// </summary>
    [JsonPropertyName("tunedModelDisplayName")]
    public string? TunedModelDisplayName { get; set; }

    /// <summary>
    /// Tuning Spec for Veo Tuning.
    /// </summary>
    [JsonPropertyName("veoTuningSpec")]
    public VeoTuningSpec? VeoTuningSpec { get; set; }
}
