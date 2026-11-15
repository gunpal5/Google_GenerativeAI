using System.Text;
using GenerativeAI.Clients;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Clients;

/// <summary>
/// End-to-end File Search flow against the real Gemini API. Skipped unless GOOGLE_API_KEY or GEMINI_API_KEY is set.
/// </summary>
public class FileSearchStores_IntegrationTests
{
    private static string? ApiKey =>
        System.Environment.GetEnvironmentVariable("GOOGLE_API_KEY") ?? System.Environment.GetEnvironmentVariable("GEMINI_API_KEY");

    [Fact]
    public async Task FileSearch_EndToEnd()
    {
        Assert.SkipWhen(string.IsNullOrEmpty(ApiKey), "GOOGLE_API_KEY/GEMINI_API_KEY is not set.");
        var ct = TestContext.Current.CancellationToken;
        var googleAi = new GoogleAi(ApiKey);
        var client = googleAi.CreateFileSearchStoresClient();

        FileSearchStore store;
        try
        {
            store = await client.CreateFileSearchStoreAsync("sdk-integration-test", cancellationToken: ct);
        }
        catch (GenerativeAI.Exceptions.ApiException ex) when (ex.ErrorCode is 402 or 429)
        {
            Assert.Skip($"The API key's project cannot use File Search right now: {ex.ErrorMessage}");
            return;
        }

        store.Name.ShouldNotBeNullOrEmpty();
        try
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(
                "The secret launch code of Project Bluebird is AZURE-7741. Project Bluebird was started in 2019."));
            var operation = await client.UploadStreamToFileSearchStoreAsync(store.Name!, stream, "text/plain",
                new UploadToFileSearchStoreConfig
                {
                    DisplayName = "bluebird.txt",
                    CustomMetadata = new List<CustomMetadata> { new("project") { StringValue = "bluebird" } }
                }, cancellationToken: ct);
            operation = await client.WaitForOperationAsync(operation, timeout: TimeSpan.FromMinutes(5),
                cancellationToken: ct);
            operation.Response?.DocumentName.ShouldNotBeNullOrEmpty();

            var documents = await client.ListDocumentsAsync(store.Name!, cancellationToken: ct);
            documents.Documents.ShouldNotBeNull();
            documents.Documents.ShouldContain(d => d.DisplayName == "bluebird.txt");

            var document = await client.GetDocumentAsync(operation.Response!.DocumentName!, ct);
            document.CustomMetadata.ShouldNotBeNull();

            var model = googleAi.CreateGenerativeModel(GoogleAIModels.Gemini35FlashLite);
            model.UseFileSearch(store.Name!);
            var response = await model.GenerateContentAsync("What is the launch code of Project Bluebird?", ct);
            response.Text().ShouldContain("AZURE-7741");
        }
        finally
        {
            await client.DeleteFileSearchStoreAsync(store.Name!, force: true, CancellationToken.None);
        }
    }
}
