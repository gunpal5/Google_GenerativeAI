using System.Diagnostics.CodeAnalysis;
using GenerativeAI.Clients;
using GenerativeAI.SemanticRetrieval.Tests;
using GenerativeAI.Tests.Base;
using GenerativeAI.Types;
using Shouldly;
using Xunit;

namespace GenerativeAI.Tests.Clients.SemanticRetrieval;

[TestCaseOrderer(typeof(PriorityOrderer))]
    
public class ChunkClient_Tests : SemanticRetrieverTestBase
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
        var parent = await GetTestDocumentId();
        var newChunk = new Chunk
        {
            Data = new ChunkData { StringValue = "Test Data" },
            CustomMetadata = new List<CustomMetadata>
            {
                new CustomMetadata { Key = "author", StringValue = "John Doe" }
            }
        };

        // Act
        var result = await client.CreateChunkAsync(parent, newChunk).ConfigureAwait(false);

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
        var parent = await GetTestDocumentId();
        var chunkList = await client.ListChunksAsync(parent).ConfigureAwait(false);
        var testChunk = chunkList.Chunks.FirstOrDefault();
        var chunkName = testChunk.Name;

        // Act
        var result = await client.GetChunkAsync(chunkName).ConfigureAwait(false);

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
        var parent = await GetTestDocumentId();

        // Act
        var result = await client.ListChunksAsync(parent, pageSize).ConfigureAwait(false);

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
        var parent = await GetTestDocumentId();
        var chunkList = await client.ListChunksAsync(parent).ConfigureAwait(false);
        var testChunk = chunkList.Chunks.FirstOrDefault();
        testChunk.Data = new ChunkData { StringValue = "Updated Data" };
        const string updateMask = "data";

        // Act
        var result = await client.UpdateChunkAsync(testChunk, updateMask).ConfigureAwait(false);

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
        var parent = await GetTestDocumentId();
        var chunkList = await client.ListChunksAsync(parent).ConfigureAwait(false);
        var testChunk = chunkList.Chunks.LastOrDefault();

        // Act and Assert
        await Should.NotThrowAsync(async () => await client.DeleteChunkAsync(testChunk.Name).ConfigureAwait(false)).ConfigureAwait(false);
        Console.WriteLine($"Deleted Chunk: Name={testChunk.Name}");
    }

    [Fact, TestPriority(6)]
    public async Task ShouldBatchCreateChunksAsync()
    {
        // Arrange
        var client = new ChunkClient(GetTestGooglePlatform());
        var parent = await GetTestDocumentId();
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
                Parent = parent,
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
        var result = await client.BatchCreateChunksAsync(parent, requests).ConfigureAwait(false);

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
        var exception = await Should.ThrowAsync<Exception>(async () => await client.GetChunkAsync(invalidName).ConfigureAwait(false)).ConfigureAwait(false);

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
        var exception = await Should.ThrowAsync<Exception>(async () => await client.DeleteChunkAsync(invalidName).ConfigureAwait(false)).ConfigureAwait(false);

        // Assert
        exception.Message.ShouldNotBeNullOrEmpty();
        Console.WriteLine($"Handled Exception While Deleting Chunk: {exception.Message}");
    }

    private async Task<string> GetTestDocumentId()
    {
        var doc = await GetTestDocument();
        return doc.Name;
    }

    private async Task<Document> GetTestDocument()
    {
        var documentClient = new DocumentsClient(GetTestGooglePlatform());
        var testCorpus = await GetTestCorpora().ConfigureAwait(false);
        var parent = $"{testCorpus.Name}";
        var documentList = await documentClient.ListDocumentsAsync(parent).ConfigureAwait(false);
        var testDocument = documentList.Documents.FirstOrDefault();
        return testDocument;
    }
    private async Task<Corpus> GetTestCorpora()
    {
        var corpusClient = new CorporaClient(GetTestGooglePlatform());
        var corpus = await corpusClient.ListCorporaAsync().ConfigureAwait(false);
        
        if(corpus == null || corpus.Corpora == null || corpus.Corpora.Count == 0)
            throw new Exception("No Corpora Found");
        return corpus.Corpora.FirstOrDefault(s=>s.DisplayName.Contains("test", StringComparison.OrdinalIgnoreCase) && s.DisplayName.Contains("corpus", StringComparison.OrdinalIgnoreCase));
        
    }
}