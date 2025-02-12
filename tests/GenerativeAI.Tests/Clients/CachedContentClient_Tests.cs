using System.Net;
using GenerativeAI.Tests.Base;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace GenerativeAI.Tests.Clients;

[TestCaseOrderer(
    ordererTypeName: "GenerativeAI.Tests.Base.PriorityOrderer",
    ordererAssemblyName: "GenerativeAI.Tests")]

public class CachingClient_Tests : TestBase
{
    public CachingClient_Tests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Fact,TestPriority(1)]
    public async Task ShouldCreateCachedContentAsync()
    {
        // Arrange
        var httpClient = new WebClient();
        var file = httpClient.DownloadString("https://storage.googleapis.com/generativeai-downloads/data/a11.txt");
        var client = new CachingClient(GetTestGooglePlatform());
        var cachedContent = new CachedContent
        {
            DisplayName = "Test Cached Content",
            Model = "models/gemini-1.5-flash-001",
            Contents = new List<Content>
                { RequestExtensions.FormatGenerateContentInput(file), },
        };

        // Act
        var result = await client.CreateCachedContentAsync(cachedContent);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldNotBeNullOrEmpty();
        result.Model.ShouldBe("models/gemini-1.5-flash-001");
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
        
        var client = new CachingClient(GetTestGooglePlatform());
        var cachedItems = await client.ListCachedContentsAsync();
        var testItem = cachedItems.CachedContents.FirstOrDefault();
        string cachedContentName = testItem.Name; // Replace with a valid test name

        // Act
        var result = await client.GetCachedContentAsync(cachedContentName);

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
        var client = new CachingClient(GetTestGooglePlatform());
        const int pageSize = 5;

        // Act
        var result = await client.ListCachedContentsAsync(pageSize);

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
        var client = new CachingClient(GetTestGooglePlatform());
        var cachedItems = await client.ListCachedContentsAsync();
        var testItem = cachedItems.CachedContents.FirstOrDefault();
        var updatedContent = new CachedContent
        {
            //Name = testItem.Name,
            DisplayName = "Updated Cached Content",
            Ttl = new Duration { Seconds = 3600 }, // Update expiration TTL
        };
        const string updateMask = "ttl";

        // Act
        var result = await client.UpdateCachedContentAsync(testItem.Name,updatedContent, updateMask);

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
        var client = new CachingClient(GetTestGooglePlatform());
        var cachedItems = await client.ListCachedContentsAsync();
        var testItem = cachedItems.CachedContents.FirstOrDefault();

        string cachedContentName = testItem.Name; // Replace with valid test data

        // Act and Assert
        await Should.NotThrowAsync(async () => await client.DeleteCachedContentAsync(cachedContentName));

        Console.WriteLine($"Cached Content Deleted: {cachedContentName}");
    }

    [Fact,TestPriority(6)]
    public async Task ShouldHandleInvalidCachedContentForRetrieveAsync()
    {
        // Arrange
        var client = new CachingClient(GetTestGooglePlatform());
        const string invalidName = "cachedContents/invalid-id";

        // Act
        var exception = await Should.ThrowAsync<Exception>(async () => await client.GetCachedContentAsync(invalidName));

        // Assert
        exception.Message.ShouldNotBeNullOrEmpty();
        Console.WriteLine($"Handled exception while retrieving Cached Content: {exception.Message}");
    }

    [Fact,TestPriority(6)]
    public async Task ShouldHandleInvalidCachedContentForDeleteAsync()
    {
        // Arrange
        var client = new CachingClient(GetTestGooglePlatform());
        const string invalidName = "cachedContents/invalid-id";

        // Act
        var exception =
            await Should.ThrowAsync<Exception>(async () => await client.DeleteCachedContentAsync(invalidName));

        // Assert
        exception.Message.ShouldNotBeNullOrEmpty();
        Console.WriteLine($"Handled exception while deleting Cached Content: {exception.Message}");
    }
}