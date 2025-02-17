# Google_GenerativeAI.Web

[![NuGet](https://img.shields.io/nuget/v/Google_GenerativeAI.Web.svg)](https://www.nuget.org/packages/Google_GenerativeAI.Web) [![License: MIT](https://img.shields.io/github/license/gunpal5/Google_GenerativeAI)](https://github.com/gunpal5/Google_GenerativeAI/blob/main/LICENSE)

`Google_GenerativeAI.Web` simplifies integrating Google Generative AI (Gemini and Vertex AI) into .NET web applications (ASP.NET Core, Minimal APIs, etc.). It provides convenient extensions for `IServiceCollection` to configure and inject `IGenerativeAiService`, making it easy to use Google's generative models in your controllers, services, or other components.

## Key Features

*   **Simplified Configuration:** Handles the setup of Google Generative AI services with various configuration options, minimizing boilerplate code.  
*   **Dependency Injection:** Registers `IGenerativeAiService` for easy injection via constructor injection.  
*   **Multiple Authentication Methods:** Supports:  
    *   API Key  
    *   OAuth 2.0 Access Tokens  
    *   Google Application Default Credentials (ADC)  
    *   Google Service Account (JSON key file)  
    *   Google Service Account (PKCS12/P12 certificate file)  
    *   Google OAuth 2.0 (client secret JSON file)  
*   **Vertex AI and Gemini Support:** Automatically determines whether to use Vertex AI (if `GOOGLE_PROJECT_ID` is set) or the Gemini API.  
*   **Flexible Options:** Configure the model, region, project ID, and other settings via:
    *   Environment variables  
    *   `appsettings.json` (or other configuration providers)  
    *   Directly in code using `GenerativeAIOptions`  
*   **Options Pattern Support:** Fully compatible with the .NET Options pattern for strongly-typed configuration.  
*   **Streaming Support:** `IGenerativeAiService` (from the underlying `Google_GenerativeAI` library) provides both streaming and non-streaming methods.

## Getting Started

### 1. Installation

Install the NuGet package:

```bash
dotnet add package Google_GenerativeAI.Web
```

### 2. Configuration

Configure the library using one of the following methods.  
**Important:** Choose one authentication method. The methods are listed in order of precedence (highest to lowest).

#### 2.a. Dedicated Authentication Methods (Highest Precedence)

These methods set the `Authenticator` property of the options, which takes precedence over any API key or access token set via `Credentials`.

**Google Application Default Credentials (ADC):**  
The recommended approach for applications running on Google Cloud (Compute Engine, Cloud Run, App Engine, etc.).

```csharp
// In Program.cs or Startup.cs
using Google_GenerativeAI.Web;

builder.Services.AddGenerativeAI().builder.Services.WithGoogleAdcAuthentication(); // Or any other AddGenerativeAI overload.
```

**Google Service Account (JSON key file):**  
For applications running outside of Google Cloud, or where you need to use a specific service account.

```csharp

builder.Services.AddGenerativeAI().WithGoogleServiceAuthentication("path/to/your/service-account.json");;
```

**Google Service Account (PKCS12/P12 certificate file):**  
An alternative to the JSON key file.

```csharp
builder.Services.AddGenerativeAI().WithGoogleServiceAuthentication("[email address removed]", "path/to/certificate.p12", "your-passphrase");
```

**Google OAuth 2.0 (client secret JSON file):**  
For applications that need to access Google AI on behalf of a user.

```csharp
builder.Services.AddGenerativeAI().WithGoogleOAuthAuthentication("path/to/your/client_secret.json");
```

#### 2.b. Using GenerativeAIOptions and GoogleAICredentials

These methods set the `Credentials` property. You can configure these options via environment variables, `appsettings.json`, or directly in code.

**Environment Variables (Recommended for API Key):**

Set the following environment variables:

- GOOGLE_API_KEY: Your Google AI API key (for Gemini). Required if not using one of the authentication methods above. 
- GOOGLE_PROJECT_ID: Your Google Cloud project ID (for Vertex AI). If set, Vertex AI will be used. 
- GOOGLE_REGION: The Google Cloud region (e.g., us-central1). Defaults to us-central1.
- GOOGLE_AI_MODEL: The model name (e.g., gemini-1.5-pro-002). Defaults to gemini-1.0-pro.

```csharp
// In Program.cs or Startup.cs
builder.Services.AddGenerativeAI(); // Reads from environment variables.
```

**appsettings.json (or other configuration providers):**

```json
{
  "GenerativeAI": {
    "Credentials": {
      "ApiKey": "YOUR_API_KEY"
      // OR, for OAuth 2.0:
      // "AccessToken": "YOUR_ACCESS_TOKEN",
      // "Expiry": "2024-03-15T12:00:00Z"  // Optional.  ISO 8601 format.
    },
    "ProjectId": "YOUR_PROJECT_ID", // Optional (for Vertex AI)
    "Region": "us-central1",        // Optional (defaults to us-central1)
    "Model": "gemini-pro",          // Optional (defaults to gemini-1.0-pro)
    "IsVertex": false,              // Optional.  Set to true to force Vertex AI.
    "ExpressMode": false            // Optional
  }
}
```

```csharp
// In Program.cs or Startup.cs
builder.Services.AddGenerativeAI(builder.Configuration.GetSection("GenerativeAI"));

// OR, using a configuration path:
builder.Services.AddGenerativeAI("GenerativeAI");
```

**Directly in Code (using GenerativeAIOptions):**

```csharp
using Google_GenerativeAI.Web;
using GenerativeAI.GoogleAuth;

// API Key:
builder.Services.AddGenerativeAI(new GenerativeAIOptions
{
    Credentials = new GoogleAICredentials("YOUR_API_KEY"),
    ProjectId = "YOUR_PROJECT_ID", // Optional (for Vertex AI)
    Region = "us-central1",        // Optional
    Model = "gemini-pro"           // Optional
});

// OR, for OAuth 2.0 Access Token:
builder.Services.AddGenerativeAI(new GenerativeAIOptions
{
    Credentials = new GoogleAICredentials(null, "YOUR_ACCESS_TOKEN", DateTime.UtcNow.AddHours(1)), // Access token with expiry
    ProjectId = "YOUR_PROJECT_ID", // Optional
    Region = "us-central1",        // Optional
    Model = "gemini-pro"           // Optional
});

// OR, using an Action:
builder.Services.AddGenerativeAI(options =>
{
    options.Credentials = new GoogleAICredentials("YOUR_API_KEY");
    // OR, for OAuth 2.0:
    // options.Credentials = new GoogleAICredentials(null, "YOUR_ACCESS_TOKEN", DateTime.UtcNow.AddHours(1));
    options.ProjectId = "YOUR_PROJECT_ID"; // Optional
    options.Region = "us-central1";        // Optional
    options.Model = "gemini-pro";          // Optional
});
```

### 3. Using IGenerativeAiService

Inject `IGenerativeAiService` into your controllers, services, or other components using constructor injection:

```csharp
using GenerativeAI; // From the main Google_GenerativeAI SDK
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class MyController : Controller
{
    private readonly IGenerativeAiService _aiService;

    public MyController(IGenerativeAiService aiService)
    {
        _aiService = aiService;
    }

    [HttpGet]
    public async Task<IActionResult> GenerateText(string prompt)
    {
        var response = await _aiService.GenerateContentAsync(prompt);
        return Ok(response.Text()); // Access the generated text.
    }

    [HttpGet]
    public async Task StreamText(string prompt)
    {
        Response.ContentType = "text/plain"; // Set content type for streaming.
        await foreach (var chunk in _aiService.GenerateContentStreamAsync(prompt))
        {
            await Response.WriteAsync(chunk.Text());
            await Response.Body.FlushAsync(); // Crucial for streaming!
        }
    }
}
```

**Complete Example (ASP.NET Core Minimal API)**

```csharp
using GenerativeAI;
using Google_GenerativeAI.Web;
using GenerativeAI.GoogleAuth;

var builder = WebApplication.CreateBuilder(args);

// API Key Example (read from environment variables):
builder.Services.AddGenerativeAI();

// OR OAuth 2.0 Example (read access token however you obtain it):
//builder.Services.AddGenerativeAI(options =>
//{
//  options.Credentials = new GoogleAICredentials(null, "YOUR_ACCESS_TOKEN", DateTime.UtcNow.AddHours(1));
//});

//OR Service Account
//builder.Services.WithGoogleServiceAuthentication("path/to/your/service-account.json");
//builder.Services.AddGenerativeAI();

var app = builder.Build();

app.MapGet("/generate", async (IGenerativeAiService aiService, string prompt) =>
{
    var response = await aiService.GenerateContentAsync(prompt);
    return response.Text();
});

app.MapGet("/stream", async (IGenerativeAiService aiService, string prompt, HttpContext context) =>
{
    context.Response.ContentType = "text/plain";
    await foreach (var chunk in aiService.GenerateContentStreamAsync(prompt))
    {
        await context.Response.WriteAsync(chunk.Text());
        await context.Response.Body.FlushAsync();
    }
});

app.Run();
```

### Dependencies

- [**Google_GenerativeAI**](https://github.com/gunpal5/Google_GenerativeAI) (Unofficial C# Google Generative AI SDK)

### Contributing

Contributions are welcome! Please submit pull requests or open issues to discuss proposed changes or report bugs.

### License

MIT License - see the LICENSE file.
