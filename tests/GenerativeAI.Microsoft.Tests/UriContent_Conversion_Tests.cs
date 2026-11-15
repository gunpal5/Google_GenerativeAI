using GenerativeAI.Microsoft.Extensions;
using GenerativeAI.Types;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace GenerativeAI.Microsoft.Tests;

public class UriContent_Conversion_Tests
{
    [Fact]
    public void UriContent_IsConvertedToFileDataPart()
    {
        var content = new UriContent(new Uri("gs://bucket/video.mp4"), "video/mp4");

        var part = content.ToPart();

        part.ShouldNotBeNull();
        part.FileData.ShouldNotBeNull();
        part.FileData.FileUri.ShouldBe("gs://bucket/video.mp4");
        part.FileData.MimeType.ShouldBe("video/mp4");
        part.InlineData.ShouldBeNull();
    }

    [Fact]
    public void FileDataPart_IsConvertedToUriContent()
    {
        var parts = new List<Part>
        {
            new() { FileData = new FileData { FileUri = "https://example.com/report.pdf", MimeType = "application/pdf" } }
        };

        var contents = parts.ToAiContents();

        var uri = contents.OfType<UriContent>().ShouldHaveSingleItem();
        uri.Uri.ShouldBe(new Uri("https://example.com/report.pdf"));
        uri.MediaType.ShouldBe("application/pdf");
    }
}
