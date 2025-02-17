using GenerativeAI.Core;
using GenerativeAI.Tests.Base;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Platforms.VertextAIModel;

public class Vertex_AI_Constructor_Tests:TestBase
{
    private const string DefaultTestModelName = GoogleAIModels.DefaultGeminiModel;
    public Vertex_AI_Constructor_Tests(ITestOutputHelper helper) : base(helper)
    {
        
    }
     /// <summary>
    /// Helper method to create a GenerativeModel using the default Gemini model name.
    /// </summary>
    private GenerativeModel CreateInitializedModel()
    {
        var platform = GetTestVertexAIPlatform();
        return new GenerativeModel(platform, DefaultTestModelName);
    }

    #region Constructors

    [Fact, TestPriority(1)]
    public void ShouldCreateWithBasicConstructor()
    {
        // Arrange
        var platform = new GoogleAIPlatformAdapter("SLDKHFLKSDAHFLKH");

        // Act
        var model = new GenerativeModel(platform, DefaultTestModelName);

        // Assert
        model.ShouldNotBeNull();
        model.Model.ShouldBe(DefaultTestModelName);
        Console.WriteLine($"Model created with basic constructor: {DefaultTestModelName}");
    }

    [Fact, TestPriority(2)]
    public void ShouldCreateWithExtendedConstructor()
    {
        // Arrange
        var platform = new GoogleAIPlatformAdapter("SLDKHFLKSDAHFLKH");
        var config = new GenerationConfig
        {
            /* Configure as needed */
        };
        var safetySettings = new List<SafetySetting>
        {
            /* Populate as needed */
        };
        // Create system instruction using extension
        var systemContent = "You are a helpful assistant.";

        // Act
        var model = new GenerativeModel(
            platform,
            DefaultTestModelName,
            config: config,
            safetySettings: safetySettings,
            systemInstruction: systemContent
        );

        // Assert
        model.ShouldNotBeNull();
        model.Model.ShouldBe(DefaultTestModelName);
        model.Config.ShouldBe(config);
        model.SafetySettings.ShouldBe(safetySettings);
        Console.WriteLine($"Model created with extended constructor: {DefaultTestModelName}");
    }

    [Fact, TestPriority(3)]
    public void ShouldCreateWithApiKeyConstructor()
    {
        // Arrange
        var apiKey = "fake-api-key";
        var modelParams = new ModelParams { Model = DefaultTestModelName };

        // Act
        var model = new GenerativeModel(apiKey, modelParams);

        // Assert
        model.ShouldNotBeNull();
        model.Model.ShouldBe(DefaultTestModelName);

        Console.WriteLine("Model created with API key constructor.");
    }

    #endregion
}