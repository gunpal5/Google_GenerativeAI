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

    /// <summary>
    /// Initializes a new instance of the TextChunkReceivedArgs class.
    /// </summary>
    /// <param name="text">The text of the received chunk.</param>
    /// <param name="isTurnFinish">A value indicating whether the turn is finished.</param>
    public TextChunkReceivedArgs(string text, bool isTurnFinish)
    {
        this.Text = text;
        this.IsTurnFinish = isTurnFinish;
    }
}