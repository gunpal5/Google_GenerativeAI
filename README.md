<!--
  Title: Unofficial Google GenerativeAI SDK
  Description: Unofficial C# .Net Google GenerativeAI SDK with Function Calling Support. It provides simplified APIs for accessing Google Generative AI Models and services such as Google Gemini, Imagen, Vertex AI etc.
  Author: Gunpal Jain
  -->
  
# Google GenerativeAI (Gemini)



[![License: MIT](https://img.shields.io/github/license/gunpal5/Google_GenerativeAI)](https://github.com/gunpal5/Google_GenerativeAI/blob/main/LICENSE)
[![Nuget package](https://img.shields.io/nuget/vpre/Google_GenerativeAI)](https://www.nuget.org/packages/Google_GenerativeAI)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Google_GenerativeAI)](https://www.nuget.org/packages/Google_GenerativeAI)
<!-- @import "[TOC]" {cmd="toc" depthFrom=1 depthTo=6 orderedList=false} -->
<!-- code_chunk_output -->

- [Google GenerativeAI (Gemini)](#introduction)
    - [Introduction](#introduction)
    - [Usage](#usage)
    - [Quick Start](#quick-start)
        - [1. Using Google AI](#1-using-google-ai)
        - [2. Using Vertex AI](#2-using-vertex-ai)
    - [Chat Mode](#chat-mode)
    - [Streaming](#streaming)
    - [Multimodal Capabilities with Overloaded `GenerateContentAsync` Methods](#multimodal-capabilities-with-overloaded-generatecontentasync-methods-)
        - [1. Generating Content with a Local File](#1-generating-content-with-a-local-file)
        - [2. Generating Content with a Remote File](#2-generating-content-with-a-remote-file)
        - [3. Initializing a Request and Attaching Files](#3-initializing-a-request-and-attaching-files)
    - [Easy JSON Handling](#easy-json-handling)
    - [Gemini Tools and Function Calling](#gemini-tools-and-function-calling)
        - [1. Inbuilt Tools (GoogleSearch, GoogleSearchRetrieval, and Code Execution)](#gemini-tools-and-function-calling)
        - [2. Function Calling](#gemini-tools-and-function-calling)
        - [3. MCP Server Integration](#4-mcp-model-context-protocol-server-integration)
    - [Image Generation and Captioning](#image-generation-and-captioning)
    - [Multimodal Live API](#multimodal-live-api)
    - [Retrieval-Augmented Generation](#retrieval-augmented-generation)
        - [Vertex RAG Engine](#vertex-rag-engine-support-grounded-and-contextual-ai)
        - [Semantic Search Retrieval (RAG) with Google AQA](#semantic-search-retrieval-rag-with-google-aqa)        
    - [Coming Soon:](#coming-soon)
    - [Credits](#credits)
    - [Explore the Wiki](https://github.com/gunpal5/Google_GenerativeAI/wiki)
    - [API Reference](https://gunpal5.github.io/generative-ai/)

<!-- /code_chunk_output -->

## Introduction

Unofficial C# .Net Google GenerativeAI (Gemini Pro) SDK based on REST APIs.  
This new version is a complete rewrite of the previous SDK, designed to improve performance, flexibility, and ease of
use. It seamlessly integrates with [LangChain.net](https://github.com/tryAGI/LangChain), providing easy methods for
JSON-based interactions and function calling with Google Gemini models.

Highlights of this release include:

1. **Complete Rewrite** – The SDK has been entirely rebuilt for improved reliability and maintainability.
2. **LangChain.net Support** – Enables you to directly use this SDK within LangChain.net workflows.
3. **Enhanced JSON Mode** – Includes straightforward methods to handle Google Gemini’s JSON mode.
4. **Function Calling with Code Generator** – Simplifies function calling by providing a source generator that
   creates argument classes and extension methods automatically.
5. **Multi-Modal Functionality** – Provides methods to easily incorporate text, images, and other data for multimodal
   operations with Google Gemini.
6. **Vertex AI Support** – Introducing direct support for Vertex AI, including multiple authentication methods such
   as OAuth, Service Account, and ADC (Application Default Credentials).
7. **Multimodal Live API** - Enables real-time interaction with multimodal content (text, images, audio) for dynamic and
   responsive applications.
8. **Build grounded AI:** Simple APIs for RAG with ***Vertex RAG Engine*** and ***Google AQA***.
9. **NativeAOT/Trimming** SDK is fully NativeAOT/Trimming Compatible since v2.4.0
10. **New Packages** – Modularizes features to help you tailor the SDK to your needs:

| **Package**                   | **Version**                                                                                                                                       | **Description**                                                                                                                                                                  |
|-------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Google_GenerativeAI.Tools     | [![NuGet version](https://img.shields.io/nuget/vpre/Google_GenerativeAI.Tools)](https://www.nuget.org/packages/Google_GenerativeAI.Tools)         | Provides function tooling and code generation using tryAgi CSharpToJsonSchema. Ideal for scenarios where you need to define functions and automate their JSON schema generation. |
| Google_GenerativeAI.Auth      | [![NuGet version](https://img.shields.io/nuget/vpre/Google_GenerativeAI.Auth)](https://www.nuget.org/packages/Google_GenerativeAI.Auth)           | Offers various Google authentication mechanisms, including OAuth, Service Account, and Application Default Credentials (ADC). Streamlines credential management.                 |
| Google_GenerativeAI.Microsoft | [![NuGet version](https://img.shields.io/nuget/vpre/Google_GenerativeAI.Microsoft)](https://www.nuget.org/packages/Google_GenerativeAI.Microsoft) | Implements the IChatClient interface from Microsoft.Extensions.AI, enabling seamless integration with Microsoft’s AI ecosystem and services.                                     |
| Google_GenerativeAI.Web       | [![NuGet version](https://img.shields.io/nuget/vpre/Google_GenerativeAI.Web)](https://www.nuget.org/packages/Google_GenerativeAI.Web)             | Contains extension methods to integrate GenerativeAI into .NET web applications, simplifying setup for web projects that utilize Gemini models.                                  |
| Google_GenerativeAI.Live      | [![NuGet version](https://img.shields.io/nuget/vpre/Google_GenerativeAI.Live)](https://www.nuget.org/packages/Google_GenerativeAI.Live)           | Enables Google Multimodal Live API integration for advanced realtime communication in .NET applications.                                                                         |

By merging the best of the old version with these new capabilities, the SDK provides a smoother developer experience and
a wide range of features to leverage Google Gemini.

---

## Usage

Use this library to access Google Gemini (Generative AI) models easily. You can start by installing the NuGet package
and obtaining the necessary API key from your Google account.

---

## Quick Start

Below are two common ways to initialize and use the SDK. For a full list of supported approaches, please refer to
our [Wiki Page](https://github.com/gunpal5/Google_GenerativeAI/wiki/initialization)

---

### 1. Using Google AI

1. **Obtain an API Key**  
   Visit [Google AI Studio](https://aistudio.google.com/app/apikey) and generate your API key.

2. **Install the NuGet Package**  
   You can install the package via NuGet Package Manager:
   ```powershell
   Install-Package Google_GenerativeAI
   ```
   Or using the .NET CLI:
   ```bash
   dotnet add package Google_GenerativeAI
   ```

3. **Initialize GoogleAI**  
   Provide the API key when creating an instance of the GoogleAI class:
   ```csharp
   var googleAI = new GoogleAi("Your_API_Key");
   ```

4. **Obtain a GenerativeModel**  
   Create a generative model using a model name (for example, "models/gemini-1.5-flash"):
   ```csharp
   var model = googleAI.CreateGenerativeModel("models/gemini-1.5-flash");
   ```

5. **Generate Content**  
   Call the GenerateContentAsync method to get a response:
   ```csharp
   var response = await model.GenerateContentAsync("How is the weather today?");
   Console.WriteLine(response.Text());
   ```
6. **Full Code at a Glance**

   ```csharp
   var apiKey = "YOUR_GOOGLE_API_KEY";
   var googleAI = new GoogleAi(apiKey);

   var googleModel = googleAI.CreateGenerativeModel("models/gemini-1.5-flash");
   var googleResponse = await googleModel.GenerateContentAsync("How is the weather today?");
   Console.WriteLine("Google AI Response:");
   Console.WriteLine(googleResponse.Text());
   Console.WriteLine();
   ```

---

### 2. Using Vertex AI

1. **Install the Google Cloud SDK (CLI)**  
   By default, Vertex AI uses Application Default Credentials (ADC).
   Follow [Google’s official instructions](https://cloud.google.com/sdk/docs/install) to install and set up the Google
   Cloud CLI.

2. **Initialize VertexAI**  
   Once the SDK is set up locally, create an instance of the VertexAI class:
   ```csharp
   var vertexAI = new VertexAI();
   ```

3. **Obtain a GenerativeModel**  
   Just like with GoogleAI, choose a model name and create the generative model:
   ```csharp
   var vertexModel = vertexAI.CreateGenerativeModel("models/gemini-1.5-flash");
   ```

4. **Generate Content**  
   Use the GenerateContentAsync method to produce text:
   ```csharp
   var response = await vertexModel.GenerateContentAsync("Hello from Vertex AI!");
   Console.WriteLine(response.Text());
   ```

5. **Full code at a Glance**
   ```csharp
   var vertexAI = new VertexAI(); //usage Google Cloud CLI's ADC to get the Access token
   var vertexModel = vertexAI.CreateGenerativeModel("models/gemini-1.5-flash");
   var vertexResponse = await vertexModel.GenerateContentAsync("Hello from Vertex AI!");
   Console.WriteLine("Vertex AI Response:");
   Console.WriteLine(vertexResponse.Text());
   ```

---

## Chat Mode

For multi-turn, conversational use cases, you can start a chat session by calling the `StartChat` method on an instance
of `GenerativeModel`. You can use any of the previously mentioned initialization methods (environment variables, direct
constructor, configuration files, ADC, service accounts, etc.) to set up credentials for your AI service first. Then you
would:

1. Create a `GenerativeModel` instance (e.g., via `googleAI.CreateGenerativeModel(...)` or
   `vertexAI.CreateGenerativeModel(...)`).
2. Call `StartChat()` on the generated model to initialize a conversation.
3. Use `GenerateContentAsync(...)` to exchange messages in the conversation.

Below is an example using the model name "gemini-1.5-flash":

```csharp
// Example: Starting a chat session with a Google AI GenerativeModel

// 1) Initialize your AI instance (GoogleAi) with credentials or environment variables
var googleAI = new GoogleAi("YOUR_GOOGLE_API_KEY");

// 2) Create a GenerativeModel using the model name "gemini-1.5-flash"
var generativeModel = googleAI.CreateGenerativeModel("models/gemini-1.5-flash");

// 3) Start a chat session from the GenerativeModel
var chatSession = generativeModel.StartChat();

// 4) Send and receive messages
var firstResponse = await chatSession.GenerateContentAsync("Welcome to the Gemini 1.5 Flash chat!");
Console.WriteLine("First response: " + firstResponse.Text());

// Continue the conversation
var secondResponse = await chatSession.GenerateContentAsync("How can you help me with my AI development?");
Console.WriteLine("Second response: " + secondResponse.Text());
```

The same approach applies if you’re using Vertex AI:

```csharp
// Example: Starting a chat session with a Vertex AI GenerativeModel

// 1) Initialize your AI instance (VertexAI) using one of the available authentication methods
var vertexAI = new VertexAI(); 

// 2) Create a GenerativeModel using "gemini-1.5-flash"
var generativeModel = vertexAI.CreateGenerativeModel("models/gemini-1.5-flash");

// 3) Start a chat
var chatSession = generativeModel.StartChat();

// 4) Send a chat message and read the response
var response = await chatSession.GenerateContentAsync("Hello from Vertex AI Chat using Gemini 1.5 Flash!");
Console.WriteLine(response.Text());
```

Each conversation preserves the context from previous messages, making it ideal for multi-turn or multi-step reasoning
tasks. For more details, please check our [Wiki](https://github.com/gunpal5/Google_GenerativeAI/wiki/Chat-Session).
---

## Streaming

The GenerativeAI SDK supports streaming responses, allowing you to receive and process parts of the model's output as
they become available, rather than waiting for the entire response to be generated. This is particularly useful for
long-running generation tasks or for creating more responsive user interfaces.

* **`StreamContentAsync()`:** Use this method for streaming text responses. It returns an
  `IAsyncEnumerable<GenerateContentResponse>`, which you can iterate over using `await foreach`.

**Example (`StreamContentAsync()`):**

```csharp
using GenerativeAI;

// ... (Assume model is already initialized) ...

var prompt = "Write a long story about a cat.";
await foreach (var chunk in model.StreamContentAsync(prompt))
{
    Console.Write(chunk.Text); // Print each chunk as it arrives
}
Console.WriteLine(); // Newline after the complete response
```

---

## Multimodal Capabilities with Overloaded `GenerateContentAsync` Methods

Google Gemini models can work with more than just text – they can handle images, audio, and videos too! This opens up a
lot of possibilities for developers. The GenerativeAI SDK makes it super easy to use these features.

Below are several examples showcasing how to incorporate files into your AI prompts:

1. Directly providing a local file path.
2. Referencing a remote file with its MIME type.
3. Creating a request object to add multiple files (local or remote).

### 1. Generating Content with a Local File

If you have a file available locally, simply pass in the file path:

```csharp
// Generate content from a local file (e.g., an image)
var response = await geminiModel.GenerateContentAsync(
    "Describe the details in this uploaded image",
    @"C:\path\to\local\image.jpg"
);

Console.WriteLine(response.Text());
```

### 2. Generating Content with a Remote File

When your file is hosted remotely, provide the file URI and its corresponding MIME type:

```csharp
// Generate content from a remote file (e.g., a PDF)
var response = await geminiModel.GenerateContentAsync(
    "Summarize the information in this PDF document",
    "https://example.com/path/to/sample.pdf",
    "application/pdf"
);

Console.WriteLine(response.Text());
```

### 3. Initializing a Request and Attaching Files

For granular control, you can create a `GenerateContentRequest`, set a prompt, and attach one or more files (local or
remote) before calling `GenerateContentAsync`:

```csharp
// Create a request with a text prompt
var request = new GenerateContentRequest();
request.AddText("Describe what's in this document");

// Attach a local file
request.AddInlineFile(@"C:\files\example.png");

// Attach a remote file with its MIME type
request.AddRemoteFile("https://example.com/path/to/sample.pdf", "application/pdf");

// Generate the content with attached files
var response = await geminiModel.GenerateContentAsync(request);
Console.WriteLine(response.Text());
```

With these overloads and request-based approaches, you can seamlessly integrate additional file-based context into your
prompts, enabling richer answers and unlocking more advanced AI-driven workflows.

---

## Easy JSON Handling

The GenerativeAI SDK makes it simple to work with JSON data from Gemini. You have several ways some of those are:

**1. Automatic JSON Handling:**

* Use `GenerateObjectAsync<T>` to directly get the deserialized object:

     ```csharp
     var myObject = await model.GenerateObjectAsync<SampleJsonClass>(request);
     ```


* Use `GenerateContentAsync` and then `ToObject<T>` to deserialize the response:

     ```csharp
     var response = await model.GenerateContentAsync<SampleJsonClass>(request);
     var myObject = response.ToObject<SampleJsonClass>();
     ```


* **Request:** Use the `UseJsonMode<T>` extension method when creating your `GenerateContentRequest`. This tells the SDK
  to expect a JSON response of the specified type.

  ```csharp
  var request = new GenerateContentRequest();
  request.UseJsonMode<SampleJsonClass>();
  request.AddText("Give me a really good response.");
  ```

**2. Manual JSON Parsing:**

* Request: Create a standard `GenerateContentRequest`.

  ```csharp
  var request = new GenerateContentRequest();
  request.AddText("Give me some JSON.");
  ```
  or
  ```csharp
  var request = new GenerateContentRequest();
  request.GenerationConfig = new GenerationConfig()
          {
              ResponseMimeType = "application/json",
              ResponseSchema = new SampleJsonClass()
          }
  request.AddText("Give me a really good response.");
  ```

* Response: Use  `ExtractJsonBlocks()` to get the raw JSON blocks from the response, and then use `ToObject<T>` to
  deserialize them.

  ```csharp
  var response = await model.GenerateContentAsync(request);
  var jsonBlocks = response.ExtractJsonBlocks();
  var myObjects = jsonBlocks.Select(block => block.ToObject<SampleJsonClass>());
  ```

These options give you flexibility in how you handle JSON data with the GenerativeAI SDK.

**Read the [wiki](https://github.com/gunpal5/Google_GenerativeAI/wiki/Json-Mode) for more options.**

## Gemini Tools and Function Calling

The GenerativeAI SDK provides built-in tools to enhance Gemini's capabilities, including Google Search, Google Search
Retrieval, and Code Execution. These tools allow Gemini to interact with the outside world and perform actions beyond
generating text.

**1. Inbuilt Tools (GoogleSearch, GoogleSearchRetrieval, and Code Execution):**

You can easily enable or disable these tools by setting the corresponding properties on the `GenerativeModel`:

* `UseGoogleSearch`: Enables or disables the Google Search tool.
* `UseGrounding`: Enables or disables the Google Search Retrieval tool (often used for grounding responses in factual
  information).
* `UseCodeExecutionTool`: Enables or disables the Code Execution tool.

```csharp
// Example: Enabling Google Search and Code Execution
var model = new GenerativeModel(apiKey: "YOUR_API_KEY");
model.UseGoogleSearch = true;
model.UseCodeExecutionTool = true;

// Example: Disabling all inbuilt tools.
var model = new GenerativeModel(apiKey: "YOUR_API_KEY");
model.UseGoogleSearch = false;
model.UseGrounding = false; 
model.UseCodeExecutionTool = false;
```

**2. Function Calling**

Function calling lets you integrate custom functionality with Gemini by defining functions it can call. This requires
the `GenerativeAI.Tools` package.

* **`FunctionCallingBehaviour`:** Customize behavior (e.g., auto-calling, error handling) using the `GenerativeModel`'s
  `FunctionCallingBehaviour` property:
    * `FunctionEnabled` (default: `true`): Enables/disables function calling.
    * `AutoCallFunction` (default: `true`):  Gemini automatically calls functions.
    * `AutoReplyFunction` (default: `true`): Gemini automatically generates responses after function calls.
    * `AutoHandleBadFunctionCalls` (default: `false`): Attempts to handle errors from incorrect calls

---
 - ### 1. Reflection-based QuickTool

 Quickly wrap an inline function using reflection. This approach is ideal for rapid prototyping.

```csharp
// Define a QuickTool using an inline async function
var quickTool = new QuickTool(
    async ([Description("Query a student record")] QueryStudentRecordRequest query) =>
    {
        return new StudentRecord
        {
            StudentId = "12345",
            FullName = query.FullName,
            EnrollmentDate = DateTime.UtcNow
        };
    },
    "GetStudentRecord",
    "Retrieve a student record"
);

// Add the function tool to your generative model
var model = new GenerativeModel("YOUR_API_KEY", GoogleAIModels.Gemini2Flash);
model.AddFunctionTool(quickTool);
```

---

 - ### 2. FunctionToolAttribute-based Function

 Annotate a method with `FunctionToolAttribute` for automatic tool generation. This method is best for a small set of functions defined as static methods.

```csharp
[FunctionTool(GoogleFunctionTool = true)]
[Description("Get book page content")]
public static Task<string> GetBookPageContentAsync(string bookName, int pageNumber)
{
    return Task.FromResult($"Content for {bookName} on page {pageNumber}");
}

// Create the model and add the function as a tool
var model = new GenerativeModel("YOUR_API_KEY", GoogleAIModels.Gemini2Flash);
model.AddFunctionTool(new Tools(new[] { GetBookPageContentAsync }));
```

---

 - ### 3. Interface-based Function Tools

 Define an interface for a reusable set of functions. This approach is great for structured and maintainable code.

```csharp
[GenerateJsonSchema(GoogleFunctionTool = true)]
public interface IWeatherFunctions
{
    [Description("Get current weather")]
    Weather GetCurrentWeather(string location);
}

public class WeatherService : IWeatherFunctions
{
    public Weather GetCurrentWeather(string location) =>
        new Weather { Location = location, Temperature = 25.0, Description = "Sunny" };
}

// Use the generated extension method to add the tool to your model
var service = new WeatherService();
var model = new GenerativeModel("YOUR_API_KEY", GoogleAIModels.Gemini2Flash);
model.AddFunctionTool(service.AsGoogleFunctionTool());
```

---

**For more details and options, see the [wiki](https://github.com/gunpal5/Google_GenerativeAI/wiki/Function-Calling).**

---

 - ### 4. MCP (Model Context Protocol) Server Integration

Integrate MCP servers to expose tools from any MCP-compatible server to Gemini. Supports **all transport protocols**: stdio, HTTP/SSE, and custom transports.

**Stdio Transport** (Launch MCP server as subprocess):
```csharp
// Create stdio transport
var transport = McpTransportFactory.CreateStdioTransport(
    "my-server",
    "npx",
    new[] { "-y", "@modelcontextprotocol/server-everything" }
);

using var mcpTool = await McpTool.CreateAsync(transport);

var model = new GenerativeModel("YOUR_API_KEY", GoogleAIModels.Gemini2Flash);
model.AddFunctionTool(mcpTool);
model.FunctionCallingBehaviour.AutoCallFunction = true;
```

**HTTP/SSE Transport** (Connect to remote MCP server):
```csharp
// Create HTTP transport
var transport = McpTransportFactory.CreateHttpTransport("http://localhost:8080");

// Or with authentication
var authTransport = McpTransportFactory.CreateHttpTransportWithAuth(
    "https://api.example.com",
    "your-auth-token"
);

using var mcpTool = await McpTool.CreateAsync(transport);
model.AddFunctionTool(mcpTool);
```

**Multiple MCP Servers**:
```csharp
var transports = new List<IClientTransport>
{
    McpTransportFactory.CreateStdioTransport("server1", "npx", new[] { "..." }),
    McpTransportFactory.CreateHttpTransport("http://localhost:8080")
};

var mcpTools = await McpTool.CreateMultipleAsync(transports);
foreach (var tool in mcpTools)
{
    model.AddFunctionTool(tool);
}
```

**Key Features**:
- Supports stdio, HTTP/SSE, and custom transports
- Auto-discovery of tools from MCP servers
- Multiple concurrent servers
- Auto-reconnection support
- Works with any MCP-compatible server (Node.js, Python, C#, etc.)

**See [samples/McpIntegrationDemo](samples/McpIntegrationDemo) for complete examples.**

---
## Image Generation and Captioning

The Google_GenerativeAI SDK enables seamless integration with the Google Imagen image generator and the Image Text Model
for tasks such as image captioning and visual question answering. It provides two model classes:

1. **ImagenModel** – For creating and generating entirely new images from text prompts.  
2. **ImageTextModel** – For image captioning and visual question answering (VQA).

### 2. Using Imagen

Below is a snippet demonstrating how to initialize an image generation model and generate an image:

```csharp
// 1. Create a Google AI client 
var googleAi = new GoogleAi(apiKey);

// 2. Create the Imagen model instance with your chosen model name.
var imageModel = googleAi.CreateImageModel("imagen-3.0-generate-002");

// 3. Generate images by providing a text prompt.
var response = await imageModel.GenerateImagesAsync("A peaceful forest clearing at sunrise");

// The response contains the generated image(s).
```

### 3. Using ImageTextModel

For captioning or visual QA tasks:

```csharp

// 1. Create a Vertex AI client (example shown here).
var vertexAi = new VertexAI(projecId, region);

// 2. Instantiate the ImageTextModel.
var imageTextModel = vertexAi.CreateImageTextModel();

// 3. Generate captions or perform visual QA.
var captionResult = await imageTextModel.GenerateImageCaptionFromLocalFileAsync("path/to/local/image.jpg");
var vqaResult = await imageTextModel.VisualQuestionAnsweringFromLocalFileAsync("What is in the picture?", "path/to/local/image.jpg");

// Results now contain the model's captions or answers.
```
---

## Multimodal Live API

The **Google_GenerativeAI SDK** now conveniently supports the **Google Multimodal Live API** through the
`Google_GenerativeAI.Live` package. This module enables real-time, interactive conversations with Gemini models by
leveraging **WebSockets** for text and audio data exchange. It’s ideally suited for building live, multimodal
experiences, such as chat or voice-enabled applications.

### Key Features

The `Google_GenerativeAI.Live` package provides a comprehensive implementation of the Multimodal Live API, offering:

- **Real-time Communication:** Enables two-way transmission of text and audio data for live conversational experiences.
- **Modality Support:** Allows model responses in multiple formats, including **text** and **audio**, depending on your
  configuration.
- **Asynchronous Operations:** Fully asynchronous API ensures non-blocking calls for data transmission and reception.
- **Event-driven Design:** Exposes events for key stages of interaction, including connection status, message reception,
  and audio streaming.
- **Audio Handling:** Built-in support for streaming audio, with configurability for sample rates and headers.
- **Custom Tool Integration:** Allows extending functionality by integrating custom tools directly into the interaction.
- **Robust Error Handling:** Manages errors gracefully, along with reconnection support.
- **Flexible Configuration:** Supports customizing generation configurations, safety settings, and system instructions
  before establishing a connection.

### How to Get Started

To leverage the Multimodal Live API in your project, you’ll need to install the `Google_GenerativeAI.Live` NuGet package
and create a `MultiModalLiveClient`. Here’s a quick overview:

#### Installation

Install the `Google_GenerativeAI.Live` package via NuGet:

```bash
Install-Package Google_GenerativeAI.Live
```

#### Example Usage

With the `MultiModalLiveClient`, interacting with the Multimodal Live API is simple:

```csharp
using GenerativeAI.Live;

public async Task RunLiveConversationAsync()
{
    var client = new MultiModalLiveClient(
        platformAdapter: new GoogleAIPlatformAdapter(), 
        modelName: "gemini-1.5-flash-exp", 
        generationConfig: new GenerationConfig { ResponseModalities = { Modality.TEXT, Modality.AUDIO } }, 
        safetySettings: null, 
        systemInstruction: "You are a helpful assistant."
    );

    client.Connected += (s, e) => Console.WriteLine("Connected!");
    client.TextChunkReceived += (s, e) => Console.WriteLine($"Text chunk: {e.TextChunk}");
    client.AudioChunkReceived += (s, e) => Console.WriteLine($"Audio received: {e.Buffer.Length} bytes");
    
    await client.ConnectAsync();

    await client.SentTextAsync("Hello, Gemini! What's the weather like?");
    await client.SendAudioAsync(audioData: new byte[] { /* audio bytes */ }, audioContentType: "audio/pcm; rate=16000");

    Console.ReadKey();
    await client.DisconnectAsync();
}
```

### Events

The `MultiModalLiveClient` provides various events to plug into for real-time updates during interaction:

- **Connected:** Triggered when the connection is successfully established.
- **Disconnected:** Triggered when the connection ends gracefully or abruptly.
- **MessageReceived:** Raised when any data (text or audio) is received.
- **TextChunkReceived:** Triggered when chunks of text are received in real time.
- **AudioChunkReceived:** Triggered when audio chunks are streamed from Gemini.
- **AudioReceiveCompleted:** Triggered when a complete audio response is received.
- **ErrorOccurred:** Raised when an error occurs during interaction or connection.

For more details and examples, refer to the  [wiki](https://github.com/gunpal5/Google_GenerativeAI/wiki).
---
## Retrieval Augmented Generation

The `Google_GenerativeAI` library makes implementing **Retrieval-Augmented Generation (RAG)** incredibly easy. RAG
combines the strengths of Large Language Models (LLMs) with the precision of information retrieval. Instead of relying
solely on the LLM's pre-trained knowledge, a RAG system first *retrieves* relevant information from a knowledge base (
a "corpus" of documents) and then uses that information to *augment* the LLM's response. This allows the LLM to generate
more accurate, factual, and context-aware answers.

### Vertex RAG Engine Support: Grounded and Contextual AI

Enhance your Gemini applications with the power of the Vertex RAG Engine. This integration enables your applications to provide more accurate and contextually relevant responses by leveraging your existing knowledge bases.

**Benefits:**

* **Improved Accuracy:** Gemini can now access and utilize your corpora and vector databases for more grounded responses.
* **Scalable Knowledge:** Supports various backends (Pinecone, Weaviate, etc.) and data sources (Slack, Drive, etc.) for flexible knowledge management.
* **Simplified RAG Implementation:** Seamlessly integrate RAG capabilities into your Gemini workflows.

**Code Example:**

```csharp
// Initialize VertexAI with your platform configuration.
var vertexAi = new VertexAI(GetTestVertexAIPlatform());

// Create an instance of the RAG manager for corpus operations.
var ragManager = vertexAi.CreateRagManager();

// Create a new corpus for your knowledge base.
// Optional: Use overload methods to specify a vector database (Pinecone, Weaviate, etc.).
// If no specific vector database is provided, a default one will be used.
var corpus = await ragManager.CreateCorpusAsync("My New Corpus", "My description");

// Import data into the corpus from a specified source.
// Replace GcsSource with the appropriate source (Jira, Slack, SharePoint, etc.) and configure it.
var fileSource = new GcsSource() { /* Configure your GcsSource here */ };
await ragManager.ImportFilesAsync(corpus.Name, fileSource);

// Create a Gemini generative model configured to use the created corpus for RAG.
// The corpusIdForRag parameter links the model to your knowledge base.
var model = vertexAi.CreateGenerativeModel(VertexAIModels.Gemini.Gemini2Flash, corpusIdForRag: corpus.Name);

// Generate content by querying the model.
// The model will retrieve relevant information from the corpus to provide a grounded response.
var result = await model.GenerateContentAsync("query related to the corpus");
```

**Learn More:**

For a deeper dive into using the Vertex RAG Engine with the Google_GenerativeAI SDK, please visit the [wiki page](https://github.com/gunpal5/Google_GenerativeAI/wiki/vertex-rag-engine).

### Semantic Search Retrieval (RAG) with Google AQA

This library integrates **Google's Attributed Question Answering (AQA)** model to enhance Retrieval-Augmented Generation (RAG) through powerful semantic search and question answering. AQA excels at understanding the intent behind a question and retrieving the most relevant passages from your corpus.

**Key Features:**

* **Deep Semantic Understanding:** AQA moves beyond keyword matching, capturing the nuanced meaning of queries and documents for more accurate retrieval.
* **Answer Confidence with Attribution:** AQA provides an "Answerable Probability" score, giving you insight into the model's confidence in the retrieved answer.
* **Simplified RAG Integration:** The `Google_GenerativeAI` library offers a straightforward API for corpus creation, document ingestion, and semantic search execution.

**Get Started with Google AQA for RAG:**

For a comprehensive guide on implementing semantic search retrieval with Google AQA, refer to the [wiki page](https://github.com/gunpal5/Google_GenerativeAI/wiki/Semantic-Search-Retrieval-with-Google-AQA).


## Coming Soon

The following features are planned for future releases of the GenerativeAI SDK:

*   [x] **Semantic Search Retrieval (RAG):**  Use Gemini as a Retrieval-Augmented Generation (RAG) system, allowing it
    to incorporate information from external sources into its responses. ***(Released on 20th Feb, 2025)***
*   [x] **Image Generation:** Generate images with imagen from text prompts, expanding Gemini's capabilities beyond
    text and code. ***(Added on 24th Feb, 2025)***
*   [x] **Multimodal Live API:**  Bidirectional Multimodal Live Chat with Gemini 2.0 Flash (Added on 22nd Fed, 2025)
*   [x] **Model Tuning:**  Customize Gemini models to better suit your specific needs and data. (Abandoned Supported Gemini Models are Deprecated)

---

## Credits

Thanks to [HavenDV](https://github.com/HavenDV) for [LangChain.net SDK](https://github.com/tryAGI/OpenAI)

---  

## Explore the Wiki

**Dive deeper into the GenerativeAI SDK!**  The [wiki](https://github.com/gunpal5/Google_GenerativeAI/wiki) is your
comprehensive resource for:

* **Detailed Guides:**  Step-by-step tutorials on various features and use cases.
* **Advanced Usage:**  Learn about advanced configuration options, error handling, and best practices.
* **Complete Code Examples:**  Find ready-to-run code snippets and larger project examples.

We encourage you to explore the wiki to unlock the full potential of the GenerativeAI SDK!

---
Feel free to open an issue or submit a pull request if you encounter any problems or want to propose improvements! Your
feedback helps us continue to refine and expand this SDK.
