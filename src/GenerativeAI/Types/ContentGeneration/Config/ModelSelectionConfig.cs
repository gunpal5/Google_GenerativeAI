using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Config for model selection.
/// </summary>
public class ModelSelectionConfig
{
    /// <summary>
    /// Options for feature selection preference.
    /// </summary>
    [JsonPropertyName("featureSelectionPreference")]
    public FeatureSelectionPreference? FeatureSelectionPreference { get; set; }
}
