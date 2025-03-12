# Google_GenerativeAI.Tools

[![NuGet](https://img.shields.io/nuget/v/GenerativeAI.Tools.svg)](https://www.nuget.org/packages/Google_GenerativeAI.Tools) [![License: MIT](https://img.shields.io/github/license/gunpal5/Google_GenerativeAI)](https://github.com/gunpal5/Google_GenerativeAI/blob/main/LICENSE)

# Google_GenerativeAI.Tools – README

A comprehensive toolkit to seamlessly integrate and structure your Google Gemini function calls in C#. **Google_GenerativeAI.Tools** is offered as part of the Google_GenerativeAI SDK, supporting multiple modes of usage: either through reflection-based approaches or via code generation.

---

## Table of Contents

1. [Installation](#installation)
2. [Approaches](#approaches)
    - [1. Reflection-Based](#1-reflection-based)
        - [Examples](#examples-reflection-based)
    - [2. Code Generator-Based](#2-code-generator-based)
        - [2.1 Individual Methods](#21-individual-methods)
        - [2.2 Interface-Based](#22-interface-based)
3. [Comparison](#comparison)
4. [Contributing](#contributing)
5. [License](#license)

---

## Introduction

**Google_GenerativeAI.Tools** streamlines how you define, discover, and call your functions for Google Gemini. 

Key features:
- Flexible reflection-based or code generation-based approaches.
- Automatic JSON schema generation for advanced scenarios.
- Strong typing and maintainability for large projects.
- NativeAOT-friendly APIs to ensure trimming safety.

---

## Installation

1. Install the NuGet package (assuming it's available on NuGet.org):

   ```bash
   dotnet add package Google.GenerativeAI.Tools
   ```

2. Reference the package in your project file:

   ```xml
   <ItemGroup>
     <PackageReference Include="Google.GenerativeAI.Tools" Version="x.x.x" />
   </ItemGroup>
   ```

3. Restore packages:

   ```bash
   dotnet restore
   ```

---

## Approaches

### 1. Reflection-Based

This method depends on runtime inspection of methods or delegates to generate the schema and create function tools.

**Pros**
- Rapid development.
- Minimal boilerplate, ideal for smaller or proof-of-concept projects.

**Cons**
- Requires manual handling of complex serialization contexts (e.g., `JsonSerializerContext`) for NativeAOT.
- Less structured than code generation.

#### Examples (Reflection-Based)

Use [QuickTool] or [QuickTools] to define reflection-based tools:

```csharp

public record StudentRecord
{
    public string StudentId { get; set; }
    public string FullName { get; set; }
    // ...
}

// Reflection-based delegate
var func = (async ([Description("Query to retrieve student record")] string fullName) =>
{
    // Implementation detail
    return new StudentRecord
    {
        StudentId = "12345",
        FullName = fullName
    };
});

// Create QuickTool
var quickFt = new QuickTool(func, "GetStudentRecordAsync", "Returns the student record");

var model = new GenerativeModel(/* your config */);
model.AddFunctionTool(quickFt);

// Usage
var result = await model.GenerateContentAsync("What's the student record for John Joe?");
Console.WriteLine(result.Text());
```

---

### 2. Code Generator-Based

Code generation automatically produces schemas and extension methods, saving you from manually dealing with reflection or serializer contexts.

**Pros**
- Automatic JSON schema creation (bypassing reflection).
- NativeAOT ready with minimal extra configuration.
- Clean, strongly typed approach for large or complex projects.

**Cons**
- Additional codegen step required.
- Slightly steeper initial setup.

#### 2.1 Individual Methods

Decorate your methods with `[FunctionTool]` to generate the needed schemas and integrate them:

```csharp
[FunctionTool(GoogleFunctionTool = true)]
[Description("Retrieves content of a specific book page")]
public static Task<string> GetBookPageContentAsync(
    string bookName, 
    int pageNumber, 
    CancellationToken cancellationToken = default)
{
    // Implementation
    return Task.FromResult($"Page {pageNumber} of {bookName}");
}

//Usage

var tools = new Tools([GetBookPageContentAsync]);
generativeModel.AddFunctionTool(tools);
```

Generated code will handle JSON schema definitions and produce extension methods to register with your `GenerativeModel`.

#### 2.2 Interface-Based

Build an interface to define one or more related methods. The code generator scans `[GenerateJsonSchema]` attributes to build tooling and registration code:

```csharp
[GenerateJsonSchema(GoogleFunctionTool = true)]
public interface IWeatherFunctions
{
    [Description("Get current weather in a location")]
    Weather GetCurrentWeather(
        [Description("City and state, e.g. San Francisco, CA")]
        string location, 
        Unit unit = Unit.Celsius);
    // ...   
}

public class WeatherService : IWeatherFunctions
{
    public Weather GetCurrentWeather(string location, Unit unit = Unit.Celsius)
    {
        return new Weather
        {
            Location = location,
            Temperature = 20,
            Unit = unit
        };
    }
}

// Register after code generation
var weatherService = new WeatherService();
var googleTool = weatherService.AsGoogleFunctionTool();
model.AddFunctionTool(googleTool);
```

**Interface Grouping**  
Group multiple functions in one interface, effectively creating a reusable “plugin.”

---

## Comparison

| Aspect                           | Reflection-Based                                            | Code Generator-Based                                                                       |
|----------------------------------|-------------------------------------------------------------|--------------------------------------------------------------------------------------------|
| **Setup Complexity**             | Low (quick prototyping)                                     | Moderate (attributes, interfaces, codegen)                                                |
| **Serialization Handling**       | Manual (must define `JsonSerializerContext` for complex)    | Automatic via generated code                                                               |
| **NativeAOT / Trimming**         | Extra steps to preserve reflection data                     | Designed for AOT; minimal extra configuration                                              |
| **Grouping Multiple Methods**    | Possible, but less structured                               | Interface-based approach naturally groups multiple methods as a reusable plugin           |
| **Use Cases**                    | Small or short-lived projects, quick PoCs                   | Enterprise solutions with complex needs requiring maintainability and strong typing        |

---
### Checkout [Wiki Page](https://github.com/gunpal5/Google_GenerativeAI/wiki/Function-Calling) for more in depth example uses
---
## Contributing

1. Fork this repository (if applicable).
2. Create a feature branch.
3. Submit a pull request describing your changes.

Please add tests and documentation for any new features.

---

## License

This project is available under [MIT] License. Refer to the LICENSE file for more details.