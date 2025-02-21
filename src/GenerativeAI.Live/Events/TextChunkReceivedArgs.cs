using System.Net.Mime;

namespace GenerativeAI.Live;

/// <summary>
/// Contains the arguments for the event when a text chunk is received.
/// </summary>
public class TextChunkReceivedArgs : EventArgs
{
    /// <summary>
    /// Gets or sets the text of the received chunk.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the turn is finished.
    /// </summary>
    public bool IsTurnFinish { get; set; }

    public TextChunkReceivedArgs(string text, bool isTurnFinish)
    {
        this.Text = text;
        this.IsTurnFinish = isTurnFinish;
    }
}