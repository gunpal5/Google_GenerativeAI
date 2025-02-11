using System.Net;

namespace GenerativeAI.Core;

/// <summary>
/// A custom StreamContent implementation to track and report progress during upload.
/// </summary>
public class ProgressStreamContent : HttpContent
{
    private readonly Stream _stream;
    private readonly Action<double> _progressCallback;

    public ProgressStreamContent(Stream stream, Action<double> progressCallback)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _progressCallback = progressCallback ?? throw new ArgumentNullException(nameof(progressCallback));
    }

    protected override async Task SerializeToStreamAsync(Stream targetStream, TransportContext? context)
    {
        var buffer = new byte[81920]; // 80 KB buffer size
        var totalBytes = _stream.Length;
        var uploadedBytes = 0L;

        while (true)
        {
            var bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            if (bytesRead == 0)
            {
                break;
            }

            await targetStream.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);

            uploadedBytes += bytesRead;

            // Report progress
            var progress = (double)uploadedBytes / totalBytes * 100;
            _progressCallback(progress);
        }
    }
  

    protected override bool TryComputeLength(out long length)
    {
        length = _stream.Length;
        return true;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _stream.Dispose();
        }

        base.Dispose(disposing);
    }
}