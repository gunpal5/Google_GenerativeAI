using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Tool to support computer use.
/// </summary>
public class ComputerUse
{
    /// <summary>
    /// Required. The environment being operated.
    /// </summary>
    [JsonPropertyName("environment")]
    public Environment? Environment { get; set; }

    /// <summary>
    /// By default, predefined functions are included in the final model call.
    /// Some of them can be explicitly excluded from being automatically included.
    /// This can serve two purposes:
    /// 1. Using a more restricted / different action space.
    /// 2. Improving the definitions / instructions of predefined functions.
    /// </summary>
    [JsonPropertyName("excludedPredefinedFunctions")]
    public List<string>? ExcludedPredefinedFunctions { get; set; }
}
