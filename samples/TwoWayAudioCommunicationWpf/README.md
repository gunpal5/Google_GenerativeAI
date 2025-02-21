# Two-Way Realtime Audio Communication with Gemini AI (WPF)

This sample demonstrates two-way audio interaction between a .NET WPF application and the Google Gemini AI model, using the [Google_GenerativeAI.Live](https://github.com/googleapis) package from the Google_GenerativeAI SDK.

## Features
- Streams microphone audio to the Gemini AI model.
- Receives and plays back AI responses audio.
- Uses .NET WPF for a graphical interface.

## Setup
1. Install [.NET 6 or later](https://dotnet.microsoft.com/).
2. Set up a [Google Cloud Project](https://cloud.google.com/) with Generative AI API enabled.
3. Provide authentication (using environment variables, for example):  
    - `GOOGLE_API_KEY="YOUR_API_KEY"` 
4. Build and run the WPF project from your IDE.

## Usage
- Launch the app.
- Press the "Start Chat" button to send microphone audio.
- Listen to the returned audio from Gemini AI.

For contributions, open an issue in the repository or submit a pull requests. The SDK is MIT licensed.
