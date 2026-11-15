using System.Text.Json.Nodes;
using GenerativeAI.Clients;
using GenerativeAI.Core;
using GenerativeAI.Exceptions;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.LiveTests;

/// <summary>
/// Live tests for recently added Vertex AI features. Skipped unless VERTEX_AI_TESTS_ENABLED=true, ADC is configured
/// and GOOGLE_PROJECT_ID is set. Uses GOOGLE_REGION (e.g. <c>global</c>) for global services; regional-only services
/// use GOOGLE_REGION when it is a real region, otherwise VERTEX_REGIONAL_LOCATION (default us-central1).
/// </summary>
public class VertexAI_LiveTests : TestBase
{
    private const string Model = VertexAIModels.Gemini.Gemini38Flash;

    public VertexAI_LiveTests(ITestOutputHelper helper) : base(helper)
    {
    }

    private IPlatformAdapter VertexPlatform()
    {
        LiveTestHelpers.RequireVertex();
        return GetTestVertexAIPlatform();
    }

    private static async Task<T> OrSkipOnRegionAsync<T>(Func<Task<T>> action, string feature, string region)
    {
        try
        {
            return await LiveTestHelpers.OrSkipAsync(action, feature, skipWhenUnavailable: false);
        }
        catch (ApiException ex) when (LiveTestHelpers.IsRegionError(ex) || LiveTestHelpers.IsFeatureUnavailable(ex))
        {
            Assert.Skip($"{feature} is not available in region '{region}' ({ex.ErrorCode} {ex.ErrorStatus}): {ex.ErrorMessage}");
            throw;
        }
    }

    [Fact]
    public async Task GeminiEmbedding001_EmbedContent_WithTaskTypeAndDimensionality_AndBatch()
    {
        var platform = VertexPlatform();
        var region = LiveTestHelpers.VertexRegion ?? "(default)";
        var model = new EmbeddingModel(platform, VertexAIModels.Embeddings.GeminiEmbedding001);
        const int dimensions = 256;

        var single = await OrSkipOnRegionAsync(() => model.EmbedContentAsync(new EmbedContentRequest
        {
            Content = new Content("What is the capital of France?", Roles.User),
            TaskType = TaskType.RETRIEVAL_QUERY,
            OutputDimensionality = dimensions
        }), "gemini-embedding-001 embedContent", region);
        single.Embedding.ShouldNotBeNull();
        single.Embedding.Values.ShouldNotBeNull();
        single.Embedding.Values.Count.ShouldBe(dimensions);

        var batch = await OrSkipOnRegionAsync(() => model.BatchEmbedContentAsync(new[]
        {
            new EmbedContentRequest
            {
                Content = new Content("Paris is the capital of France.", Roles.User),
                TaskType = TaskType.RETRIEVAL_DOCUMENT, OutputDimensionality = dimensions
            },
            new EmbedContentRequest
            {
                Content = new Content("Berlin is the capital of Germany.", Roles.User),
                TaskType = TaskType.RETRIEVAL_DOCUMENT, OutputDimensionality = dimensions
            }
        }), "gemini-embedding-001 batch embed", region);
        batch.Embeddings.ShouldNotBeNull();
        batch.Embeddings.Count.ShouldBe(2);
        batch.Embeddings.ShouldAllBe(e => e.Values != null && e.Values.Count == dimensions);
    }

    [Fact]
    public async Task TextEmbedding005_Predict_IsRegional()
    {
        LiveTestHelpers.RequireVertex();
        var region = LiveTestHelpers.RegionalLocation;
        Console?.WriteLine($"Using regional location {region} for text-embedding-005.");
        var model = new EmbeddingModel(LiveTestHelpers.CreateRegionalVertexPlatform(),
            VertexAIModels.Gemini.TextEmbedding005);

        var single = await OrSkipOnRegionAsync(() => model.EmbedContentAsync(new EmbedContentRequest
        {
            Content = new Content("What is the capital of France?", Roles.User),
            TaskType = TaskType.RETRIEVAL_QUERY,
            OutputDimensionality = 128
        }), "text-embedding-005 predict", region);
        single.Embedding?.Values.ShouldNotBeNull();
        single.Embedding!.Values!.Count.ShouldBe(128);

        var batch = await OrSkipOnRegionAsync(() => model.BatchEmbedContentAsync(new[]
        {
            new Content("first", Roles.User),
            new Content("second", Roles.User)
        }), "text-embedding-005 batch predict", region);
        batch.Embeddings.ShouldNotBeNull();
        batch.Embeddings.Count.ShouldBe(2);
        batch.Embeddings.ShouldAllBe(e => e.Values != null && e.Values.Count > 0);
    }

    [Fact]
    public async Task FunctionCalling_AutomaticRoundTrip_SendsUserRoleResults()
    {
        var platform = VertexPlatform();
        var tool = new SecretNumberTool();
        var model = new GenerativeModel(platform, Model, SmallConfig());
        model.AddFunctionTool(tool);

        var response = await LiveTestHelpers.OrSkipAsync(() => model.GenerateContentAsync(
            "Call get_secret_number and reply with only the number it returns.",
            TestContext.Current.CancellationToken), "Vertex function calling", skipWhenUnavailable: false);

        Console?.WriteLine($"Answer: {response.Text()}");
        tool.Calls.ShouldBeGreaterThan(0);
        response.Text().ShouldNotBeNull();
        response.Text()!.ShouldContain(SecretNumberTool.SecretNumber.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public async Task FunctionCalling_ManualRoundTrip_LegacyFunctionRoleIsAccepted()
    {
        var platform = VertexPlatform();
        var ct = TestContext.Current.CancellationToken;
        var tool = new SecretNumberTool();
        var model = new GenerativeModel(platform, Model, SmallConfig());
        model.AddFunctionTool(tool, functionCallingBehaviour: new FunctionCallingBehaviour
        {
            AutoCallFunction = false, AutoReplyFunction = false
        });

        var prompt = new Content("Call get_secret_number and reply with only the number it returns.", Roles.User);
        var first = await LiveTestHelpers.OrSkipAsync(() => model.GenerateContentAsync(
            new GenerateContentRequest { Contents = new List<Content> { prompt } }, ct),
            "Vertex function calling", skipWhenUnavailable: false);
        var call = first.GetFunction();
        call.ShouldNotBeNull("The model did not call the function.");
        call.Name.ShouldBe(SecretNumberTool.FunctionName);

        // History saved by older SDK versions used the "function" role; the SDK must convert it to "user".
        var result = (await tool.CallAsync(call, ct))!;
        var second = await model.GenerateContentAsync(new GenerateContentRequest
        {
            Contents = new List<Content>
            {
                prompt,
                first.Candidates![0].Content!,
                new Content(new[] { new Part { FunctionResponse = result } }, Roles.Function)
            }
        }, ct);

        Console?.WriteLine($"Answer: {second.Text()}");
        second.Text()!.ShouldContain(SecretNumberTool.SecretNumber.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public async Task ComputeTokens_ReturnsTokensInfo()
    {
        var platform = VertexPlatform();
        var client = new ModelClient(platform);

        var response = await LiveTestHelpers.OrSkipAsync(() => client.ComputeTokensAsync(Model,
            new[] { new Content("Hello world", Roles.User) }, TestContext.Current.CancellationToken),
            "computeTokens");

        response.TokensInfo.ShouldNotBeNull();
        response.TokensInfo.ShouldNotBeEmpty();
        var info = response.TokensInfo[0];
        info.TokenIds.ShouldNotBeNull();
        info.TokenIds.ShouldNotBeEmpty();
        info.Tokens.ShouldNotBeNull();
        info.Tokens.Count.ShouldBe(info.TokenIds.Count);
        Console?.WriteLine($"Role {info.Role}: {info.TokenIds.Count} tokens");
    }

    [Fact]
    public async Task Tuning_ListJobs()
    {
        LiveTestHelpers.RequireVertex();
        var region = LiveTestHelpers.RegionalLocation;
        var client = new TuningClient(LiveTestHelpers.CreateRegionalVertexPlatform());

        var page = await OrSkipOnRegionAsync(() => client.ListAsync(pageSize: 5,
            cancellationToken: TestContext.Current.CancellationToken), "Tuning jobs", region);

        page.ShouldNotBeNull();
        Console?.WriteLine($"Listed {page.TuningJobs?.Count ?? 0} tuning jobs in {region}.");
    }

    [Fact]
    public async Task Interactions_CreateOnVertex()
    {
        var platform = VertexPlatform();
        var client = new InteractionsClient(platform);
        var ct = TestContext.Current.CancellationToken;

        Interaction interaction;
        try
        {
            interaction = await LiveTestHelpers.OrSkipAsync(() => client.CreateAsync(new CreateInteractionRequest
            {
                Model = Model,
                Input = "Reply with the single word: pong",
                GenerationConfig = Interactions_LiveTests.SmallConfig()
            }, ct), "Interactions on Vertex AI");
        }
        catch (ApiException ex) when (ex.ErrorCode == 400)
        {
            Assert.Skip($"Vertex AI rejected the interaction request (likely unsupported here): {ex.ErrorMessage}");
            return;
        }

        try
        {
            Console?.WriteLine($"{interaction.Id}: {interaction.Status} '{interaction.OutputText}'");
            interaction.Status.ShouldBe(InteractionStatuses.Completed);
            interaction.OutputText.ShouldNotBeNullOrWhiteSpace();
        }
        finally
        {
            if (!string.IsNullOrEmpty(interaction.Id))
                await LiveTestHelpers.CleanupAsync(() => client.DeleteAsync(interaction.Id!, CancellationToken.None),
                    Console, $"interaction {interaction.Id}");
        }
    }

    private static GenerationConfig SmallConfig() => new()
    {
        MaxOutputTokens = 512,
        ThinkingConfig = new ThinkingConfig { ThinkingLevel = ThinkingLevel.LOW }
    };

    /// <summary>A parameterless function tool returning a fixed number the model cannot guess.</summary>
    private sealed class SecretNumberTool : IFunctionTool
    {
        public const string FunctionName = "get_secret_number";
        public const int SecretNumber = 7243;

        public int Calls { get; private set; }

        public Tool AsTool() => new()
        {
            FunctionDeclarations = new List<FunctionDeclaration>
            {
                new() { Name = FunctionName, Description = "Returns the secret number." }
            }
        };

        public Task<FunctionResponse?> CallAsync(FunctionCall functionCall, CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult<FunctionResponse?>(new FunctionResponse(FunctionName)
            {
                Id = functionCall.Id,
                Response = new JsonObject { ["value"] = SecretNumber }
            });
        }

        public bool IsContainFunction(string name) => name == FunctionName;
    }
}
