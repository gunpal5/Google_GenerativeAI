using GenerativeAI.Helpers;
using GenerativeAI.Models;
using GenerativeAI.Types;
using Shouldly;
using Xunit.Abstractions;

namespace GenerativeAI.Tests.Model
{
    public class GeminiPro_Tests
    {
        private ITestOutputHelper Console;
        public GeminiPro_Tests(ITestOutputHelper helper)
        {
            this.Console = helper;
        }
        [Fact]
        public async Task ShouldGenerateResult()
        {
            var apiKey = Environment.GetEnvironmentVariable("Gemini_API_Key", EnvironmentVariableTarget.User);

            var model = new GenerativeModel(apiKey);

            var res = await model.GenerateContentAsync("How are you doing?");

            res.ShouldNotBeNullOrEmpty();

            Console.WriteLine(res);
        }

        [Fact]
        public async Task ShouldCountTokens()
        {
            var apiKey = Environment.GetEnvironmentVariable("Gemini_API_Key", EnvironmentVariableTarget.User);

            var text =
                "In the realm of dreams where magic thrives,\nWhere whispers of fantasy come alive,\nA tale unfolds, a journey to behold,\nWhere dreams take flight, like stories untold.\n\nAmidst the stars, a shimmering light,\nA celestial vision, enchanting the night,\nA dreamer awakens, with eyes wide open,\nEmbracing the wonders, that lie unspoken.\n\nThrough landscapes vast, where colors dance,\nGuided by hope, a heart's keen glance,\nWith every step, the dreamer learns,\nThat courage and kindness the spirit earns.\n\nIn shadows deep, where secrets reside,\nMysteries unravel, side by side,\nEach challenge faced, a lesson to embrace,\nThe dreamer's resolve, forever in grace.\n\nThrough trials and triumphs, the path unwinds,\nA tapestry of dreams, where destiny binds,\nFor in the realms of dreams, where magic dwells,\nThe power of belief forever excels.\n\nSo let us wander, with hearts set free,\nIn this realm of dreams, where possibilities decree,\nFor in these enchanted lands, we find,\nThe magic that lies within our own mind.\r\n";
            var content = RequestExtensions.FormatGenerateContentInput(text);
            var model = new GenerativeModel(apiKey);
            
            
            var res = await model.CountTokens(new CountTokensRequest(){Contents = new[]{content}});
            res.TotalTokens.ShouldBeGreaterThan(0);


            Console.WriteLine($"Tokens count = {res.TotalTokens}");
        }
    }
}
