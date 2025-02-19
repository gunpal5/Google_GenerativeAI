using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Platforms.VertextAIModel;

public class VertexAIModel_MultiModel_Tests : TestBase
{
    public VertexAIModel_MultiModel_Tests(ITestOutputHelper helper) : base(helper)
    {
        Assert.SkipWhen(SkipVertexAITests, VertextTestSkipMesaage);
    }

   
    private const string TestModel = GoogleAIModels.Gemini2Flash;

    private VertexAIModel CreateInitializedModel()
    {
        return new VertexAIModel(GetTestVertexAIPlatform(), TestModel);
    }

    [Fact]
    public async Task ShouldIdentifyObjectInImage()
    {
        //Arrange
        var model = CreateInitializedModel();
        var request = new GenerateContentRequest();
        request.AddInlineFile("image.png", false);
        request.AddText("Identify objects in the image?");

        //Act
        var result = await model.GenerateContentAsync(request).ConfigureAwait(false);

        //Assert
        var text = result.Text();
        text.ShouldNotBeNull();
        text.ShouldContain("blueberry", Case.Insensitive);
        Console.WriteLine(result.Text());
    }

    [Fact]
    public async Task ShouldIdentifyImageWithFileUri()
    {
        //Arrange
        var model = CreateInitializedModel();

        string prompt = "Identify objects in the image?";

        var uri =
            "https://images.pexels.com/photos/28587807/pexels-photo-28587807/free-photo-of-traditional-turkish-coffee-brewed-in-istanbul-sand.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1";
        //Act
        var result = await model.GenerateContentAsync(prompt, uri, "image/jpeg").ConfigureAwait(false);

        //Assert
        result.ShouldNotBeNull();
        var text = result.Text();
        text.ShouldNotBeNull();
        text.ShouldContain("coffee", Case.Insensitive);
        Console.WriteLine(result.Text());
    }

    [Fact]
    public async Task ShouldProcessVideoWithFileUri()
    {
        //Arrange
        var model = CreateInitializedModel();

        string prompt = "Describe this video?";
        string uri = "https://videos.pexels.com/video-files/3192305/3192305-uhd_2560_1440_25fps.mp4";

        //Act
        var result = await model.GenerateContentAsync(prompt, uri, "video/mp4").ConfigureAwait(false);

        //Assert
        result.ShouldNotBeNull();
        var text = result.Text();
        text.ShouldNotBeNull();
        text.ShouldContain("meeting", Case.Insensitive);
        Console.WriteLine(result.Text());
    }

    [Fact]
    public async Task ShouldProcessAudioWithFilePath()
    {
        //Arrange
        // var model = CreateInitializedModel();
        //
        // string prompt = "Describe this audio?";
        //
        // //Act
        // var result = await model.GenerateContentAsync(prompt, "TestData/testaudio.mp3");
        //
        // //Assert
        // result.ShouldNotBeNull();
        // var text = result.Text();
        // text.ShouldNotBeNull();
        // text.ShouldContain("theological", Case.Insensitive);
        // Console.WriteLine(result.Text());
    }

    [Fact]
    public async Task ShouldIdentifyImageWithWithStreaming()
    {
        var imageFile =
            "https://images.pexels.com/photos/28587807/pexels-photo-28587807/free-photo-of-traditional-turkish-coffee-brewed-in-istanbul-sand.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1";

        string prompt = "Identify objects in the image?";


        var model = CreateInitializedModel();

        var responses = new List<string>();
        await foreach (var response in model.StreamContentAsync(prompt, imageFile, "image/jpeg").ConfigureAwait(false))
        {
            response.ShouldNotBeNull();
            responses.Add(response.Text() ?? string.Empty);
            Console.WriteLine($"Chunk: {response.Text()}");
        }

        responses.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ShouldIdentifyImageWithChatAndStreaming()
    {
        // var imageFile = "image.png";
        //
        // string prompt = "Identify objects in the image?";
        //
        //
        // var model = CreateInitializedModel();
        //
        // var chatSession = model.StartChat();
        // var responses = new List<string>();
        //
        // await foreach (var response in chatSession.StreamContentAsync(prompt, imageFile))
        // {
        //     response.ShouldNotBeNull();
        //     responses.Add(response.Text() ?? string.Empty);
        //     Console.WriteLine($"Chunk: {response.Text()}");
        // }
        // var fullString = String.Join("", responses);
        //
        // responses.Count.ShouldBeGreaterThan(0);
        //
        // fullString.ShouldContain("blueberry", Case.Insensitive);
        //
        // var response2 = await chatSession.GenerateContentAsync("can you estimate number of blueberries?");
        // response2.Text().ShouldContain("blueberries", Case.Insensitive);
        // Console.WriteLine(response2.Text());
    }
}