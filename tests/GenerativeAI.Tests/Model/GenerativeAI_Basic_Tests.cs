using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GenerativeAI;
using GenerativeAI.Core;
using GenerativeAI.Tests.Base;
using GenerativeAI.Types;
using Shouldly;
using Xunit;

namespace GenerativeAI.Tests.Model
{
    [TestCaseOrderer(
        ordererType: typeof(PriorityOrderer))]
    public class GenerativeModel_Tests : TestBase
    {
        private const string DefaultTestModelName = "gemini-2.5-flash";

        public GenerativeModel_Tests(ITestOutputHelper helper) : base(testOutputHelper: helper)
        {
        }

        /// <summary>
        /// Helper method to create a GenerativeModel using the default Gemini model name.
        /// </summary>
        private GenerativeModel CreateInitializedModel()
        {
            Assert.SkipUnless(condition: IsGoogleApiKeySet,reason: GoogleTestSkipMessage);
            var platform = GetTestGooglePlatform();
            return new GenerativeModel(platform: platform, model: DefaultTestModelName);
        }

        #region Constructors

        [Fact, TestPriority(priority: 1)]
        public void ShouldCreateWithBasicConstructor()
        {
            // Arrange
            var platform = new GoogleAIPlatformAdapter(googleApiKey: "aldkfhlakjd");

            // Act
            var model = new GenerativeModel(platform: platform, model: DefaultTestModelName);

            // Assert
            model.ShouldNotBeNull();
            model.Model.ShouldBe(expected: DefaultTestModelName);
            Console.WriteLine(message: $"Model created with basic constructor: {DefaultTestModelName}");
        }

        [Fact, TestPriority(priority: 2)]
        public void ShouldCreateWithExtendedConstructor()
        {
            // Arrange
            var platform = new GoogleAIPlatformAdapter(googleApiKey: "aldkfhlakjd");
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
                platform: platform,
                model: DefaultTestModelName,
                config: config,
                safetySettings: safetySettings,
                systemInstruction: systemContent
            );

            // Assert
            model.ShouldNotBeNull();
            model.Model.ShouldBe(expected: DefaultTestModelName);
            model.Config.ShouldBe(expected: config);
            model.SafetySettings.ShouldBe(expected: safetySettings);
            Console.WriteLine(message: $"Model created with extended constructor: {DefaultTestModelName}");
        }

        [Fact, TestPriority(priority: 3)]
        public void ShouldCreateWithApiKeyConstructor()
        {
            // Arrange
            var apiKey = "fake-api-key";
            var modelParams = new ModelParams { Model = DefaultTestModelName };

            // Act
            var model = new GenerativeModel(apiKey: apiKey, modelParams: modelParams);

            // Assert
            model.ShouldNotBeNull();
            model.Model.ShouldBe(expected: DefaultTestModelName);

            Console.WriteLine(message: "Model created with API key constructor.");
        }

        #endregion

        #region GenerateContentAsync Overloads

        [Fact, TestPriority(priority: 5)]
        public async Task ShouldGenerateContentWithSingleContentRequest()
        {
            // Arrange
            var model = CreateInitializedModel();

            // Use RequestExtension to format single user content
            var singleContent =
                RequestExtensions.FormatGenerateContentInput(@params: "Write an inspiring paragraph about achieving dreams.");
            var request = new GenerateContentRequest(content: singleContent);

            // Act
            var response = await model.GenerateContentAsync(request: request, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            response.ShouldNotBeNull();
            response.Text().ShouldNotBeNullOrEmpty();
            Console.WriteLine(message: $"Response: {response.Text()}");
        }

        [Fact, TestPriority(priority: 6)]
        public async Task ShouldGenerateContentWithMultipleContentRequest()
        {
            // Arrange
            var model = CreateInitializedModel();

            // Use extension to format content from different inputs
            var content1 = RequestExtensions.FormatGenerateContentInput(@params: "Create a futuristic description of life on a space station.");
            var content2 = RequestExtensions.FormatGenerateContentInput(@params: "Explain the concept of time travel in a simple way.");
            var request = new GenerateContentRequest(contents: new List<Content> { content1, content2 });

            // Act
            var response = await model.GenerateContentAsync(request: request, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            response.ShouldNotBeNull();
            response.Text().ShouldNotBeNullOrEmpty();
            Console.WriteLine(message: $"Response: {response.Text()}");
        }

        [Fact, TestPriority(priority: 7)]
        public async Task ShouldGenerateContentWithString()
        {
            // Arrange
            var model = CreateInitializedModel();

            // Pass a raw string, model internally uses single-argument overload
            var response = await model.GenerateContentAsync(prompt: "Generate a poetic description of nature during autumn.", cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            response.ShouldNotBeNull();
            response.Text().ShouldNotBeNullOrEmpty();
            Console.WriteLine(message: $"Response: {response.Text()}");
        }

        [Fact, TestPriority(priority: 8)]
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
            var response = await model.GenerateContentAsync(parts: parts, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            response.ShouldNotBeNull();
            response.Text().ShouldNotBeNullOrEmpty();
            Console.WriteLine(message: $"Response: {response.Text()}");
        }

        [Fact, TestPriority(priority: 9)]
        public async Task ShouldStreamContentWithSingleContentRequest()
        {
            // Arrange
            var model = CreateInitializedModel();
            var singleContent =
                RequestExtensions.FormatGenerateContentInput(@params: "Create a short poem about the beauty of the sunset.");
            var request = new GenerateContentRequest(content: singleContent);

            // Act
            var responses = new List<string>();
            await foreach (var response in model.StreamContentAsync(request: request, cancellationToken: TestContext.Current.CancellationToken))
            {
                response.ShouldNotBeNull();
                responses.Add(item: response.Text() ?? string.Empty);
                Console.WriteLine(message: $"Chunk: {response.Text()}");
            }

            // Assert
            responses.ShouldNotBeNull();
            responses.ShouldNotBeEmpty();
        }

        [Fact, TestPriority(priority: 10)]
        public async Task ShouldStreamContentWithMultipleContentRequest()
        {
            // Arrange
            var model = CreateInitializedModel();
            var content1 =
                RequestExtensions.FormatGenerateContentInput(
                    @params: "Write a motivational quote for someone starting a new journey.");
            var content2 = RequestExtensions.FormatGenerateContentInput(@params: "Generate fun facts about space exploration.");
            var contents = new List<Content> { content1, content2 };

            // Act
            var responses = new List<string>();
            await foreach (var response in model.StreamContentAsync(contents: contents, cancellationToken: TestContext.Current.CancellationToken))
            {
                response.ShouldNotBeNull();
                responses.Add(item: response.Text() ?? string.Empty);
                Console.WriteLine(message: $"Chunk: {response.Text()}");
            }

            // Assert
            responses.ShouldNotBeNull();
            responses.ShouldNotBeEmpty();
        }

        [Fact, TestPriority(priority: 11)]
        public async Task ShouldStreamContentWithString()
        {
            // Arrange
            var model = CreateInitializedModel();
            var input = "Describe what a day in the life of an astronaut on Mars might look like.";

            // Act
            var responses = new List<string>();
            await foreach (var response in model.StreamContentAsync(prompt: input,cancellationToken: TestContext.Current.CancellationToken))
            {
                response.ShouldNotBeNull();
                responses.Add(item: response.Text() ?? string.Empty);
                Console.WriteLine(message: $"Chunk: {response.Text()}");
            }

            // Assert
            responses.ShouldNotBeNull();
            responses.ShouldNotBeEmpty();
        }

        [Fact, TestPriority(priority: 12)]
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
            await foreach (var response in model.StreamContentAsync(parts: parts, cancellationToken: TestContext.Current.CancellationToken))
            {
                response.ShouldNotBeNull();
                responses.Add(item: response.Text() ?? string.Empty);
                Console.WriteLine(message: $"Chunk: {response.Text()}");
            }

            // Assert
            responses.ShouldNotBeNull();
            responses.ShouldNotBeEmpty();
        }

        #endregion
        
        #region CountTokensAsync Overloads
        
        [Fact, TestPriority(priority: 13)]
        public async Task ShouldCountTokensWithRequest()
        {
            // Arrange
            var model = CreateInitializedModel();
        
            // Create a CountTokensRequest
            var content = RequestExtensions.FormatGenerateContentInput(@params: "Calculate the number of tokens required for this very detailed and comprehensive paragraph that spans across multiple subjects, ideas, and sentences to ensure there are sufficient tokens counted in the response.");
            var request = new CountTokensRequest
            {
                Contents = new List<Content> { content }
            };
        
            // Act
            var response = await model.CountTokensAsync(request: request, cancellationToken: TestContext.Current.CancellationToken);
        
            // Assert
            response.ShouldNotBeNull();
            response.TotalTokens.ShouldBeGreaterThan(expected: 0);
            Console.WriteLine(message: $"Total Tokens: {response.TotalTokens}");
        }
        
        [Fact, TestPriority(priority: 14)]
        public async Task ShouldCountTokensWithContents()
        {
            // Arrange
            var model = CreateInitializedModel();
        
            // Prepare Content objects
            var content1 = RequestExtensions.FormatGenerateContentInput(@params: "First input content for token counting. This content includes a comprehensive explanation of various techniques used in counting tokens in different scenarios and environments.");
            var content2 = RequestExtensions.FormatGenerateContentInput(@params: "Second input content. It describes the significance of token counts in large scale systems, emphasizing the role they play in accurate calculations.");
            var contents = new List<Content> { content1, content2 };
        
            // Act
            var response = await model.CountTokensAsync(contents: contents, cancellationToken: TestContext.Current.CancellationToken);
        
            // Assert
            response.ShouldNotBeNull();
            response.TotalTokens.ShouldBeGreaterThan(expected: 0);
            Console.WriteLine(message: $"Total Tokens: {response.TotalTokens}");
        }
        
        [Fact, TestPriority(priority: 15)]
        public async Task ShouldCountTokensWithParts()
        {
            // Arrange
            var model = CreateInitializedModel();
        
            // Create a collection of Parts
            var parts = new[]
            {
                new Part { Text = "Count tokens for this part that contains an elaborate discussion about the different types of tokens and their respective usage in the text generation models." },
                new Part { Text = "And for this part as well, which elaborates further on the tokenization process used in complex and lengthy content types with detailed breakdowns." }
            };
        
            // Act
            var response = await model.CountTokensAsync(parts: parts, cancellationToken: TestContext.Current.CancellationToken);
        
            // Assert
            response.ShouldNotBeNull();
            response.TotalTokens.ShouldBeGreaterThan(expected: 0);
            Console.WriteLine(message: $"Total Tokens: {response.TotalTokens}");
        }
        
        [Fact, TestPriority(priority: 16)]
        public async Task ShouldCountTokensWithGenerateContentRequest()
        {
            // Arrange
            var model = CreateInitializedModel();
        
            // Create a GenerateContentRequest
            var singleContent = RequestExtensions.FormatGenerateContentInput(@params: "Token count should be calculated here for this large piece of content that includes a detailed description, analysis, and examples of how token counting is used in AI-generated responses, particularly in models designed to understand and generate natural language.");
            
            var generateRequest = new GenerateContentRequest(content: singleContent);
            
            // Act
            var response = await model.CountTokensAsync(generateContentRequest: generateRequest, cancellationToken: TestContext.Current.CancellationToken);
        
            // Assert
            response.ShouldNotBeNull();
            response.TotalTokens.ShouldBeGreaterThan(expected: 0);
            Console.WriteLine(message: $"Total Tokens: {response.TotalTokens}");
        }
        
        
      
        #endregion

        protected override IPlatformAdapter GetTestGooglePlatform()
        {
            Assert.SkipUnless(condition: IsGoogleApiKeySet,reason: GoogleTestSkipMessage);
            return base.GetTestGooglePlatform();
        }
    }
}