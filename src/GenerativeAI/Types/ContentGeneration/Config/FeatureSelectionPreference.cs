using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Options for feature selection preference.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<FeatureSelectionPreference>))]
public enum FeatureSelectionPreference
{
    /// <summary>
    /// Feature selection preference unspecified.
    /// </summary>
    FEATURE_SELECTION_PREFERENCE_UNSPECIFIED = 0,

    /// <summary>
    /// Prioritize quality over cost.
    /// </summary>
    PRIORITIZE_QUALITY = 1,

    /// <summary>
    /// Balanced between quality and cost.
    /// </summary>
    BALANCED = 2,

    /// <summary>
    /// Prioritize cost over quality.
    /// </summary>
    PRIORITIZE_COST = 3
}
