using GenerativeAI.Clients;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Clients;

/// <summary>
/// Live tests for the Interactions API. Skipped unless GOOGLE_API_KEY or GEMINI_API_KEY is set.
/// </summary>
public class InteractionsClient_IntegrationTests : TestBase
{
    private static string? ApiKey =>
        System.Environment.GetEnvironmentVariable("GOOGLE_API_KEY") ?? System.Environment.GetEnvironmentVariable("GEMINI_API_KEY");

    public InteractionsClient_IntegrationTests(ITestOutputHelper helper) : base(helper)
    {
    }

    private static InteractionsClient CreateClient()
    {
        Assert.SkipWhen(string.IsNullOrWhiteSpace(ApiKey),
            "Interactions tests skipped. Set GOOGLE_API_KEY or GEMINI_API_KEY to run them.");
        return new InteractionsClient(new GoogleAIPlatformAdapter(ApiKey));
    }

    [Fact]
    public async Task CreateGetStreamDelete_AgainstLiveApi()
    {
        var client = CreateClient();
        var ct = TestContext.Current.CancellationToken;

        Interaction interaction;
        try
        {
            interaction = await client.CreateAsync(InteractionModels.Gemini3FlashPreview,
                "Reply with the single word: pong", ct);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("PaymentRequired", StringComparison.Ordinal) ||
                                              ex.Message.Contains("TooManyRequests", StringComparison.Ordinal))
        {
            Assert.Skip($"The API key's project cannot run interactions right now: {ex.Message}");
            return;
        }
        interaction.Id.ShouldNotBeNullOrEmpty();
        interaction.Status.ShouldBe(InteractionStatuses.Completed);
        interaction.OutputText.ShouldNotBeNullOrWhiteSpace();
        Console?.WriteLine($"{interaction.Id}: {interaction.OutputText}");

        var fetched = await client.GetAsync(interaction.Id!, cancellationToken: ct);
        fetched.Id.ShouldBe(interaction.Id);

        var text = new System.Text.StringBuilder();
        await foreach (var e in client.StreamAsync(new CreateInteractionRequest
                       {
                           Model = InteractionModels.Gemini3FlashPreview,
                           Input = "Count from 1 to 5.",
                           PreviousInteractionId = interaction.Id
                       }, ct))
        {
            if (e is InteractionStepDeltaEvent { Delta: InteractionTextDelta delta })
                text.Append(delta.Text);
        }

        text.ToString().ShouldNotBeNullOrWhiteSpace();
        await client.DeleteAsync(interaction.Id!, ct);
    }
}
