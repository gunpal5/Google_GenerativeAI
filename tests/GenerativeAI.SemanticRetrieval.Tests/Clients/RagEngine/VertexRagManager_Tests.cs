using GenerativeAI.Authenticators;
using GenerativeAI.Core;
using GenerativeAI.SemanticRetrieval.Tests;
using GenerativeAI.Tests.Base;
using GenerativeAI.Types.RagEngine;
using Shouldly;
using Xunit;

namespace GenerativeAI.Tests.Clients.RagEngine;

[TestCaseOrderer(
    typeof(PriorityOrderer))]
public class VertexRagManager_Tests : SemanticRetrieverTestBase
{
    public VertexRagManager_Tests(ITestOutputHelper output) : base(output)
    {
        Assert.SkipWhen(SkipVertexAITests, VertextTestSkipMesaage);
        Assert.SkipUnless(IsSemanticTestsEnabled, SemanticTestsDisabledMessage);
    }

    [Fact, TestPriority(1)]
    public async Task ShouldCreateCorpusWithDefaultStore()
    {
        // Arrange
        var client = new VertexRagManager(GetTestVertexAIPlatform(), null);


        // Act
        var result = await client.CreateCorpusAsync(
            "test-corpus-default",
            "test corpus description",
            cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(true);

        //await client.AwaitForCreation(result.Name);
        // Assert
        result.ShouldNotBeNull();
        result.DisplayName.ShouldNotBeNullOrEmpty();
        result.DisplayName.ShouldBe("test-corpus-default");

        Console.WriteLine($"Corpus Created: Name={result.DisplayName}, DisplayName={result.DisplayName}");
    }

    [Fact, TestPriority(1)]
    public async Task ShouldCreateCorpusWithPineconeAsync()
    {
        // Requires a Pinecone index and a Secret Manager secret (readable by the Vertex AI RAG service agent)
        // holding the Pinecone API key.
        var secretVersion = Environment.GetEnvironmentVariable("PINECONE_API_KEY_SECRET_VERSION");
        var indexName = Environment.GetEnvironmentVariable("PINECONE_INDEX_NAME");
        Assert.SkipWhen(string.IsNullOrWhiteSpace(secretVersion) || string.IsNullOrWhiteSpace(indexName),
            "Set PINECONE_API_KEY_SECRET_VERSION (projects/{p}/secrets/{s}/versions/{v}) and PINECONE_INDEX_NAME to run the Pinecone corpus test.");

        // Arrange
        var client = new VertexRagManager(GetTestVertexAIPlatform(), null);

        // Act
        var result = await client.CreateCorpusAsync(
                displayName: "test-corpus-pinecone",
                description: "test corpus description",
                pineconeConfig: new RagVectorDbConfigPinecone()
                {
                    IndexName = indexName
                },
                apiKeyResourceName: secretVersion,
                cancellationToken: TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldNotBeNullOrEmpty();
        result.DisplayName.ShouldBe("test-corpus-pinecone");
        result.CreateTime.ShouldNotBeNull();

        Console.WriteLine(
            "Corpus Created: Name={result.Name}, DisplayName={result.DisplayName}, CreateTime={result.CreateTime}");
    }
    //
    // [Fact, TestPriority(2)]
    // public async Task ShouldCreateCorpusWithWeaviateAsync()
    // {
    //     // Arrange
    //     var client = new VertexRagManager(GetTestVertexAIPlatform(), null);
    //     var newCorpus = new RagCorpus
    //     {
    //         DisplayName = "Test Corpus",
    //     };
    //
    //     // Act
    //     var result = await client.CreateCorpusAsync(
    //         displayName: "test-corpus-weaviate",
    //         description: "test corpus description",
    //         weaviateConfig: new WeaviateConfig
    //         {
    //             Endpoint = "test-endpoint",
    //             Project = "test-project"
    //         },
    //         apiKeyResourceName: "projects/generative-ai-test-398714/secrets/weaviate-key/versions/1")
    //         .ConfigureAwait(true);
    //
    //     // Assert
    //     result.ShouldNotBeNull();
    //     result.Name.ShouldNotBeNullOrEmpty();
    //     result.DisplayName.ShouldBe("test-corpus-weaviate");
    //     result.CreateTime.ShouldNotBeNull();
    //
    //     Console.WriteLine($\"Corpus Created: Name={result.Name}, DisplayName={result.DisplayName}, CreateTime={result.CreateTime}\");
    // }
    //
    // [Fact, TestPriority(3)]
    // public async Task ShouldCreateCorpusWithVertexFeatureStoreAsync()
    // {
    //     // Arrange
    //     var client = new VertexRagManager(GetTestVertexAIPlatform(), null);
    //     var newCorpus = new RagCorpus
    //     {
    //         DisplayName = "Test Corpus",
    //     };
    //
    //     // Act
    //     var result = await client.CreateCorpusAsync(
    //         displayName: "test-corpus-vertex-feature-store",
    //         description: "test corpus description",
    //         vertexFeatureStoreConfig: new VertexFeatureStoreConfig
    //         {
    //             Project = "test-project",
    //             Location = "test-location",
    //             FeatureStore = "test-feature-store",
    //             EntitySetName = "test-entity-set-name",
    //             VectorFieldName = "test-vector-field-name"
    //         })
    //         .ConfigureAwait(true);
    //
    //     // Assert
    //     result.ShouldNotBeNull();
    //     result.Name.ShouldNotBeNullOrEmpty();
    //     result.DisplayName.ShouldBe("test-corpus-vertex-feature-store");
    //     result.CreateTime.ShouldNotBeNull();
    //
    //     Console.WriteLine($\"Corpus Created: Name={result.Name}, DisplayName={result.DisplayName}, CreateTime={result.CreateTime}\");
    // }
    //
    // [Fact, TestPriority(4)]
    // public async Task ShouldCreateCorpusWithVertexVectorSearchAsync()
    // {
    //     // Arrange
    //     var client = new VertexRagManager(GetTestVertexAIPlatform(), null);
    //     var newCorpus = new RagCorpus
    //     {
    //         DisplayName = "Test Corpus",
    //     };
    //
    //     // Act
    //     var result = await client.CreateCorpusAsync(
    //         displayName: "test-corpus-vertex-vector-search",
    //         description: "test corpus description",
    //         vertexSearchStoreConfig: new VertexVectorSearchConfig
    //         {
    //             Project = "test-project",
    //             Location = "test-location",
    //             Cluster = "test-cluster",
    //             Index = "test-index"
    //         })
    //         .ConfigureAwait(true);
    //
    //     // Assert
    //     result.ShouldNotBeNull();
    //     result.Name.ShouldNotBeNullOrEmpty();
    //     result.DisplayName.ShouldBe("test-corpus-vertex-vector-search");
    //     result.CreateTime.ShouldNotBeNull();
    //
    //     Console.WriteLine($\"Corpus Created: Name={result.Name}, DisplayName={result.DisplayName}, CreateTime={result.CreateTime}\");
    // }

    // [Fact, TestPriority(5)]
    // public async Task ShouldGetCorpusAsync()
    // {
    //     // Arrange
    //     var client = new VertexRagManager(GetTestVertexAIPlatform(), null);
    //     var corpusName = "test-corpus-pinecone";
    //     
    //     // Act
    //     var result = await client.GetRagCorpusAsync(corpusName,cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(true);
    //     
    //     // Assert
    //     result.ShouldNotBeNull();
    //     result.Name.ShouldNotBeNullOrEmpty();
    //     result.DisplayName.ShouldBe(corpusName);
    //     result.CreateTime.ShouldNotBeNull();
    //     
    //     Console.WriteLine("Corpus Retrieved: Name={result.Name}, DisplayName={result.DisplayName}, CreateTime={result.CreateTime}");
    // }

    [Fact, TestPriority(6)]
    public async Task ShouldListCorporaAsync()
    {
        // Arrange
        var client = new VertexRagManager(GetTestVertexAIPlatform(), null);

        // Act - newly created corpora can take a while to appear in the list (Serverless mode), so poll briefly.
        var result = await ListCorporaWithRetryAsync(client).ConfigureAwait(true);

        // Assert
        result.ShouldNotBeNull();
        result.RagCorpora.ShouldNotBeNull();
        result.RagCorpora.Count.ShouldBeGreaterThan(0);

        Console.WriteLine($"Number of Corpora: {result.RagCorpora.Count}");
    }

    // [Fact, TestPriority(6)]
    // public async Task ShouldUpdateCorpusAsync()
    // {
    //     // Arrange
    //     var client = new VertexRagManager(GetTestVertexAIPlatform(), null);
    //
    //     // Act
    //     var result = await client.ListRagCorporaAsync(,cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(true);
    //
    //     // Assert
    //     result.ShouldNotBeNull();
    //     result.RagCorpora.ShouldNotBeNull();
    //     result.RagCorpora.Count.ShouldBeGreaterThan(0);
    //
    //     var first = result.RagCorpora.FirstOrDefault(s=>s.DisplayName.Contains("test", StringComparison.OrdinalIgnoreCase));
    //
    //     var toUpdate = new RagCorpus();
    //     toUpdate.Description = "Updated Corpus Name 2";
    //     toUpdate.Name = first.Name;
    //     toUpdate.DisplayName = first.DisplayName;
    //     
    //     
    //     var updated = await client.UpdateCorpusAsync(toUpdate,cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(true);
    //     
    //     updated.Description.ShouldBe("Updated Corpus Name 2");
    //     
    //     Console.WriteLine($"Corpora updated: {updated.DisplayName}, ");
    // }

    //[Fact(Skip = "Not needed", Explicit = true), TestPriority(100)]
    [RunnableInDebugOnly]
    public async Task ShouldDeleteCorporaAsync()
    {
        // Arrange
        var client = new VertexRagManager(GetTestVertexAIPlatform(), null);

        var list = await client.ListCorporaAsync(cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(true);
        foreach (var l in list.RagCorpora ?? new List<RagCorpus>())
        {
            await client.DeleteRagCorpusAsync(l.Name,cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(true);
            
        }
        // var corpusName = list.RagCorpora
        //     .FirstOrDefault(s => s.DisplayName.Contains("test", StringComparison.OrdinalIgnoreCase)).Name;
        //
        // // Act
        // await client.DeleteRagCorpusAsync(corpusName,cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(true);

        // Assert
        // No exception should be thrown

        //Console.WriteLine($"Corpus Deleted: {corpusName}");
    }

    [Fact, TestPriority(7)]
    public async Task ShouldUploadLocalFileAsync()
    {
        // Arrange
        var client = new VertexRagManager(GetTestVertexAIPlatform(), null);

        var corpusName = await GetTestCorpusNameOrSkipAsync(client).ConfigureAwait(true);

        var file = "TestData/pg1184.txt";
        // Act - a freshly created corpus can briefly reject uploads, so retry a few times.
        RagFile? result = null;
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                result = await client.UploadLocalFileAsync(corpusName, file, "The Count of Monte Cristo",
                        "This ebook is for the use of anyone anywhere in the United States and\nmost other parts of the world at no cost and with almost no restrictions\nwhatsoever. You may copy it, give it away or re-use it under the terms\nof the Project Gutenberg License included with this ebook or online\nat www.gutenberg.org. If you are not located in the United States,\nyou will have to check the laws of the country where you are located\nbefore using this eBook.", cancellationToken: TestContext.Current.CancellationToken)
                    .ConfigureAwait(true);
                break;
            }
            catch (GenerativeAI.Exceptions.VertexAIException ex) when (attempt < 4)
            {
                Console.WriteLine($"Upload attempt {attempt} failed ({ex.Message}); retrying.");
                await Task.Delay(TimeSpan.FromSeconds(15), TestContext.Current.CancellationToken).ConfigureAwait(true);
            }
        }

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldNotBeNullOrEmpty();
        Console.WriteLine($"File uploaded: {result.Name} to {corpusName}");
    }

    //[Fact(Skip = "Not needed",Explicit = true), TestPriority(7)]
    [Fact, TestPriority(7)]
    public async Task ShouldImportFileAsync()
    {
        // Requires a Google Drive file shared with the Vertex AI RAG service agent.
        var driveFileId = Environment.GetEnvironmentVariable("RAG_IMPORT_DRIVE_FILE_ID");
        Assert.SkipWhen(string.IsNullOrWhiteSpace(driveFileId),
            "Set RAG_IMPORT_DRIVE_FILE_ID to a Google Drive file shared with the Vertex AI RAG service agent to run the import test.");

        // Arrange
        var client = new VertexRagManager(GetTestVertexAIPlatform(), null);

        var corpusName = await GetTestCorpusNameOrSkipAsync(client).ConfigureAwait(true);

        var request = new ImportRagFilesRequest();
        request.AddGooglDriveSource(new GoogleDriveSourceResourceId()
        {
            ResourceId = driveFileId,
            ResourceType = GoogleDriveSourceResourceIdResourceType.RESOURCE_TYPE_FILE
        });
        // Act
        var result = await client.ImportFilesAsync(corpusName, request, cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(true);

        result.Metadata.ShouldContainKey("importRagFilesConfig");
    }
    
    [Fact, TestPriority(7)]
    public async Task ShouldListFilesAsync()
    {
        // Arrange
        var client = new VertexRagManager(GetTestVertexAIPlatform(), null);

        var corpusName = await GetTestCorpusNameOrSkipAsync(client).ConfigureAwait(true);

       var files = await client.ListFilesAsync(corpusName, cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(true);
       files.ShouldNotBeNull();
       Assert.SkipWhen(files.RagFiles == null || files.RagFiles.Count == 0,
           $"Corpus {corpusName} has no files yet; ShouldUploadLocalFileAsync must succeed first.");
       files.RagFiles!.Count.ShouldBeGreaterThan(0);
    }
    
    [Fact, TestPriority(7)]
    public async Task ShouldGetFileAsync()
    {
        // Arrange
        var client = new VertexRagManager(GetTestVertexAIPlatform(), null);

        var corpusName = await GetTestCorpusNameOrSkipAsync(client).ConfigureAwait(true);

        var files = await client.ListFilesAsync(corpusName, cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(true);
        files.ShouldNotBeNull();
        Assert.SkipWhen(files.RagFiles == null || files.RagFiles.Count == 0,
            $"Corpus {corpusName} has no files yet; ShouldUploadLocalFileAsync must succeed first.");

        var last = files.RagFiles!.Last();

        var f = await client.GetFileAsync(last.Name!, cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(true);
        f.ShouldNotBeNull();
        f.Name.ShouldBe(last.Name);
    }
    
    //[Fact(Skip = "Not needed",Explicit = true), TestPriority(7)]
    [Fact, TestPriority(7)]
    public async Task ShouldQueryWithCorpusAsync()
    {
        // Arrange
        var vertexAi = new VertexAI(GetTestVertexAIPlatform());

        var client = vertexAi.CreateRagManager();

        var corpusName = await GetTestCorpusNameOrSkipAsync(client).ConfigureAwait(true);
        
        var model = vertexAi.CreateGenerativeModel(VertexRegionalTestModel,corpusIdForRag: corpusName);

        var result =await model.GenerateContentAsync("what does the marketing plan said about the youtube?", cancellationToken: TestContext.Current.CancellationToken);
    }
    
    [Fact(Skip = "Not needed", Explicit = true), TestPriority(7)]
    public async Task ShouldDeleteFileAsync()
    {
        // Arrange
        var client = new VertexRagManager(GetTestVertexAIPlatform(), null);

        var corpusName = await GetTestCorpusNameOrSkipAsync(client).ConfigureAwait(true);

        var files = await client.ListFilesAsync(corpusName, cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(true);
        files.ShouldNotBeNull();
        files.RagFiles.Count.ShouldBeGreaterThan(0);

        var last = files.RagFiles.LastOrDefault();
        
        await client.DeleteFileAsync(last.Name, cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(true);
    }


    /// <summary>
    /// Lists corpora, retrying for up to a minute while the list is empty: in Serverless mode a newly created
    /// corpus can take a while to appear.
    /// </summary>
    private static async Task<ListRagCorporaResponse?> ListCorporaWithRetryAsync(VertexRagManager client)
    {
        var ct = TestContext.Current.CancellationToken;
        ListRagCorporaResponse? list = null;
        for (var attempt = 0; attempt < 12; attempt++)
        {
            list = await client.ListCorporaAsync(cancellationToken: ct).ConfigureAwait(true);
            if (list?.RagCorpora is { Count: > 0 })
                break;
            await Task.Delay(TimeSpan.FromSeconds(5), ct).ConfigureAwait(true);
        }

        return list;
    }

    /// <summary>
    /// Returns the name of a corpus whose display name contains "test" (created by the priority-1 tests),
    /// or skips the test when there is none.
    /// </summary>
    private static async Task<string> GetTestCorpusNameOrSkipAsync(VertexRagManager client)
    {
        var list = await ListCorporaWithRetryAsync(client).ConfigureAwait(true);
        var corpus = list?.RagCorpora?.FirstOrDefault(c =>
            c.DisplayName?.Contains("test", StringComparison.OrdinalIgnoreCase) == true);
        Assert.SkipWhen(corpus?.Name == null,
            "No test corpus exists; ShouldCreateCorpusWithDefaultStore must succeed first.");
        return corpus!.Name!;
    }

    // The RAG Engine is not available on the global endpoint, so always use a regional one.
    protected override IPlatformAdapter GetTestVertexAIPlatform()
    {
        var platform = GetTestVertexAIRegionalPlatform();
        var jsonFile = Environment.GetEnvironmentVariable("Google_Service_Account_Json");
        platform.SetAuthenticator(new GoogleServiceAccountAuthenticator(jsonFile));

        return platform;
    }
}