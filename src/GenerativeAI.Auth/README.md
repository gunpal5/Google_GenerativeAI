# Google_GenerativeAI.Auth

[![NuGet](https://img.shields.io/nuget/v/Google_GenerativeAI.Auth.svg)](https://www.nuget.org/packages/Google_GenerativeAI.Auth)  [![License: MIT](https://img.shields.io/github/license/gunpal5/Google_GenerativeAI)](https://github.com/gunpal5/Google_GenerativeAI/blob/main/LICENSE)

`Google_GenerativeAI.Auth` is a helper library for the [Unofficial C# Google Generative AI SDK (Google Gemini)](https://github.com/gunpal5/Google_GenerativeAI) that simplifies authentication using OAuth and Service Accounts. It provides concrete implementations for the `IGoogleAuthenticator` interface, making it easy to integrate authentication into your applications using the SDK. This library handles the complexities of obtaining access tokens, so you can focus on building generative AI applications.

## Features

* **Service Account Authentication:** Authenticate using Google Service Accounts, either with a JSON key file or with a key and password.  
* **`IGoogleAuthenticator` Implementation:** Seamlessly integrates with the Google Generative AI SDK by implementing the `IGoogleAuthenticator` interface.  
* **Easy-to-Use API:** Provides simple methods for creating authenticators and retrieving access tokens.  
* **Asynchronous Support:** Uses `async/await` for non-blocking, asynchronous operations.

## Installation

Install the `Google_GenerativeAI.Auth` NuGet package:

```bash
dotnet add package Google_GenerativeAI.Auth
```

You will also need to install the main Google Generative AI SDK:

```bash
dotnet add package Google_GenerativeAI  
```

## Usage

The Google_GenerativeAI.Auth library provides two main ways to authenticate with a service account:

1. **Using a JSON Key File (Recommended):**  
   Uses the path to a service account JSON key file. This is the preferred method for most use cases.

2. **Using Key and Password:**  
   Requires the service account email, key, and password.

### 1. Service Account Authentication (JSON Key File)

This is the recommended approach. Download a JSON key file for your service account from the Google Cloud Console. Keep this file secure!

```csharp
// Assuming 'jsonFilePath' is the path to your service account JSON key file.
var authenticator = new GoogleServiceAccountAuthenticator(jsonFilePath);
var vertexAi = new VertexAIModel(authenticator: authenticator);
```

**Important:** `jsonFilePath` should be the full path to your JSON key file. Do not hardcode this path. Instead, retrieve it from an environment variable or a secure configuration store:

### 2. Service Account Authentication (Key and Password)

This method is less common and requires careful handling of the credentials.

```csharp
// Assuming 'email', 'key', and 'password' are your service account credentials.
var authenticator = new GoogleServiceAccountAuthenticator(email, key, password);
var vertexAi = new VertexAIModel(authenticator: authenticator);
```

**Important:** `email`, `key`, and `password` should be your actual service account credentials. Do not hardcode these values. Use environment variables or a secure configuration system:

### 2. OAuth Authentication with Client Secret and Client Id


```csharp
// Assuming 'credentialFile' your client_secret.json file.
var authenticator = new GoogleOAuthAuthenticator(credentialFile);
var vertexAi = new VertexAIModel(authenticator: authenticator);
```

### Getting the Access Token Directly

You can get the access token directly from the `IGoogleAuthenticator` if needed:

```csharp
var token = await authenticator.GetAccessTokenAsync();
Console.WriteLine(token.AccessToken); // Use the token as needed.
```

### Important Security Considerations

1. **NEVER** hardcode credentials directly in your source code. Use environment variables, configuration files, or a secrets management service (like Azure Key Vault or AWS Secrets Manager).  
2. Protect your service account JSON key file. Store it securely and do **not** commit it to version control. Treat it like a password. Add it to your `.gitignore` file.  
3. Grant the least privilege necessary to your service account. Only give it the permissions required to access the Google Generative AI APIs.

## Dependencies

- [Google Generative AI SDK (C#)](https://github.com/Google_GenerativeAI) (This is a peer dependency.)  
- Google.Apis.Auth

## Contributing

Contributions are welcome! Please open an issue to discuss proposed changes or create a pull request.

## License

MIT License
