using System.Text.Json;
using System.Text.Json.Nodes;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Converters;

/// <summary>
/// Wire-format tests for fields added to match google-genai v3.6.1 and the Gemini API discovery revision 20261123.
/// </summary>
public class ApiFieldUpdates361_Tests
{
    private static T Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, DefaultSerializerOptions.Options)!;

    private static JsonNode Serialize<T>(T value) =>
        JsonNode.Parse(JsonSerializer.Serialize(value, DefaultSerializerOptions.Options))!;

    [Fact]
    public void PartPreprocessingMetadata_SerializesWithWireNames()
    {
        var part = new Part
        {
            FileData = new FileData { FileUri = "gs://b/report.pdf", MimeType = "application/pdf" },
            DocumentMetadata = new DocumentMetadata
            {
                StartPage = 2, EndPage = 5,
                ContentLayers = new List<DocumentMetadataContentLayer> { DocumentMetadataContentLayer.PAGE_TEXT }
            },
            SpreadsheetMetadata = new SpreadsheetMetadata
            {
                SheetIndex = 1, VisibleCells = "A1:D10",
                ContentLayers = new List<SpreadsheetMetadataContentLayer> { SpreadsheetMetadataContentLayer.FORMULAS }
            },
            AudioMetadata = new AudioMetadata { StartOffset = "1s", EndOffset = "9s" }
        };

        var json = Serialize(part);
        json["documentMetadata"]!["startPage"]!.GetValue<long>().ShouldBe(2);
        json["documentMetadata"]!["endPage"]!.GetValue<long>().ShouldBe(5);
        json["documentMetadata"]!["contentLayers"]![0]!.GetValue<string>().ShouldBe("PAGE_TEXT");
        json["spreadsheetMetadata"]!["sheetIndex"]!.GetValue<long>().ShouldBe(1);
        json["spreadsheetMetadata"]!["visibleCells"]!.GetValue<string>().ShouldBe("A1:D10");
        json["spreadsheetMetadata"]!["contentLayers"]![0]!.GetValue<string>().ShouldBe("FORMULAS");
        json["audioMetadata"]!["startOffset"]!.GetValue<string>().ShouldBe("1s");
    }

    [Fact]
    public void FunctionResponsePart_MetadataAndVideoContentLayers_RoundTrip()
    {
        var part = Deserialize<FunctionResponsePart>("""
            {"videoMetadata": {"startOffset": "0s", "fps": 2, "contentLayers": ["FRAMES", "TRANSCRIPT"]},
             "audioMetadata": {"endOffset": "3s"}}
            """);

        part.VideoMetadata!.ContentLayers.ShouldBe(new[]
        {
            VideoMetadataContentLayer.FRAMES, VideoMetadataContentLayer.TRANSCRIPT
        });
        part.AudioMetadata!.EndOffset.ShouldBe("3s");
        Serialize(part)["videoMetadata"]!["contentLayers"]![1]!.GetValue<string>().ShouldBe("TRANSCRIPT");
    }

    [Fact]
    public void SpeechConfig_TextNormalization_SerializesAsString()
    {
        var json = Serialize(new SpeechConfig { TextNormalization = SpeechConfigTextNormalization.DISABLED });
        json["textNormalization"]!.GetValue<string>().ShouldBe("DISABLED");
    }

    [Fact]
    public void VertexVideoPayload_LabelsAndResizeMode_UseVertexWireNames()
    {
        var json = Serialize(new VertexGenerateVideosPayload
        {
            Parameters = new VideoParameters { ResizeMode = ImageResizeMode.PAD },
            Labels = new Dictionary<string, string> { ["team"] = "media" }
        });

        json["parameters"]!["resizeMode"]!.GetValue<string>().ShouldBe("PAD");
        json["labels"]!["team"]!.GetValue<string>().ShouldBe("media");
    }
}
