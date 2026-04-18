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
    public RemoteFileContent(RemoteFile remoteFile, string? mimeType = null)
    {
        RemoteFile = remoteFile;
        MimeType = mimeType;
    }

    public RemoteFile RemoteFile { get; }
    public string? MimeType { get; }
}
