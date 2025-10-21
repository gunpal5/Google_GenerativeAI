using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Tuning data statistics for Supervised Fine-Tuning.
/// Provides comprehensive analytics about the training dataset.
/// </summary>
public class SupervisedTuningDataStats
{
    /// <summary>
    /// Output only. Number of examples in the tuning dataset.
    /// </summary>
    [JsonPropertyName("tuningDatasetExampleCount")]
    public long? TuningDatasetExampleCount { get; set; }

    /// <summary>
    /// Output only. Number of tuning steps for this Tuning Job.
    /// </summary>
    [JsonPropertyName("tuningStepCount")]
    public long? TuningStepCount { get; set; }

    /// <summary>
    /// Output only. Number of billable characters in the tuning dataset.
    /// </summary>
    [JsonPropertyName("totalBillableCharacterCount")]
    public long? TotalBillableCharacterCount { get; set; }

    /// <summary>
    /// Output only. Number of billable tokens in the tuning dataset.
    /// </summary>
    [JsonPropertyName("totalBillableTokenCount")]
    public long? TotalBillableTokenCount { get; set; }

    /// <summary>
    /// Output only. Number of tuning characters in the tuning dataset.
    /// </summary>
    [JsonPropertyName("totalTuningCharacterCount")]
    public long? TotalTuningCharacterCount { get; set; }

    /// <summary>
    /// Output only. The number of examples in the dataset that have been dropped.
    /// An example can be dropped for reasons including: too many tokens, contains an invalid image, contains too many images, etc.
    /// </summary>
    [JsonPropertyName("totalTruncatedExampleCount")]
    public long? TotalTruncatedExampleCount { get; set; }

    /// <summary>
    /// Output only. A partial sample of the indices (starting from 1) of the dropped examples.
    /// </summary>
    [JsonPropertyName("truncatedExampleIndices")]
    public List<int>? TruncatedExampleIndices { get; set; }

    /// <summary>
    /// Output only. For each index in truncatedExampleIndices, the user-facing reason why the example was dropped.
    /// </summary>
    [JsonPropertyName("droppedExampleReasons")]
    public List<string>? DroppedExampleReasons { get; set; }

    /// <summary>
    /// Output only. Sample user messages in the training dataset uri.
    /// </summary>
    [JsonPropertyName("userDatasetExamples")]
    public List<Content>? UserDatasetExamples { get; set; }

    /// <summary>
    /// Output only. Dataset distributions for the user input tokens.
    /// </summary>
    [JsonPropertyName("userInputTokenDistribution")]
    public SupervisedTuningDatasetDistribution? UserInputTokenDistribution { get; set; }

    /// <summary>
    /// Output only. Dataset distributions for the user output tokens.
    /// </summary>
    [JsonPropertyName("userOutputTokenDistribution")]
    public SupervisedTuningDatasetDistribution? UserOutputTokenDistribution { get; set; }

    /// <summary>
    /// Output only. Dataset distributions for the messages per example.
    /// </summary>
    [JsonPropertyName("userMessagePerExampleDistribution")]
    public SupervisedTuningDatasetDistribution? UserMessagePerExampleDistribution { get; set; }
}
