using System.Text.Json;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Converters;

public class LenientFinishReasonConverter_Tests
{
    [Fact]
    public void Read_KnownValues_DeserializeCorrectly()
    {
        // Old value
        JsonSerializer.Deserialize<FinishReason>("\"STOP\"").ShouldBe(FinishReason.STOP);

        // New value with lowercase (tests case insensitivity)
        JsonSerializer.Deserialize<FinishReason>("\"unexpected_tool_call\"").ShouldBe(FinishReason.UNEXPECTED_TOOL_CALL);
    }

    [Fact]
    public void Read_UnknownValue_FallsBackToOther()
    {
        var result = JsonSerializer.Deserialize<FinishReason>("\"UNKNOWN_FUTURE_VALUE\"");
        result.ShouldBe(FinishReason.OTHER);
    }

    [Fact]
    public void Write_EnumValue_SerializesCorrectly()
    {
        var json = JsonSerializer.Serialize(FinishReason.UNEXPECTED_TOOL_CALL);
        json.ShouldBe("\"UNEXPECTED_TOOL_CALL\"");
    }
}
