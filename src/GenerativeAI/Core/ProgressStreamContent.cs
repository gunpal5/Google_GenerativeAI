﻿using System.Net;

namespace GenerativeAI.Core;

/// <summary>
/// A custom StreamContent implementation to track and report progress during upload.
/// </summary>
public class ProgressStreamContent : HttpContent
{
    private readonly Stream _stream;
    private readonly Action<double> _progressCallback;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressStreamContent"/> class.
    /// </summary>
    /// <param name="stream">The stream to upload with progress tracking.</param>
    /// <param name="progressCallback">The callback to report upload progress as a percentage (0.0 to 100.0).</param>
    public ProgressStreamContent(Stream stream, Action<double> progressCallback)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _progressCallback = progressCallback ?? throw new ArgumentNullException(nameof(progressCallback));
    }

    /// <summary>
    /// Serializes the content of the stream to a target stream asynchronously
    /// while tracking and reporting the upload progress.
    /// </summary>
    /// <param name="targetStream">The target stream where the content will be serialized.</param>
    /// <param name="context">An optional transport context that provides additional information about the stream operation.</param>
    /// <returns>A task representing the asynchronous operation of writing the content to the target stream.</returns>
    protected override async Task SerializeToStreamAsync(Stream targetStream, TransportContext? context)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(targetStream);
#else
        if (targetStream == null) throw new ArgumentNullException(nameof(targetStream));
#endif
        var buffer = new byte[81920]; // 80 KB buffer size
        var totalBytes = _stream.Length;
        var uploadedBytes = 0L;

        while (true)
        {
#if NET6_0_OR_GREATER
            var bytesRead = await _stream.ReadAsync(buffer.AsMemory(0, buffer.Length)).ConfigureAwait(false);
#else
            var bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
#endif
            if (bytesRead == 0)
            {
                break;
            }

#if NET6_0_OR_GREATER
            await targetStream.WriteAsync(buffer.AsMemory(0, bytesRead)).ConfigureAwait(false);
#else
            await targetStream.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
#endif

            uploadedBytes += bytesRead;

            // Report progress
            var progress = (double)uploadedBytes / totalBytes * 100;
            _progressCallback(progress);
        }
    }


    /// <summary>
    /// Attempts to compute the length of the stream content.
    /// </summary>
    /// <param name="length">When this method returns, contains the computed length of the stream if it exists; otherwise, 0.</param>
    /// <returns>true if the length of the stream can be determined; otherwise, false.</returns>
    protected override bool TryComputeLength(out long length)
    {
        length = _stream.Length;
        return true;
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="ProgressStreamContent"/>
    /// and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// true to release both managed and unmanaged resources;
    /// false to release only unmanaged resources.
    /// </param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _stream.Dispose();
        }

        base.Dispose(disposing);
    }
}