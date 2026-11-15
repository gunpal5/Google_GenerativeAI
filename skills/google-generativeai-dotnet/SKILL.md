---
name: google-generativeai-dotnet
description: Write C#/.NET code that uses Google Gemini or Vertex AI through the Google_GenerativeAI NuGet packages (GenerativeAI namespace, GoogleAi, VertexAI, GenerativeModel, MultiModalLiveClient, GenerativeAIChatClient). Use when adding Gemini features to a .NET app, upgrading Google_GenerativeAI from 3.x to 4.x, or fixing code that uses this SDK.
---

# Google GenerativeAI .NET SDK (v4)

The unofficial `Google_GenerativeAI` SDK targets the Gemini API (Google AI Studio) and Vertex AI with the same API.
Documentation: https://github.com/gunpal5/Google_GenerativeAI/wiki. A single-file copy for agents is at
https://raw.githubusercontent.com/gunpal5/Google_GenerativeAI/main/llms-full.txt. Read the relevant wiki page before
using an API you have not seen in this file.

## Packages

| Need | Package | Main namespace |
|---|---|---|
| Core (models, clients, types) | `Google_GenerativeAI` | `GenerativeAI`, `GenerativeAI.Types`, `GenerativeAI.Clients` |
| Function calling, MCP | `Google_GenerativeAI.Tools` | `GenerativeAI.Tools`, `GenerativeAI.Tools.Mcp` |
| Service account / OAuth / ADC | `Google_GenerativeAI.Auth` | `GenerativeAI.Authenticators` |
| Microsoft.Extensions.AI | `Google_GenerativeAI.Microsoft` | `GenerativeAI.Microsoft` |
| ASP.NET Core DI | `Google_GenerativeAI.Web` | `GenerativeAI.Web` |
| Live API, Live Music | `Google_GenerativeAI.Live` | `GenerativeAI.Live` |

## Core patterns

```csharp
using GenerativeAI;

// Gemini API (API key from https://aistudio.google.com/app/apikey, or GOOGLE_API_KEY)
var googleAi = new GoogleAi(apiKey);
var model = googleAi.CreateGenerativeModel(GoogleAIModels.Gemini38Flash);

// Vertex AI (Application Default Credentials by default)
var vertexAi = new VertexAI(projectId: "my-project", region: "global");
var vertexModel = vertexAi.CreateGenerativeModel(VertexAIModels.Gemini.Gemini38Flash);

var response = await model.GenerateContentAsync("Hello");
Console.WriteLine(response.Text());                       // extension method, not a property

await foreach (var chunk in model.StreamContentAsync("Tell me a story"))
    Console.Write(chunk.Text());

var chat = model.StartChat();                               // keeps history
var reply = await chat.GenerateContentAsync("Hi");

var data = await model.GenerateObjectAsync<MyDto>("Return JSON"); // JSON mode
```

## Rules that prevent common mistakes

1. **Models:** use current constants from `GoogleAIModels` / `VertexAIModels` (for example `Gemini38Flash`,
   `Gemini38Live`, `GeminiEmbedding`, `GeminiNanoBanana21`). Constants marked `[Obsolete]` are retired models; the
   obsolete message names the replacement. Never use `gemini-1.5-*`, `gemini-2.0-*`, `gemini-pro` or
   `text-embedding-004`.
2. **Vertex locations:** the newest Gemini models are served from `global`. The RAG Engine, `gemini-embedding-001`
   and tuning need a regional location such as `us-central1`.
3. **Function calling:** add tools with `model.AddFunctionTool(...)` (`QuickTool`, `[FunctionTool]` methods,
   `[GenerateJsonSchema]` interfaces, or `McpTool`). Calls run automatically (`FunctionCallingBehaviour.AutoCallFunction`).
   When building function results manually, use `Roles.User`; Gemini 3.x rejects the `function` role.
4. **Errors:** API failures throw `GenerativeAI.Exceptions.ApiException` with `ErrorCode`, `ErrorStatus` and
   `ErrorMessage`. Check `ErrorCode == 429` for rate limits.
5. **`Environment` name clash:** `GenerativeAI.Types.Environment` exists. With `using GenerativeAI.Types;`, write
   `System.Environment.GetEnvironmentVariable(...)`.
6. **Live API:** `new MultiModalLiveClient(new GoogleAIPlatformAdapter(apiKey), GoogleAIModels.Gemini38Live,
   new GenerationConfig { ResponseModalities = new List<Modality> { Modality.AUDIO } })`. Live models need the AUDIO
   modality; turn on `OutputAudioTranscriptionEnabled` to also get text. With manual activity detection, wrap
   **audio** in `SendActivityStartAsync` / `SendActivityEndAsync`; realtime text sent between the markers is rejected.
   Message types are `LiveClientMessage`, `LiveClientSetup`, `LiveServerMessage`, `LiveServerContent` (the v3
   `Bidi*` names no longer exist).
7. **Interactions API:** `googleAi.CreateInteractionsClient()`; `CreateAsync(new CreateInteractionRequest { Model =
   InteractionModels.Gemini38Flash, Input = "..." })`; read `OutputText`. Use `low`, `medium` or `high` thinking
   levels with Gemini 3.8 Flash.
8. **Media:** Veo video generation works on Vertex AI only (`VertexAI.CreateVideoGenerationModel`); Imagen is not
   available on the Gemini API. For images on the Gemini API, use a Gemini image model with
   `ResponseModalities = { Modality.TEXT, Modality.IMAGE }` or `GenerativeAIImageGenerator`.
9. **Microsoft.Extensions.AI:** `IChatClient client = new GenerativeAIChatClient(apiKey, GoogleAIModels.Gemini38Flash);`
   then `GetResponseAsync` / `GetStreamingResponseAsync` (not the old `CompleteAsync`).
10. **ASP.NET Core:** `services.AddGenerativeAI(new GenerativeAIOptions { Credentials = new GoogleAICredentials(key),
    Model = GoogleAIModels.Gemini38Flash })`, then inject `IGenerativeAiService` and call `CreateInstance()`.

## Upgrading from 3.x

Follow https://github.com/gunpal5/Google_GenerativeAI/wiki/Migration-Guide-v4. The main changes: retired model
constants are `[Obsolete]`, Live `Bidi*` types were renamed to the official SDK names, function results use the
`user` role, and `BaseModel.EmbedContentAsync` / `BatchEmbedContentAsync` overrides take a `CancellationToken`.
