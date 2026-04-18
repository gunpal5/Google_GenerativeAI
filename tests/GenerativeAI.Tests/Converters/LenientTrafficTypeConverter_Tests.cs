using System.Text.Json;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Converters;

public class LenientTrafficTypeConverter_Tests
{
    [Fact]
    public void Read_KnownValues_DeserializeCorrectly()
    {
        JsonSerializer.Deserialize<TrafficType>("\"ON_DEMAND\"").ShouldBe(TrafficType.ON_DEMAND);
        JsonSerializer.Deserialize<TrafficType>("\"PROVISIONED_THROUGHPUT\"").ShouldBe(TrafficType.PROVISIONED_THROUGHPUT);

        // New values added from Vertex AI REST reference
        JsonSerializer.Deserialize<TrafficType>("\"ON_DEMAND_PRIORITY\"").ShouldBe(TrafficType.ON_DEMAND_PRIORITY);
        JsonSerializer.Deserialize<TrafficType>("\"ON_DEMAND_FLEX\"").ShouldBe(TrafficType.ON_DEMAND_FLEX);

        // Case insensitivity
        JsonSerializer.Deserialize<TrafficType>("\"on_demand_priority\"").ShouldBe(TrafficType.ON_DEMAND_PRIORITY);
    }

    [Fact]
    public void Read_UnknownValue_FallsBackToUnspecified()
    {
        var result = JsonSerializer.Deserialize<TrafficType>("\"UNKNOWN_FUTURE_VALUE\"");
        result.ShouldBe(TrafficType.TRAFFIC_TYPE_UNSPECIFIED);
    }

    [Fact]
    public void Read_NullableUnknownValue_InUsageMetadata_DoesNotThrow()
    {
        const string json = """
        {
            "promptTokenCount": 10,
            "candidatesTokenCount": 20,
            "totalTokenCount": 30,
            "trafficType": "ON_DEMAND_PRIORITY"
        }
        """;

        var metadata = JsonSerializer.Deserialize<UsageMetadata>(json);
        metadata.ShouldNotBeNull();
        metadata.TrafficType.ShouldBe(TrafficType.ON_DEMAND_PRIORITY);
    }

    [Fact]
    public void Write_EnumValue_SerializesCorrectly()
    {
        var json = JsonSerializer.Serialize(TrafficType.ON_DEMAND_PRIORITY);
        json.ShouldBe("\"ON_DEMAND_PRIORITY\"");
    }
}
