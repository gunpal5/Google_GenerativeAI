using System.Diagnostics.CodeAnalysis;
using GenerativeAI.Clients;
using GenerativeAI.Tests.Base;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Clients.SemanticRetrieval;

[TestCaseOrderer(typeof(PriorityOrderer))]
    
public class ChunkClient_Tests : TestBase
{
    
    public ChunkClient_Tests(ITestOutputHelper output) : base(output)
    {
        Assert.SkipWhen(GitHubEnvironment(), "Github Environment");
        Assert.SkipUnless(IsSemanticTestsEnabled, SemanticTestsDisabledMessage);
    }

    [Fact, TestPriority(1)]
    public async Task ShouldCreateChunkAsync()
    {
        // Arrange
        var client = new ChunkClient(GetTestGooglePlatform());
        var parent = "corpora/test-corpus-id/documents/test-doc-id";
        var newChunk = new Chunk
        {
            Data = new ChunkData { StringValue = "Test Data" },
            CustomMetadata = new List<CustomMetadata>
            {
                new CustomMetadata { Key = "author", StringValue = "John Doe" }
            }
        };

        // Act
        var result = await client.CreateChunkAsync(parent, newChunk);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldNotBeNullOrEmpty();
        result.Data.ShouldNotBeNull();
        result.Data.StringValue.ShouldBe("Test Data");

        Console.WriteLine($"Chunk Created: Name={result.Name}, Data={result.Data.StringValue}");
    }

    [Fact, TestPriority(2)]
    public async Task ShouldGetChunkAsync()
    {
        // Arrange
        var client = new ChunkClient(GetTestGooglePlatform());
        var parent = "corpora/test-corpus-id/documents/test-doc-id";
        var chunkList = await client.ListChunksAsync(parent);
        var testChunk = chunkList.Chunks.FirstOrDefault();
        var chunkName = testChunk.Name;

        // Act
        var result = await client.GetChunkAsync(chunkName);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(chunkName);
        result.Data.ShouldNotBeNull();
        result.Data.StringValue.ShouldNotBeNullOrEmpty();

        Console.WriteLine($"Retrieved Chunk: Name={result.Name}, Data={result.Data.StringValue}");
    }

    [Fact, TestPriority(3)]
    public async Task ShouldListChunksAsync()
    {
        // Arrange
        var client = new ChunkClient(GetTestGooglePlatform());
        const int pageSize = 10;
        var parent = "corpora/test-corpus-id/documents/test-doc-id";

        // Act
        var result = await client.ListChunksAsync(parent, pageSize);

        // Assert
        result.ShouldNotBeNull();
        result.Chunks.ShouldNotBeNull();
        result.Chunks.Count.ShouldBeGreaterThan(0);
        result.Chunks.Count.ShouldBeLessThanOrEqualTo(pageSize);

        foreach (var chunk in result.Chunks)
        {
            chunk.Name.ShouldNotBeNullOrEmpty();
            chunk.Data.ShouldNotBeNull();
            chunk.Data.StringValue.ShouldNotBeNullOrEmpty();
        }

        Console.WriteLine($"Listed {result.Chunks.Count} Chunks");
    }

    [Fact, TestPriority(4)]
    public async Task ShouldUpdateChunkAsync()
    {
        // Arrange
        var client = new ChunkClient(GetTestGooglePlatform());
        var parent = "corpora/test-corpus-id/documents/test-doc-id";
        var chunkList = await client.ListChunksAsync(parent);
        var testChunk = chunkList.Chunks.FirstOrDefault();
        testChunk.Data = new ChunkData { StringValue = "Updated Data" };
        const string updateMask = "data";

        // Act
        var result = await client.UpdateChunkAsync(testChunk, updateMask);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(testChunk.Name);
        result.Data.ShouldNotBeNull();
        result.Data.StringValue.ShouldBe("Updated Data");

        Console.WriteLine($"Updated Chunk: Name={result.Name}, Data={result.Data.StringValue}");
    }

    [Fact, TestPriority(5)]
    public async Task ShouldDeleteChunkAsync()
    {
        // Arrange
        var client = new ChunkClient(GetTestGooglePlatform());
        var parent = "corpora/test-corpus-id/documents/test-doc-id";
        var chunkList = await client.ListChunksAsync(parent);
        var testChunk = chunkList.Chunks.LastOrDefault();

        // Act and Assert
        await Should.NotThrowAsync(async () => await client.DeleteChunkAsync(testChunk.Name));
        Console.WriteLine($"Deleted Chunk: Name={testChunk.Name}");
    }

    [Fact, TestPriority(6)]
    public async Task ShouldBatchCreateChunksAsync()
    {
        // Arrange
        var client = new ChunkClient(GetTestGooglePlatform());
        var parent = "corpora/test-corpus-id/documents/test-doc-id";
        var requests = new List<CreateChunkRequest>
        {
            new CreateChunkRequest
            {
                Chunk = new Chunk
                {
                    Data = new ChunkData { StringValue = "Batch Chunk 1" },
                    CustomMetadata = new List<CustomMetadata>
                    {
                        new CustomMetadata { Key = "type", StringValue = "batch" }
                    }
                }
            },
            new CreateChunkRequest
            {
                Chunk = new Chunk
                {
                    Data = new ChunkData { StringValue = "Batch Chunk 2" },
                    CustomMetadata = new List<CustomMetadata>
                    {
                        new CustomMetadata { Key = "type", StringValue = "batch" }
                    }
                }
            }
        };

        // Act
        var result = await client.BatchCreateChunksAsync(parent, requests);

        // Assert
        result.ShouldNotBeNull();
        result.Chunks.ShouldNotBeNull();
        result.Chunks.Count.ShouldBe(requests.Count);

        foreach (var chunk in result.Chunks)
        {
            chunk.Name.ShouldNotBeNullOrEmpty();
            chunk.Data.ShouldNotBeNull();
        }

        Console.WriteLine($"Batch Created {result.Chunks.Count} Chunks");
    }

    [Fact, TestPriority(7)]
    public async Task ShouldHandleInvalidChunkForRetrieveAsync()
    {
        // Arrange
        var client = new ChunkClient(GetTestGooglePlatform());
        const string invalidName = "corpora/test-corpus-id/documents/test-doc-id/chunks/invalid-id";

        // Act
        var exception = await Should.ThrowAsync<Exception>(async () => await client.GetChunkAsync(invalidName));

        // Assert
        exception.Message.ShouldNotBeNullOrEmpty();
        Console.WriteLine($"Handled Exception While Retrieving Chunk: {exception.Message}");
    }

    [Fact, TestPriority(8)]
    public async Task ShouldHandleInvalidChunkForDeleteAsync()
    {
        // Arrange
        var client = new ChunkClient(GetTestGooglePlatform());
        const string invalidName = "corpora/test-corpus-id/documents/test-doc-id/chunks/invalid-id";

        // Act
        var exception = await Should.ThrowAsync<Exception>(async () => await client.DeleteChunkAsync(invalidName));

        // Assert
        exception.Message.ShouldNotBeNullOrEmpty();
        Console.WriteLine($"Handled Exception While Deleting Chunk: {exception.Message}");
    }
}