using GenerativeAI.Types;
using Shouldly;
using Xunit.Abstractions;

namespace GenerativeAI.Tests.Model;

public class GenerativeAI_GenerateAnswer_Tests:TestBase
{
    public GenerativeAI_GenerateAnswer_Tests(ITestOutputHelper helper) : base(helper)
    {
        
    }
    private GenerativeModel CreateInitializedModel()
    {
        return new GenerativeModel(GetTestGooglePlatform(), GoogleAIModels.DefaultGeminiModel);
    }

    [Fact]
    public async Task ShouldGenerateAnswerWithBasicRequest()
    {
        // Arrange
        var model = CreateInitializedModel();
        var request = new GenerateAnswerRequest();
        request.AddText("What is the capital of France?");

        // Act
        var response = await model.GenerateAnswerAsync(request);

        // Assert
        response.ShouldNotBeNull();

        // Verify the main answer object
        response.Answer.ShouldNotBeNull("The response should include a valid answer candidate.");
        // If your Candidate class has a property named 'Content' (or similar), you can check:
        // response.Answer.Content.ShouldNotBeNullOrWhiteSpace();

        // Verify the probability
        // If the service sometimes returns null, adjust the test accordingly:
        response.AnswerableProbability.ShouldNotBeNull();
        response.AnswerableProbability.Value.ShouldBeInRange(0.0, 1.0, "Probability should be within [0,1].");

        // Verify input feedback if expected
        response.InputFeedback.ShouldNotBeNull();
        // Further checks on response.InputFeedback if necessary ...
    }

    [Fact]
    public async Task ShouldGenerateAnswerWithStringPrompt()
    {
        // Arrange
        var model = CreateInitializedModel();
        var prompt = "Tell me a joke about technology.";

        // Act
        var response = await model.GenerateAnswerAsync(prompt);

        // Assert
        response.ShouldNotBeNull();
        response.Answer.ShouldNotBeNull("The response should include a valid answer candidate.");
        // response.Answer.Content.ShouldNotBeNullOrWhiteSpace();

        response.AnswerableProbability.ShouldNotBeNull();
        response.AnswerableProbability.Value.ShouldBeInRange(0.0, 1.0);

        response.InputFeedback.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldGenerateAnswerWithSpecifiedAnswerStyle()
    {
        // Arrange
        var model = CreateInitializedModel();
        var prompt = "Explain polymorphism in OOP.";
        var style = AnswerStyle.VERBOSE; // Example style

        // Act
        var response = await model.GenerateAnswerAsync(prompt, style);

        // Assert
        response.ShouldNotBeNull();
        response.Answer.ShouldNotBeNull("Expected an answer candidate for the specified style.");
        // response.Answer.Content.ShouldNotBeNullOrWhiteSpace();

        response.AnswerableProbability.ShouldNotBeNull();
        response.AnswerableProbability.Value.ShouldBeInRange(0.0, 1.0);

        response.InputFeedback.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldGenerateAnswerWithSafetySettings()
    {
        // Arrange
        var model = CreateInitializedModel();
        var safetySettings = new List<SafetySetting>
        {
            // Example safety settings:
            new SafetySetting
            {
                /* Initialize as desired */
            }
        };
        var prompt = "Describe an edge-case scenario politely.";

        // Act
        var response = await model.GenerateAnswerAsync(prompt, safetySettings: safetySettings);

        // Assert
        response.ShouldNotBeNull();
        response.Answer.ShouldNotBeNull("Expected a valid answer with safety settings applied.");
        // response.Answer.Content.ShouldNotBeNullOrWhiteSpace();

        response.AnswerableProbability.ShouldNotBeNull();
        response.AnswerableProbability.Value.ShouldBeInRange(0.0, 1.0);

        response.InputFeedback.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldRespectCancellationToken()
    {
        // Arrange
        var model = CreateInitializedModel();
        var request = new GenerateAnswerRequest();
        request.AddText("This request should be canceled.");
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Immediately cancel

        // Act & Assert
        await Should.ThrowAsync<TaskCanceledException>(async () =>
        {
            await model.GenerateAnswerAsync(request, cts.Token);
        });
    }
}