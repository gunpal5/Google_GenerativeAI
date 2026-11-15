using System.Text.Json;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Converters;

/// <summary>
/// Deserializes Live API server messages in the wire format the server sends.
/// </summary>
public class LiveServerMessages_Tests
{
    [Fact]
    public void GoAway_DeserializesTimeLeft()
    {
        const string json = """{"goAway":{"timeLeft":"30s"}}""";

        var payload = JsonSerializer.Deserialize<LiveServerMessage>(json, DefaultSerializerOptions.Options);

        payload.ShouldNotBeNull();
        payload.GoAway.ShouldNotBeNull();
        payload.GoAway.TimeLeft.ShouldBe("30s");
        payload.GoAway.TimeLeftDuration.ShouldBe(TimeSpan.FromSeconds(30));
    }

    [Theory]
    [InlineData("1.5s", 1500)]
    [InlineData("0s", 0)]
    public void GoAway_TimeLeftDuration_ParsesFractionalSeconds(string timeLeft, int expectedMs)
    {
        new LiveServerGoAway { TimeLeft = timeLeft }.TimeLeftDuration.ShouldBe(TimeSpan.FromMilliseconds(expectedMs));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("soon")]
    public void GoAway_TimeLeftDuration_IsNullForMissingOrInvalidValues(string? timeLeft)
    {
        new LiveServerGoAway { TimeLeft = timeLeft }.TimeLeftDuration.ShouldBeNull();
    }

    [Fact]
    public void SessionResumptionUpdate_DeserializesWireFields()
    {
        // int64 values are encoded as strings in protobuf JSON
        const string json =
            """{"sessionResumptionUpdate":{"newHandle":"handle-123","resumable":true,"lastConsumedClientMessageIndex":"42"}}""";

        var payload = JsonSerializer.Deserialize<LiveServerMessage>(json, DefaultSerializerOptions.Options);

        payload.ShouldNotBeNull();
        var update = payload.SessionResumptionUpdate;
        update.ShouldNotBeNull();
        update.NewHandle.ShouldBe("handle-123");
        update.Resumable.ShouldBe(true);
        update.LastConsumedClientMessageIndex.ShouldBe(42);
    }

    [Fact]
    public void SessionResumptionUpdate_AcceptsNumericMessageIndex()
    {
        const string json = """{"sessionResumptionUpdate":{"resumable":false,"lastConsumedClientMessageIndex":7}}""";

        var payload = JsonSerializer.Deserialize<LiveServerMessage>(json, DefaultSerializerOptions.Options);

        payload!.SessionResumptionUpdate!.Resumable.ShouldBe(false);
        payload.SessionResumptionUpdate.LastConsumedClientMessageIndex.ShouldBe(7);
        payload.SessionResumptionUpdate.NewHandle.ShouldBeNull();
    }
}
