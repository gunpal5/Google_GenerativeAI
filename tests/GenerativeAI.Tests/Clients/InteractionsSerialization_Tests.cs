using System.Text.Json;
using System.Text.Json.Nodes;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Clients;

/// <summary>
/// Serialization round-trip tests for the Interactions types (unknown fields and variants must survive).
/// </summary>
public class InteractionsSerialization_Tests
{
    [Fact]
    public void Interaction_RoundTrip_KeepsUnknownFieldsAtTheirLevel()
    {
        const string json = """
            {"id":"x","status":"completed","some_future_field":{"a":1},
             "steps":[{"type":"model_output","new_step_field":2,"content":[{"type":"text","text":"a","annotations":[{"type":"zz","foo":"bar"}]}]},
                      {"type":"function_call","id":"f","name":"n","arguments":{"c":"P"}},
                      {"type":"holo","p":1}]}
            """;

        var interaction = JsonSerializer.Deserialize(json, InteractionsSerializerContext.Default.Interaction)!;
        var written = JsonNode.Parse(JsonSerializer.Serialize(interaction, InteractionsSerializerContext.Default.Interaction))!;

        written["some_future_field"]!["a"]!.GetValue<int>().ShouldBe(1);
        written["steps"]![0]!["new_step_field"]!.GetValue<int>().ShouldBe(2);
        written["steps"]![0]!["type"]!.GetValue<string>().ShouldBe("model_output");
        written["steps"]![0]!["content"]![0]!["annotations"]![0]!["foo"]!.GetValue<string>().ShouldBe("bar");
        written["steps"]![1]!["arguments"]!["c"]!.GetValue<string>().ShouldBe("P");
        written["steps"]![2]!["p"]!.GetValue<int>().ShouldBe(1);
        JsonNode.DeepEquals(written["steps"]![2], JsonNode.Parse("""{"type":"holo","p":1}""")).ShouldBeTrue();
    }

    [Fact]
    public void Request_ExtensionData_IsSentAsTopLevelFields()
    {
        var request = new CreateInteractionRequest
        {
            Model = "m",
            ExtensionData = new Dictionary<string, JsonElement>
            {
                ["future_flag"] = JsonDocument.Parse("true").RootElement.Clone()
            }
        };

        var written = JsonNode.Parse(JsonSerializer.Serialize(request, InteractionsSerializerContext.Default.CreateInteractionRequest))!;
        written["future_flag"]!.GetValue<bool>().ShouldBeTrue();
        written["model"]!.GetValue<string>().ShouldBe("m");
    }
}
