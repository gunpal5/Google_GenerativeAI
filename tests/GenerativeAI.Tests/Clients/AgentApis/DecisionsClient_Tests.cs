using System.Text.Json;
using System.Text.Json.Nodes;
using GenerativeAI.Clients;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Clients.AgentApis;

/// <summary>
/// Offline tests of <see cref="DecisionsClient"/> (wire format follows python-genai's <c>_gaos/decisions.py</c>).
/// </summary>
public class DecisionsClient_Tests
{
    private const string ResponseJson = """
        {
          "answers": {
            "is_spam": {"type": "noul", "noul": 0.92, "confidence": 0.8},
            "topic": {"type": "choice", "choice": "billing", "confidence": 0.7,
                      "probabilities": {"billing": 0.7, "tech": 0.3}},
            "urgency": {"type": "score", "score": 2, "confidence": 0.6, "legend": {"1": "low", "2": "high"},
                        "probabilities": {"1": 0.4, "2": 0.6}},
            "future": {"type": "ranking", "order": ["a", "b"]}
          },
          "prompt_feedback": {"block_reason": "other", "block_reason_message": "msg"},
          "usage": {"total_input_tokens": 10, "total_tokens": 12}
        }
        """;

    [Fact]
    public async Task CreateAsync_PostsSnakeCaseQuestionsAndParsesAnswers()
    {
        var handler = new AgentApiTestHandler().Enqueue(ResponseJson);
        var client = new DecisionsClient(AgentApiTestHandler.GooglePlatform(), new HttpClient(handler));

        var response = await client.CreateAsync(DecisionModels.GeminiDecision1, "Please refund my last invoice!!!",
            new Dictionary<string, DecisionQuestion>
            {
                ["is_spam"] = new NoulDecisionQuestion
                {
                    Instructions = "Is this spam?",
                    Criteria = new NoulCriteria { TrueDescription = "spam", FalseDescription = "not spam" }
                },
                ["topic"] = new ChoiceDecisionQuestion
                {
                    Instructions = InteractionInput.FromContents(new[] { new InteractionTextContent { Text = "Topic?" } }),
                    Criteria = new Dictionary<string, string> { ["billing"] = "Billing", ["tech"] = "Tech" }
                },
                ["urgency"] = new ScoreDecisionQuestion
                {
                    Instructions = "How urgent?",
                    Criteria = new List<string> { "low", "high" }
                }
            }, TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Post);
        request.Url.AbsoluteUri.ShouldBe("https://generativelanguage.googleapis.com/v1beta/decisions");
        request.Headers["x-goog-api-key"].ShouldBe("test-key");
        var body = JsonNode.Parse(request.Body!)!;
        body["model"]!.GetValue<string>().ShouldBe("gemini-decision-1");
        body["input"]!.GetValue<string>().ShouldBe("Please refund my last invoice!!!");
        var questions = body["questions"]!;
        questions["is_spam"]!["type"]!.GetValue<string>().ShouldBe("noul");
        questions["is_spam"]!["instructions"]!.GetValue<string>().ShouldBe("Is this spam?");
        questions["is_spam"]!["criteria"]!["true_description"]!.GetValue<string>().ShouldBe("spam");
        questions["is_spam"]!["criteria"]!["false_description"]!.GetValue<string>().ShouldBe("not spam");
        questions["topic"]!["type"]!.GetValue<string>().ShouldBe("choice");
        questions["topic"]!["instructions"]![0]!["type"]!.GetValue<string>().ShouldBe("text");
        questions["topic"]!["criteria"]!["billing"]!.GetValue<string>().ShouldBe("Billing");
        questions["urgency"]!["type"]!.GetValue<string>().ShouldBe("score");
        questions["urgency"]!["criteria"]![1]!.GetValue<string>().ShouldBe("high");

        response.Answers!.Count.ShouldBe(4);
        var noul = response.Answers["is_spam"].ShouldBeOfType<NoulDecisionAnswer>();
        noul.Noul.ShouldBe(0.92);
        noul.Confidence.ShouldBe(0.8);
        var choice = response.Answers["topic"].ShouldBeOfType<ChoiceDecisionAnswer>();
        choice.Choice.ShouldBe("billing");
        choice.Probabilities!["tech"].ShouldBe(0.3);
        var score = response.Answers["urgency"].ShouldBeOfType<ScoreDecisionAnswer>();
        score.Score.ShouldBe(2);
        score.Legend!["2"].ShouldBe("high");
        var unknown = response.Answers["future"].ShouldBeOfType<UnknownDecisionAnswer>();
        unknown.Type.ShouldBe("ranking");
        unknown.Raw["order"]![0]!.GetValue<string>().ShouldBe("a");
        response.PromptFeedback!.BlockReason.ShouldBe(DecisionBlockReasons.Other);
        response.PromptFeedback.BlockReasonMessage.ShouldBe("msg");
        response.Usage!.TotalTokens.ShouldBe(12);
    }

    [Fact]
    public async Task CreateAsync_VertexUsesProjectLocationPath()
    {
        var handler = new AgentApiTestHandler().Enqueue("{}");
        var platform = AgentApiTestHandler.VertexPlatform();
        var client = new DecisionsClient(platform, new HttpClient(handler));

        await client.CreateAsync(DecisionModels.GeminiDecision1, "x",
            new Dictionary<string, DecisionQuestion> { ["q"] = new NoulDecisionQuestion { Instructions = "?" } },
            TestContext.Current.CancellationToken);

        handler.Requests.Single().Url.AbsoluteUri.ShouldBe(
            $"https://us-central1-aiplatform.googleapis.com/{platform.GetApiVersion()}/projects/test-project/locations/us-central1/decisions");
    }

    [Fact]
    public async Task CreateAsync_ValidatesRequest()
    {
        var client = new DecisionsClient(AgentApiTestHandler.GooglePlatform(), new HttpClient(new AgentApiTestHandler()));
        await Should.ThrowAsync<ArgumentException>(() => client.CreateAsync(new CreateDecisionRequest
        {
            Model = DecisionModels.GeminiDecision1, Input = "x"
        }, TestContext.Current.CancellationToken));
    }

    [Fact]
    public void UnknownQuestion_RoundTripsRawJson()
    {
        const string json = """{"model":"m","input":"x","questions":{"q":{"type":"future","foo":1}}}""";
        var request = JsonSerializer.Deserialize(json, AgentApisSerializerContext.Default.CreateDecisionRequest)!;
        request.Questions!["q"].ShouldBeOfType<UnknownDecisionQuestion>().Type.ShouldBe("future");
        var back = JsonNode.Parse(JsonSerializer.Serialize(request, AgentApisSerializerContext.Default.CreateDecisionRequest))!;
        back["questions"]!["q"]!["foo"]!.GetValue<int>().ShouldBe(1);
    }
}
