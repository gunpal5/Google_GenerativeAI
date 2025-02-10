// using GenerativeAI.Classes;
// using GenerativeAI.Models;
// using GenerativeAI.Types;
// using Xunit.Abstractions;
//
// namespace GenerativeAI.Tests.Model
// {
//     public class GeminiProVision_Tests
//     {
//         private ITestOutputHelper Console;
//         public GeminiProVision_Tests(ITestOutputHelper helper)
//         {
//             this.Console = helper;
//         }
//
//         [Fact]
//         public async Task ShouldIdentifyObjectInImage()
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
//             var apiKey = Environment.GetEnvironmentVariable("Gemini_API_Key", EnvironmentVariableTarget.User);
//
//             var visionModel = new GeminiProVision(apiKey);
//
//             var result = await visionModel.GenerateContentAsync(parts);
//
//             Console.WriteLine(result.Text());
//         }
//
//         [Fact]
//         public async Task ShouldIdentifyImageWithFileObject()
//         {
//             var imageBytes = await File.ReadAllBytesAsync("image.png");
//
//             string prompt = "What is in the image?";
//
//             var apiKey = Environment.GetEnvironmentVariable("Gemini_API_Key", EnvironmentVariableTarget.User);
//
//             var visionModel = new GeminiProVision(apiKey);
//
//             var result = await visionModel.GenerateContentAsync(prompt,new FileObject(imageBytes,"image.png"));
//
//             Console.WriteLine(result.Text());
//         }
//
//         [Fact]
//         public async Task ShouldIdentifyImageWithFileObjectWithStreaming()
//         {
//             var imageBytes = await File.ReadAllBytesAsync("image.png");
//
//             string prompt = "What is in the image?";
//
//             var apiKey = Environment.GetEnvironmentVariable("Gemini_API_Key", EnvironmentVariableTarget.User);
//
//             var visionModel = new GeminiProVision(apiKey);
//
//             Action<string> handler = (a) =>
//             {
//                 Console.WriteLine(a);
//             };
//
//             var result = await visionModel.StreamContentAsync(prompt, new FileObject(imageBytes, "image.png"),handler);
//
//             Console.WriteLine("");
//             Console.WriteLine(result);
//         }
//
//         [Fact]
//         public async Task ShouldIdentifyImageWithFileObjectWithChatAndStreaming()
//         {
//             var imageBytes = await File.ReadAllBytesAsync("image.png");
//
//             string prompt = "What is in the image?";
//
//             var apiKey = Environment.GetEnvironmentVariable("Gemini_API_Key", EnvironmentVariableTarget.User);
//
//             var visionModel = new GeminiProVision(apiKey);
//
//             var chat = visionModel.StartChat(new StartChatParams());
//
//             Action<string> handler = (a) =>
//             {
//                 Console.WriteLine(a);
//             };
//
//             var result = await chat.StreamContentVisionAsync(prompt, new FileObject(imageBytes, "image.png"), handler);
//
//             Console.WriteLine("");
//             Console.WriteLine(result);
//         }
//     }
// }
