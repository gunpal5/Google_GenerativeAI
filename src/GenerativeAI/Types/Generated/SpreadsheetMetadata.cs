using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Metadata describes the input spreadsheet content. Set this in `Part.spreadsheet_metadata` or
/// `FunctionResponsePart.spreadsheet_metadata` when `inline_data` or `file_data` contains a spreadsheet to
/// configure static spreadsheet preprocessing.
/// </summary>
/// <remarks>Generated from python-genai by scripts/drift/update_dtos.py.</remarks>
public class SpreadsheetMetadata
{
    /// <summary>
    /// Content layers to return. If empty, defaults to [DATA, FORMULAS] when SpreadsheetMetadata is set.
    /// </summary>
    [JsonPropertyName("contentLayers")]
    public List<SpreadsheetMetadataContentLayer>? ContentLayers { get; set; }

    /// <summary>
    /// The 1-indexed index of the sheet to extract, in workbook order. If not set, the first sheet is used.
    /// </summary>
    [JsonPropertyName("sheetIndex")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? SheetIndex { get; set; }

    /// <summary>
    /// The range of cells to extract, in A1 notation (e.g. "A1:D10"). If not set, the full occupied area of the sheet
    /// is used.
    /// </summary>
    [JsonPropertyName("visibleCells")]
    public string? VisibleCells { get; set; }
}
