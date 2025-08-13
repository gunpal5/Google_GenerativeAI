namespace GenerativeAI.Live.Events;

/// <summary>
/// Provides error message event data.
/// </summary>
public class ErrorMessageEventArgs : EventArgs
{
    /// <summary>
    /// Gets the payload of the received message.
    /// </summary>
    public string ErrorMessage { get; }

    /// <devdoc>
    ///    Initializes a new instance of the class.
    /// </devdoc>
    public ErrorMessageEventArgs(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }
}
