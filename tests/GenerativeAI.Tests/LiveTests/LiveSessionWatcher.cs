using System.Text;
using GenerativeAI.Live;
using GenerativeAI.Live.Events;
using GenerativeAI.Types;

namespace GenerativeAI.Tests.LiveTests;

/// <summary>
/// Observes a <see cref="MultiModalLiveClient"/> session and exposes the server events the live tests wait for.
/// A server-side close with a reason (e.g. an invalid setup) is surfaced as a test failure instead of a timeout.
/// </summary>
internal sealed class LiveSessionWatcher
{
    private readonly object _lock = new();
    private readonly StringBuilder _text = new();
    private TaskCompletionSource<bool> _response = NewSource();
    private TaskCompletionSource<bool> _turnComplete = NewSource();

    public LiveSessionWatcher(MultiModalLiveClient client, ITestOutputHelper? output)
    {
        Output = output;
        client.MessageReceived += OnMessage;
        client.Disconnected += (_, e) =>
        {
            if (e is ErrorMessageEventArgs message && !string.IsNullOrEmpty(message.ErrorMessage))
                Fail($"The server closed the session: {message.ErrorMessage}");
        };
        client.ErrorOccurred += (_, e) => Output?.WriteLine($"Live client error: {e.GetException().Message}");
    }

    private ITestOutputHelper? Output { get; }

    /// <summary>Completes when the server acknowledges the setup.</summary>
    public TaskCompletionSource<bool> SetupComplete { get; } = NewSource();

    /// <summary>Faulted when the server closes the session with an error.</summary>
    public TaskCompletionSource<bool> Failure { get; } = NewSource();

    /// <summary>Number of audio bytes received in model turns.</summary>
    public int AudioBytes { get; private set; }

    /// <summary>Number of session resumption updates received.</summary>
    public int ResumptionUpdates { get; private set; }

    /// <summary>Text and output transcription received so far.</summary>
    public string Text
    {
        get
        {
            lock (_lock)
                return _text.ToString();
        }
    }

    /// <summary>Completes on the first model output (text, audio or transcription) of the current turn.</summary>
    public Task Response
    {
        get
        {
            lock (_lock)
                return _response.Task;
        }
    }

    /// <summary>Completes when the current turn is complete.</summary>
    public Task TurnComplete
    {
        get
        {
            lock (_lock)
                return _turnComplete.Task;
        }
    }

    /// <summary>Resets the per-turn signals before starting a new turn.</summary>
    public void StartNewTurn()
    {
        lock (_lock)
        {
            _response = NewSource();
            _turnComplete = NewSource();
        }
    }

    /// <summary>Waits for <paramref name="task"/>, failing on a server-side close or after <paramref name="timeout"/>.</summary>
    public async Task WaitAsync(Task task, TimeSpan timeout, string what)
    {
        var timeoutTask = Task.Delay(timeout, TestContext.Current.CancellationToken);
        var finished = await Task.WhenAny(task, Failure.Task, timeoutTask).ConfigureAwait(false);
        if (finished == Failure.Task)
            await Failure.Task.ConfigureAwait(false); // rethrows the failure
        if (finished == timeoutTask)
            Assert.Fail($"Timed out after {timeout.TotalSeconds:0}s waiting for {what}. Text so far: '{Text}'");
        await task.ConfigureAwait(false);
    }

    private void OnMessage(object? sender, MessageReceivedEventArgs e)
    {
        var payload = e.Payload;
        if (payload.SetupComplete != null)
            SetupComplete.TrySetResult(true);

        if (payload.SessionResumptionUpdate != null)
        {
            ResumptionUpdates++;
            Output?.WriteLine(
                $"Session resumption update: resumable={payload.SessionResumptionUpdate.Resumable}, has handle={!string.IsNullOrEmpty(payload.SessionResumptionUpdate.NewHandle)}");
        }

        if (payload.GoAway != null)
            Output?.WriteLine("GoAway received.");

        var content = payload.ServerContent;
        if (content == null)
            return;

        var gotOutput = false;
        lock (_lock)
        {
            if (content.ModelTurn?.Parts != null)
            {
                foreach (var part in content.ModelTurn.Parts)
                {
                    if (!string.IsNullOrEmpty(part.Text))
                    {
                        _text.Append(part.Text);
                        gotOutput = true;
                    }

                    if (!string.IsNullOrEmpty(part.InlineData?.Data))
                    {
                        AudioBytes += part.InlineData!.Data!.Length * 3 / 4;
                        gotOutput = true;
                    }
                }
            }

            if (!string.IsNullOrEmpty(content.OutputTranscription?.Text))
            {
                _text.Append(content.OutputTranscription!.Text);
                gotOutput = true;
            }

            if (gotOutput)
                _response.TrySetResult(true);
            if (content.TurnComplete == true)
            {
                _response.TrySetResult(true);
                _turnComplete.TrySetResult(true);
            }
        }
    }

    private void Fail(string message)
    {
        Output?.WriteLine(message);
        Failure.TrySetException(new InvalidOperationException(message));
    }

    private static TaskCompletionSource<bool> NewSource() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);
}
