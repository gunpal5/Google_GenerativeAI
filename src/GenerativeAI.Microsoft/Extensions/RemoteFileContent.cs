using GenerativeAI.Types;
using Microsoft.Extensions.AI;

namespace GenerativeAI.Microsoft.Extensions;

/// <summary>
/// Marker AIContent that points at a previously-uploaded Files-API
/// asset. Produced by <see cref="MicrosoftExtensions.PromoteOversizedAttachmentsAsync"/>
/// when a <see cref="DataContent"/> exceeds Gemini's 20 MB inline_data
/// limit; <see cref="MicrosoftExtensions.ToPart"/> then emits a
/// <c>FileData</c> part referencing the remote URI instead of an
/// inline blob.
/// </summary>
public sealed class RemoteFileContent : AIContent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteFileContent"/> class.
    /// </summary>
    /// <param name="remoteFile">The uploaded file to reference.</param>
    /// <param name="mimeType">Optional MIME type override; defaults to the file's own MIME type.</param>
    public RemoteFileContent(RemoteFile remoteFile, string? mimeType = null)
    {
        RemoteFile = remoteFile;
        MimeType = mimeType;
    }

    /// <summary>
    /// Gets the uploaded file this content refers to.
    /// </summary>
    public RemoteFile RemoteFile { get; }

    /// <summary>
    /// Gets the MIME type override, or null to use <see cref="RemoteFile"/>'s MIME type.
    /// </summary>
    public string? MimeType { get; }
}
