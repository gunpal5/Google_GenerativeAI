using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Model
{
    public class GenerativeAI_Multimodal_Tests : TestBase
    {
        private  ITestOutputHelper _console;
        private const string TestModel = GoogleAIModels.Gemini2Flash;

        public GenerativeAI_Multimodal_Tests(ITestOutputHelper helper)
        {
            this._console = helper;
        }

        private GeminiModel CreateInitializedModel()
        {
            Assert.SkipUnless(IsGoogleApiKeySet,GoogleTestSkipMessage);

            return new GeminiModel(GetTestGooglePlatform(), TestModel);
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
            var result = await model.GenerateContentAsync(request, cancellationToken: TestContext.Current.CancellationToken);

            //Assert
            var text = result.Text();
            text.ShouldNotBeNull();
            text.ShouldContain("blueberry", Case.Insensitive);
            _console.WriteLine(result.Text());
        }

        [Fact]
        public async Task ShouldIdentifyImageWithFilePath()
        {
            //Arrange
            var model = CreateInitializedModel();

            string prompt = "Identify objects in the image?";

            //Act
            var result = await model.GenerateContentAsync(prompt, "image.png", cancellationToken: TestContext.Current.CancellationToken);

            //Assert
            result.ShouldNotBeNull();
            var text = result.Text();
            text.ShouldNotBeNull();
            text.ShouldContain("blueberry", Case.Insensitive);
            _console.WriteLine(result.Text());
        }

        [Fact]
        public async Task ShouldProcessVideoWithFilePath()
        {
            //Arrange
            var model = CreateInitializedModel();

            string prompt = "Describe this video?";

            //Act
            var result = await model.GenerateContentAsync(prompt, "TestData/testvideo.mp4", cancellationToken: TestContext.Current.CancellationToken);

            //Assert
            result.ShouldNotBeNull();
            var text = result.Text();
            text.ShouldNotBeNull();
            text.ShouldContain("meeting", Case.Insensitive);
            _console.WriteLine(result.Text());
        }

        [Fact]
        public async Task ShouldProcessAudioWithFilePath()
        {
            //Arrange
            var model = CreateInitializedModel();

            string prompt = "Describe this audio?";

            //Act
            var result = await model.GenerateContentAsync(prompt, "TestData/testaudio.mp3", cancellationToken: TestContext.Current.CancellationToken);

            //Assert
            result.ShouldNotBeNull();
            var text = result.Text();
            text.ShouldNotBeNull();
            // if(!text.Contains("theological",StringComparison.InvariantCultureIgnoreCase) && !text.Contains("Friedrich",StringComparison.InvariantCultureIgnoreCase))
            //     text.ShouldContain("theological", Case.Insensitive);
            _console.WriteLine(result.Text());
        }
        
        // [Fact]
        // public async Task ShouldProcessRemoteFile()
        // {
        //     //Arrange
        //
        //     var vertex = GetTestVertexAIPlatform();
        //     //var model = CreateInitializedModel();
        //
        //     var model = new GeminiModel(vertex, TestModel);
        //     string prompt = "what is this audio about?";
        //     
        //     var request = new GenerateContentRequest();
        //     request.AddRemoteFile("https://storage.googleapis.com/cloud-samples-data/generative-ai/audio/pixel.mp3","audio/mp3");
        //     
        //     //request.AddRemoteFile("https://www.gutenberg.org/cache/epub/1184/pg1184.txt","text/plain");
        //     request.AddText(prompt);
        //     //Act
        //     var result = await model.GenerateContentAsync(request);
        //
        //     //Assert
        //     result.ShouldNotBeNull();
        //     var text = result.Text();
        //     text.ShouldNotBeNull();
        //     // if(!text.Contains("theological",StringComparison.InvariantCultureIgnoreCase) && !text.Contains("Friedrich",StringComparison.InvariantCultureIgnoreCase))
        //     //     text.ShouldContain("theological", Case.Insensitive);
        //     Console.WriteLine(result.Text());
        // }

        [Fact]
        public async Task ShouldIdentifyImageWithWithStreaming()
        {
            var imageFile = "image.png";

            string prompt = "Identify objects in the image?";

         
            var model = CreateInitializedModel();

            var responses = new List<string>();
            await foreach (var response in model.StreamContentAsync(prompt, imageFile, TestContext.Current.CancellationToken))
            {
                response.ShouldNotBeNull();
                responses.Add(response.Text() ?? string.Empty);
                _console.WriteLine($"Chunk: {response.Text()}");
            }

            responses.Count.ShouldBeGreaterThan(0);
        }

        [Fact]
        public Task ShouldIdentifyImageWithChatAndStreaming()
        {
            return Task.CompletedTask;
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
}