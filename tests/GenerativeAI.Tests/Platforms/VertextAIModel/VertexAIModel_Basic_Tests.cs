using GenerativeAI.Core;
using GenerativeAI.Tests.Base;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Platforms.VertextAIModel;

public class VertexAIModel_Basic_Tests : TestBase
{
    public VertexAIModel_Basic_Tests(ITestOutputHelper output) : base(output)
    {
        Assert.SkipWhen(SkipVertexAITests,VertextTestSkipMesaage);
    }

    private const string DefaultTestModelName = GoogleAIModels.DefaultGeminiModel;


    /// <summary>
    /// Helper method to create a GenerativeModel using the default Gemini model name.
    /// </summary>
    private GenerativeModel CreateInitializedModel()
    {
        var platform = GetTestVertexAIPlatform();
        return new GenerativeModel(platform, DefaultTestModelName);
    }

    #region GenerateContentAsync Overloads

    [Fact, TestPriority(5)]
    public async Task ShouldGenerateContentWithSingleContentRequest()
    {
        // Arrange
        var model = CreateInitializedModel();

        // Use RequestExtension to format single user content
        var singleContent =
            RequestExtensions.FormatGenerateContentInput("Write an inspiring paragraph about achieving dreams.");
        var request = new GenerateContentRequest(singleContent);

        // Act
        var response = await model.GenerateContentAsync(request).ConfigureAwait(false);

        // Assert
        response.ShouldNotBeNull();
        response.Text().ShouldNotBeNullOrEmpty();
        Console.WriteLine($"Response: {response.Text()}");
    }

    [Fact, TestPriority(6)]
    public async Task ShouldGenerateContentWithMultipleContentRequest()
    {
        // Arrange
        var model = CreateInitializedModel();

        // Use extension to format content from different inputs
        var content1 =
            RequestExtensions.FormatGenerateContentInput("Create a futuristic description of life on a space station.");
        var content2 =
            RequestExtensions.FormatGenerateContentInput("Explain the concept of time travel in a simple way.");
        var request = new GenerateContentRequest(new List<Content> { content1, content2 });

        // Act
        var response = await model.GenerateContentAsync(request).ConfigureAwait(false);

        // Assert
        response.ShouldNotBeNull();
        response.Text().ShouldNotBeNullOrEmpty();
        Console.WriteLine($"Response: {response.Text()}");
    }

    [Fact, TestPriority(7)]
    public async Task ShouldGenerateContentWithString()
    {
        // Arrange
        var model = CreateInitializedModel();

        // Pass a raw string, model internally uses single-argument overload
        var response = await model.GenerateContentAsync("Generate a poetic description of nature during autumn.").ConfigureAwait(false);

        // Assert
        response.ShouldNotBeNull();
        response.Text().ShouldNotBeNullOrEmpty();
        Console.WriteLine($"Response: {response.Text()}");
    }

    [Fact, TestPriority(8)]
    public async Task ShouldGenerateContentWithParts()
    {
        // Arrange
        var model = CreateInitializedModel();
        var parts = new[]
        {
            new Part { Text = "Compose a short story about an explorer discovering a hidden underwater city." },
            new Part { Text = "Include details about the creatures and plants found in this city." }
        };

        // Act
        var response = await model.GenerateContentAsync(parts).ConfigureAwait(false);

        // Assert
        response.ShouldNotBeNull();
        response.Text().ShouldNotBeNullOrEmpty();
        Console.WriteLine($"Response: {response.Text()}");
    }

    [Fact, TestPriority(9)]
    public async Task ShouldStreamContentWithSingleContentRequest()
    {
        // Arrange
        var model = CreateInitializedModel();
        var singleContent =
            RequestExtensions.FormatGenerateContentInput("Create a short poem about the beauty of the sunset.");
        var request = new GenerateContentRequest(singleContent);

        // Act
        var responses = new List<string>();
        await foreach (var response in model.StreamContentAsync(request).ConfigureAwait(false))
        {
            response.ShouldNotBeNull();
            responses.Add(response.Text() ?? string.Empty);
            Console.WriteLine($"Chunk: {response.Text()}");
        }

        // Assert
        responses.ShouldNotBeNull();
        responses.ShouldNotBeEmpty();
    }

    [Fact, TestPriority(10)]
    public async Task ShouldStreamContentWithMultipleContentRequest()
    {
        // Arrange
        var model = CreateInitializedModel();
        var content1 =
            RequestExtensions.FormatGenerateContentInput(
                "Write a motivational quote for someone starting a new journey.");
        var content2 = RequestExtensions.FormatGenerateContentInput("Generate fun facts about space exploration.");
        var contents = new List<Content> { content1, content2 };

        // Act
        var responses = new List<string>();
        await foreach (var response in model.StreamContentAsync(contents).ConfigureAwait(false))
        {
            response.ShouldNotBeNull();
            responses.Add(response.Text() ?? string.Empty);
            Console.WriteLine($"Chunk: {response.Text()}");
        }

        // Assert
        responses.ShouldNotBeNull();
        responses.ShouldNotBeEmpty();
    }

    [Fact, TestPriority(11)]
    public async Task ShouldStreamContentWithString()
    {
        // Arrange
        var model = CreateInitializedModel();
        var input = "Describe what a day in the life of an astronaut on Mars might look like.";

        // Act
        var responses = new List<string>();
        await foreach (var response in model.StreamContentAsync(input).ConfigureAwait(false))
        {
            response.ShouldNotBeNull();
            responses.Add(response.Text() ?? string.Empty);
            Console.WriteLine($"Chunk: {response.Text()}");
        }

        // Assert
        responses.ShouldNotBeNull();
        responses.ShouldNotBeEmpty();
    }

    [Fact, TestPriority(12)]
    public async Task ShouldStreamContentWithParts()
    {
        // Arrange
        var model = CreateInitializedModel();
        var parts = new[]
        {
            new Part { Text = "Write a story about an AI assistant learning to paint." },
            new Part { Text = "Include a scene where the AI experiences a breakthrough moment." }
        };


        // Act
        var responses = new List<string>();
        await foreach (var response in model.StreamContentAsync(parts).ConfigureAwait(false))
        {
            response.ShouldNotBeNull();
            responses.Add(response.Text() ?? string.Empty);
            Console.WriteLine($"Chunk: {response.Text()}");
        }

        // Assert
        responses.ShouldNotBeNull();
        responses.ShouldNotBeEmpty();
    }

    #endregion

    #region CountTokensAsync Overloads

    [Fact, TestPriority(13)]
    public async Task ShouldCountTokensWithRequest()
    {
        // Arrange
        var model = CreateInitializedModel();

        // Create a CountTokensRequest
        var content = RequestExtensions.FormatGenerateContentInput(
            "Calculate the number of tokens required for this very detailed and comprehensive paragraph that spans across multiple subjects, ideas, and sentences to ensure there are sufficient tokens counted in the response.");
        var request = new CountTokensRequest
        {
            Contents = new List<Content> { content }
        };

        // Act
        var response = await model.CountTokensAsync(request).ConfigureAwait(false);

        // Assert
        response.ShouldNotBeNull();
        response.TotalTokens.ShouldBeGreaterThan(0);
        Console.WriteLine($"Total Tokens: {response.TotalTokens}");
    }

    [Fact, TestPriority(14)]
    public async Task ShouldCountTokensWithContents()
    {
        // Arrange
        var model = CreateInitializedModel();

        // Prepare Content objects
        var content1 = RequestExtensions.FormatGenerateContentInput(
            "First input content for token counting. This content includes a comprehensive explanation of various techniques used in counting tokens in different scenarios and environments.");
        var content2 = RequestExtensions.FormatGenerateContentInput(
            "Second input content. It describes the significance of token counts in large scale systems, emphasizing the role they play in accurate calculations.");
        var contents = new List<Content> { content1, content2 };

        // Act
        var response = await model.CountTokensAsync(contents).ConfigureAwait(false);

        // Assert
        response.ShouldNotBeNull();
        response.TotalTokens.ShouldBeGreaterThan(0);
        Console.WriteLine($"Total Tokens: {response.TotalTokens}");
    }

    [Fact, TestPriority(15)]
    public async Task ShouldCountTokensWithParts()
    {
        // Arrange
        var model = CreateInitializedModel();

        // Create a collection of Parts
        var parts = new[]
        {
            new Part
            {
                Text =
                    "Count tokens for this part that contains an elaborate discussion about the different types of tokens and their respective usage in the text generation models."
            },
            new Part
            {
                Text =
                    "And for this part as well, which elaborates further on the tokenization process used in complex and lengthy content types with detailed breakdowns."
            }
        };

        // Act
        var response = await model.CountTokensAsync(parts).ConfigureAwait(false);

        // Assert
        response.ShouldNotBeNull();
        response.TotalTokens.ShouldBeGreaterThan(0);
        Console.WriteLine($"Total Tokens: {response.TotalTokens}");
    }

    // [Fact, TestPriority(16)]
    // public async Task ShouldCountTokensWithGenerateContentRequest()
    // {
    //     // Arrange
    //     var model = CreateInitializedModel();
    //
    //     // Create a GenerateContentRequest
    //     var singleContent = RequestExtensions.FormatGenerateContentInput("Token count should be calculated here for this large piece of content that includes a detailed description, analysis, and examples of how token counting is used in AI-generated responses, particularly in models designed to understand and generate natural language.");
    //     var generateRequest = new GenerateContentRequest(singleContent);
    //     
    //     // Act
    //     var response = await model.CountTokensAsync(generateRequest);
    //
    //     // Assert
    //     response.ShouldNotBeNull();
    //     response.TotalTokens.ShouldBeGreaterThan(0);
    //     Console.WriteLine($"Total Tokens: {response.TotalTokens}");
    // }
    //

    #endregion
}