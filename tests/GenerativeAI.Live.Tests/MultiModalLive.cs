using GenerativeAI.Clients;
using GenerativeAI.Live;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace GenerativeAI.Tests.Model;
public class MultiModalLive_Tests
{
    public static async Task Main(string[] args)
    {
        await new MultiModalLive_Tests().ShouldRunMultiModalLive();
    }

    public async Task ShouldRunMultiModalLive()
    {
        
        var logger = LoggerFactory.Create((s) =>
        {
            s.AddSimpleConsole();
        }).CreateLogger<MultiModalLiveClient>();
        
        var exitEvent = new ManualResetEvent(false);
        var multiModalLive = new MultiModalLiveClient(new GoogleAIPlatformAdapter(EnvironmentVariables.GOOGLE_API_KEY),
            "gemini-2.0-flash-exp", new GenerationConfig()
            {
                ResponseModalities = [Modality.TEXT]
            },logger:logger);
        multiModalLive.MessageReceived += (sender, e) =>
        {
            if (e.Payload.SetupComplete != null)
            {
                System.Console.WriteLine($"Setup complete: {e.Payload.SetupComplete}");
            }

            Console.WriteLine("Payload received.");
            if (e.Payload.ServerContent != null)
            {
                if (e.Payload.ServerContent.ModelTurn != null)
                {
                    foreach (var s in e.Payload.ServerContent.ModelTurn?.Parts.Select(s => s.Text))
                    {
                        System.Console.Write(s);
                    }

                    if (e.Payload.ServerContent.TurnComplete == true)
                    {
                        System.Console.WriteLine();
                    }
                }
            }
        };
        multiModalLive.UseGoogleSearch = true;
        await multiModalLive.ConnectAsync(cancellationToken: CancellationToken.None);
        do
        {
            System.Console.WriteLine("Enter your message:");
            var content = System.Console.ReadLine();
            if (content?.ToLower() == "exit") break;
            
            var clientContent = new BidiGenerateContentClientContent();
            clientContent.Turns = new[] { new Content(content, Roles.User) };
            clientContent.TurnComplete = true;
            await multiModalLive.SendClientContentAsync(clientContent, CancellationToken.None);
        } while (true);

        exitEvent.WaitOne();
    }
}