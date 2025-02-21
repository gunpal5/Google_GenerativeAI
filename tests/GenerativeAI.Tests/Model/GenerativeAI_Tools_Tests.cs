using Shouldly;

namespace GenerativeAI.Tests.Model;

public class GenerativeAI_Tools_Tests:TestBase
{
    public GenerativeAI_Tools_Tests(ITestOutputHelper helper) : base(helper)
    {
        
    }
    
    private GenerativeModel InitializeModel(
        bool useGoogleSearch = false,
        bool useGrounding = false,
        bool useCodeExecutionTool = false)
    {
        Assert.SkipUnless(IsGeminiApiKeySet,GeminiTestSkipMessage);

        return new GenerativeModel(
            platform: GetTestGooglePlatform()!,
            model: GoogleAIModels.Gemini2Flash
        )
        {
            UseGoogleSearch = useGoogleSearch,
            UseGrounding = useGrounding,
            UseCodeExecutionTool = useCodeExecutionTool
        };
    }
    
    [Fact]
    public async Task Should_Add_GoogleSearch_Tool_When_UseGoogleSearch_Is_True()
    {
        // Arrange
        var model = InitializeModel(useGoogleSearch: true);

        var prompt = "Search on Google and find links for blogs about creating lasagne";
        

        // Act
        var response = await model.GenerateContentAsync(prompt).ConfigureAwait(false);

        // Assert
        response.Candidates.ShouldNotBeNull();
        foreach (var candidate in response.Candidates)
        {
            candidate.GroundingMetadata.ShouldNotBeNull();
            candidate.GroundingMetadata.GroundingChunks.ShouldNotBeNull();
            candidate.GroundingMetadata.GroundingChunks.Count.ShouldBeGreaterThan(0);
        }

        var text = response.Text();
        text.ShouldNotBeNullOrEmpty();
        text.ShouldContain("lasagna");
        Console.WriteLine(text);
    }
    
    [Fact]
    public async Task Should_Add_GoogleSearch_Grounding_Tool_When_UseGoogleSearch_Is_True()
    {
        // Arrange
        var model = InitializeModel(useGoogleSearch: true);
        model.UseGoogleSearch = true;
        var prompt = "What is the current Google stock price?";
        

        // Act
        var response = await model.GenerateContentAsync(prompt).ConfigureAwait(false);

        // Assert
        response.Candidates.ShouldNotBeNull();
        foreach (var candidate in response.Candidates)
        {
            candidate.GroundingMetadata.ShouldNotBeNull();
            candidate.GroundingMetadata.GroundingChunks.ShouldNotBeNull();
            candidate.GroundingMetadata.GroundingChunks.Count.ShouldBeGreaterThan(0);
        }

        var text = response.Text();
        text.ShouldNotBeNullOrEmpty();
        text.ShouldContain("price");
        Console.WriteLine(text);
    }


    
}