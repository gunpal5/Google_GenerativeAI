using GenerativeAI.Clients;
using Shouldly;

namespace GenerativeAI.Tests.LiveTests;

/// <summary>
/// Live tests for <see cref="FileClient.RegisterFilesAsync"/> and <see cref="FileClient.DownloadFileAsync(string, CancellationToken)"/>.
/// Both need resources that cannot be created cheaply on demand, so they are gated on environment variables:
/// GCS_TEST_FILE_URI (a gs:// object readable by the ADC / access token in use) and GEMINI_GENERATED_FILE_NAME (a
/// <c>files/...</c> name of a model-generated file; only generated files can be downloaded).
/// </summary>
public class Files_LiveTests : TestBase
{
    public Files_LiveTests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Fact]
    public async Task RegisterFiles_FromGcsUri()
    {
        var platform = LiveTestHelpers.RequireGeminiPlatform();
        var uri = Environment.GetEnvironmentVariable("GCS_TEST_FILE_URI");
        Assert.SkipWhen(string.IsNullOrWhiteSpace(uri),
            "Set GCS_TEST_FILE_URI to a gs:// object readable by your Application Default Credentials to run this test.");
        var ct = TestContext.Current.CancellationToken;
        var client = new FileClient(platform);

        var response = await LiveTestHelpers.OrSkipAsync(() => client.RegisterFilesAsync(new[] { uri! },
            cancellationToken: ct), "Register files", skipWhenUnavailable: false);
        response.Files.ShouldNotBeNull();
        response.Files.ShouldNotBeEmpty();
        try
        {
            foreach (var file in response.Files)
            {
                Console?.WriteLine($"Registered {file.Name} ({file.MimeType}, {file.State})");
                file.Name.ShouldNotBeNullOrEmpty();
            }
        }
        finally
        {
            foreach (var file in response.Files.Where(f => !string.IsNullOrEmpty(f.Name)))
                await LiveTestHelpers.CleanupAsync(() => client.DeleteFileAsync(file.Name!, CancellationToken.None),
                    Console, $"registered file {file.Name}");
        }
    }

    [Fact]
    public async Task DownloadFile_GeneratedFile()
    {
        var platform = LiveTestHelpers.RequireGeminiPlatform();
        var name = Environment.GetEnvironmentVariable("GEMINI_GENERATED_FILE_NAME");
        Assert.SkipWhen(string.IsNullOrWhiteSpace(name),
            "Set GEMINI_GENERATED_FILE_NAME to the files/... name of a model-generated file to run this test (uploaded files cannot be downloaded).");
        var ct = TestContext.Current.CancellationToken;
        var client = new FileClient(platform);

        var bytes = await LiveTestHelpers.OrSkipAsync(() => client.DownloadFileAsync(name!, ct), "File download",
            skipWhenUnavailable: false);
        Console?.WriteLine($"Downloaded {bytes.Length} bytes from {name}");
        bytes.Length.ShouldBeGreaterThan(0);
    }
}
