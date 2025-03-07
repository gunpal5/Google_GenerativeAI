using GenerativeAI;
using GenerativeAI.Live;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace AotTest;

public class LiveTest
{
    public async Task ShouldRunMultiModalLive()
    {
        var exitEvent = new ManualResetEvent(false);
        var multiModalLive = new MultiModalLiveClient(new GoogleAIPlatformAdapter(EnvironmentVariables.GOOGLE_API_KEY),
            "gemini-2.0-flash-exp", new GenerationConfig()
            {
                ResponseModalities = [Modality.TEXT]
            });
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
        await multiModalLive.ConnectAsync();
        var content = "write a poem about stars";
        var clientContent = new BidiGenerateContentClientContent();
        clientContent.Turns = new[] { new Content(content, Roles.User) };
        clientContent.TurnComplete = true;
        await multiModalLive.SendClientContentAsync(clientContent);

        Task.WaitAll();
        await multiModalLive.DisconnectAsync();
    }
}