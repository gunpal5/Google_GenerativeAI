using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenerativeAI.Models;
using GenerativeAI.Types;
using Xunit.Abstractions;

namespace GenerativeAI.Tests.Model
{
    public class GeminiProVision_Tests
    {
        private ITestOutputHelper Console;
        public GeminiProVision_Tests(ITestOutputHelper helper)
        {
            this.Console = helper;
        }

        [Fact]
        public async Task ShouldIdentifyObjectInImage()
        {
            var imageBytes = await File.ReadAllBytesAsync("image.png");
            
            var imagePart = new Part()
            {
                InlineData = new GenerativeContentBlob()
                {
                    MimeType = "image/png",
                    Data = Convert.ToBase64String(imageBytes)
                }
            };
            var textPart = new Part()
            {
                Text = "What is in the image?"
            };
            var parts = new[] { textPart, imagePart };
            //var content = new Content(new[] { textPart, imagePart }, Roles.User);

            var apiKey = Environment.GetEnvironmentVariable("Gemini_API_Key", EnvironmentVariableTarget.User);

            var visionModel = new GeminiProVision(apiKey);

            var result = await visionModel.GenerateContentAsync(parts);

            Console.WriteLine(result.Text());
        }
    }
}
