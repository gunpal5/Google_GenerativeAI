using GenerativeAI.Core;
using GenerativeAI.Exceptions;

namespace GenerativeAI.Tests.LiveTests;

/// <summary>
/// Shared helpers for the live (real API) tests: credential gating and turning "feature not available for this
/// project / region / quota" errors into skipped tests rather than failures.
/// </summary>
internal static class LiveTestHelpers
{
    public const string GeminiKeySkipMessage =
        "Live Gemini API test skipped. Set GOOGLE_API_KEY or GEMINI_API_KEY to run it.";

    /// <summary>The Gemini API key from GOOGLE_API_KEY or GEMINI_API_KEY, or null.</summary>
    public static string? GeminiApiKey
    {
        get
        {
            var key = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
            if (string.IsNullOrWhiteSpace(key))
                key = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            return string.IsNullOrWhiteSpace(key) ? null : key;
        }
    }

    /// <summary>Skips the test when no Gemini API key is configured; otherwise returns the key.</summary>
    public static string RequireGeminiKey()
    {
        var key = GeminiApiKey;
        Assert.SkipWhen(key == null, GeminiKeySkipMessage);
        return key!;
    }

    /// <summary>Creates a Gemini API platform adapter, skipping the test when no key is configured.</summary>
    public static GoogleAIPlatformAdapter RequireGeminiPlatform() => new(RequireGeminiKey());

    /// <summary>Skips the test unless VERTEX_AI_TESTS_ENABLED=true and ADC are configured.</summary>
    public static void RequireVertex()
    {
        Assert.SkipWhen(TestBase.SkipVertexAITests, TestBase.VertextTestSkipMesaage);
        Assert.SkipWhen(string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID")),
            "Vertex AI live test skipped. Set GOOGLE_PROJECT_ID (and optionally GOOGLE_REGION).");
    }

    /// <summary>The configured Vertex AI region (GOOGLE_REGION), or null.</summary>
    public static string? VertexRegion => Environment.GetEnvironmentVariable("GOOGLE_REGION");

    /// <summary>
    /// The region used for regional-only Vertex AI services (predict embeddings, tuning, ...): GOOGLE_REGION when it
    /// is a real region, otherwise VERTEX_REGIONAL_LOCATION, otherwise <c>us-central1</c>.
    /// </summary>
    public static string RegionalLocation
    {
        get
        {
            var region = VertexRegion;
            if (!string.IsNullOrWhiteSpace(region) && !string.Equals(region, "global", StringComparison.OrdinalIgnoreCase))
                return region!;
            var fallback = Environment.GetEnvironmentVariable("VERTEX_REGIONAL_LOCATION");
            return string.IsNullOrWhiteSpace(fallback) ? "us-central1" : fallback!;
        }
    }

    /// <summary>A Vertex AI adapter for <see cref="RegionalLocation"/>, built like <c>TestBase.GetTestVertexAIPlatform</c>.</summary>
    public static IPlatformAdapter CreateRegionalVertexPlatform()
    {
        return new VertextPlatformAdapter(Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID"), RegionalLocation,
            false, Environment.GetEnvironmentVariable("GOOGLE_API_KEY"));
    }

    /// <summary>Quota exhausted / billing required.</summary>
    public static bool IsQuotaOrBilling(ApiException ex) => ex.ErrorCode is 402 or 429;

    /// <summary>
    /// The feature is not enabled for this project/key, not available in this region, or not supported on this
    /// platform (403/404/501, PERMISSION_DENIED, NOT_FOUND, FAILED_PRECONDITION, UNIMPLEMENTED).
    /// </summary>
    public static bool IsFeatureUnavailable(ApiException ex) =>
        ex.ErrorCode is 403 or 404 or 501 ||
        ex.ErrorStatus is "PERMISSION_DENIED" or "NOT_FOUND" or "FAILED_PRECONDITION" or "UNIMPLEMENTED";

    /// <summary>The service rejected the request because of the region/location.</summary>
    public static bool IsRegionError(ApiException ex)
    {
        var message = (ex.ErrorMessage ?? string.Empty) + " " + ex.Message;
        return ex.ErrorStatus == "FAILED_PRECONDITION" ||
               message.IndexOf("region", StringComparison.OrdinalIgnoreCase) >= 0 ||
               message.IndexOf("location", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    /// <summary>
    /// Runs <paramref name="action"/>; quota/billing errors (and, when <paramref name="skipWhenUnavailable"/> is
    /// true, feature-unavailable errors) skip the test with the API message instead of failing it.
    /// </summary>
    public static async Task<T> OrSkipAsync<T>(Func<Task<T>> action, string feature, bool skipWhenUnavailable = true)
    {
        try
        {
            return await action().ConfigureAwait(false);
        }
        catch (ApiException ex) when (IsQuotaOrBilling(ex))
        {
            Assert.Skip($"{feature}: the project cannot run this right now ({ex.ErrorCode} {ex.ErrorStatus}): {ex.ErrorMessage}");
            throw;
        }
        catch (ApiException ex) when (skipWhenUnavailable && IsFeatureUnavailable(ex))
        {
            Assert.Skip($"{feature} is not available for this project/region ({ex.ErrorCode} {ex.ErrorStatus}): {ex.ErrorMessage}");
            throw;
        }
    }

    /// <summary>Like <see cref="OrSkipAsync{T}"/> for actions without a result.</summary>
    public static Task OrSkipAsync(Func<Task> action, string feature, bool skipWhenUnavailable = true) =>
        OrSkipAsync(async () =>
        {
            await action().ConfigureAwait(false);
            return true;
        }, feature, skipWhenUnavailable);

    /// <summary>Best-effort cleanup that never masks the test outcome.</summary>
    public static async Task CleanupAsync(Func<Task> cleanup, ITestOutputHelper? output, string what)
    {
        try
        {
            await cleanup().ConfigureAwait(false);
            output?.WriteLine($"Cleaned up {what}.");
        }
#pragma warning disable CA1031
        catch (Exception ex)
        {
            output?.WriteLine($"WARNING: failed to clean up {what}: {ex.Message}");
        }
#pragma warning restore CA1031
    }

    /// <summary>A short unique suffix for resource names.</summary>
    public static string UniqueSuffix() => Guid.NewGuid().ToString("N").Substring(0, 10);
}
