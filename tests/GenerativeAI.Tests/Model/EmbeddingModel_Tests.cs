using System;
using System.Collections.Generic;
using System.Linq;
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
        typeof(PriorityOrderer))]
    public class GenerativeModelEmbedding_Tests : TestBase
    {
        private const string DefaultTestModelName = GoogleAIModels.TextEmbedding;

        public GenerativeModelEmbedding_Tests(ITestOutputHelper helper) : base(helper)
        {
        }

        protected override IPlatformAdapter GetTestGooglePlatform()
        {
            Assert.SkipWhen(!IsGeminiApiKeySet,GeminiTestSkipMessage); 

            return base.GetTestGooglePlatform();
        }

        /// <summary>
        /// Creates an instance of a model configured for embedding operations.
        /// </summary>
        private EmbeddingModel CreateInitializedModel()
        {
            var platform = GetTestGooglePlatform();
            return new EmbeddingModel(platform, DefaultTestModelName);
        }

        #region EmbedContentAsync Overloads

        [Fact, TestPriority(1)]
        public async Task ShouldEmbedContentWithContent()
        {
            // Arrange
            var model = CreateInitializedModel();
            var content = RequestExtensions.FormatGenerateContentInput("Embed this content", Roles.User);
            
            // Act
            var response = await model.EmbedContentAsync(content).ConfigureAwait(false);

            // Assert
            response.ShouldNotBeNull();
            response.Embedding.ShouldNotBeNull();
            Console.WriteLine("EmbedContentAsync with single Content passed.");
        }

        [Fact, TestPriority(2)]
        public async Task ShouldEmbedContentWithRequestObject()
        {
            // Arrange
            var model = CreateInitializedModel();

            var embedRequest = new EmbedContentRequest
            {
                Model = DefaultTestModelName,
                Content = RequestExtensions.FormatGenerateContentInput("Embed from request object", Roles.User),
                TaskType = TaskType.RETRIEVAL_DOCUMENT,
                Title = "Test Title",
                OutputDimensionality = 256
            };

            // Act
            var response = await model.EmbedContentAsync(embedRequest).ConfigureAwait(false);

            // Assert
            response.ShouldNotBeNull();
            response.Embedding.ShouldNotBeNull();
            Console.WriteLine("EmbedContentAsync with EmbedContentRequest including new properties passed.");
        }

        [Fact, TestPriority(3)]
        public async Task ShouldEmbedContentWithString()
        {
            // Arrange
            var model = CreateInitializedModel();
            var textToEmbed = "This is a string to embed";

            // Act
            var response = await model.EmbedContentAsync(textToEmbed).ConfigureAwait(false);

            // Assert
            response.ShouldNotBeNull();
            response.Embedding.ShouldNotBeNull();
            Console.WriteLine("EmbedContentAsync with string passed.");
        }

        [Fact, TestPriority(4)]
        public async Task ShouldEmbedContentWithParts()
        {
            // Arrange
            var model = CreateInitializedModel();
            var parts = new[]
            {
                new Part { Text = "First part" },
                new Part { Text = "Second part" }
            };

            // Act
            var response = await model.EmbedContentAsync(parts).ConfigureAwait(false);

            // Assert
            response.ShouldNotBeNull();
            response.Embedding.ShouldNotBeNull();
            Console.WriteLine("EmbedContentAsync with parts passed.");
        }

        [Fact, TestPriority(5)]
        public async Task ShouldEmbedContentWithMultipleStrings()
        {
            // Arrange
            var model = CreateInitializedModel();
            var textsToEmbed = new List<string>
            {
                "Text one",
                "Text two"
            };

            // Act
            var response = await model.EmbedContentAsync(textsToEmbed).ConfigureAwait(false);

            // Assert
            response.ShouldNotBeNull();
            response.Embedding.ShouldNotBeNull();
            Console.WriteLine("EmbedContentAsync with multiple strings passed.");
        }

        #endregion

        #region BatchEmbedContentAsync Overloads

        [Fact, TestPriority(6)]
        public async Task ShouldBatchEmbedContentWithRequests()
        {
            // Arrange
            var model = CreateInitializedModel();
            
            var requests = new List<EmbedContentRequest>
            {
                new EmbedContentRequest
                {
                    Model = DefaultTestModelName,
                    Content = RequestExtensions.FormatGenerateContentInput("First request", Roles.User)
                },
                new EmbedContentRequest
                {
                    Model = DefaultTestModelName,
                    Content = RequestExtensions.FormatGenerateContentInput("Second request", Roles.User),
                    
                    OutputDimensionality = 128
                }
            };

            // Act
            var response = await model.BatchEmbedContentAsync(requests).ConfigureAwait(false);

            // Assert
            response.ShouldNotBeNull();
            response.Embeddings.ShouldNotBeNull();
            response.Embeddings.ShouldNotBeEmpty();
            
            Console.WriteLine("BatchEmbedContentAsync with multiple requests passed.");
            foreach (var embedding in response.Embeddings)
            {
                Console.WriteLine($"[{string.Join(",",embedding.Values)}]");
            }
        }

        [Fact, TestPriority(7)]
        public async Task ShouldBatchEmbedContentWithContents()
        {
            // Arrange
            var model = CreateInitializedModel();

            var contents = new List<Content>
            {
                RequestExtensions.FormatGenerateContentInput("Batch content one", Roles.User),
                RequestExtensions.FormatGenerateContentInput("Batch content two", Roles.User)
            };

            // Act
            var response = await model.BatchEmbedContentAsync(contents).ConfigureAwait(false);

            // Assert
            response.ShouldNotBeNull();
            response.Embeddings.ShouldNotBeEmpty();
            
            Console.WriteLine("BatchEmbedContentAsync with multiple contents passed.");
            foreach (var embedding in response.Embeddings)
            {
                Console.WriteLine($"[{string.Join(",",embedding.Values)}]");
            }
        }

        #endregion
    }
}