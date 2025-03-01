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
            "test corpus description"
        ).ConfigureAwait(false);

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
        // Arrange
        var client = new VertexRagManager(GetTestVertexAIPlatform(), null);
        var newCorpus = new RagCorpus
        {
            DisplayName = "Test Pinecone Corpus",
        };

        // Act
        var result = await client.CreateCorpusAsync(
                displayName: "test-corpus-pinecone",
                description: "test corpus description",
                pineconeConfig: new RagVectorDbConfigPinecone()
                {
                    IndexName = "test-index-5"
                },
                apiKeyResourceName: Environment.GetEnvironmentVariable("pinecone-secret"))
            .ConfigureAwait(false);

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
    //         .ConfigureAwait(false);
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
    //         .ConfigureAwait(false);
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
    //         .ConfigureAwait(false);
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
    //     var result = await client.GetRagCorpusAsync(corpusName).ConfigureAwait(false);
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

        // Act
        var result = await client.ListCorporaAsync().ConfigureAwait(false);

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
    //     var result = await client.ListRagCorporaAsync().ConfigureAwait(false);
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
    //     var updated = await client.UpdateCorpusAsync(toUpdate).ConfigureAwait(false);
    //     
    //     updated.Description.ShouldBe("Updated Corpus Name 2");
    //     
    //     Console.WriteLine($"Corpora updated: {updated.DisplayName}, ");
    // }

    [Fact, TestPriority(100)]
    public async Task ShouldDeleteCorporaAsync()
    {
        // Arrange
        var client = new VertexRagManager(GetTestVertexAIPlatform(), null);

        var list = await client.ListCorporaAsync().ConfigureAwait(false);
        var corpusName = list.RagCorpora
            .FirstOrDefault(s => s.DisplayName.Contains("test", StringComparison.OrdinalIgnoreCase)).Name;

        // Act
        await client.DeleteRagCorpusAsync(corpusName).ConfigureAwait(false);

        // Assert
        // No exception should be thrown

        Console.WriteLine($"Corpus Deleted: {corpusName}");
    }

    [Fact, TestPriority(7)]
    public async Task ShouldUploadLocalFileAsync()
    {
        // Arrange
        var client = new VertexRagManager(GetTestVertexAIPlatform(), null);

        var list = await client.ListCorporaAsync().ConfigureAwait(false);
        var corpusName = list.RagCorpora
            .FirstOrDefault(s => s.DisplayName.Contains("test", StringComparison.OrdinalIgnoreCase)).Name;

        var file = "TestData/pg1184.txt";
        // Act
        var result = await client.UploadLocalFileAsync(corpusName, file, "The Count of Monte Cristo",
                "This ebook is for the use of anyone anywhere in the United States and\nmost other parts of the world at no cost and with almost no restrictions\nwhatsoever. You may copy it, give it away or re-use it under the terms\nof the Project Gutenberg License included with this ebook or online\nat www.gutenberg.org. If you are not located in the United States,\nyou will have to check the laws of the country where you are located\nbefore using this eBook.")
            .ConfigureAwait(false);

        // Assert
        // No exception should be thrown
        Console.WriteLine($"Corpus Deleted: {corpusName}");
    }

    [Fact(Skip = "Not needed",Explicit = true), TestPriority(7)]
    public async Task ShouldImportFileAsync()
    {
        // Arrange
        var client = new VertexRagManager(GetTestVertexAIPlatform(), null);

        var list = await client.ListCorporaAsync().ConfigureAwait(false);
        var corpusName = list.RagCorpora
            .FirstOrDefault(s => s.DisplayName.Contains("test", StringComparison.OrdinalIgnoreCase)).Name;

        var request = new ImportRagFilesRequest();
        // request.AddGooglDriveSource(new GoogleDriveSourceResourceId()
        // {
        //     ResourceId = "",
        //     ResourceType = GoogleDriveSourceResourceIdResourceType.RESOURCE_TYPE_FILE
        // });
        var file = "TestData/pg1184.txt";
        // Act
        var result = await client.ImportFilesAsync(corpusName, request).ConfigureAwait(false);

        result.Metadata.ShouldContainKey("importRagFilesConfig");
    }
    
    [Fact, TestPriority(7)]
    public async Task ShouldListFilesAsync()
    {
        // Arrange
        var client = new VertexRagManager(GetTestVertexAIPlatform(), null);

        var list = await client.ListCorporaAsync().ConfigureAwait(false);
        var corpusName = list.RagCorpora
            .FirstOrDefault(s => s.DisplayName.Contains("test", StringComparison.OrdinalIgnoreCase)).Name;

       var files = await client.ListFilesAsync(corpusName).ConfigureAwait(false);
       files.ShouldNotBeNull();
       files.RagFiles.Count.ShouldBeGreaterThan(0);
    }
    
    [Fact, TestPriority(7)]
    public async Task ShouldGetFileAsync()
    {
        // Arrange
        var client = new VertexRagManager(GetTestVertexAIPlatform(), null);

        var list = await client.ListCorporaAsync().ConfigureAwait(false);
        var corpusName = list.RagCorpora
            .FirstOrDefault(s => s.DisplayName.Contains("test", StringComparison.OrdinalIgnoreCase)).Name;

        var files = await client.ListFilesAsync(corpusName).ConfigureAwait(false);
        files.ShouldNotBeNull();
        files.RagFiles.Count.ShouldBeGreaterThan(0);

        var last = files.RagFiles.LastOrDefault();
        
        var f = await client.GetFileAsync(last.Name).ConfigureAwait(false);
    }
    
    [Fact(Skip = "Not needed",Explicit = true), TestPriority(7)]
    public async Task ShouldQueryWithCorpusAsync()
    {
        // Arrange
        var vertexAi = new VertexAI(GetTestVertexAIPlatform());

        var client = vertexAi.CreateRagManager();

        var list = await client.ListCorporaAsync().ConfigureAwait(false);
        var corpusName = list.RagCorpora
            .FirstOrDefault(s => s.DisplayName.Contains("test", StringComparison.OrdinalIgnoreCase)).Name;
        
        var model = vertexAi.CreateGenerativeModel(VertexAIModels.Gemini.Gemini2Flash,corpusIdForRag: corpusName);

        var result =await model.GenerateContentAsync("what does the marketing plan said about the youtube?");
    }
    
    [Fact(Skip = "Not needed", Explicit = true), TestPriority(7)]
    public async Task ShouldDeleteFileAsync()
    {
        // Arrange
        var client = new VertexRagManager(GetTestVertexAIPlatform(), null);

        var list = await client.ListCorporaAsync().ConfigureAwait(false);
        var corpusName = list.RagCorpora
            .FirstOrDefault(s => s.DisplayName.Contains("test", StringComparison.OrdinalIgnoreCase)).Name;

        var files = await client.ListFilesAsync(corpusName).ConfigureAwait(false);
        files.ShouldNotBeNull();
        files.RagFiles.Count.ShouldBeGreaterThan(0);

        var last = files.RagFiles.LastOrDefault();
        
        await client.DeleteFileAsync(last.Name).ConfigureAwait(false);
    }


    protected override IPlatformAdapter GetTestVertexAIPlatform()
    {
        var platform = base.GetTestVertexAIPlatform();
        var jsonFile = Environment.GetEnvironmentVariable("Google_Service_Account_Json");
        platform.SetAuthenticator(new GoogleServiceAccountAuthenticator(jsonFile));

        return platform;
    }
}