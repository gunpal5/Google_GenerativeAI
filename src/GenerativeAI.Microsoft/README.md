# Google_GenerativeAI.Microsoft

[![NuGet](https://img.shields.io/nuget/v/Google_GenerativeAI.Microsoft.svg)](https://www.nuget.org/packages/Google_GenerativeAI.Microsoft) [![License: MIT](https://img.shields.io/github/license/gunpal5/Google_GenerativeAI)](https://github.com/gunpal5/Google_GenerativeAI/blob/main/LICENSE)

`Google_GenerativeAI.Microsoft` implements the [`Microsoft.Extensions.AI`](https://learn.microsoft.com/dotnet/ai/microsoft-extensions-ai)
abstractions for Google Gemini, using the [Google_GenerativeAI](https://github.com/gunpal5/Google_GenerativeAI) SDK.

| Abstraction | Implementation | Default model |
|---|---|---|
| `IChatClient` | `GenerativeAIChatClient` | `GoogleAIModels.DefaultGeminiModel` |
| `IEmbeddingGenerator<string, Embedding<float>>` | `GenerativeAIEmbeddingGenerator` | `GoogleAIModels.GeminiEmbedding` |
| `IImageGenerator` | `GenerativeAIImageGenerator` (Gemini native image models) | `GoogleAIModels.GeminiNanoBanana21` |

Each type accepts either an API key (Gemini API) or an `IPlatformAdapter`, for example a `VertextPlatformAdapter` for
Vertex AI.

## Installation

```bash
dotnet add package Google_GenerativeAI.Microsoft
```

`Microsoft.Extensions.AI.Abstractions` 10.x is used on .NET 8 and .NET 9, and 11.x on .NET 10+, .NET Standard 2.0 and
.NET Framework 4.6.2.

## Chat

```csharp
using GenerativeAI;
using GenerativeAI.Microsoft;
using Microsoft.Extensions.AI;

IChatClient chatClient = new GenerativeAIChatClient(
    System.Environment.GetEnvironmentVariable("GOOGLE_API_KEY")!, GoogleAIModels.Gemini38Flash);

// Single response
ChatResponse response = await chatClient.GetResponseAsync("What is the capital of France?");
Console.WriteLine(response.Text);

// Multi-turn conversation
var history = new List<ChatMessage>
{
    new(ChatRole.System, "You are a concise assistant."),
    new(ChatRole.User, "Suggest a name for a coffee shop.")
};
response = await chatClient.GetResponseAsync(history);
history.AddMessages(response);

// Streaming
await foreach (var update in chatClient.GetStreamingResponseAsync("Tell me a short story."))
    Console.Write(update.Text);
```

### Function calling

Functions passed in `ChatOptions.Tools` are called automatically and their results are sent back to the model. Pass
`autoCallFunction: false` to the constructor if you prefer `ChatClientBuilder.UseFunctionInvocation()`.

```csharp
[Description("Gets the current weather for a city")]
static string GetWeather(string city) => $"It is 22°C and sunny in {city}.";

var options = new ChatOptions { Tools = [AIFunctionFactory.Create(GetWeather)] };
var answer = await chatClient.GetResponseAsync("What's the weather in Paris?", options);
```

### Dependency injection

```csharp
builder.Services.AddSingleton<IChatClient>(_ =>
    new GenerativeAIChatClient(builder.Configuration["Gemini:ApiKey"]!, GoogleAIModels.Gemini38Flash));
```

## Embeddings

```csharp
IEmbeddingGenerator<string, Embedding<float>> embeddings =
    new GenerativeAIEmbeddingGenerator(System.Environment.GetEnvironmentVariable("GOOGLE_API_KEY")!);

var vectors = await embeddings.GenerateAsync(["first document", "second document"]);
```

## Images

```csharp
#pragma warning disable MEAI001
IImageGenerator images = new GenerativeAIImageGenerator(System.Environment.GetEnvironmentVariable("GOOGLE_API_KEY")!);

var generated = await images.GenerateAsync(new ImageGenerationRequest { Prompt = "A watercolor lighthouse at sunrise" });
var image = generated.Contents.OfType<DataContent>().First();
await File.WriteAllBytesAsync("lighthouse.png", image.Data.ToArray());

// Gemini image models can also edit and combine existing images.
var edited = await images.GenerateAsync(new ImageGenerationRequest
{
    Prompt = "Turn this into a night scene.",
    OriginalImages = [image]
});
```

## Documentation

- [Microsoft.Extensions.AI integration guide](https://github.com/gunpal5/Google_GenerativeAI/wiki/Microsoft.Extensions.AI-IChatClient-Implementation)
- [Image Generation](https://github.com/gunpal5/Google_GenerativeAI/wiki/Image-Generation)
- [Embeddings](https://github.com/gunpal5/Google_GenerativeAI/wiki/Embeddings)

## License

MIT License. See [LICENSE](https://github.com/gunpal5/Google_GenerativeAI/blob/main/LICENSE).
