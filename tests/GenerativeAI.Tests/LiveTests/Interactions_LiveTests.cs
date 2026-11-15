using System.Text;
using GenerativeAI.Clients;
using GenerativeAI.Exceptions;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.LiveTests;

/// <summary>
/// Live tests for the Interactions API on the Gemini API. Skipped unless GOOGLE_API_KEY or GEMINI_API_KEY is set.
/// </summary>
public class Interactions_LiveTests : TestBase
{
    private const string Model = InteractionModels.Gemini38Flash;

    public Interactions_LiveTests(ITestOutputHelper helper) : base(helper)
    {
    }

    private static InteractionsClient CreateClient() => new(LiveTestHelpers.RequireGeminiPlatform());

    // Low thinking keeps the request cheap while leaving room for the (tiny) answer.
    internal static InteractionGenerationConfig SmallConfig() => new()
    {
        MaxOutputTokens = 256,
        ThinkingLevel = InteractionThinkingLevels.Low
    };

    [Fact]
    public async Task Stream_DeliversEventsAndCompletes()
    {
        var client = CreateClient();
        var ct = TestContext.Current.CancellationToken;
        var request = new CreateInteractionRequest
        {
            Model = Model,
            Input = "Reply with exactly: hello world",
            GenerationConfig = SmallConfig()
        };

        var events = new List<InteractionSseEvent>();
        var text = new StringBuilder();
        string? interactionId = null;
        try
        {
            await LiveTestHelpers.OrSkipAsync(async () =>
            {
                await foreach (var e in client.StreamAsync(request, ct))
                {
                    events.Add(e);
                    switch (e)
                    {
                        case InteractionCreatedEvent created:
                            interactionId ??= created.Interaction?.Id;
                            break;
                        case InteractionStepDeltaEvent { Delta: InteractionTextDelta delta }:
                            text.Append(delta.Text);
                            break;
                        case InteractionCompletedEvent completed:
                            interactionId ??= completed.Interaction?.Id;
                            break;
                        case InteractionErrorEvent error:
                            Assert.Fail($"Stream error event: {error.Error?.Message}");
                            break;
                    }
                }
            }, "Interactions streaming", skipWhenUnavailable: false);

            Console?.WriteLine($"Received {events.Count} events: {string.Join(", ", events.Select(e => e.EventType))}");
            Console?.WriteLine($"Streamed text: {text}");

            events.ShouldNotBeEmpty();
            events.OfType<InteractionCreatedEvent>().ShouldNotBeEmpty();
            var completedEvent = events.OfType<InteractionCompletedEvent>().LastOrDefault();
            completedEvent.ShouldNotBeNull("No interaction.completed event was received.");
            text.ToString().ShouldNotBeNullOrWhiteSpace();
            interactionId.ShouldNotBeNullOrEmpty();

            // The completed interaction is retrievable and exposes the concatenated output text.
            var fetched = await client.GetAsync(interactionId!, cancellationToken: ct);
            fetched.Status.ShouldBe(InteractionStatuses.Completed);
            fetched.OutputText.ShouldNotBeNullOrWhiteSpace();
        }
        finally
        {
            if (interactionId != null)
                await LiveTestHelpers.CleanupAsync(() => client.DeleteAsync(interactionId, CancellationToken.None),
                    Console, $"interaction {interactionId}");
        }
    }

    [Fact]
    public async Task BackgroundCreate_WaitForCompletion_GetAndDelete()
    {
        var client = CreateClient();
        var ct = TestContext.Current.CancellationToken;

        Interaction created;
        try
        {
            created = await LiveTestHelpers.OrSkipAsync(() => client.CreateAsync(new CreateInteractionRequest
            {
                Model = Model,
                Input = "Reply with the single word: pong",
                Background = true,
                GenerationConfig = SmallConfig()
            }, ct), "Background interactions", skipWhenUnavailable: false);
        }
        catch (ApiException ex) when (ex.ErrorCode == 400 &&
                                      ex.ErrorMessage.IndexOf("background", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            Assert.Skip($"The API does not accept background=true for model interactions: {ex.ErrorMessage}");
            return;
        }

        created.Id.ShouldNotBeNullOrEmpty();
        Console?.WriteLine($"Created background interaction {created.Id} with status {created.Status}");
        var deleted = false;
        try
        {
            var final = await client.WaitForCompletionAsync(created.Id!, TimeSpan.FromSeconds(2),
                TimeSpan.FromMinutes(3), ct);
            final.Status.ShouldBe(InteractionStatuses.Completed);
            final.OutputText.ShouldNotBeNullOrWhiteSpace();
            Console?.WriteLine($"Output: {final.OutputText}");

            var fetched = await client.GetAsync(created.Id!, cancellationToken: ct);
            fetched.Id.ShouldBe(created.Id);

            await client.DeleteAsync(created.Id!, ct);
            deleted = true;
        }
        finally
        {
            if (!deleted)
                await LiveTestHelpers.CleanupAsync(() => client.DeleteAsync(created.Id!, CancellationToken.None),
                    Console, $"interaction {created.Id}");
        }
    }
}
