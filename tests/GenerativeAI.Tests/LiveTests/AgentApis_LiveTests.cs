using System.Text;
using GenerativeAI.Clients;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.LiveTests;

/// <summary>
/// Live tests for the agent APIs (decisions, voices, agents, environments, credentials, triggers). Skipped unless
/// GOOGLE_API_KEY or GEMINI_API_KEY is set. A 403/404/FAILED_PRECONDITION (feature not enabled for the project) or
/// 402/429 skips the test with the API message. Every created resource is deleted.
/// </summary>
public class AgentApis_LiveTests : TestBase
{
    public AgentApis_LiveTests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Fact]
    public async Task Decisions_ChoiceQuestion_ReturnsAnswer()
    {
        var client = new DecisionsClient(LiveTestHelpers.RequireGeminiPlatform());
        var ct = TestContext.Current.CancellationToken;

        var response = await LiveTestHelpers.OrSkipAsync(() => client.CreateAsync(DecisionModels.GeminiDecision1,
            "The sky on a clear day is blue.",
            new Dictionary<string, DecisionQuestion>
            {
                ["sky_color"] = new ChoiceDecisionQuestion
                {
                    Instructions = "Which color does the text say the sky is?",
                    Criteria = new Dictionary<string, string>
                    {
                        ["blue"] = "The text says the sky is blue.",
                        ["green"] = "The text says the sky is green."
                    }
                }
            }, ct), "Decisions");

        response.Answers.ShouldNotBeNull();
        response.Answers.ShouldContainKey("sky_color");
        var answer = response.Answers["sky_color"].ShouldBeOfType<ChoiceDecisionAnswer>();
        Console?.WriteLine($"Choice: {answer.Choice} (confidence {answer.Confidence})");
        answer.Choice.ShouldBe("blue");
    }

    [Fact]
    public async Task Voices_ListAndDesign()
    {
        var client = new VoicesClient(LiveTestHelpers.RequireGeminiPlatform());
        var ct = TestContext.Current.CancellationToken;

        var page = await LiveTestHelpers.OrSkipAsync(() => client.ListAsync(pageSize: 5, cancellationToken: ct),
            "Voices");
        page.Voices.ShouldNotBeNull();
        Console?.WriteLine($"Listed {page.Voices.Count} voices: {string.Join(", ", page.Voices.Select(v => v.Id))}");

        // Designing a voice generates audio samples, so it is opt-in.
        if (System.Environment.GetEnvironmentVariable("LIVE_TESTS_DESIGN_VOICE")?.ToLowerInvariant() != "true")
        {
            Console?.WriteLine("Voice design skipped; set LIVE_TESTS_DESIGN_VOICE=true to also design (and delete) a voice.");
            return;
        }

        var voice = await LiveTestHelpers.OrSkipAsync(() => client.DesignAsync(
            "A calm, warm narrator voice.", "sdk-live-test-" + LiveTestHelpers.UniqueSuffix(), store: true, ct),
            "Voice design");
        try
        {
            Console?.WriteLine($"Designed voice {voice.Id} ({voice.Type})");
            voice.Id.ShouldNotBeNullOrEmpty();
            voice.Type.ShouldBe(VoiceTypes.Prompted);
        }
        finally
        {
            if (!string.IsNullOrEmpty(voice.Id))
                await LiveTestHelpers.CleanupAsync(() => client.DeleteAsync(voice.Id!, CancellationToken.None),
                    Console, $"voice {voice.Id}");
        }
    }

    [Fact]
    public async Task Agents_List()
    {
        var client = new AgentsClient(LiveTestHelpers.RequireGeminiPlatform());
        var ct = TestContext.Current.CancellationToken;

        var page = await LiveTestHelpers.OrSkipAsync(() => client.ListAsync(pageSize: 5, cancellationToken: ct),
            "Agents");
        page.ShouldNotBeNull();
        Console?.WriteLine($"Listed {page.Agents?.Count ?? 0} agents.");
    }

    [Fact]
    public async Task Triggers_List()
    {
        var client = new TriggersClient(LiveTestHelpers.RequireGeminiPlatform());
        var ct = TestContext.Current.CancellationToken;

        var page = await LiveTestHelpers.OrSkipAsync(() => client.ListAsync(pageSize: 5, cancellationToken: ct),
            "Triggers");
        page.ShouldNotBeNull();
        Console?.WriteLine($"Listed {page.Triggers?.Count ?? 0} triggers.");
    }

    [Fact]
    public async Task Environments_UploadListDownload()
    {
        var client = new EnvironmentsClient(LiveTestHelpers.RequireGeminiPlatform());
        var ct = TestContext.Current.CancellationToken;

        var environment = await LiveTestHelpers.OrSkipAsync(() => client.CreateAsync(cancellationToken: ct),
            "Environments");
        environment.Id.ShouldNotBeNullOrEmpty();
        Console?.WriteLine($"Created environment {environment.Id} ({environment.Status})");
        try
        {
            const string path = "sdk-live-test/hello.txt";
            var bytes = Encoding.UTF8.GetBytes("hello from the GenerativeAI SDK live tests");
            using (var stream = new MemoryStream(bytes))
            {
                await client.UploadFileAsync(environment.Id!, path, stream, "text/plain", overwrite: true,
                    cancellationToken: ct);
            }

            var files = new List<EnvironmentFile>();
            await foreach (var file in client.ListAllFilesAsync(environment.Id!, recursive: true, cancellationToken: ct))
                files.Add(file);
            Console?.WriteLine($"Files: {string.Join(", ", files.Select(f => f.Path ?? f.Name))}");
            files.ShouldContain(f => (f.Path ?? f.Name ?? string.Empty).TrimStart('/').EndsWith("hello.txt",
                StringComparison.Ordinal));

            var downloaded = await client.DownloadFileAsync(environment.Id!, path, ct);
            downloaded.ShouldBe(bytes);
        }
        finally
        {
            await LiveTestHelpers.CleanupAsync(() => client.DeleteAsync(environment.Id!, CancellationToken.None),
                Console, $"environment {environment.Id}");
        }
    }

    [Fact]
    public async Task Credentials_CreateGetDelete()
    {
        var client = new CredentialsClient(LiveTestHelpers.RequireGeminiPlatform());
        var ct = TestContext.Current.CancellationToken;
        var id = "sdk-live-test-" + LiveTestHelpers.UniqueSuffix();

        var created = await LiveTestHelpers.OrSkipAsync(() => client.CreateAsync(new EnvironmentVariableCredentialConfig
        {
            Id = id,
            Value = "dummy-value-not-a-secret",
            InjectionLocation = new List<string> { CredentialInjectionLocations.Header },
            TrustedDomains = new List<string> { "example.com" }
        }, ct), "Credentials");
        try
        {
            Console?.WriteLine($"Created credential {created.Id} ({created.Type}, {created.Status})");
            created.Id.ShouldNotBeNullOrEmpty();
            created.Id!.ShouldEndWith(id);

            var fetched = await client.GetAsync(id, ct);
            fetched.Id.ShouldNotBeNull();
            fetched.Id!.ShouldEndWith(id);
            fetched.Type.ShouldBe(CredentialTypes.EnvironmentVariable);
        }
        finally
        {
            await LiveTestHelpers.CleanupAsync(() => client.DeleteAsync(id, CancellationToken.None), Console,
                $"credential {id}");
        }
    }
}
