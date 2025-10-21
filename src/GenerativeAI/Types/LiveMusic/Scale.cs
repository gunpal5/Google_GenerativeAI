using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Scale of the generated music.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<Scale>))]
public enum Scale
{
    /// <summary>
    /// Default value. This value is unused.
    /// </summary>
    [JsonPropertyName("SCALE_UNSPECIFIED")]
    SCALE_UNSPECIFIED = 0,

    /// <summary>
    /// C major or A minor.
    /// </summary>
    [JsonPropertyName("C_MAJOR_A_MINOR")]
    C_MAJOR_A_MINOR = 1,

    /// <summary>
    /// Db major or Bb minor.
    /// </summary>
    [JsonPropertyName("D_FLAT_MAJOR_B_FLAT_MINOR")]
    D_FLAT_MAJOR_B_FLAT_MINOR = 2,

    /// <summary>
    /// D major or B minor.
    /// </summary>
    [JsonPropertyName("D_MAJOR_B_MINOR")]
    D_MAJOR_B_MINOR = 3,

    /// <summary>
    /// Eb major or C minor.
    /// </summary>
    [JsonPropertyName("E_FLAT_MAJOR_C_MINOR")]
    E_FLAT_MAJOR_C_MINOR = 4,

    /// <summary>
    /// E major or Db minor.
    /// </summary>
    [JsonPropertyName("E_MAJOR_D_FLAT_MINOR")]
    E_MAJOR_D_FLAT_MINOR = 5,

    /// <summary>
    /// F major or D minor.
    /// </summary>
    [JsonPropertyName("F_MAJOR_D_MINOR")]
    F_MAJOR_D_MINOR = 6,

    /// <summary>
    /// Gb major or Eb minor.
    /// </summary>
    [JsonPropertyName("G_FLAT_MAJOR_E_FLAT_MINOR")]
    G_FLAT_MAJOR_E_FLAT_MINOR = 7,

    /// <summary>
    /// G major or E minor.
    /// </summary>
    [JsonPropertyName("G_MAJOR_E_MINOR")]
    G_MAJOR_E_MINOR = 8,

    /// <summary>
    /// Ab major or F minor.
    /// </summary>
    [JsonPropertyName("A_FLAT_MAJOR_F_MINOR")]
    A_FLAT_MAJOR_F_MINOR = 9,

    /// <summary>
    /// A major or Gb minor.
    /// </summary>
    [JsonPropertyName("A_MAJOR_G_FLAT_MINOR")]
    A_MAJOR_G_FLAT_MINOR = 10,

    /// <summary>
    /// Bb major or G minor.
    /// </summary>
    [JsonPropertyName("B_FLAT_MAJOR_G_MINOR")]
    B_FLAT_MAJOR_G_MINOR = 11,

    /// <summary>
    /// B major or Ab minor.
    /// </summary>
    [JsonPropertyName("B_MAJOR_A_FLAT_MINOR")]
    B_MAJOR_A_FLAT_MINOR = 12
}
