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

## Dependencies

- [Google_GenerativeAI](https://github.com/Google_GenerativeAI) (Unofficial C# Google Generative AI SDK)  
- Microsoft.Extensions.AI  

## Contributing

Contributions are welcome! Submit pull requests or open issues.

## License

MIT License - see the LICENSE file.
