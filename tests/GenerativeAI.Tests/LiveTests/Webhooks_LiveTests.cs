using GenerativeAI.Clients;
using GenerativeAI.Exceptions;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.LiveTests;

/// <summary>
/// Live test for the Webhooks API (<see cref="WebhooksClient"/>). Skipped unless GOOGLE_API_KEY or GEMINI_API_KEY is
/// set. Creates one webhook pointing at example.com and always deletes it.
/// </summary>
public class Webhooks_LiveTests : TestBase
{
    private const string TestUri = "https://example.com/generativeai-sdk-test";

    public Webhooks_LiveTests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Fact]
    public async Task Create_Get_List_Ping_Rotate_Delete()
    {
        var client = new WebhooksClient(LiveTestHelpers.RequireGeminiPlatform());
        var ct = TestContext.Current.CancellationToken;

        Webhook webhook;
        try
        {
            webhook = await LiveTestHelpers.OrSkipAsync(() => client.CreateAsync(TestUri,
                new[] { WebhookEvents.BatchSucceeded }, cancellationToken: ct), "Webhooks");
        }
        catch (ApiException ex) when (ex.ErrorCode == 400 &&
                                      (ex.ErrorMessage.IndexOf("verif", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                       ex.ErrorMessage.IndexOf("uri", StringComparison.OrdinalIgnoreCase) >= 0))
        {
            Assert.Skip($"The service rejected the test webhook URI: {ex.ErrorMessage}");
            return;
        }

        webhook.Id.ShouldNotBeNullOrEmpty();
        Console?.WriteLine($"Created webhook {webhook.Id} (state {webhook.State})");
        var deleted = false;
        try
        {
            webhook.Uri.ShouldBe(TestUri);
            webhook.SubscribedEvents.ShouldNotBeNull();
            webhook.SubscribedEvents.ShouldContain(WebhookEvents.BatchSucceeded);

            var fetched = await client.GetAsync(webhook.Id!, ct);
            fetched.Id.ShouldBe(webhook.Id);
            fetched.Uri.ShouldBe(TestUri);

            var found = false;
            await foreach (var item in client.ListAllAsync(pageSize: 50, cancellationToken: ct))
            {
                if (item.Id == webhook.Id)
                {
                    found = true;
                    break;
                }
            }

            found.ShouldBeTrue("The created webhook was not returned by list.");

            var rotated = await client.RotateSigningSecretAsync(webhook.Id!,
                WebhookRevocationBehaviors.RevokePreviousSecretsImmediately, ct);
            rotated.Secret.ShouldNotBeNullOrEmpty();

            string? pingProblem = null;
            try
            {
                await client.PingAsync(webhook.Id!, ct);
                Console?.WriteLine("Ping accepted.");
            }
            catch (ApiException ex)
            {
                // example.com does not accept webhook deliveries; the service may refuse to ping it.
                pingProblem = $"Ping was rejected ({ex.ErrorCode} {ex.ErrorStatus}): {ex.ErrorMessage}";
                Console?.WriteLine(pingProblem);
            }

            await client.DeleteAsync(webhook.Id!, ct);
            deleted = true;

            if (pingProblem != null)
                Assert.Skip($"Create/get/list/rotate/delete passed; ping inconclusive. {pingProblem}");
        }
        finally
        {
            if (!deleted)
                await LiveTestHelpers.CleanupAsync(() => client.DeleteAsync(webhook.Id!, CancellationToken.None),
                    Console, $"webhook {webhook.Id}");
        }
    }
}
