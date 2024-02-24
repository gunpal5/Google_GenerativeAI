# Google GenerativeAI (Gemini)
<!-- @import "[TOC]" {cmd="toc" depthFrom=1 depthTo=6 orderedList=false} -->
[![Nuget package](https://img.shields.io/nuget/vpre/Google_GenerativeAI)](https://www.nuget.org/packages/Google_GenerativeAI)
[![License: MIT](https://img.shields.io/github/license/gunpal5/Google_GenerativeAI)](https://github.com/tryAGI/OpenAI/blob/main/LICENSE.txt)
<!-- code_chunk_output -->

- [Google GenerativeAI (Gemini)](#google-generativeai-gemini)
    - [Usage](#usage)
    - [Quick Start](#quick-start)
    - [Chat Mode](#chat-mode)
    - [Vision](#vision)
    - [Function Calling](#function-calling)
    - [Streaming](#streaming)
      - [Streaming with Generative Model](#streaming-with-generative-model)
      - [Streaming With GeminiProVision](#streaming-with-googlegeminipro)
      - [Streaming with ChatSession](#streaming-with-chatsession)
    - [ModelInfoService](#modelinfoservice)
      - [Get List of Available Models](#get-list-of-available-models)
      - [Get Model Info with Model ID](#get-model-info-with-model-id)
    - [Credits](#credits)

<!-- /code_chunk_output -->


Unofficial C# SDK based on Google GenerativeAI (Gemini Pro) REST APIs.

This package includes C# Source Generator which allows you to define functions natively through a C# interface,
and also provides extensions that make it easier to call this interface later.  
In addition to easy function implementation and readability,
it generates Args classes, extension methods to easily pass a functions to API,
and extension methods to simply call a function via json and return json.  
Currently only System.Text.Json is supported.  

### Usage

### Quick Start

1) [Obtain an API](https://makersuite.google.com/app/apikey) key to use with the Google AI SDKs.

2) Install Google_GenerativeAI Nuget Package

```
Install-Package Google_GenerativeAI
```

or

```
dotnet add package Google_GenerativeAI
```

Write some codes:

```csharp
 var apiKey = 'Your API Key';

 var model = new GenerativeModel(apiKey);
 //or var model = new GeminiProModel(apiKey)

 var res = await model.GenerateContentAsync("How are you doing?");

```


### Chat Mode

```csharp
 var apiKey = Environment.GetEnvironmentVariable("Gemini_API_Key", EnvironmentVariableTarget.User);

 var model = new GenerativeModel(apiKey);
 //or var model = new GeminiProModel(apiKey)

 var chat = model.StartChat(new StartChatParams());

 var result = await chat.SendMessageAsync("Write a poem");
 Console.WriteLine("Initial Poem\r\n");
 Console.WriteLine(result);

 var result2 = await chat.SendMessageAsync("Make it longer");
 Console.WriteLine("Long Poem\r\n");
 Console.WriteLine(result2);
 
```
### Vision

```csharp
var imageBytes = await File.ReadAllBytesAsync("image.png");

string prompt = "What is in the image?";

var apiKey = Environment.GetEnvironmentVariable("Gemini_API_Key", EnvironmentVariableTarget.User);

var visionModel = new GeminiProVision(apiKey);

var result = await visionModel.GenerateContentAsync(prompt,new FileObject(imageBytes,"image.png"));

Console.WriteLine(result.Text());

```

or

```csharp
var imageBytes = await File.ReadAllBytesAsync("image.png");

var imagePart = new Part()
{
    InlineData = new GenerativeContentBlob()
    {
        MimeType = "image/png",
        Data = Convert.ToBase64String(imageBytes)
    }
};

var textPart = new Part()
{
    Text = "What is in the image?"
};

var parts = new[] { textPart, imagePart };

var apiKey = Environment.GetEnvironmentVariable("Gemini_API_Key", EnvironmentVariableTarget.User);
var visionModel = new GeminiProVision(apiKey);
var result = await visionModel.GenerateContentAsync(parts);

Console.WriteLine(result.Text());
```

### Function Calling

```csharp
using GenerativeAI;

public enum Unit
{
    Celsius,
    Fahrenheit,
    Imperial
}

public class Weather
{
    public string Location { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public Unit Unit { get; set; }
    public string Description { get; set; } = string.Empty;
}

[GenerativeAIFunctions]
public interface IWeatherFunctions
{
    [Description("Get the current weather in a given location")]
    public Task<Weather> GetCurrentWeatherAsync(
        [Description("The city and state, e.g. San Francisco, CA")] string location,
        Unit unit = Unit.Celsius,
        CancellationToken cancellationToken = default);
}

public class WeatherService : IWeatherFunctions
{
    public Task<Weather> GetCurrentWeatherAsync(string location, Unit unit = Unit.Celsius, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new Weather
        {
            Location = location,
            Temperature = 22.0,
            Unit = unit,
            Description = "Sunny",
        });
    }
}

 WeatherService service = new WeatherService();
 
 var apiKey = Environment.GetEnvironmentVariable("Gemini_API_Key", EnvironmentVariableTarget.User);

 var model = new GenerativeModel(apiKey);

 // Add Global Functions
 model.AddGlobalFunctions(service.AsGoogleFunctions(), service.AsGoogleCalls())

 var result = await model.GenerateContentAsync("How is the weather in San Francisco today?");
 
 Console.WriteLine(result);
```
### Streaming
***streaming doesn't support Function calling***
#### Streaming with Generative Model

```csharp
var apiKey = Environment.GetEnvironmentVariable("Gemini_API_Key", EnvironmentVariableTarget.User);

var model = new GenerativeModel(apiKey);
//or var model = new GeminiProModel(apiKey);

var action = new Action<string>(s =>
{
    Console.Write(s);
});

await model.StreamContentAsync("How are you doing?",action);
```
#### Streaming With GeminiProVision

```csharp
var imageBytes = await File.ReadAllBytesAsync("image.png");

string prompt = "What is in the image?";

var apiKey = Environment.GetEnvironmentVariable("Gemini_API_Key", EnvironmentVariableTarget.User);

var visionModel = new GeminiProVision(apiKey);

var chat = visionModel.StartChat(new StartChatParams());

Action<string> handler = (a) =>
{
    Console.WriteLine(a);
};

var result = await chat.StreamContentVisionAsync(prompt, new FileObject(imageBytes, "image.png"), handler);

```

#### Streaming with ChatSession
```csharp
var apiKey = Environment.GetEnvironmentVariable("Gemini_API_Key", EnvironmentVariableTarget.User);

var model = new GenerativeModel(apiKey);

var handler = new Action<string>((a) =>
{
    Console.Write(a);
});

var chat = model.StartChat(new StartChatParams());
await chat.StreamContentAsync("Write a poem", handler);
```

### ModelInfoService
This service can be used to get all the Google Generative AI Models.

#### Get List of Available Models
```csharp
var apiKey = Environment.GetEnvironmentVariable("Gemini_API_Key", EnvironmentVariableTarget.User);

var service = new ModelInfoService(apiKey);

var models = await service.GetModelsAsync();
```

#### Get Model info with Model Id

```csharp
 var apiKey = Environment.GetEnvironmentVariable("Gemini_API_Key", EnvironmentVariableTarget.User);

 var service = new ModelInfoService(apiKey);

 var modelInfo = await service.GetModelInfoAsync("gemini-pro");
```

### Credits
Thanks to [HavenDV](https://github.com/HavenDV) for [OpenAI SDK](https://github.com/tryAGI/OpenAI)