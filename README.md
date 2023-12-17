# Google GenerativeAI (Gemini)
<!-- @import "[TOC]" {cmd="toc" depthFrom=1 depthTo=6 orderedList=false} -->

<!-- code_chunk_output -->

- [Google GenerativeAI (Gemini)](#google-generativeai-gemini)
    - [Usage](#usage)
    - [Quick Start](#quick-start)
    - [Function Calling](#function-calling)

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

```csharp
 var apiKey = 'Your API Key';

 var model = new GenerativeModel(apiKey);

 var res = await model.GenerateContentAsync("How are you doing?");

```

### Function Calling

```csharp
using GenerativeAI;

public enum Unit
{
    Celsius,
    Fahrenheit,
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

 var model = new GenerativeModel(apiKey, functions:service.AsFunctions(),calls:service.AsCalls());

 var result = await model.GenerateContentAsync("What is the weather in San Francisco today?");
 
 Console.WriteLine(result);
```

