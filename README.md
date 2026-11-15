<!--
  Title: Google GenerativeAI .NET SDK
  Description: Unofficial C# .NET SDK for Google Gemini (Gemini API) and Vertex AI: content generation, multimodal input, JSON mode, function calling, MCP, Live API, Interactions API, File Search, batch, tuning, RAG and Microsoft.Extensions.AI integration.
  Author: Gunpal Jain
-->

<h1 align="center">Google GenerativeAI .NET SDK</h1>

<p align="center">
  The unofficial, full-featured .NET SDK for <b>Google Gemini</b> and <b>Vertex AI</b>.
</p>

<p align="center">
  <a href="https://www.nuget.org/packages/Google_GenerativeAI"><img alt="NuGet" src="https://img.shields.io/nuget/v/Google_GenerativeAI?label=NuGet"></a>
  <a href="https://www.nuget.org/packages/Google_GenerativeAI"><img alt="NuGet downloads" src="https://img.shields.io/nuget/dt/Google_GenerativeAI?label=Downloads"></a>
  <a href="https://github.com/gunpal5/Google_GenerativeAI/blob/main/LICENSE"><img alt="License: MIT" src="https://img.shields.io/github/license/gunpal5/Google_GenerativeAI"></a>
  <img alt=".NET" src="https://img.shields.io/badge/.NET-6%20%7C%207%20%7C%208%20%7C%209%20%7C%2010%20%7C%2011-512BD4">
  <img alt="NativeAOT" src="https://img.shields.io/badge/NativeAOT-compatible-success">
</p>

<p align="center">
  <a href="https://github.com/gunpal5/Google_GenerativeAI/wiki">Documentation</a> ·
  <a href="https://github.com/gunpal5/Google_GenerativeAI/wiki/Release-Notes-v4">What's new in v4</a> ·
  <a href="https://github.com/gunpal5/Google_GenerativeAI/wiki/Migration-Guide-v4">Migration guide</a> ·
  <a href="https://gunpal5.github.io/generative-ai/">API reference</a> ·
  <a href="samples">Samples</a>
</p>

---

## Overview

`Google_GenerativeAI` is a REST-based SDK that gives .NET developers idiomatic, strongly typed access to the
**Gemini API** (Google AI Studio) and **Vertex AI**, using one API surface for both platforms.

- **Generation:** text, chat and streaming, multimodal input (images, audio, video, PDF), JSON mode and thinking.
- **Tools:** function calling with source-generated tools, built-in tools (Google Search, code execution, URL context,
  File Search) and Model Context Protocol (MCP) servers.
- **Newer Gemini APIs:** the Interactions API, the Multimodal Live API with ephemeral tokens, Live Music, File Search,
  Batch, webhooks, model tuning and agent APIs.
- **Media:** native image generation, video generation (Veo on Vertex AI), speech generation, embeddings and
  context caching.
- **Grounding:** Vertex AI RAG Engine and Google Search grounding.
- **Ecosystem:** `Microsoft.Extensions.AI` (`IChatClient`, embeddings, images), ASP.NET Core dependency injection, and
  NativeAOT and trimming support.

## Packages

| Package | NuGet | Description |
|---|---|---|
| `Google_GenerativeAI` | [![NuGet](https://img.shields.io/nuget/v/Google_GenerativeAI)](https://www.nuget.org/packages/Google_GenerativeAI) | Core SDK for the Gemini API and Vertex AI. |
| `Google_GenerativeAI.Tools` | [![NuGet](https://img.shields.io/nuget/v/Google_GenerativeAI.Tools)](https://www.nuget.org/packages/Google_GenerativeAI.Tools) | Function calling helpers, source-generated tools and MCP integration. |
| `Google_GenerativeAI.Auth` | [![NuGet](https://img.shields.io/nuget/v/Google_GenerativeAI.Auth)](https://www.nuget.org/packages/Google_GenerativeAI.Auth) | OAuth, service account and Application Default Credentials authenticators. |
| `Google_GenerativeAI.Microsoft` | [![NuGet](https://img.shields.io/nuget/v/Google_GenerativeAI.Microsoft)](https://www.nuget.org/packages/Google_GenerativeAI.Microsoft) | `Microsoft.Extensions.AI` chat, embedding and image generator implementations. |
| `Google_GenerativeAI.Web` | [![NuGet](https://img.shields.io/nuget/v/Google_GenerativeAI.Web)](https://www.nuget.org/packages/Google_GenerativeAI.Web) | ASP.NET Core dependency injection and configuration. |
| `Google_GenerativeAI.Live` | [![NuGet](https://img.shields.io/nuget/v/Google_GenerativeAI.Live)](https://www.nuget.org/packages/Google_GenerativeAI.Live) | Real-time Multimodal Live API and Live Music over WebSockets. |

Supported frameworks: .NET 6, 7, 8, 9, 10 and 11, .NET Standard 2.0, and .NET Framework 4.6.2.

## Installation

```bash
dotnet add package Google_GenerativeAI
```

Add the other packages as you need them, for example `dotnet add package Google_GenerativeAI.Microsoft`.

## Quick start

### Gemini API

Get an API key from [Google AI Studio](https://aistudio.google.com/app/apikey).

```csharp
using GenerativeAI;

var googleAi = new GoogleAi("YOUR_API_KEY");   // or set GOOGLE_API_KEY and use new GoogleAi()
var model = googleAi.CreateGenerativeModel(GoogleAIModels.Gemini38Flash);

var response = await model.GenerateContentAsync("Explain quantum computing in one paragraph.");
Console.WriteLine(response.Text());
```

### Vertex AI

Vertex AI uses Application Default Credentials (`gcloud auth application-default login`) by default.

```csharp
using GenerativeAI;

var vertexAi = new VertexAI(projectId: "your-project", region: "global");
var model = vertexAi.CreateGenerativeModel(VertexAIModels.Gemini.Gemini38Flash);

var response = await model.GenerateContentAsync("Hello from Vertex AI!");
Console.WriteLine(response.Text());
```

> The newest Gemini models are served from the `global` location. Regional services such as the RAG Engine need a
> region like `us-central1`. See [Initialization](https://github.com/gunpal5/Google_GenerativeAI/wiki/Initialization),
> [Authentication](https://github.com/gunpal5/Google_GenerativeAI/wiki/Authentication) and
> [Configuration & Error Handling](https://github.com/gunpal5/Google_GenerativeAI/wiki/Configuration-and-Error-Handling)
> for all options, including service accounts, OAuth, express mode and environment variables.

## Features at a glance

### Chat and streaming

```csharp
var chat = model.StartChat();
var reply = await chat.GenerateContentAsync("Hi! Remember that my name is Ada.");
Console.WriteLine(reply.Text());

await foreach (var chunk in chat.StreamContentAsync("What is my name?"))
    Console.Write(chunk.Text());
```

More: [Chat Session](https://github.com/gunpal5/Google_GenerativeAI/wiki/Chat-Session) ·
[Streaming](https://github.com/gunpal5/Google_GenerativeAI/wiki/Streaming)

### Multimodal input

```csharp
var request = new GenerateContentRequest();
request.AddText("Compare the image with the PDF.");
request.AddInlineFile(@"C:\files\chart.png");                                  // local file, sent inline
request.AddRemoteFile("https://example.com/report.pdf", "application/pdf");   // remote file

var response = await model.GenerateContentAsync(request);
```

More: [Multimodal Features](https://github.com/gunpal5/Google_GenerativeAI/wiki/Multimodal-Features) ·
[Files Client](https://github.com/gunpal5/Google_GenerativeAI/wiki/Files-Client)

### Structured output (JSON mode)

```csharp
public class Recipe { public string? Name { get; set; } public List<string>? Ingredients { get; set; } }

var recipe = await model.GenerateObjectAsync<Recipe>("Give me a pancake recipe.");
```

More: [JSON Mode](https://github.com/gunpal5/Google_GenerativeAI/wiki/Json-Mode)

### Function calling

Requires `Google_GenerativeAI.Tools`. Functions run automatically and their results go back to the model.

```csharp
var weatherTool = new QuickTool(
    ([Description("City name")] string city) => $"It is 22°C and sunny in {city}.",
    "GetWeather", "Gets the current weather for a city");

model.AddFunctionTool(weatherTool);
var answer = await model.GenerateContentAsync("What's the weather in Paris?");
```

Source-generated tools (`[FunctionTool]` and `[GenerateJsonSchema]` interfaces) and built-in tools such as Google
Search and code execution are covered in
[Function Calling](https://github.com/gunpal5/Google_GenerativeAI/wiki/Function-Calling).

### Model Context Protocol (MCP)

Connect any MCP server (stdio, HTTP/SSE or a custom transport) and expose its tools to Gemini.

```csharp
var transport = McpTransportFactory.CreateStdioTransport(
    "everything", "npx", new[] { "-y", "@modelcontextprotocol/server-everything" });

using var mcpTool = await McpTool.CreateAsync(transport);
model.AddFunctionTool(mcpTool);
```

More: [MCP Integration](https://github.com/gunpal5/Google_GenerativeAI/wiki/MCP-(Model-Context-Protocol)-Integration) ·
[samples/McpIntegrationDemo](samples/McpIntegrationDemo)

### Microsoft.Extensions.AI

Requires `Google_GenerativeAI.Microsoft`.

```csharp
IChatClient chatClient = new GenerativeAIChatClient("YOUR_API_KEY", GoogleAIModels.Gemini38Flash);
var result = await chatClient.GetResponseAsync("Write a haiku about .NET.");
Console.WriteLine(result.Text);
```

More: [Microsoft.Extensions.AI](https://github.com/gunpal5/Google_GenerativeAI/wiki/Microsoft.Extensions.AI-IChatClient-Implementation) ·
[ASP.NET Core / Web](https://github.com/gunpal5/Google_GenerativeAI/wiki/Web-Application-Integration)

### Multimodal Live API

Requires `Google_GenerativeAI.Live`. Real-time, bidirectional audio and video sessions over WebSockets.

```csharp
using var live = new MultiModalLiveClient(new GoogleAIPlatformAdapter("YOUR_API_KEY"),
    GoogleAIModels.Gemini38Live,
    new GenerationConfig { ResponseModalities = new List<Modality> { Modality.AUDIO } });

live.AudioChunkReceived += (_, e) => PlayAudio(e.Buffer);
await live.ConnectAsync();
await live.SentTextAsync("Hello, Gemini!");
```

Session resumption, transcription, manual activity detection, context updates, ephemeral tokens and Live Music:
[Multimodal Live API](https://github.com/gunpal5/Google_GenerativeAI/wiki/Multimodal-Live-API) ·
[samples/TwoWayAudioCommunicationWpf](samples/TwoWayAudioCommunicationWpf)

### Interactions API

Stateful model and agent interactions with streaming, background execution and automatic continuation.

```csharp
var interactions = googleAi.CreateInteractionsClient();
var interaction = await interactions.CreateAsync(new CreateInteractionRequest
{
    Model = InteractionModels.Gemini38Flash,
    Input = "Summarize the history of the transistor.",
    Background = true
});

var completed = await interactions.WaitForCompletionAsync(interaction.Id!, timeout: TimeSpan.FromMinutes(5));
Console.WriteLine(completed.OutputText);
```

More: [Interactions API](https://github.com/gunpal5/Google_GenerativeAI/wiki/Interactions-API)

### File Search (managed RAG)

```csharp
var stores = googleAi.CreateFileSearchStoresClient();
var store = await stores.CreateFileSearchStoreAsync("product-docs");
await stores.WaitForOperationAsync(await stores.UploadToFileSearchStoreAsync(store.Name!, "manual.pdf"));

model.UseFileSearch(store.Name!);
var grounded = await model.GenerateContentAsync("How do I reset the device?");
```

More: [File Search](https://github.com/gunpal5/Google_GenerativeAI/wiki/File-Search) ·
[Vertex RAG Engine](https://github.com/gunpal5/Google_GenerativeAI/wiki/Vertex-RAG-Engine)

### More capabilities

| Capability | Documentation |
|---|---|
| Embeddings (Gemini API and Vertex AI, task types, output dimensionality) | [Embeddings](https://github.com/gunpal5/Google_GenerativeAI/wiki/Embeddings) |
| Context caching | [Context Caching](https://github.com/gunpal5/Google_GenerativeAI/wiki/Context-Caching) |
| Image generation (Gemini native image models) | [Image Generation](https://github.com/gunpal5/Google_GenerativeAI/wiki/Image-Generation) |
| Video generation (Veo on Vertex AI) | [Video Generation](https://github.com/gunpal5/Google_GenerativeAI/wiki/Video-Generation) |
| Speech generation (text-to-speech, single and multi-speaker) | [Speech Generation](https://github.com/gunpal5/Google_GenerativeAI/wiki/Speech-Generation) |
| Batch processing (inline, file, GCS and BigQuery) | [Batch API](https://github.com/gunpal5/Google_GenerativeAI/wiki/Batch-API) |
| Model tuning | [Model Tuning](https://github.com/gunpal5/Google_GenerativeAI/wiki/Model-Tuning) |
| Webhooks | [Webhooks](https://github.com/gunpal5/Google_GenerativeAI/wiki/Webhooks) |
| Agent APIs (decisions, voices, agents, environments, triggers, credentials) | [Agent APIs](https://github.com/gunpal5/Google_GenerativeAI/wiki/Agent-APIs) |
| Vertex AI RAG Engine | [Vertex RAG Engine](https://github.com/gunpal5/Google_GenerativeAI/wiki/Vertex-RAG-Engine) |
| Model and file management | [Model Client](https://github.com/gunpal5/Google_GenerativeAI/wiki/Model-Client) · [Files Client](https://github.com/gunpal5/Google_GenerativeAI/wiki/Files-Client) |
| Supported models and replacements for retired models | [Supported Models](https://github.com/gunpal5/Google_GenerativeAI/wiki/Supported-Models) |
| NativeAOT and trimming | [NativeAOT & Trimming](https://github.com/gunpal5/Google_GenerativeAI/wiki/NativeAOT-and-Trimming-Guidelines) |

## What's new in v4

- **Latest Google APIs:** the Interactions API, File Search, webhooks, ephemeral Live tokens, Live Music, model tuning,
  agent APIs, RAG Engine configuration and the newest Gemini models.
- **Live API additions:** manual activity signals, realtime text and video, context updates, session resumption and
  transcription settings.
- **Platform:** new `net10.0` and `net11.0` targets, updated dependencies (including `Microsoft.Extensions.AI` and
  `ModelContextProtocol` 2.x), and structured API error parsing.
- **Gemini 3.x compatibility:** function results are sent with the `user` role automatically.
- **Retired models:** constants for retired models are marked `[Obsolete]` and name their replacements.

v4 contains breaking changes. Read the [Migration Guide](https://github.com/gunpal5/Google_GenerativeAI/wiki/Migration-Guide-v4)
before upgrading, and the [Release Notes](https://github.com/gunpal5/Google_GenerativeAI/wiki/Release-Notes-v4) for the
full list.

## Documentation

The **[wiki](https://github.com/gunpal5/Google_GenerativeAI/wiki)** has a page for every feature, with complete
examples, platform availability notes (Gemini API or Vertex AI) and troubleshooting tips. The generated
[API reference](https://gunpal5.github.io/generative-ai/) documents every public type.

**For AI coding agents:** [`llms.txt`](llms.txt) is a documentation index, [`llms-full.txt`](llms-full.txt) contains
the whole wiki in one file, and [`skills/google-generativeai-dotnet`](skills) is an installable Claude Code skill that
teaches agents the v4 APIs. Contributors' agents should read [`AGENTS.md`](AGENTS.md).

## Samples

| Sample | Shows |
|---|---|
| [McpIntegrationDemo](samples/McpIntegrationDemo) | MCP servers over stdio and HTTP |
| [TwoWayAudioCommunicationWpf](samples/TwoWayAudioCommunicationWpf) | Real-time voice conversation with the Live API |
| [WebAppIntegration](samples/WebAppIntegration) | ASP.NET Core dependency injection |
| [VertexRAGSimpleQA](samples/VertexRAGSimpleQA) | Question answering with the Vertex AI RAG Engine |
| [JavascriptCodeExecutor](samples/JavascriptCodeExecutor) | A custom code-execution function tool |
| [SemanticRetrievalDemo](samples/SemanticRetrievalDemo) | Legacy semantic retrieval (AQA) |

## Contributing

Issues and pull requests are welcome. Please open an [issue](https://github.com/gunpal5/Google_GenerativeAI/issues) to
report a bug or discuss a feature.

## Credits

Thanks to [HavenDV](https://github.com/HavenDV) for [tryAGI/LangChain](https://github.com/tryAGI/LangChain) and
[CSharpToJsonSchema](https://github.com/tryAGI/CSharpToJsonSchema), which powers the source-generated function tools.

## License

Licensed under the [MIT License](LICENSE). This is an unofficial SDK and is not affiliated with or endorsed by Google.
