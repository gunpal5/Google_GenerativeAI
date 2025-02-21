# Google_GenerativeAI.Live

[![Nuget package](https://img.shields.io/nuget/vpre/Google_GenerativeAI)](https://www.nuget.org/packages/Google_GenerativeAI)
[![License: MIT](https://img.shields.io/github/license/gunpal5/Google_GenerativeAI)](https://github.com/gunpal5/Google_GenerativeAI/blob/main/LICENSE)

This is a part of the .NET Google_GenerativeAI SDK. It provides Google Multimodal Live API implementation using
WebSockets, enabling you to send and receive text and audio data in real-time for live conversations and interactive
applications.

## Overview

The `MultiModalLiveClient` class is the core component of this module. It manages the WebSocket connection to the Gemini API, handles message serialization and deserialization, and provides events for receiving text and audio chunks, as well as other connection-related events.

This implementation focuses on providing a robust and flexible way to interact with the Gemini Multimodal Live API. It includes features such as:

* **Real-time communication:** Send and receive text and audio data over WebSockets for a truly interactive experience.
* **Asynchronous operations:** All network operations are asynchronous, allowing your application to remain responsive.
* **Event-driven architecture:** Events are raised for various stages of the interaction, including connection status, message reception, and audio chunk arrival.
* **Audio processing:** Handles audio data in chunks, allowing for streaming audio input and output. Includes logic for handling varying sample rates and optional headers.
* **Tool integration:** Supports the use of function tools, allowing you to extend the capabilities of the model with custom code.
* **Error handling:** Includes robust error handling and provides events for error reporting.
* **Disconnection/Reconnection:** Handles disconnections gracefully and supports reconnection attempts.
* **Setup Configuration:** Allows for configuring the model, system instructions, and tools before starting the interaction.

## Getting Started

### Installation

Install the `Google_GenerativeAI.Live` NuGet package

```bash
Install-Package Google_GenerativeAI.Live
```

### Usage

There are two ways to create a `MultiModalLiveClient`:

1. **Using the constructor:**

```csharp
using GenerativeAI.Live;
using GenerativeAI.Types; // For GenerationConfig, SafetySetting, etc.
//... other necessary using statements

// Example usage:
public async Task RunGeminiLiveAsync()
{
    // 1. Initialize Platform Adapter
    IPlatformAdapter platformAdapter = new GoogleAIPlatformAdapter(); // or VertextPlatformAdapter

    // 2. Create the MultiModalLiveClient
    var client = new MultiModalLiveClient(platformAdapter, "gemini-1.5-flash-exp", // Model Name
        new GenerationConfig { ResponseModalities = { Modality.AUDIO } }, // Generation Config
        null, // Safety Settings (Optional)
        "You are a helpful assistant."); // System Instruction (Optional) 

        // 3. Event Handlers
        client.Connected += (sender, args) => Console.WriteLine("Connected!");
        client.Disconnected += (sender, args) => Console.WriteLine("Disconnected!");
        client.MessageReceived += (sender, args) =>
        {
            Console.WriteLine($"Message Received: {args.Payload}");
        };
        client.TextChunkReceived += (sender, args) =>
        {
            Console.Write(args.TextChunk);
            if (args.TurnComplete) Console.WriteLine();
        };
        client.AudioChunkReceived += (sender, args) =>
        {
            Console.WriteLine($"Audio Chunk Received: {args.Buffer.Length} bytes");
            // Process audio data
        };
        client.AudioReceiveCompleted += (sender, args) =>
        {
            Console.WriteLine($"Audio Reception Completed: {args.Buffer.Length} bytes");
            // Process complete audio data
        };
        client.ErrorOccurred += (sender, args) =>
        {
            Console.WriteLine($"Error: {args.Exception.Message}");
        };

        // 4. Connect
        await client.ConnectAsync();

        // 5. Send text input
        await client.SentTextAsync("Hello, Gemini!");

        // 6. Send Audio Input
        byte[] audioData = { /* Your audio data */ };
        await client.SendAudioAsync(audioData, "audio/pcm; rate=16000");

        // 7. Keep the application running to receive responses
        Console.ReadKey();

        // 8. Disconnect
        await client.DisconnectAsync();        

}
```

2. **Using the `GenerativeModel` extension:**

```csharp
using GenerativeAI; // For GenerativeModel
using GenerativeAI.Live;
using GenerativeAI.Types; // For GenerationConfig, SafetySetting, etc.
//... other necessary using statements

public async Task RunGeminiLiveAsync()
{
    // 1. Initialize GoogleAi
    var googleAi = new GoogleAi(apiKey);

    // 2. Initialize GenerativeModel
    var generativeModel = googleAi.CreateGenerativeModel('gemini-1.5-flash-exp')

    // 2. Create the MultiModalLiveClient using the extension method
    var client = generativeModel.CreateMultiModalLiveClient(
        new GenerationConfig { ResponseModalities = { Modality.TEXT, Modality.AUDIO } }, // Generation Config
        null, // Safety Settings (Optional)
        "You are a helpful assistant."); // System Instruction (Optional) 

    //... (rest of the code remains the same)
}
```

## Events

The `MultiModalLiveClient` class exposes several events that you can subscribe to:

- **Connected**: Raised when the client successfully connects to the API.
- **Disconnected**: Raised when the client disconnects from the API.
- **MessageReceived**: Raised when a message is received from the API.
- **TextChunkReceived**: Raised when a text chunk is received.
- **AudioChunkReceived**: Raised when an audio chunk is received.
- **AudioReceiveCompleted**: Raised when the complete audio stream is received.
- **ErrorOccurred**: Raised when an error occurs.
- **GenerationInterrupted**: Raised when the generation process is interrupted.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.

## License

This module is distributed under the MIT License.
