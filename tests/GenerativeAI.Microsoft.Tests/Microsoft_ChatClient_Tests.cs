using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GenerativeAI.Core;
using GenerativeAI.Microsoft;
using GenerativeAI.Microsoft.Extensions;
using GenerativeAI.Tests.Base;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace GenerativeAI.Tests.Microsoft;

/// <summary>
/// Tests for <see cref="GenerativeAIChatClient"/>.
/// Demonstrates a style similar to other Generative AI tests,
/// using xUnit and Shouldly for assertions.
/// </summary>
[TestCaseOrderer(
    typeof(TestPriorityAttribute))]
public class Microsoft_ChatClient_Tests : TestBase
{
    private const string DefaultTestModelName = GoogleAIModels.DefaultGeminiModel;

    public Microsoft_ChatClient_Tests(ITestOutputHelper helper) : base(helper)
    {
    }

    #region Consoles

    /// <summary>
    /// Creates a minimal mock or fake platform adapter for testing.
    /// This can be replaced by a more feature-complete mock if needed.
    /// </summary>
    private IPlatformAdapter CreateTestPlatformAdapter()
    {
        return new GoogleAIPlatformAdapter("ldkfhkldsa hfhls");
    }

    #endregion

    #region Constructor Tests

   

    [Fact, TestPriority(1)]
    public void ShouldCreateWithBasicConstructor()
    {
        // Arrange
        var adapter = CreateTestPlatformAdapter();

        // Act
        var client = new GenerativeAIChatClient(adapter);

        // Assert
        client.ShouldNotBeNull();
        client.model.ShouldNotBeNull();
        client.model.Model.ShouldBe(DefaultTestModelName);
        Console.WriteLine("GenerativeAIChatClient created successfully with the basic constructor.");
    }

    [Fact, TestPriority(2)]
    public void ShouldCreateWithCustomModelName()
    {
        // Arrange
        var adapter = CreateTestPlatformAdapter();
        var customModel = "my-custom-model";

        // Act
        var client = new GenerativeAIChatClient(adapter, customModel);

        // Assert
        client.ShouldNotBeNull();
        client.model.ShouldNotBeNull();
        client.model.Model.ShouldBe(customModel);
        Console.WriteLine($"GenerativeAIChatClient created with custom model: {customModel}");
    }

    #endregion

    #region CompleteAsync Tests

    [Fact, TestPriority(3)]
    public async Task ShouldThrowArgumentNullExceptionWhenChatMessagesIsNull()
    {
        // Arrange
        var adapter = CreateTestPlatformAdapter();
        var client = new GenerativeAIChatClient(adapter);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
        {
            await client.GetResponseAsync((string)null!).ConfigureAwait(false);
        }).ConfigureAwait(false);
        Console.WriteLine("CompleteAsync threw ArgumentNullException as expected when chatMessages was null.");
    }

    [Fact, TestPriority(4)]
    public async Task ShouldReturnChatCompletionOnValidInput()
    {
        Assert.SkipWhen(!IsGeminiApiKeySet, GeminiTestSkipMessage);
        // Arrange
        var adapter = GetTestGooglePlatform();
        var client = new GenerativeAIChatClient(adapter);

        // We can simulate some ChatMessage list for testing:
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, "What's wrong with hitler?")
        };

        // We’ll stub out the model’s behavior by providing a minimal response
        // This would normally be mocked more extensively.
        // For demonstration, we assume GenerateContentAsync(...) works.

        // Act
        var result = await client.GetResponseAsync(messages).ConfigureAwait(false);

        // Assert
        result.ShouldNotBeNull();
        result.Choices.ShouldNotBeNull();
        Console.WriteLine(result.Choices[0].Text);


        Console.WriteLine("CompleteAsync returned a valid ChatCompletion when given valid input.");
    }

    #endregion

    #region CompleteStreamingAsync Tests

    [Fact, TestPriority(5)]
    public async Task ShouldThrowArgumentNullExceptionWhenChatMessagesIsNullForStreaming()
    {
        // Arrange
        var adapter = CreateTestPlatformAdapter();
        var client = new GenerativeAIChatClient(adapter);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
        {
            await foreach (var _ in client.GetStreamingResponseAsync((string)null!).ConfigureAwait(false))
            {
                // Should never get here
                Console.WriteLine(_.Text ?? "null");
            }
        }).ConfigureAwait(false);
        Console.WriteLine("CompleteStreamingAsync threw ArgumentNullException as expected when chatMessages was null.");
    }

    [Fact, TestPriority(6)]
    public async Task ShouldReturnStreamOfMessagesOnValidInput()
    {
        // Arrange
        var adapter = GetTestGooglePlatform();
        var client = new GenerativeAIChatClient(adapter);

        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, "write a poem")
        };

        // Act
        var updates = new List<ChatResponseUpdate>();
        await foreach (var update in client.GetStreamingResponseAsync(messages).ConfigureAwait(false))
        {
            updates.Add(update);
            Console.WriteLine(update.Text ?? "null");
        }

        // Assert
        updates.ShouldNotBeEmpty();
        Console.WriteLine("CompleteStreamingAsync returned a stream of updates on valid input.");
    }

    #endregion

    #region GetService Tests

    [Fact, TestPriority(7)]
    public void ShouldReturnSelfFromGetServiceIfTypeMatches()
    {
        // Arrange
        var adapter = CreateTestPlatformAdapter();
        var client = new GenerativeAIChatClient(adapter);

        // Act
        var service = client.GetService(typeof(GenerativeAIChatClient));

        // Assert
        service.ShouldNotBeNull();
        service.ShouldBeOfType<GenerativeAIChatClient>();
        service.ShouldBe(client);
        Console.WriteLine("GetService returned the correct instance when serviceType matches the client type.");
    }

    [Fact, TestPriority(8)]
    public void ShouldReturnNullFromGetServiceIfTypeDoesNotMatch()
    {
        // Arrange
        var adapter = GetTestGooglePlatform();
        var client = new GenerativeAIChatClient(adapter);

        // Act
        var service = client.GetService(typeof(object));

        // Assert
        service.ShouldBeNull();
        Console.WriteLine("GetService returned null when the requested serviceType did not match.");
    }

    #endregion

    #region Metadata Tests

    [Fact, TestPriority(9)]
    public void MetadataShouldBeNullByDefault()
    {
        // Arrange
        var adapter = CreateTestPlatformAdapter();
        var client = new GenerativeAIChatClient(adapter);

        // Assert
        client.GetService<ChatClientMetadata>().ShouldBeNull();
        Console.WriteLine("By default, metadata is null in GenerativeAIChatClient.");
    }

    #endregion

    protected override IPlatformAdapter GetTestGooglePlatform()
    {
        Assert.SkipWhen(!IsGeminiApiKeySet, GeminiTestSkipMessage);
        return new GoogleAIPlatformAdapter("sldakfhklash fklasdhklf");
    }
}