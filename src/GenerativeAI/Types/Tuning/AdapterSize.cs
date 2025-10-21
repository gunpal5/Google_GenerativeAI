using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Optional. Adapter size for tuning.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<AdapterSize>))]
public enum AdapterSize
{
    /// <summary>
    /// Adapter size is unspecified.
    /// </summary>
    ADAPTER_SIZE_UNSPECIFIED = 0,

    /// <summary>
    /// Adapter size 1.
    /// </summary>
    ADAPTER_SIZE_ONE = 1,

    /// <summary>
    /// Adapter size 2.
    /// </summary>
    ADAPTER_SIZE_TWO = 2,

    /// <summary>
    /// Adapter size 4.
    /// </summary>
    ADAPTER_SIZE_FOUR = 3,

    /// <summary>
    /// Adapter size 8.
    /// </summary>
    ADAPTER_SIZE_EIGHT = 4,

    /// <summary>
    /// Adapter size 16.
    /// </summary>
    ADAPTER_SIZE_SIXTEEN = 5,

    /// <summary>
    /// Adapter size 32.
    /// </summary>
    ADAPTER_SIZE_THIRTY_TWO = 6
}
