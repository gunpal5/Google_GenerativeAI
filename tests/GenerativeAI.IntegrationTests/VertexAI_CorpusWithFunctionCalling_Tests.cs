using System.ComponentModel;
using GenerativeAI.Core;
using GenerativeAI.Tests;
using GenerativeAI.Tools;
using GenerativeAI.Types;
using GenerativeAI.Types.RagEngine;
using Shouldly;

namespace GenerativeAI.IntegrationTests;

/// <summary>
/// Integration tests for Issue #102: Testing VertexAI with corpusIdForRag and function calling together.
///
/// This test suite verifies that:
/// 1. VertexAI works with corpusIdForRag alone
/// 2. VertexAI works with function tools alone
/// 3. VertexAI works with BOTH corpusIdForRag AND function tools together
///
/// Issue: https://github.com/gunpal5/Google_GenerativeAI/issues/102
/// </summary>
public class VertexAI_CorpusWithFunctionCalling_Tests : TestBase
{
    private const string DefaultTestModelName = VertexAIModels.Gemini.Gemini25Flash;
    private const string TestCorpusDisplayName = "Issue102-Test-Corpus";
    private const string TestDocumentDisplayName = "Issue102-Test-Document";

    public VertexAI_CorpusWithFunctionCalling_Tests(ITestOutputHelper output) : base(output)
    {
        Assert.SkipWhen(SkipVertexAITests, VertextTestSkipMesaage);
    }

    #region Helper Methods

    /// <summary>
    /// Creates a VertexAI instance using test platform configuration.
    /// </summary>
    private VertexAI CreateVertexAI()
    {
        var platform = GetTestVertexAIPlatform();
        return new VertexAI(platform);
    }

    /// <summary>
    /// Creates a simple test function tool for weather queries.
    /// </summary>
    private QuickTool CreateWeatherTool()
    {
        return new QuickTool(
            ([Description("The city name")] string city) =>
            {
                Console.WriteLine($"Weather tool called for city: {city}");
                return new Dictionary<string, object>
                {
                    { "city", city },
                    { "temperature", 22.5 },
                    { "condition", "Sunny" },
                    { "humidity", 65 }
                };
            },
            "GetWeather",
            "Get the current weather for a specified city"
        );
    }

    /// <summary>
    /// Creates a simple test function tool that returns a test string.
    /// This mimics the exact tool from Issue #102.
    /// </summary>
    private QuickTool CreateTestStringTool()
    {
        return new QuickTool(
            () =>
            {
                Console.WriteLine("GetTestString tool called");
                return new Dictionary<string, string>
                {
                    { "Foo", "alpha bravo charlie" }
                };
            },
            "GetTestString",
            "Gets a test string"
        );
    }

    /// <summary>
    /// Gets or creates the test RAG corpus for Issue #102 testing using Vertex AI RAG Engine.
    /// </summary>
    private async Task<RagCorpus?> GetOrCreateTestRagCorpusAsync(VertexRagManager ragManager)
    {
        // Check if corpus already exists
        var existingCorpora = await ragManager.ListCorporaAsync(cancellationToken: TestContext.Current.CancellationToken);
        var existingCorpus = existingCorpora?.RagCorpora?.FirstOrDefault(c => c.DisplayName == TestCorpusDisplayName);

        if (existingCorpus != null)
        {
            Console.WriteLine($"Test RAG corpus already exists: {existingCorpus.Name}");
            return existingCorpus;
        }

        // Create new RAG corpus
        Console.WriteLine("Creating new RAG corpus...");
        var result = await ragManager.CreateCorpusAsync(
            displayName: TestCorpusDisplayName,
            description: "Test corpus for Issue #102 - Testing VertexAI with corpusIdForRag and function calling",
            cancellationToken: TestContext.Current.CancellationToken);

        Console.WriteLine($"Test RAG corpus created: Name={result?.Name}, DisplayName={result?.DisplayName}");
        return result;
    }

    /// <summary>
    /// Creates a temporary test file with sample content for RAG upload.
    /// </summary>
    private string CreateTestDataFile()
    {
        var testContent = @"# Company Knowledge Base

## Product Catalog
The company's product catalog includes software development tools, cloud services, and AI-powered analytics platforms. Our flagship product is the Enterprise AI Suite which provides machine learning capabilities for business applications.

## Customer Support
Customer support is available 24/7 through multiple channels including phone, email, and live chat. For technical issues, customers can also access our comprehensive knowledge base and community forums.

## Pricing Structure
The pricing structure includes three tiers:
- Basic: $99/month
- Professional: $299/month
- Enterprise: custom pricing

All plans include core features with advanced capabilities in higher tiers.

## API Documentation
Our API documentation provides detailed guides for integrating with third-party applications. The REST API supports JSON format and includes authentication via API keys or OAuth 2.0.

## Development Roadmap
The development roadmap for 2024 includes enhanced AI capabilities, improved user interface, mobile application support, and expanded integration options with popular business tools.
";

        var tempFilePath = Path.Combine(Path.GetTempPath(), $"issue102_test_data_{Guid.NewGuid():N}.txt");
        File.WriteAllText(tempFilePath, testContent);
        return tempFilePath;
    }

    #endregion

    #region Corpus Setup Tests

    /// <summary>
    /// Creates and sets up the test RAG corpus with sample data for Issue #102 testing.
    /// Run this test first to set up the corpus.
    /// Uses Vertex AI RAG Engine API.
    /// </summary>
    [Fact]
    public async Task Setup_CreateTestCorpusWithData()
    {
        var vertexAI = CreateVertexAI();
        var ragManager = vertexAI.CreateRagManager();

        // Step 1: Create or get RAG corpus
        Console.WriteLine("Step 1: Setting up RAG corpus...");
        var corpus = await GetOrCreateTestRagCorpusAsync(ragManager);
        corpus.ShouldNotBeNull("RAG corpus should not be null");
        corpus.Name.ShouldNotBeNullOrEmpty("RAG corpus name should not be null");

        // Step 2: Upload test file to corpus
        Console.WriteLine("Step 2: Uploading test data to corpus...");
        var existingFiles = await ragManager.ListFilesAsync(corpus.Name!, cancellationToken: TestContext.Current.CancellationToken);

        if (existingFiles?.RagFiles?.Count > 0)
        {
            Console.WriteLine($"Test files already exist: {existingFiles.RagFiles.Count} files found");
        }
        else
        {
            // Create and upload test file
            var testFilePath = CreateTestDataFile();
            try
            {
                var uploadedFile = await ragManager.UploadLocalFileAsync(
                    corpus.Name!,
                    testFilePath,
                    displayName: TestDocumentDisplayName,
                    description: "Test data for Issue #102",
                    cancellationToken: TestContext.Current.CancellationToken);

                Console.WriteLine($"Test file uploaded: {uploadedFile?.Name}");
            }
            finally
            {
                // Clean up temp file
                if (File.Exists(testFilePath))
                    File.Delete(testFilePath);
            }
        }

        // Verify setup
        Console.WriteLine("");
        Console.WriteLine("=== Test RAG Corpus Setup Complete ===");
        Console.WriteLine($"Corpus: {corpus.Name}");
        Console.WriteLine($"Display Name: {corpus.DisplayName}");
        Console.WriteLine("You can now run the Issue #102 tests!");
    }

    /// <summary>
    /// Verifies the test RAG corpus is properly set up.
    /// </summary>
    [Fact]
    public async Task Setup_VerifyTestCorpusReady()
    {
        var vertexAI = CreateVertexAI();
        var ragManager = vertexAI.CreateRagManager();

        var corpora = await ragManager.ListCorporaAsync(cancellationToken: TestContext.Current.CancellationToken);
        var testCorpus = corpora?.RagCorpora?.FirstOrDefault(c => c.DisplayName == TestCorpusDisplayName);

        if (testCorpus == null)
        {
            Assert.Skip("Test RAG corpus not found. Run Setup_CreateTestCorpusWithData first.");
            return;
        }

        Console.WriteLine($"Test RAG corpus found: {testCorpus.Name}");
        Console.WriteLine($"Display Name: {testCorpus.DisplayName}");
        Console.WriteLine($"Description: {testCorpus.Description}");

        // List files in the corpus
        var files = await ragManager.ListFilesAsync(testCorpus.Name!, cancellationToken: TestContext.Current.CancellationToken);
        Console.WriteLine($"Files in corpus: {files?.RagFiles?.Count ?? 0}");

        if (files?.RagFiles != null)
        {
            foreach (var file in files.RagFiles)
            {
                Console.WriteLine($"  - {file.DisplayName} ({file.Name})");
            }
        }
    }

    /// <summary>
    /// Cleanup test - removes the test RAG corpus (run manually if needed).
    /// </summary>
    [Fact(Skip = "Run manually to clean up test corpus")]
    public async Task Cleanup_DeleteTestCorpus()
    {
        var vertexAI = CreateVertexAI();
        var ragManager = vertexAI.CreateRagManager();

        var corpora = await ragManager.ListCorporaAsync(cancellationToken: TestContext.Current.CancellationToken);
        var testCorpus = corpora?.RagCorpora?.FirstOrDefault(c => c.DisplayName == TestCorpusDisplayName);

        if (testCorpus != null)
        {
            await ragManager.DeleteRagCorpusAsync(testCorpus.Name!, cancellationToken: TestContext.Current.CancellationToken);
            Console.WriteLine($"Deleted test RAG corpus: {testCorpus.Name}");
        }
        else
        {
            Console.WriteLine("Test RAG corpus not found - nothing to delete");
        }
    }

    #endregion

    #region Baseline Tests - Function Tools Only (No RAG)

    [Fact]
    public async Task ShouldWork_WithFunctionToolsOnly_NoCorpus()
    {
        // Arrange - Create model WITHOUT corpusIdForRag
        var vertexAI = CreateVertexAI();
        var model = vertexAI.CreateGenerativeModel(DefaultTestModelName);

        model.UseGoogleSearch = false;
        model.UseCodeExecutionTool = false;
        model.UseGrounding = false;

        var weatherTool = CreateWeatherTool();
        model.AddFunctionTool(weatherTool);

        // Act
        var response = await model.GenerateContentAsync(
            "What is the weather in San Francisco?",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.ShouldNotBeNull();
        var text = response.Text();
        text.ShouldNotBeNullOrEmpty();
        Console.WriteLine($"Response: {text}");

        // Should contain weather-related information
        text.ToLower().ShouldContain("san francisco");
    }

    [Fact]
    public async Task ShouldWork_WithTestStringTool_NoCorpus()
    {
        // Arrange - Using the exact tool pattern from Issue #102, but without corpus
        var vertexAI = CreateVertexAI();
        var model = vertexAI.CreateGenerativeModel(DefaultTestModelName);

        model.UseGoogleSearch = false;
        model.UseCodeExecutionTool = false;
        model.UseGrounding = false;

        var testTool = CreateTestStringTool();
        model.AddFunctionTool(testTool);

        // Act
        var response = await model.GenerateContentAsync(
            "Please call the GetTestString function and tell me what it returns.",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.ShouldNotBeNull();
        var text = response.Text();
        text.ShouldNotBeNullOrEmpty();
        Console.WriteLine($"Response: {text}");
    }

    [Fact]
    public async Task ChatSession_ShouldWork_WithFunctionToolsOnly_NoCorpus()
    {
        // Arrange
        var vertexAI = CreateVertexAI();
        var model = vertexAI.CreateGenerativeModel(DefaultTestModelName);

        model.UseGoogleSearch = false;
        model.UseCodeExecutionTool = false;
        model.UseGrounding = false;

        var weatherTool = CreateWeatherTool();
        model.AddFunctionTool(weatherTool);

        var chatSession = model.StartChat();

        // Act - First message
        var response1 = await chatSession.GenerateContentAsync(
            "What's the weather in Tokyo?",
            cancellationToken: TestContext.Current.CancellationToken);

        response1.ShouldNotBeNull();
        Console.WriteLine($"Response 1: {response1.Text()}");

        // Act - Follow-up message
        var response2 = await chatSession.GenerateContentAsync(
            "And what about Paris?",
            cancellationToken: TestContext.Current.CancellationToken);

        response2.ShouldNotBeNull();
        Console.WriteLine($"Response 2: {response2.Text()}");

        // Assert
        chatSession.History.Count.ShouldBeGreaterThan(0);
    }

    #endregion

    #region Issue #102 - Corpus with Function Calling Tests

    /// <summary>
    /// This test reproduces the exact scenario from Issue #102.
    /// It creates a model with corpusIdForRag and adds a function tool.
    ///
    /// Expected: Currently fails with INVALID_ARGUMENT (Code: 400)
    /// </summary>
    [Fact]
    public async Task Issue102_ShouldWork_WithCorpusAndFunctionTool()
    {
        // Skip if no corpus is available for testing
        var vertexAI = CreateVertexAI();
        var ragManager = vertexAI.CreateRagManager();

        var corpora = await ragManager.ListCorporaAsync(cancellationToken: TestContext.Current.CancellationToken);
        var testCorpus = corpora?.RagCorpora?.FirstOrDefault();

        if (testCorpus == null)
        {
            Assert.Skip("No RAG corpus available for testing. Create a test corpus first.");
            return;
        }

        Console.WriteLine($"Using corpus: {testCorpus.Name} ({testCorpus.DisplayName})");

        // Arrange - Reproduce exact scenario from Issue #102
        var model = vertexAI.CreateGenerativeModel(
            DefaultTestModelName,
            corpusIdForRag: testCorpus.Name
        );

        model.UseGoogleSearch = false;
        model.UseCodeExecutionTool = false;
        model.UseGrounding = false;

        // Add the same test tool from the issue
        var testTool = CreateTestStringTool();
        model.AddFunctionTool(testTool);

        // Act - Query that should trigger function call
        try
        {
            var response = await model.GenerateContentAsync(
                "Please call the GetTestString function and tell me what it returns.",
                cancellationToken: TestContext.Current.CancellationToken);

            // If we get here, the issue is fixed
            response.ShouldNotBeNull();
            var text = response.Text();
            text.ShouldNotBeNullOrEmpty();
            Console.WriteLine($"SUCCESS - Response: {text}");
        }
        catch (Exception ex)
        {
            // Log the exception for investigation
            Console.WriteLine($"FAILED with exception: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");

            // Re-throw to fail the test - this is the bug we're tracking
            throw;
        }
    }

    /// <summary>
    /// Tests corpus query first, then function call in same session.
    /// </summary>
    [Fact]
    public async Task Issue102_ChatSession_CorpusQueryThenFunctionCall()
    {
        var vertexAI = CreateVertexAI();
        var ragManager = vertexAI.CreateRagManager();

        var corpora = await ragManager.ListCorporaAsync(cancellationToken: TestContext.Current.CancellationToken);
        var testCorpus = corpora?.RagCorpora?.FirstOrDefault();

        if (testCorpus == null)
        {
            Assert.Skip("No RAG corpus available for testing.");
            return;
        }

        Console.WriteLine($"Using corpus: {testCorpus.Name}");

        // Arrange
        var model = vertexAI.CreateGenerativeModel(
            DefaultTestModelName,
            corpusIdForRag: testCorpus.Name
        );

        model.UseGoogleSearch = false;
        model.UseCodeExecutionTool = false;
        model.UseGrounding = false;

        var testTool = CreateTestStringTool();
        model.AddFunctionTool(testTool);

        var chatSession = model.StartChat();

        // Act - Step 1: Query the corpus (should work)
        Console.WriteLine("Step 1: Querying corpus...");
        try
        {
            var response1 = await chatSession.GenerateContentAsync(
                "What information do you have in your knowledge base?",
                cancellationToken: TestContext.Current.CancellationToken);

            response1.ShouldNotBeNull();
            Console.WriteLine($"Corpus query response: {response1.Text()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Corpus query failed: {ex.Message}");
            // Continue to test function call
        }

        // Act - Step 2: Trigger function call (this is where Issue #102 fails)
        Console.WriteLine("\nStep 2: Triggering function call...");
        try
        {
            var response2 = await chatSession.GenerateContentAsync(
                "Now please call the GetTestString function.",
                cancellationToken: TestContext.Current.CancellationToken);

            response2.ShouldNotBeNull();
            Console.WriteLine($"Function call response: {response2.Text()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FAILED - Function call with corpus failed: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Tests with manual function calling behavior to inspect raw responses.
    /// </summary>
    [Fact]
    public async Task Issue102_ManualFunctionCalling_WithCorpus()
    {
        var vertexAI = CreateVertexAI();
        var ragManager = vertexAI.CreateRagManager();

        var corpora = await ragManager.ListCorporaAsync(cancellationToken: TestContext.Current.CancellationToken);
        var testCorpus = corpora?.RagCorpora?.FirstOrDefault();

        if (testCorpus == null)
        {
            Assert.Skip("No RAG corpus available for testing.");
            return;
        }

        // Arrange - Disable auto function calling to inspect raw response
        var model = vertexAI.CreateGenerativeModel(
            DefaultTestModelName,
            corpusIdForRag: testCorpus.Name
        );

        model.UseGoogleSearch = false;
        model.UseCodeExecutionTool = false;
        model.UseGrounding = false;

        // Disable auto function calling
        model.FunctionCallingBehaviour = new FunctionCallingBehaviour
        {
            FunctionEnabled = true,
            AutoCallFunction = false,
            AutoReplyFunction = false
        };

        var testTool = CreateTestStringTool();
        model.AddFunctionTool(testTool);

        // Act
        try
        {
            var response = await model.GenerateContentAsync(
                "Call the GetTestString function.",
                cancellationToken: TestContext.Current.CancellationToken);

            response.ShouldNotBeNull();

            // Check if we got a function call
            var functionCalls = response.GetFunctions();
            if (functionCalls != null && functionCalls.Any())
            {
                Console.WriteLine($"Got function call(s): {string.Join(", ", functionCalls.Select(f => f.Name))}");

                // Execute the function manually
                foreach (var fc in functionCalls)
                {
                    var result = await testTool.CallAsync(fc, TestContext.Current.CancellationToken);
                    Console.WriteLine($"Function {fc.Name} returned: {result?.Response}");
                }
            }
            else
            {
                Console.WriteLine($"No function call in response. Text: {response.Text()}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FAILED: {ex.GetType().Name} - {ex.Message}");
            throw;
        }
    }

    #endregion

    #region Additional Diagnostic Tests

    /// <summary>
    /// Tests to verify that corpus retrieval tool is properly configured.
    /// </summary>
    [Fact]
    public async Task Diagnostic_VerifyRetrievalToolConfiguration()
    {
        var vertexAI = CreateVertexAI();
        var ragManager = vertexAI.CreateRagManager();

        var corpora = await ragManager.ListCorporaAsync(cancellationToken: TestContext.Current.CancellationToken);
        var testCorpus = corpora?.RagCorpora?.FirstOrDefault();

        if (testCorpus == null)
        {
            Assert.Skip("No RAG corpus available for testing.");
            return;
        }

        // Create model with corpus
        var model = vertexAI.CreateGenerativeModel(
            DefaultTestModelName,
            corpusIdForRag: testCorpus.Name
        );

        // Verify RetrievalTool is set
        model.RetrievalTool.ShouldNotBeNull("RetrievalTool should be configured when corpusIdForRag is provided");
        model.RetrievalTool.Retrieval.ShouldNotBeNull("Retrieval property should be set");

        Console.WriteLine("RetrievalTool is properly configured");

        // Now add a function tool and verify both are present
        var testTool = CreateTestStringTool();
        model.AddFunctionTool(testTool);

        model.FunctionTools.Count.ShouldBe(1, "Should have 1 function tool");
        model.RetrievalTool.ShouldNotBeNull("RetrievalTool should still be present after adding function tool");

        Console.WriteLine($"Model has {model.FunctionTools.Count} function tool(s) AND RetrievalTool configured");
    }

    /// <summary>
    /// Test with weather tool which has parameters, combined with corpus.
    /// </summary>
    [Fact]
    public async Task Issue102_WeatherToolWithParameters_AndCorpus()
    {
        var vertexAI = CreateVertexAI();
        var ragManager = vertexAI.CreateRagManager();

        var corpora = await ragManager.ListCorporaAsync(cancellationToken: TestContext.Current.CancellationToken);
        var testCorpus = corpora?.RagCorpora?.FirstOrDefault();

        if (testCorpus == null)
        {
            Assert.Skip("No RAG corpus available for testing.");
            return;
        }

        // Arrange
        var model = vertexAI.CreateGenerativeModel(
            DefaultTestModelName,
            corpusIdForRag: testCorpus.Name
        );

        model.UseGoogleSearch = false;
        model.UseCodeExecutionTool = false;
        model.UseGrounding = false;

        var weatherTool = CreateWeatherTool();
        model.AddFunctionTool(weatherTool);

        // Act
        try
        {
            var response = await model.GenerateContentAsync(
                "What's the weather like in New York?",
                cancellationToken: TestContext.Current.CancellationToken);

            response.ShouldNotBeNull();
            Console.WriteLine($"Response: {response.Text()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FAILED: {ex.GetType().Name} - {ex.Message}");
            throw;
        }
    }

    #endregion
}
