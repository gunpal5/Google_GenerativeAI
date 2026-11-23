using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Content layers to extract from a spreadsheet.
/// </summary>
/// <remarks>Generated from python-genai by scripts/drift/update_dtos.py.</remarks>
[JsonConverter(typeof(JsonStringEnumConverter<SpreadsheetMetadataContentLayer>))]
public enum SpreadsheetMetadataContentLayer
{
    /// <summary>
    /// The API value "CONTENT_LAYER_UNSPECIFIED".
    /// </summary>
    CONTENT_LAYER_UNSPECIFIED = 0,

    /// <summary>
    /// The API value "DATA".
    /// </summary>
    DATA = 1,

    /// <summary>
    /// The API value "FORMULAS".
    /// </summary>
    FORMULAS = 2,

    /// <summary>
    /// The API value "VISUAL".
    /// </summary>
    VISUAL = 3
}
