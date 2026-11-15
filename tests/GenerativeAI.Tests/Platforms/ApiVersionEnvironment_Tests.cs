using Shouldly;

namespace GenerativeAI.Tests.Platforms;

/// <summary>
/// Runs on its own: these tests change a process-wide environment variable that other tests' URLs depend on.
/// </summary>
[CollectionDefinition(nameof(ApiVersionEnvironmentCollection), DisableParallelization = true)]
public class ApiVersionEnvironmentCollection
{
}

/// <summary>
/// GOOGLE_GENAI_API_VERSION selects the API version when none is passed, matching the official SDKs.
/// </summary>
[Collection(nameof(ApiVersionEnvironmentCollection))]
public class ApiVersionEnvironment_Tests : IDisposable
{
    private const string Variable = "GOOGLE_GENAI_API_VERSION";
    private readonly string? _original = Environment.GetEnvironmentVariable(Variable);

    public void Dispose() => Environment.SetEnvironmentVariable(Variable, _original);

    [Fact]
    public void NoVariable_UsesDefaults()
    {
        Environment.SetEnvironmentVariable(Variable, null);

        new GoogleAIPlatformAdapter("key").GetApiVersion().ShouldBe("v1beta");
        CreateVertex().GetApiVersion().ShouldBe("v1beta1");
    }

    [Fact]
    public void Variable_IsUsedWhenNoVersionIsPassed()
    {
        Environment.SetEnvironmentVariable(Variable, "v1");

        var google = new GoogleAIPlatformAdapter("key");
        google.GetApiVersion().ShouldBe("v1");
        google.GetBaseUrl().ShouldEndWith("/v1");
        CreateVertex().GetApiVersion().ShouldBe("v1");
    }

    [Fact]
    public void BetaAlias_MapsToEachPlatformsBetaVersion()
    {
        Environment.SetEnvironmentVariable(Variable, "beta");

        new GoogleAIPlatformAdapter("key").GetApiVersion().ShouldBe("v1beta");
        CreateVertex().GetApiVersion().ShouldBe("v1beta1");
    }

    [Fact]
    public void ExplicitVersion_WinsOverVariable()
    {
        Environment.SetEnvironmentVariable(Variable, "v1");

        new GoogleAIPlatformAdapter("key", apiVersion: "v1alpha").GetApiVersion().ShouldBe("v1alpha");
    }

    private static VertextPlatformAdapter CreateVertex() =>
        new("project", "us-central1", accessToken: "token", validateAccessToken: false);
}
