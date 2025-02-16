# Google GenerativeAI (Gemini)
[![Nuget package](https://img.shields.io/nuget/vpre/Google_GenerativeAI)](https://www.nuget.org/packages/Google_GenerativeAI)
[![License: MIT](https://img.shields.io/github/license/gunpal5/Google_GenerativeAI)](https://github.com/tryAGI/OpenAI/blob/main/LICENSE.txt)

<!-- @import "[TOC]" {cmd="toc" depthFrom=1 depthTo=6 orderedList=false} -->
<!-- code_chunk_output -->

- [Google GenerativeAI (Gemini)](#google-generativeai-gemini)
  - [Introduction](#introduction)
  - [Usage](#usage)
  - [Quick Start](#quick-start)
    - [1. Using Google AI](#1-using-google-ai)
    - [2. Using Vertex AI](#2-using-vertex-ai)
  - [Chat Mode Using `GenerativeModel` (Example: "gemini-1.5-flash")](#chat-mode-using-generativemodel-example-gemini-15-flash)
  - [Multimodal Capabilities with Overloaded `GenerateContentAsync` Methods](#multimodal-capabilities-with-overloaded-generatecontentasync-methods)
    - [1. Generating Content with a Local File](#1-generating-content-with-a-local-file)
    - [2. Generating Content with a Remote File](#2-generating-content-with-a-remote-file)
    - [3. Initializing a Request and Attaching Files](#3-initializing-a-request-and-attaching-files)
  - [Easy JSON Handling](#easy-json-handling)
  - [Gemini Tools and Function Calling](#gemini-tools-and-function-calling)
    - [1. Inbuilt Tools (GoogleSearch, GoogleSearchRetrieval, and Code Execution):](#1-inbuilt-tools-googlesearch-googlesearchretrieval-and-code-execution)
    - [2. Function Calling:](#2-function-calling)
  - [Streaming](#streaming)
  - [4. Coming Soon:](#4-coming-soon) 
  - [Credits](#credits)
  - [Explore the Wiki](#explore-the-wiki)

<!-- /code_chunk_output -->

## Introduction

Unofficial C# SDK based on Google GenerativeAI (Gemini Pro) REST APIs.  
This new version is a complete rewrite of the previous SDK, designed to improve performance, flexibility, and ease of use. It seamlessly integrates with [LangChain.net](https://github.com/tryAGI/LangChain), providing easy methods for JSON-based interactions and function calling with Google Gemini models.

Highlights of this release include:  
1. **Complete Rewrite** – The SDK has been entirely rebuilt for improved reliability and maintainability.  
2. **LangChain.net Support** – Enables you to directly use this SDK within LangChain.net workflows.  
3. **Enhanced JSON Mode** – Includes straightforward methods to handle Google Gemini’s JSON mode.  
4. **Function Calling with Code Generator** – Simplifies function calling by providing a source generator that creates argument classes and extension methods automatically.  
5. **Multi-Modal Functionality** – Provides methods to easily incorporate text, images, and other data for multimodal operations with Google Gemini.  
6. **Vertex AI Support** – Introducing direct support for Vertex AI, including multiple authentication methods such as OAuth, Service Account, and ADC (Application Default Credentials).  
7. **New Packages** – Modularizes features to help you tailor the SDK to your needs:

| **Package**                | **Version**                                                                                                          | **Description**                                                                                                                                                     |
|----------------------------|----------------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| GenerativeAI.Tools         | [![NuGet version](https://img.shields.io/nuget/vpre/GenerativeAI.Tools)](https://www.nuget.org/packages/GenerativeAI.Tools)             | Provides function tooling and code generation using tryAgi CSharpToJsonSchema. Ideal for scenarios where you need to define functions and automate their JSON schema generation. |
| GenerativeAI.Auth          | [![NuGet version](https://img.shields.io/nuget/vpre/GenerativeAI.Auth)](https://www.nuget.org/packages/GenerativeAI.Auth)                | Offers various Google authentication mechanisms, including OAuth, Service Account, and Application Default Credentials (ADC). Streamlines credential management.     |
| GenerativeAI.Microsoft     | [![NuGet version](https://img.shields.io/nuget/vpre/GenerativeAI.Microsoft)](https://www.nuget.org/packages/GenerativeAI.Microsoft)      | Implements the IChatClient interface from Microsoft.Extensions.AI, enabling seamless integration with Microsoft’s AI ecosystem and services.                       |
| GenerativeAI.Web           | [![NuGet version](https://img.shields.io/nuget/vpre/GenerativeAI.Web)](https://www.nuget.org/packages/GenerativeAI.Web)                  | Contains extension methods to integrate GenerativeAI into .NET web applications, simplifying setup for web projects that utilize Gemini models.                     |

By merging the best of the old version with these new capabilities, the SDK provides a smoother developer experience and a wide range of features to leverage Google Gemini.

---

## Usage

Use this library to access Google Gemini (Generative AI) models easily. You can start by installing the NuGet package and obtaining the necessary API key from your Google account.

---

## Quick Start

Below are two common ways to initialize and use the SDK. For a full list of supported approaches, please refer to our [Wiki Page](https://github.com/gunpal5/Google_GenerativeAI/wiki) (replace with actual link).

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
   var googleAI = new GoogleAI("Your_API_Key");
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
6. **Full Code at a Glance"

   ```csharp
   var apiKey = "YOUR_GOOGLE_API_KEY";
   var googleAI = new GoogleAI(apiKey);

   var googleModel = googleAI.CreateGenerativeModel("models/gemini-1.5-flash");
   var googleResponse = await googleModel.GenerateContentAsync("How is the weather today?");
   Console.WriteLine("Google AI Response:");
   Console.WriteLine(googleResponse.Text());
   Console.WriteLine();
   ```


---

### 2. Using Vertex AI

1. **Install the Google Cloud SDK (CLI)**  
   By default, Vertex AI uses Application Default Credentials (ADC). Follow [Google’s official instructions](https://cloud.google.com/sdk/docs/install) to install and set up the Google Cloud CLI.

2. **Initialize VertexAI**  
   Once the SDK is set up locally, create an instance of the VertexAI class:
   ```csharp
   var vertexAI = new VertexAI();
   ```

3. **Obtain a GenerativeModel**  
   Just like with GoogleAI, choose a model name and create the generative model:
   ```csharp
   var vertexModel = vertexAI.CreateGenerativeModel("models/chat-bison-001");
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
   var vertexModel = vertexAI.CreateGenerativeModel("models/chat-bison-001");
   var vertexResponse = await vertexModel.GenerateContentAsync("Hello from Vertex AI!");
   Console.WriteLine("Vertex AI Response:");
   Console.WriteLine(vertexResponse.Text());
   ```

---

For more details and additional setup options, please refer to our [Wiki](https://github.com/gunpal5/Google_GenerativeAI/wiki).


## Chat Mode Using `GenerativeModel` (Example: "gemini-1.5-flash")

For multi-turn, conversational use cases, you can start a chat session by calling the `StartChat` method on an instance of `GenerativeModel`. You can use any of the previously mentioned initialization methods (environment variables, direct constructor, configuration files, ADC, service accounts, etc.) to set up credentials for your AI service first. Then you would:

1. Create a `GenerativeModel` instance (e.g., via `googleAI.CreateGenerativeModel(...)` or `vertexAI.CreateGenerativeModel(...)`).  
2. Call `StartChat()` on the generated model to initialize a conversation.  
3. Use `SendAsync(...)` to exchange messages in the conversation.

Below is an example using the model name "gemini-1.5-flash":

```csharp
// Example: Starting a chat session with a Google AI GenerativeModel

// 1) Initialize your AI instance (GoogleAI) with credentials or environment variables
var googleAI = new GoogleAI("YOUR_GOOGLE_API_KEY");

// 2) Create a GenerativeModel using the model name "gemini-1.5-flash"
var generativeModel = googleAI.CreateGenerativeModel("models/gemini-1.5-flash");

// 3) Start a chat session from the GenerativeModel
var chatSession = generativeModel.StartChat();

// 4) Send and receive messages
var firstResponse = await chatSession.SendAsync("Welcome to the Gemini 1.5 Flash chat!");
Console.WriteLine("First response: " + firstResponse.Text());

// Continue the conversation
var secondResponse = await chatSession.SendAsync("How can you help me with my AI development?");
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
var response = await chatSession.SendAsync("Hello from Vertex AI Chat using Gemini 1.5 Flash!");
Console.WriteLine(response.Text());
```

Each conversation preserves the context from previous messages, making it ideal for multi-turn or multi-step reasoning tasks. For more details, please check our [Wiki](https://github.com/gunpal5/Google_GenerativeAI/wiki).

---

## Multimodal Capabilities with Overloaded `GenerateContentAsync` Methods

Google Gemini models can work with more than just text – they can handle images, audio, and videos too! This opens up a lot of possibilities for developers. The GenerativeAI SDK makes it super easy to use these features.

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

For granular control, you can create a `GenerateContentRequest`, set a prompt, and attach one or more files (local or remote) before calling `GenerateContentAsync`:

```csharp
// Create a request with a text prompt
var request = new GenerateContentRequest
{
    Prompt = "Describe what's in this document"
};

// Attach a local file
request.AddInlineFile(@"C:\files\example.png");

// Attach a remote file with its MIME type
request.AddRemoteFile("https://example.com/path/to/sample.pdf", "application/pdf");

// Generate the content with attached files
var response = await geminiModel.GenerateContentAsync(request);
Console.WriteLine(response.Text());
```

With these overloads and request-based approaches, you can seamlessly integrate additional file-based context into your prompts, enabling richer answers and unlocking more advanced AI-driven workflows.


---
## Easy JSON Handling

The GenerativeAI SDK makes it simple to work with JSON data from Gemini. You have several ways some of those are:


**1. Automatic JSON Handling:**
 *   Use `GenerateObjectAsync<T>` to directly get the deserialized object:

        ```csharp
        var myObject = await model.GenerateObjectAsync<SampleJsonClass>(request);
        ```

 *   Use `GenerateContentAsync` and then `ToObject<T>` to deserialize the response:

        ```csharp
        var response = await model.GenerateContentAsync(request);
        var myObject = response.ToObject<SampleJsonClass>();
        ```


*   **Request:** Use the `UseJsonMode<T>` extension method when creating your `GenerateContentRequest`. This tells the SDK to expect a JSON response of the specified type.

    ```csharp
    var request = new GenerateContentRequest();
    request.UseJsonMode<SampleJsonClass>();
    request.AddText("Give me a really good response.");
    ```

   
**2. Manual JSON Parsing:**

*   Request: Create a standard `GenerateContentRequest`.

    ```csharp
    var request = new GenerateContentRequest();
    request.AddText("Give me some JSON.");
    ```

*   Response: Use  `ExtractJsonBlocks()` to get the raw JSON blocks from the response, and then use `ToObject<T>` to deserialize them.

    ```csharp
    var response = await model.GenerateContentAsync(request);
    var jsonBlocks = response.ExtractJsonBlocks();
    var myObjects = jsonBlocks.Select(block => block.ToObject<SampleJsonClass>());
    ```

These options give you flexibility in how you handle JSON data with the GenerativeAI SDK.

**Read the [wiki](https://github.com/gunpal5/Google_GenerativeAI/wiki) for more options.**

## Gemini Tools and Function Calling

The GenerativeAI SDK provides built-in tools to enhance Gemini's capabilities, including Google Search, Google Search Retrieval, and Code Execution.  These tools allow Gemini to interact with the outside world and perform actions beyond generating text.

**1. Inbuilt Tools (GoogleSearch, GoogleSearchRetrieval, and Code Execution):**

You can easily enable or disable these tools by setting the corresponding properties on the `GenerativeModel`:

*   `UseGoogleSearch`: Enables or disables the Google Search tool.
*   `UseGrounding`: Enables or disables the Google Search Retrieval tool (often used for grounding responses in factual information).  *Note:  The original prompt used "UseGrounding", which is likely referring to retrieval.  It's important to use the correct property name from the SDK.*
*  `UseCodeExecutionTool`: Enables or disables the Code Execution tool.

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

**2. Function Calling:**

Function calling lets you integrate custom functionality with Gemini by defining functions it can call. This requires the `GenerativeAI.Tools` package.

*   **Setup:**
    1.  Define an interface for your functions, using the `[GenerateJsonSchema()]` attribute.
    2.  Implement the interface.
    3.  Create `tools` and `calls` using `AsTools()` and `AsCalls()`.
    4.  Create a `GenericFunctionTool` instance.
    5.  Add the tool to your `GenerativeModel` with `AddFunctionTool()`.

*   **`FunctionCallingBehaviour`:** Customize behavior (e.g., auto-calling, error handling) using the `GenerativeModel`'s `FunctionCallingBehaviour` property:
    *   `FunctionEnabled` (default: `true`): Enables/disables function calling.
    *   `AutoCallFunction` (default: `true`):  Gemini automatically calls functions.
    *   `AutoReplyFunction` (default: `true`): Gemini automatically generates responses after function calls.
    *   `AutoHandleBadFunctionCalls` (default: `false`): Attempts to handle errors from incorrect calls


```csharp
// Install-Package GenerativeAI.Tools
using GenerativeAI;
using GenerativeAI.Tools;

[GenerateJsonSchema()]
public interface IWeatherFunctions // Simplified Interface
{
    [Description("Get the current weather")]
    Weather GetCurrentWeather(string location);
}

public class WeatherService : IWeatherFunctions
{  // ... (Implementation - see full example in wiki) ...
    public Weather GetCurrentWeather(string location)
      =>  new Weather
        {
            Location = location,
            Temperature = 30.0,
            Unit = Unit.Celsius,
            Description = "Sunny",
        };
}

// --- Usage ---
var service = new WeatherService();
var tools = service.AsTools();
var calls = service.AsCalls();
var tool = new GenericFunctionTool(tools, calls);
var model = new GenerativeModel(apiKey: "YOUR_API_KEY");
model.AddFunctionTool(tool);
//Example for FunctionCallingBehaviour
model.FunctionCallingBehaviour = new FunctionCallingBehaviour { AutoCallFunction = false }; // Example

var result = await model.GenerateContentAsync("Weather in SF?");
Console.WriteLine(result.Text);
```

**For more details and options, see the [wiki](https://github.com/gunpal5/Google_GenerativeAI/wiki).**

## Streaming

**3. Streaming:**

The GenerativeAI SDK supports streaming responses, allowing you to receive and process parts of the model's output as they become available, rather than waiting for the entire response to be generated. This is particularly useful for long-running generation tasks or for creating more responsive user interfaces.

* **`StreamContentAsync()`:** Use this method for streaming text responses. It returns an `IAsyncEnumerable<GenerateContentResponse>`, which you can iterate over using `await foreach`.

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

**4. Coming Soon:**

The following features are planned for future releases of the GenerativeAI SDK:

*   [ ] **Model Tuning:**  Customize Gemini models to better suit your specific needs and data.
*   [ ] **Semantic Search Retrieval (RAG):**  Use Gemini as a Retrieval-Augmented Generation (RAG) system, allowing it to incorporate information from external sources into its responses.
*   [ ] **Image Generation:** Generate images with imagen from text prompts, expanding Gemini's capabilities beyond text and code.

---

## Credits

Thanks to [HavenDV](https://github.com/HavenDV) for [LangChain.net SDK](https://github.com/tryAGI/OpenAI)

---  

## Explore the Wiki

**Dive deeper into the GenerativeAI SDK!**  The [wiki](https://github.com/gunpal5/Google_GenerativeAI/wiki) is your comprehensive resource for:

*   **Detailed Guides:**  Step-by-step tutorials on various features and use cases.
*   **Advanced Usage:**  Learn about advanced configuration options, error handling, and best practices.
*   **Complete Code Examples:**  Find ready-to-run code snippets and larger project examples.

We encourage you to explore the wiki to unlock the full potential of the GenerativeAI SDK!

---
Feel free to open an issue or submit a pull request if you encounter any problems or want to propose improvements! Your feedback helps us continue to refine and expand this SDK. 