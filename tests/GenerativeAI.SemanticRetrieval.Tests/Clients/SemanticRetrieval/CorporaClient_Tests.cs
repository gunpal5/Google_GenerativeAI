using System.Diagnostics.CodeAnalysis;
using GenerativeAI.Authenticators;
using GenerativeAI.Clients;
using GenerativeAI.Core;
using GenerativeAI.SemanticRetrieval.Tests;
using GenerativeAI.Tests.Base;
using GenerativeAI.Types;
using Shouldly;
using Xunit;
using Xunit.Sdk;

namespace GenerativeAI.Tests.Clients.SemanticRetrieval;


[TestCaseOrderer(
    typeof(PriorityOrderer))]
public class CorporaClient_Tests : SemanticRetrieverTestBase
{
    public CorporaClient_Tests(ITestOutputHelper output) : base(output)
    {
        Assert.SkipWhen(GitHubEnvironment(), "Github");
        Assert.SkipUnless(IsSemanticTestsEnabled, SemanticTestsDisabledMessage);
    }

    [Fact, TestPriority(1)]
    public async Task ShouldCreateCorpusAsync()
    {
        // Arrange
        var client = new CorporaClient(GetTestGooglePlatform());
        var newCorpus = new Corpus
        {
            DisplayName = "Test Corpus",
        };

        // Act
        var result = await client.CreateCorpusAsync(newCorpus).ConfigureAwait(false);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldNotBeNullOrEmpty();
        result.DisplayName.ShouldBe("Test Corpus");
        result.CreateTime.ShouldNotBeNull();

        Console.WriteLine($"Corpus Created: Name={result.Name}, DisplayName={result.DisplayName}, CreateTime={result.CreateTime}");
    }

    [Fact, TestPriority(2)]
    public async Task ShouldGetCorpusAsync()
    {
        // Arrange
        var client = new CorporaClient(GetTestGooglePlatform());
        var corporaList = await client.ListCorporaAsync().ConfigureAwait(false);
        var testCorpus = corporaList.Corpora.FirstOrDefault();
        var corpusName = testCorpus.Name;

        // Act
        var result = await client.GetCorpusAsync(corpusName).ConfigureAwait(false);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(corpusName);
        result.DisplayName.ShouldNotBeNullOrEmpty();
        result.CreateTime.ShouldNotBeNull();

        Console.WriteLine($"Retrieved Corpus: Name={result.Name}, DisplayName={result.DisplayName}");
    }

    [Fact, TestPriority(3)]
    public async Task ShouldListCorporaAsync()
    {
        // Arrange
        var client = new CorporaClient(GetTestGooglePlatform());
        const int pageSize = 10;

        // Act
        var result = await client.ListCorporaAsync(pageSize).ConfigureAwait(false);

        // Assert
        result.ShouldNotBeNull();
        result.Corpora.ShouldNotBeNull();
        result.Corpora.Count.ShouldBeGreaterThan(0);
        result.Corpora.Count.ShouldBeLessThanOrEqualTo(pageSize);

        foreach (var corpus in result.Corpora)
        {
            corpus.Name.ShouldNotBeNullOrEmpty();
            corpus.DisplayName.ShouldNotBeNullOrEmpty();
        }

        Console.WriteLine($"Listed {result.Corpora.Count} Corpora");
    }

    [Fact, TestPriority(4)]
    public async Task ShouldUpdateCorpusAsync()
    {
        // Arrange
        var client = new CorporaClient(GetTestGooglePlatform());
        var corporaList = await client.ListCorporaAsync().ConfigureAwait(false);
        var testCorpus = corporaList.Corpora.FirstOrDefault();
        var updatedCorpus = new Corpus
        {
            DisplayName = "Updated Corpus Name",
        };
        const string updateMask = "displayName";

        // Act
        var result = await client.UpdateCorpusAsync(testCorpus.Name, updatedCorpus, updateMask).ConfigureAwait(false);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(testCorpus.Name);
        result.DisplayName.ShouldBe("Updated Corpus Name");

        Console.WriteLine($"Updated Corpus: Name={result.Name}, DisplayName={result.DisplayName}");
    }

    [Fact, TestPriority(5)]
    public async Task ShouldDeleteCorpusAsync()
    {
        // Arrange
        var client = new CorporaClient(GetTestGooglePlatform());
        var corporaList = await client.ListCorporaAsync().ConfigureAwait(false);
        var testCorpus = corporaList.Corpora.LastOrDefault();

        // Act and Assert
        await Should.NotThrowAsync(async () => await client.DeleteCorpusAsync(testCorpus.Name).ConfigureAwait(false)).ConfigureAwait(false);
        Console.WriteLine($"Deleted Corpus: Name={testCorpus.Name}");
    }

    [Fact(Skip = "Need to work on this test sorry!"), TestPriority(6)]
    public async Task ShouldQueryCorpusAsync()
    {
        // Arrange
        var client = new CorporaClient(GetTestGooglePlatform());
        var corporaList = await client.ListCorporaAsync().ConfigureAwait(false);
        var testCorpus = corporaList.Corpora.FirstOrDefault();
        var queryRequest = new QueryCorpusRequest
        {
            Query = "Test Query",
            ResultsCount = 5
        };
        
        // Act
        var result = await client.QueryCorpusAsync(testCorpus.Name, queryRequest).ConfigureAwait(false);

        // Assert
        result.ShouldNotBeNull();
        result.RelevantChunks.ShouldNotBeNull();
        result.RelevantChunks.Count.ShouldBeLessThanOrEqualTo(5);

        Console.WriteLine($"Queried Corpus: Name={testCorpus.Name}, Retrieved {result.RelevantChunks.Count} Relevant Chunks");
    }

    [Fact, TestPriority(7)]
    public async Task ShouldHandleInvalidCorpusForRetrieveAsync()
    {
        // Arrange
        var client = new CorporaClient(GetTestGooglePlatform());
        const string invalidName = "corpora/invalid-id";

        // Act
        var exception = await Should.ThrowAsync<Exception>(async () => await client.GetCorpusAsync(invalidName).ConfigureAwait(false)).ConfigureAwait(false);

        // Assert
        exception.Message.ShouldNotBeNullOrEmpty();
        Console.WriteLine($"Handled Exception While Retrieving Corpus: {exception.Message}");
    }

    [Fact, TestPriority(8)]
    public async Task ShouldHandleInvalidCorpusForDeleteAsync()
    {
        // Arrange
        var client = new CorporaClient(GetTestGooglePlatform());
        const string invalidName = "corpora/invalid-id";

        // Act
        var exception = await Should.ThrowAsync<Exception>(async () => await client.DeleteCorpusAsync(invalidName).ConfigureAwait(false)).ConfigureAwait(false);

        // Assert
        exception.Message.ShouldNotBeNullOrEmpty();
        Console.WriteLine($"Handled Exception While Deleting Corpus: {exception.Message}");
    }

   
}