// using GenerativeAI.Helpers;
// using GenerativeAI.Models;
// using GenerativeAI.Types;
// using Shouldly;
// using System.Reflection;
// using Xunit.Abstractions;
//
// namespace GenerativeAI.Tests.Model
// {
//     public class GeminiPro_Tests
//     {
//         private ITestOutputHelper Console;
//         public GeminiPro_Tests(ITestOutputHelper helper) => this.Console = helper;
//
//         [Fact]
//         public async Task ShouldGenerateResult()
//         {
//             var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
//
//             var model = new GenerativeModel(apiKey);
//
//             var res = await model.GenerateContentAsync("How are you doing?");
//
//             res.ShouldNotBeNullOrEmpty();
//
//             Console.WriteLine(res);
//         }
//
//         [Fact]
//         public async Task ShouldGenerateResultWithFlash()
//         {
//             var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
//
//             var model = new Gemini15Flash(apiKey);
//
//             var res = await model.GenerateContentAsync("How are you doing?");
//
//             res.ShouldNotBeNullOrEmpty();
//
//             Console.WriteLine(res);
//         }
//
//         [Fact]
//         public async Task ShouldGenerateResultWith15Pro()
//         {
//             var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
//
//             var model = new Gemini15Pro(apiKey);
//
//             var res = await model.GenerateContentAsync("How are you doing?");
//
//             res.ShouldNotBeNullOrEmpty();
//
//             Console.WriteLine(res);
//         }
//
//         [Fact]
//         public async Task SystemInstructionTests()
//         {
//             var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
//
//             var SystemInstruction =
//                 "You are a coding expert that specializes in rendering code for front-end interfaces. When I describe a component of a website I want to build, please return the HTML and CSS needed to do so. Do not give an explanation for this code. Also offer some UI design suggestions.";
//             var prompt =
//                 "Create a box in the middle of the page that contains a rotating selection of images each with a caption. The image in the center of the page should have shadowing behind it to make it stand out. It should also link to another page of the site. Leave the URL blank so that I can fill it in.";
//
//             var model = new Gemini15Flash(apiKey, systemInstruction:SystemInstruction);
//
//             var res = await model.GenerateContentAsync(prompt);
//
//             res.ShouldNotBeNullOrEmpty();
//
//             Console.WriteLine(res);
//         }
//
//         [Fact]
//         public async Task ShouldGenerateStreamResponse()
//         {
//             var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
//
//             var model = new GenerativeModel(apiKey);
//
//             var action = new Action<string>(s =>
//             {
//                 Console.WriteLine(s);
//             });
//
//             await model.StreamContentAsync("How are you doing?",action);
//
//         }
//
//         [Fact]
//         public async Task ShouldGenerateStreamResponseWithParts()
//         {
//             var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
//
//             var model = new GenerativeModel(apiKey);
//
//             var action = new Action<string>(s =>
//             {
//                 Console.WriteLine(s);
//             });
//
//             await model.StreamContentAsync(new []{new Part(){Text = "How are you doing?" } }, action);
//         }
//
//         [Fact]
//         public async Task ShouldGenerateStreamResponseWithRequest()
//         {
//             var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
//
//             var model = new GenerativeModel(apiKey);
//
//             var action = new Action<string>(s =>
//             {
//                 Console.WriteLine(s);
//             });
//
//             await model.StreamContentAsync(new GenerateContentRequest(){Contents = new []{RequestExtensions.FormatGenerateContentInput("How are you doing?")}}, action);
//         }
//
//         [Fact]
//         public async Task ShouldCreateRequestWithSafetySettings()
//         {
//             var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
//
//             var model = new GenerativeModel(apiKey, new ModelParams()
//             {
//                 SafetySettings = new[]
//                 {
//                     new SafetySetting()
//                     {
//                         Category = HarmCategory.HARM_CATEGORY_DANGEROUS_CONTENT,
//                         Threshold = HarmBlockThreshold.BLOCK_ONLY_HIGH
//                     }
//                 }
//             });
//             model.Version = "v1";
//             var response = await model.GenerateContentAsync(new GenerateContentRequest()
//             {
//                 Contents = new[] { RequestExtensions.FormatGenerateContentInput("Tell me about terrorism") },
//                 SafetySettings = new[]
//                 {
//                     new SafetySetting()
//                     {
//                         Category = HarmCategory.HARM_CATEGORY_DANGEROUS_CONTENT,
//                         Threshold = HarmBlockThreshold.BLOCK_ONLY_HIGH
//                     }
//                 }
//             });
//             response.UsageMetadata.ShouldNotBeNull();
//         }
//         [Fact]
//         public async Task ShouldCountTokens()
//         {
//             var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
//
//             var text =
//                 "In the realm of dreams where magic thrives,\nWhere whispers of fantasy come alive,\nA tale unfolds, a journey to behold,\nWhere dreams take flight, like stories untold.\n\nAmidst the stars, a shimmering light,\nA celestial vision, enchanting the night,\nA dreamer awakens, with eyes wide open,\nEmbracing the wonders, that lie unspoken.\n\nThrough landscapes vast, where colors dance,\nGuided by hope, a heart's keen glance,\nWith every step, the dreamer learns,\nThat courage and kindness the spirit earns.\n\nIn shadows deep, where secrets reside,\nMysteries unravel, side by side,\nEach challenge faced, a lesson to embrace,\nThe dreamer's resolve, forever in grace.\n\nThrough trials and triumphs, the path unwinds,\nA tapestry of dreams, where destiny binds,\nFor in the realms of dreams, where magic dwells,\nThe power of belief forever excels.\n\nSo let us wander, with hearts set free,\nIn this realm of dreams, where possibilities decree,\nFor in these enchanted lands, we find,\nThe magic that lies within our own mind.\r\n";
//             var content = RequestExtensions.FormatGenerateContentInput(text);
//             var model = new GenerativeModel(apiKey);
//             
//             
//             var res = await model.CountTokens(new CountTokensRequest(){Contents = new[]{content}});
//             res.TotalTokens.ShouldBeGreaterThan(0);
//
//
//             Console.WriteLine($"Tokens count = {res.TotalTokens}");
//         }
//     }
// }
