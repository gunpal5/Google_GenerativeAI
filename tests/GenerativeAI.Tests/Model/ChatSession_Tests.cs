// using GenerativeAI.Models;
// using GenerativeAI.Types;
// using Xunit.Abstractions;
//
// namespace GenerativeAI.Tests.Model
// {
//     public class ChatSession_Tests
//     {
//         ITestOutputHelper Console;
//         public ChatSession_Tests(ITestOutputHelper helper) => this.Console = helper;
//
//         [Fact]
//         public async Task ChatSession_Run()
//         {
//             var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
//
//             var model = new GenerativeModel(apiKey);
//
//             var chat = model.StartChat(new StartChatParams());
//             var result = await chat.SendMessageAsync("Write a poem");
//             Console.WriteLine("Initial Poem\r\n");
//             Console.WriteLine(result);
//
//             var result2 = await chat.SendMessageAsync("Make it longer");
//             Console.WriteLine("\r\nLong Poem\r\n");
//             Console.WriteLine(result2);
//         }
//
//
//         [Fact]
//         public async Task ShouldChatWithStreaming()
//         {
//             var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
//
//             var model = new GenerativeModel(apiKey);
//
//             var handler = new Action<string>((a) =>
//             {
//                 Console.WriteLine(a);
//             });
//
//             var chat = model.StartChat(new StartChatParams());
//             var result = await chat.StreamContentAsync("Write a poem",handler);
//             Console.WriteLine("Initial Poem\r\n");
//             Console.WriteLine(result);
//
//             var result2 = await chat.StreamContentAsync("Make it longer", handler);
//             Console.WriteLine("\r\nLong Poem\r\n");
//             Console.WriteLine(result2);
//         }
//
//
//         [Fact]
//         public async Task ShouldWorkWithGeminiProVision()
//         {
//             var imageBytes = await File.ReadAllBytesAsync("image.png");
//
//             var imagePart = new Part()
//             {
//                 InlineData = new GenerativeContentBlob()
//                 {
//                     MimeType = "image/png",
//                     Data = Convert.ToBase64String(imageBytes)
//                 }
//             };
//             var textPart = new Part()
//             {
//                 Text = "What is in the image?"
//             };
//             var parts = new[] { textPart, imagePart };
//             //var content = new Content(new[] { textPart, imagePart }, Roles.User);
//
//             var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
//
//             var visionModel = new GeminiProVision(apiKey);
//
//
//             var chat = visionModel.StartChat(new StartChatParams());
//
//             var result = await chat.SendMessageAsync(parts);
//
//             Console.WriteLine(result.Text());
//         }
//     }
// }
