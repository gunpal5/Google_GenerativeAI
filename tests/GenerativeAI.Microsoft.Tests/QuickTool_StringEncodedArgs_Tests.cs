using System.Text.Json.Nodes;
using GenerativeAI.Tools;
using GenerativeAI.Types;
using Shouldly;
using Xunit;

namespace GenerativeAI.Tests.Microsoft;

/// <summary>
/// Checks that <see cref="QuickTool"/> turns string arguments into date, time, duration and UUID parameters,
/// including the non-canonical forms models often send.
/// </summary>
public class QuickTool_StringEncodedArgs_Tests
{
    private static string Describe(DateTime when, DateOnly day, TimeOnly at, TimeSpan span, Guid id,
        DateTimeOffset stamp, DateTime? optional) =>
        string.Join("|", when.ToString("O"), day.ToString("O"), at.ToString("HH:mm:ss"), span.ToString("c"),
            id.ToString("D"), stamp.ToString("O"), optional?.ToString("O") ?? "null");

    private static async Task<string> InvokeAsync(JsonObject args)
    {
        var tool = new QuickTool(Describe);
        var response = await tool.CallAsync(new FunctionCall { Name = tool.FunctionDeclaration.Name, Args = args },
            TestContext.Current.CancellationToken);
        return response!.Response!["content"]!.GetValue<string>();
    }

    [Fact]
    public async Task CanonicalIsoStrings_AreParsed()
    {
        var result = await InvokeAsync(new JsonObject
        {
            ["when"] = "2024-05-01T10:30:00",
            ["day"] = "2024-05-01",
            ["at"] = "10:30:00",
            ["span"] = "01:30:00",
            ["id"] = "00000000-0000-0000-0000-000000000001",
            ["stamp"] = "2024-05-01T10:30:00+05:30",
            ["optional"] = "2024-05-02T00:00:00Z"
        });

        result.ShouldBe("2024-05-01T10:30:00.0000000|2024-05-01|10:30:00|01:30:00|" +
                        "00000000-0000-0000-0000-000000000001|2024-05-01T10:30:00.0000000+05:30|" +
                        "2024-05-02T00:00:00.0000000Z");
    }

    [Fact]
    public async Task NonCanonicalStrings_AreParsedLeniently()
    {
        var result = await InvokeAsync(new JsonObject
        {
            ["when"] = "2024-05-01 10:30:00",
            ["day"] = "2024-05-01T00:00:00",
            ["at"] = "10:30",
            ["span"] = "PT1H30M",
            ["id"] = "00000000000000000000000000000001",
            ["stamp"] = "2024-05-01 10:30:00",
            ["optional"] = null
        });

        result.ShouldBe("2024-05-01T10:30:00.0000000|2024-05-01|10:30:00|01:30:00|" +
                        "00000000-0000-0000-0000-000000000001|2024-05-01T10:30:00.0000000+00:00|null");
    }

    [Fact]
    public async Task InvalidString_StillThrows()
    {
        await Should.ThrowAsync<System.Text.Json.JsonException>(() => InvokeAsync(new JsonObject
        {
            ["when"] = "not a date",
            ["day"] = "2024-05-01",
            ["at"] = "10:30:00",
            ["span"] = "01:30:00",
            ["id"] = "00000000-0000-0000-0000-000000000001",
            ["stamp"] = "2024-05-01T10:30:00Z"
        }));
    }
}
