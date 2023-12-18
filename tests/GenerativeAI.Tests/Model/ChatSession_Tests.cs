using GenerativeAI.Models;
using GenerativeAI.Types;
using Xunit.Abstractions;

namespace GenerativeAI.Tests.Model
{
    public class ChatSession_Tests
    {
        ITestOutputHelper Console;
        public ChatSession_Tests(ITestOutputHelper helper)
        {
            this.Console = helper;
        }

        [Fact]
        public async Task ChatSession_Run()
        {
            var apiKey = Environment.GetEnvironmentVariable("Gemini_API_Key", EnvironmentVariableTarget.User);

            var model = new GenerativeModel(apiKey);

            var chat = model.StartChat(new StartChatParams());
            var result = await chat.SendMessageAsync("Write a poem");
            Console.WriteLine("Initial Poem\r\n");
            Console.WriteLine(result.Text());

            var result2 = await chat.SendMessageAsync("Make it longer");
            Console.WriteLine("\r\nLong Poem\r\n");
            Console.WriteLine(result2.Text());
        }
    }
}
