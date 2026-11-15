using System.Net;
using GenerativeAI.Clients;
using GenerativeAI.Tests.Base;
using GenerativeAI.Types;
using Shouldly;
using Xunit;

namespace GenerativeAI.Tests.Clients;

[TestCaseOrderer(
    typeof(PriorityOrderer))]

public class CachingClient_Tests : TestBase
{
    public CachingClient_Tests(ITestOutputHelper helper) : base(helper)
    {
        Assert.SkipWhen(GitHubEnvironment(), "Github");
    }

    [Fact,TestPriority(1)]
    public async Task ShouldCreateCachedContentAsync()
    {
        // Arrange
        var client = CreateCachingClient();

        // Act
        var result = await CreateTestCacheAsync(client);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldNotBeNullOrEmpty();
        result.Model.ShouldBe(GoogleAIModels.Gemini38Flash);
        //result.DisplayName.ShouldBe("Test Cached Content");
        result.CreateTime.ShouldNotBeNull();
        result.UsageMetadata.ShouldNotBeNull();

        Console.WriteLine(
            $"Cached Content Created: {result.Name}, Model: {result.Model}, DisplayName: {result.DisplayName}");
    }

    [Fact,TestPriority(2)]
    public async Task ShouldGetCachedContentAsync()
    {
        // Arrange
        
        var client = CreateCachingClient();
        var cachedContentName = (await GetOrCreateTestCacheAsync(client)).Name!;

        // Act
        var result = await client.GetCachedContentAsync(cachedContentName, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(cachedContentName);
        result.Model.ShouldNotBeNullOrEmpty();
        //result.DisplayName.ShouldNotBeNullOrEmpty();
        result.CreateTime.ShouldNotBeNull();
        result.ExpireTime.ShouldNotBeNull();

        Console.WriteLine(
            $"Cached Content Retrieved: {result.Name}, Model: {result.Model}, DisplayName: {result.DisplayName}");
    }

    [Fact,TestPriority(3)]
    public async Task ShouldListCachedContentsAsync()
    {
        // Arrange
        var client = CreateCachingClient();
        const int pageSize = 5;
        await GetOrCreateTestCacheAsync(client); // make sure there is something to list

        // Act
        var result = await client.ListCachedContentsAsync(pageSize, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.CachedContents.ShouldNotBeNull();
        result.CachedContents.Count.ShouldBeGreaterThan(0);
        result.CachedContents.Count.ShouldBeLessThanOrEqualTo(pageSize);

        foreach (var cachedContent in result.CachedContents)
        {
            cachedContent.Name.ShouldNotBeNullOrEmpty();
            //cachedContent.DisplayName.ShouldNotBeNullOrEmpty();
            cachedContent.Model.ShouldNotBeNullOrEmpty();
        }

        Console.WriteLine($"Listed {result.CachedContents.Count} Cached Content Items");
    }

    [Fact,TestPriority(4)]
    public async Task ShouldUpdateCachedContentAsync()
    {
        // Arrange
        var client = CreateCachingClient();
        var testItem = await GetOrCreateTestCacheAsync(client);
        var updatedContent = new CachedContent
        {
            //Name = testItem.Name,
            DisplayName = "Updated Cached Content",
            Ttl = new Duration { Seconds = 3600 }, // Update expiration TTL
        };
        const string updateMask = "ttl";

        // Act
        var result = await client.UpdateCachedContentAsync(testItem.Name,updatedContent, updateMask, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(testItem.Name);
        // result.Ttl.ShouldNotBeNull();
        // result.Ttl.Seconds.ShouldBe(3600);

        Console.WriteLine($"Cached Content Updated: {result.Name}");
    }

    [Fact,TestPriority(5)]
    public async Task ShouldDeleteCachedContentAsync()
    {
        // Arrange
        // Delete a cache this test created itself, so parallel runs (other target frameworks) keep theirs.
        var client = CreateCachingClient();
        var cachedContentName = (await CreateTestCacheAsync(client)).Name!;

        // Act and Assert
        await Should.NotThrowAsync(async () => await client.DeleteCachedContentAsync(cachedContentName, cancellationToken: TestContext.Current.CancellationToken));

        Console.WriteLine($"Cached Content Deleted: {cachedContentName}");
    }

    [Fact,TestPriority(6)]
    public async Task ShouldHandleInvalidCachedContentForRetrieveAsync()
    {
        // Arrange
         var client = CreateCachingClient();
        const string invalidName = "cachedContents/invalid-id";

        // Act
        var exception = await Should.ThrowAsync<Exception>(async () => await client.GetCachedContentAsync(invalidName, cancellationToken: TestContext.Current.CancellationToken));

        // Assert
        exception.Message.ShouldNotBeNullOrEmpty();
        Console.WriteLine($"Handled exception while retrieving Cached Content: {exception.Message}");
    }

    [Fact,TestPriority(6)]
    public async Task ShouldHandleInvalidCachedContentForDeleteAsync()
    {
        // Arrange
         var client = CreateCachingClient();
        const string invalidName = "cachedContents/invalid-id";

        // Act
        var exception =
            await Should.ThrowAsync<Exception>(async () => await client.DeleteCachedContentAsync(invalidName, cancellationToken: TestContext.Current.CancellationToken));

        // Assert
        exception.Message.ShouldNotBeNullOrEmpty();
        Console.WriteLine($"Handled exception while deleting Cached Content: {exception.Message}");
    }

    private const string TestCacheDisplayName = "Test Cached Content";

    /// <summary>
    /// Creates a small cached content (~10k tokens, 5 minute TTL) so tests do not depend on each other.
    /// </summary>
    private static async Task<CachedContent> CreateTestCacheAsync(CachingClient client)
    {
        var ct = TestContext.Current.CancellationToken;
        using var httpClient = new HttpClient();
        var file = await httpClient.GetStringAsync("https://storage.googleapis.com/generativeai-downloads/data/a11.txt", ct);
        var text = file.Length > 40_000 ? file.Substring(0, 40_000) : file;

        var created = await client.CreateCachedContentAsync(new CachedContent
        {
            DisplayName = TestCacheDisplayName,
            Model = GoogleAIModels.Gemini38Flash,
            Contents = new List<Content> { RequestExtensions.FormatGenerateContentInput(text) },
            Ttl = new Duration { Seconds = 300 }
        }, cancellationToken: ct);
        created.ShouldNotBeNull();
        created.Name.ShouldNotBeNullOrEmpty();
        return created;
    }

    /// <summary>Returns an existing cached content, or creates one when the account has none.</summary>
    private static async Task<CachedContent> GetOrCreateTestCacheAsync(CachingClient client)
    {
        var list = await client.ListCachedContentsAsync(cancellationToken: TestContext.Current.CancellationToken);
        var existing = list?.CachedContents?.FirstOrDefault(c => !string.IsNullOrEmpty(c.Name));
        return existing ?? await CreateTestCacheAsync(client);
    }

    public CachingClient CreateCachingClient()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);
        return new CachingClient(GetTestGooglePlatform());
    }
}