# Google_GenerativeAI.Tools

[![NuGet](https://img.shields.io/nuget/v/GenerativeAI.Tools.svg)](https://www.nuget.org/packages/Google_GenerativeAI.Tools) [![License: MIT](https://img.shields.io/github/license/gunpal5/Google_GenerativeAI)](https://github.com/gunpal5/Google_GenerativeAI/blob/main/LICENSE)

`Google_GenerativeAI.Tools` is a library that provides concrete implementations of function tools for use with the Google Generative AI SDK (specifically the unofficial C# SDK [Google_GenerativeAI](https://github.com/gunpal5/Google_GenerativeAI)). It simplifies the process of integrating function calling capabilities (similar to OpenAI's function calling) into your Gemini-powered applications. This project extends the functionality of the `IFunctionTool` interface defined in the main SDK.

## Key Features

* **Easy Function Tool Creation:** Quickly create function tools from existing C# classes and methods using the provided extension methods.  
* **Automatic Schema Generation:** Leverages the `[GenerateJsonSchema()]` attribute (from the `Google_GenerativeAI` SDK) to automatically generate the required JSON schema for your functions.  
* **Synchronous and Asynchronous Support:** Handles both synchronous and asynchronous methods.  
* **Seamless Integration:** Works directly with the `GenerativeModel` class from the `Google_GenerativeAI` SDK.  
* **Enum Support:** Correctly handles enum parameters in your function definitions.

## Getting Started

### 1. Installation

Install the `Google_GenerativeAI.Tools` NuGet package:

```bash
dotnet add package Google_GenerativeAI.Tools
```

### 2. Define Your Functions

Create a C# interface and class that defines the functions you want to expose to the Gemini model. Use the `[Description]` attribute to provide descriptions for the functions and parameters. Use the `[GenerateJsonSchema()]` attribute on the interface.

```csharp
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using GenerativeAI; // From the main Google_GenerativeAI SDK
using GenerativeAI.Tools; // From this library

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

[GenerateJsonSchema()]
public interface IWeatherFunctions
{
    [Description("Get the current weather in a given location")]
    public Weather GetCurrentWeather(
        [Description("The city and state, e.g. San Francisco, CA")]
        string location,
        Unit unit = Unit.Celsius);

    [Description("Get the current weather in a given location")]
    public Task<Weather> GetCurrentWeatherAsync(
        [Description("The city and state, e.g. San Francisco, CA")]
        string location,
        Unit unit = Unit.Celsius,
        CancellationToken cancellationToken = default);
}

public class WeatherService : IWeatherFunctions
{
    public Weather GetCurrentWeather(string location, Unit unit = Unit.Celsius)
    {
        //  In a real application, you'd fetch the weather data here.
        return new Weather
        {
            Location = location,
            Temperature = 30.0,
            Unit = unit,
            Description = "Sunny",
        };
    }

    public Task<Weather> GetCurrentWeatherAsync(string location, Unit unit = Unit.Celsius,
        CancellationToken cancellationToken = default)
    {
        // In a real application, you'd fetch the weather data asynchronously here.
        return Task.FromResult(new Weather
        {
            Location = location,
            Temperature = 22.0,
            Unit = unit,
            Description = "Sunny",
        });
    }
}
```

### 3. Create the Function Tool

Use the extension methods provided by **GenerativeAI.Tools** to create a `GenericFunctionTool` instance.

```csharp
WeatherService service = new WeatherService();
var tools = service.AsTools();  // Creates the Tool instances.
var calls = service.AsCalls();  // Creates the delegate map for function calls.
var tool = new GenericFunctionTool(tools, calls); // Combines them into a usable tool.
```

### 4. Integrate with the `GenerativeModel`

Create a `GenerativeModel` instance and add the function tool.

```csharp
// Assuming you have a method to get your API key.
string apiKey = GetTestGooglePlatform();
var model = new GenerativeModel(apiKey, GoogleAIModels.DefaultGeminiModel);
model.AddFunctionTool(tool);
```

### 5. Make Requests

You can now use `tool.GenerateToolResponseAsync` or `tool.GenerateToolResponse` for getting response from the tool.

```csharp
var request = new GenerateContentRequest();
request.AddText("What is the weather in san francisco today?");
var response = await model.GenerateToolResponseAsync(request);



var result = await model.GenerateContentAsync(response.Text());
```

Or you can simply send a prompt to the model.

```csharp
var result = await model.GenerateContentAsync("What is the weather in San Francisco today?");
Console.WriteLine(result.Text());
```

## Dependencies

- [Google_GenerativeAI](https://github.com/gunpal5/Google_GenerativeAI) (Unofficial C# Google Generative AI SDK)

## Contributing

Contributions are welcome! Please submit pull requests or open issues to discuss proposed changes.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
