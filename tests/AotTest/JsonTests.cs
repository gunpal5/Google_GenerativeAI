using System.Text.Json;
using System.Text.Json.Serialization;
using GenerativeAI;
using GenerativeAI.Core;
using GenerativeAI.Types;
using Shouldly;


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
        var response = await model.GenerateContentAsync<SampleJsonClass>(request).ConfigureAwait(false);

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
        var result = await model.GenerateObjectAsync<SampleJsonClass>(request).ConfigureAwait(false);

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
        var result = await model.GenerateObjectAsync<SampleJsonClass>(prompt).ConfigureAwait(false);

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
        var result = await model.GenerateObjectAsync<SampleJsonClass>(parts).ConfigureAwait(false);

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
        var response = await model.GenerateContentAsync<ComplexDataTypeClass>(request).ConfigureAwait(false);

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
        var response = await model.GenerateContentAsync<ComplexJsonClass>(request).ConfigureAwait(false);

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
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);

        return new GoogleAIPlatformAdapter(apiKey);
    }
}

/// <summary>
/// A complex sample class with nested classes and collections used for testing JSON deserialization.
/// </summary>
internal class ComplexJsonClass
{
    public string? Description { get; set; }
    public Detail? Details { get; set; }
    public List<Child2>? Children { get; set; }

    public class Detail
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
    }

    public class Child2
    {
        public string? Name { get; set; }
        public List<int>? Values { get; set; }
    }
}

/// <summary>
/// A small sample class used for testing JSON deserialization. 
/// The property name can be adjusted as needed for your test scenarios.
/// </summary>
internal class SampleJsonClass
{
    public string? Message { get; set; }
}

/// <summary>
/// A sample class used to test serialization and deserialization with various data types.
/// </summary>
internal class ComplexDataTypeClass
{
    public string? Title { get; set; }
    [JsonIgnore] public Dictionary<string, string>? Metadata { get; set; }
    public int[]? Numbers { get; set; }
    public List<Child>? Children { get; set; }
    public string? OptionalField { get; set; }

    public class Child
    {
        public string? Name { get; set; }
        public List<int>? Values { get; set; }
    }
}

[JsonSerializable(typeof(SampleJsonClass))]
[JsonSerializable(typeof(ComplexDataTypeClass.Child))]
[JsonSerializable(typeof(ComplexJsonClass.Child2))]
[JsonSerializable(typeof(ComplexJsonClass.Detail))]
[JsonSerializable(typeof(ComplexDataTypeClass))]
[JsonSerializable(typeof(ComplexJsonClass))]
[JsonSerializable(typeof(QueryStudentRecordRequest))]
[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
internal partial class TestJsonSerializerContext : JsonSerializerContext
{
}