using System.Text.Json;
using GenerativeAI;
using GenerativeAI.Core;
using GenerativeAI.Types;
using Shouldly;
using Environment = System.Environment;


namespace AotTest;

public class JsonModeTests
{
    private const string DefaultTestModelName = GoogleAIModels.DefaultGeminiModel;

    private JsonSerializerOptions TestSerializerOptions
    {
        get
        {
            return new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                TypeInfoResolver = TestJsonSerializerContext.Default
            };
        }
    }

    /// <summary>
    /// A helper method to create a GenerativeModel using the default Gemini model name,
    /// matching the style used in basic tests.
    /// </summary>
    private GenerativeModel CreateInitializedModel()
    {
        var platform = GetTestGooglePlatform();

        var model = new GenerativeModel(platform, DefaultTestModelName);
        model.GenerateObjectJsonSerializerOptions = TestSerializerOptions;
        return model;
    }

    #region GenerateObjectAsync Overloads

    public async Task ShouldGenerateContentAsync_WithJsonMode_GenericParameter()
    {
        // Arrange
        var model = CreateInitializedModel();

        // We'll use a sample input request mimicking JSON-based generation
        var request = new GenerateContentRequest();
        request.AddText("Give me a really good message.", false);

        // Act
        var response = await model.GenerateContentAsync<SampleJsonClass>(request, cancellationToken: CancellationToken.None);

        // Assert
        response.ShouldNotBeNull();
        response.Text().ShouldNotBeNull();
        var obj = response.ToObject<SampleJsonClass>(TestSerializerOptions);
        obj.Message.ShouldNotBeNullOrWhiteSpace();
        // Additional checks as needed for any placeholders or content
        Console.WriteLine("GenerateContentAsync<T> returned a valid GenerateContentResponse.");
    }


    public async Task ShouldGenerateObjectAsync_WithGenericParameter()
    {
        // Arrange
        var model = CreateInitializedModel();

        var request = new GenerateContentRequest();
        request.AddText("write a text message for my boss that I'm resigning from the job.", false);

        // Act
        var result = await model.GenerateObjectAsync<SampleJsonClass>(request, cancellationToken: CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Message.ShouldNotBeNullOrWhiteSpace();
        Console.WriteLine($"GenerateObjectAsync<T>(request) returned: {result.Message}");
    }


    public async Task ShouldGenerateObjectAsync_WithStringPrompt()
    {
        // Arrange
        var model = CreateInitializedModel();
        var prompt = "I need a birthday message for my wife.";

        // Act
        var result = await model.GenerateObjectAsync<SampleJsonClass>(prompt, cancellationToken: CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Message.ShouldNotBeNullOrWhiteSpace();
        Console.WriteLine($"GenerateObjectAsync<T>(string prompt) returned: {result.Message}");
    }


    public async Task ShouldGenerateObjectAsync_WithPartsEnumerable()
    {
        // Arrange
        var model = CreateInitializedModel();

        // Build content parts with an imaginary scenario
        var parts = new List<Part>
        {
            new Part() { Text = "I am very busy person. i always need AI help for my work." },
            new Part() { Text = "I need a message for my boss to provide me a paid subscription to Gemini Advanced." }
        };

        // Act
        var result = await model.GenerateObjectAsync<SampleJsonClass>(parts, cancellationToken: CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Message.ShouldNotBeNullOrWhiteSpace();
        Console.WriteLine($"GenerateObjectAsync<T>(IEnumerable parts) returned: {result.Message}");
    }

    #endregion

    public async Task ShouldGenerateComplexObjectAsync_WithVariousDataTypes()
    {
        // Arrange
        var model = CreateInitializedModel();
        var request = new GenerateContentRequest();
        request.AddText(
            "Generate a structured object with various data types including dictionary, list, array, and nested objects.",
            false);

        // Act
        var response = await model.GenerateContentAsync<ComplexDataTypeClass>(request, cancellationToken: CancellationToken.None);

        // Assert
        response.ShouldNotBeNull();
        response.Text().ShouldNotBeNullOrWhiteSpace();
        var obj = response.ToObject<ComplexDataTypeClass>(TestSerializerOptions);

        obj.Title.ShouldNotBeNullOrWhiteSpace();
        // obj.Metadata.ShouldNotBeNull();
        // obj.Metadata.ShouldContainKey("key1");
        obj.Numbers.ShouldNotBeNull();
        obj.Numbers.Length.ShouldBeGreaterThan(0);
        obj.Children.ShouldNotBeNull();
        obj.Children.ForEach(child =>
        {
            child.Name.ShouldNotBeNullOrWhiteSpace();
            child.Values.ShouldNotBeNull();
            child.Values.ShouldNotBeEmpty();
        });

        //obj.OptionalField.ShouldBeNull();
        Console.WriteLine("GenerateContentAsync<T> with various data types returned a valid response.");
    }
    public async Task ShouldGenerateNestedObjectAsync_WithJsonMode()
    {
        // Arrange
        var model = CreateInitializedModel();
        var request = new GenerateContentRequest();
        request.AddText("Generate a complex JSON object with nested properties.", false);

        // Act
        var response = await model.GenerateContentAsync<ComplexJsonClass>(request, cancellationToken: CancellationToken.None);

        // Assert
        response.ShouldNotBeNull();
        
        response.Text().ShouldNotBeNullOrWhiteSpace();
        var obj = response.ToObject<ComplexJsonClass>(TestSerializerOptions);
        obj.Description.ShouldNotBeNullOrWhiteSpace();
        obj.Details.ShouldNotBeNull();
        obj.Details.Title.ShouldNotBeNullOrWhiteSpace();
        obj.Children.ShouldNotBeNull();
        obj.Children.ShouldNotBeEmpty();
        obj.Children.ForEach(child =>
        {
            child.Name.ShouldNotBeNullOrWhiteSpace();
            child.Values.ShouldNotBeNull();
            child.Values.ShouldNotBeEmpty();
        });
        Console.WriteLine("GenerateContentAsync<T> with nested types returned a valid response.");
    }
    
    protected virtual IPlatformAdapter GetTestGooglePlatform()
    {
        //return GetTestVertexAIPlatform();
        var apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY_TEST", EnvironmentVariableTarget.User);

        return new GoogleAIPlatformAdapter(apiKey);
    }
}