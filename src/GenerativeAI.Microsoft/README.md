# Google_GenerativeAI.Microsoft

[![NuGet](https://img.shields.io/nuget/v/Google_GenerativeAI.Microsoft.svg)](https://www.nuget.org/packages/Google_GenerativeAI.Microsoft) [![License: MIT](https://img.shields.io/github/license/gunpal5/Google_GenerativeAI)](https://github.com/gunpal5/Google_GenerativeAI/blob/main/LICENSE)

`Google_GenerativeAI.Microsoft` provides an `IChatClient` implementation (from `Microsoft.Extensions.AI`) for the Google Generative AI SDK (C# [Google_GenerativeAI](https://github.com/gunpal5/Google_GenerativeAI)). It lets you use Google's Gemini models in apps built with Microsoft's AI abstractions.

## Key Features

* **`IChatClient` Implementation:** `GenerativeAIChatClient` class implements `IChatClient`.  
* **Easy Integration:** Works with .NET's dependency injection.
* **Streaming/Non-Streaming:** Supports `CompleteAsync` and `CompleteStreamingAsync`.
* **Prompt History:** Handles chat history within the `IChatClient`.
* **Configurable:** Configure the underlying `GenerativeModel`.

## Getting Started

### 1. Installation

```bash
dotnet add package Google_GenerativeAI.Microsoft
```

### 2. Register Services

Add GenerativeAIChatClient to your service collection (e.g., in Startup.cs or Program.cs). Store your API key securely (e.g., environment variables, user secrets). Do not hardcode it.

```csharp
using Google_GenerativeAI.Microsoft;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

// ...

public void ConfigureServices(IServiceCollection services)
{
    // Get API key from environment variables (recommended).
    string apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");

    // Basic configuration:
    services.AddScoped<IChatClient>(provider => new GenerativeAIChatClient(apiKey));    
}
```

### 3. Using the IChatClient

Inject `IChatClient` and use `GenerateMessageAsync` (non-streaming) or `StreamGenerateMessageAsync` (streaming).

```csharp
using Microsoft.Extensions.AI;
using System.Threading.Tasks;
using System.Threading;

public class MyChatService
{
    private readonly IChatClient _chatClient;

    public MyChatService(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<string> GetChatResponse(string userMessage)
    {
        // IChatClient maintains history internally. Add messages for multi-turn context.
        var chatHistory = new List<ChatMessage>
        {
            new ChatMessage(AuthorRole.User, userMessage)
        };
        return (await _chatClient.GenerateMessageAsync(chatHistory)).Content;
    }

    public async Task StreamChatResponseToConsole(string userMessage, CancellationToken cancellationToken = default)
    {
         var chatHistory = new List<ChatMessage>
         {
             new ChatMessage(AuthorRole.User, userMessage)
         };

         await foreach (var chunk in _chatClient.StreamGenerateMessageAsync(chatHistory, cancellationToken: cancellationToken))
         {
             Console.Write(chunk.Content);
         }
         Console.WriteLine();
    }
}
```

### 4. Using IImageClient

Image client can also be used from a service as above.  Here's a sample that shows it's capabilities.

```C#
using System.Diagnostics;
using GenerativeAI.Microsoft;
using Microsoft.Extensions.AI;

#pragma warning disable MEAI001
// ImageGen creates high quality initial images
IImageGenerator imageGenerator = new GenerativeAIImagenGenerator(
    Environment.GetEnvironmentVariable("GOOGLE_API_KEY"),
    "imagen-4.0-fast-generate-001");

var response = await imageGenerator.GenerateImagesAsync("A clown fish with orange and black-bordered white stripes.");
var img1 = GetImageContent(response);
SaveImage(img1, "i1.png");
ShowImage("i1.png");

response = await imageGenerator.GenerateImagesAsync("A blue tang fish, blue and black with yellow tipped fin and tail.");
var img2 = GetImageContent(response);
SaveImage(img2, "i2.png");
ShowImage("i2.png");

// Imagen cannot edit, but we can use the gemini model for that.
IImageGenerator imageGeneratorEdit = new GenerativeAIImageGenerator(
    Environment.GetEnvironmentVariable("GOOGLE_API_KEY"),
    "gemini-2.5-flash-image-preview");
var request = new ImageGenerationRequest()
{
    Prompt = "Combine the two images into a single scene.",
    OriginalImages = new[] { img1, img2 }
};
response = await imageGeneratorEdit.GenerateAsync(request);
var scene = GetImageContent(response);
SaveImage(scene, "scene.png");
ShowImage("scene.png");

response = await imageGeneratorEdit.EditImageAsync(scene, "Change the setting to a fish tank.");
var edit = GetImageContent(response);
SaveImage(edit, "edit.png");
ShowImage("edit.png");

DataContent GetImageContent(ImageGenerationResponse response) =>
    response.Contents.OfType<DataContent>().Single();

void SaveImage(DataContent content, string fileName) =>
    File.WriteAllBytes(fileName, content.Data.Span);

void ShowImage(string fileName)
{
    Process.Start(new ProcessStartInfo
    {
        FileName = fileName,
        UseShellExecute = true
    });
}
```

## Dependencies

- [Google_GenerativeAI](https://github.com/Google_GenerativeAI) (Unofficial C# Google Generative AI SDK)  
- Microsoft.Extensions.AI  

## Contributing

Contributions are welcome! Submit pull requests or open issues.

## License

MIT License - see the LICENSE file.
