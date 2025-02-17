using GenerativeAI.Clients;
using GenerativeAI.Tests.Base;
using Shouldly;
using Xunit.Abstractions;

namespace GenerativeAI.Tests.Clients;
[TestCaseOrderer(
    ordererTypeName: "GenerativeAI.Tests.Base.PriorityOrderer",
    ordererAssemblyName: "GenerativeAI.Tests")]
public class FileClient_Tests : TestBase
{
    public FileClient_Tests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Fact,TestPriority(3)]
    public async Task ShouldGetFileMetadata()
    {
        var client = new FileClient(GetTestGooglePlatform());

        var files = await client.ListFilesAsync();
        var fileX = files.Files.FirstOrDefault();
        var fileName = fileX.Name; // Example file name, replace with test data.
        var file = await client.GetFileAsync(fileName);

        file.ShouldNotBeNull();
        file.Name.ShouldBe(fileName);
        file.DisplayName.ShouldNotBeNullOrEmpty();
        file.MimeType.ShouldNotBeNullOrEmpty();
        file.SizeBytes.ShouldNotBeNull();
        file.Sha256Hash.ShouldNotBeNullOrEmpty();
        file.Uri.ShouldNotBeNullOrEmpty();
        //file.DownloadUri.ShouldNotBeNullOrEmpty();
        file.CreateTime.ShouldNotBeNull();
        file.UpdateTime.ShouldNotBeNull();
        file.State.ShouldNotBeNull();
        file.Source.ShouldNotBeNull();

        Console.WriteLine($"File Metadata: {file.Name}, {file.DisplayName}, {file.MimeType}, {file.SizeBytes}");
    }

    [Fact,TestPriority(2)]
    public async Task ShouldListFiles()
    {
        var client = new FileClient(GetTestGooglePlatform());

        var result = await client.ListFilesAsync(pageSize: 5); // Example: Fetch a maximum of 5 files.

        result.ShouldNotBeNull();
        result.Files.ShouldNotBeNull();
        result.Files.Count.ShouldBeGreaterThan(0);
        //result.Files.Count.ShouldBeLessThanOrEqualTo(5); // Validate the page size.

        foreach (var file in result.Files)
        {
            file.Name.ShouldNotBeNullOrEmpty();
            file.DisplayName.ShouldNotBeNullOrEmpty();
            file.MimeType.ShouldNotBeNullOrEmpty();
            file.SizeBytes.ShouldNotBeNull();
            file.Uri.ShouldNotBeNull();
            file.State.ShouldNotBeNull();
        }

        System.Console.WriteLine("File List Retrieved");
        result.Files.ForEach(file => Console.WriteLine($"File: {file.Name}, {file.DisplayName}, {file.MimeType}"));
    }

    [Fact,TestPriority(1)]
    public async Task ShouldUploadFileAsync()
    {
        // Arrange
        var client = new FileClient(GetTestGooglePlatform());
        
        string tempFilePath = Path.Combine(Path.GetTempPath(), "test-upload-file.txt");

        // Create a temporary file to simulate upload
        File.WriteAllText(tempFilePath, "This is a test file for upload.");

        try
        {
            double progressReported = 0;
            Action<double> progressCallback = progress =>
            {
                progressReported = progress;
                Console.WriteLine($"Upload Progress: {progress:P}");
            };

            // Act
            var result = await client.UploadFileAsync(tempFilePath, progressCallback);

            // Assert
            result.ShouldNotBeNull();                           // Check response is not null
            result.Name.ShouldNotBeNullOrEmpty();              // Verify file name in the result
            result.DisplayName.ShouldBe("test-upload-file");   // Check the display name
            progressReported.ShouldBeGreaterThan(0);           // Ensure progress callback was called

            Console.WriteLine($"File uploaded successfully: {result.Name}, Display Name: {result.DisplayName}");
        }
        finally
        {
            // Cleanup: Delete temporary file
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }
    
    [Fact,TestPriority(4)]
    public async Task ShouldDeleteFile()
    {
        var client = new FileClient(GetTestGooglePlatform());

        var files = await client.ListFilesAsync();
        var fileX = files.Files.FirstOrDefault(s=>s.DisplayName.Contains("test-upload-file"));
        
        var fileName = fileX.Name; // Example file ID to delete, replace with test data.

        await Should.NotThrowAsync(async () => await client.DeleteFileAsync(fileName));
        Console.WriteLine($"File {fileName} deleted successfully.");
    }

    [Fact,TestPriority(6)]
    public async Task ShouldHandleInvalidFileForRetrieve()
    {
        var client = new FileClient(GetTestGooglePlatform());

        var invalidFileName = "files/invalid-id"; // Simulating a bad file ID.

        var exception = await Should.ThrowAsync<Exception>(async () => await client.GetFileAsync(invalidFileName));
        exception.Message.ShouldNotBeNullOrEmpty();

        Console.WriteLine($"Handled exception while retrieving file: {exception.Message}");
    }

    [Fact,TestPriority(6)]
    public async Task ShouldHandleInvalidFileForDelete()
    {
        var client = new FileClient(GetTestGooglePlatform());

        var invalidFileName = "files/invalid-id"; // Simulating a bad file ID.

        var exception = await Should.ThrowAsync<Exception>(async () => await client.DeleteFileAsync(invalidFileName));
        exception.Message.ShouldNotBeNullOrEmpty();

        Console.WriteLine($"Handled exception while deleting file: {exception.Message}");
    }
    
    [Fact,TestPriority(1)]
    public async Task ShouldUploadStream()
    {
        // Arrange
        var client = new FileClient(GetTestGooglePlatform());

        // Create a simulated stream (e.g., a memory stream)
        var fileContent = "This is a test file content for stream upload.";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContent));

        var displayName = "test-upload-stream";
        var mimeType = "text/plain"; // Example MIME type

        double progressReported = 0;
        Action<double> progressCallback = progress =>
        {
            progressReported = progress;
            Console.WriteLine($"Upload Progress: {progress:P}");
        };

        // Act
        var result = await client.UploadStreamAsync(stream, displayName, mimeType, progressCallback);

        // Assert
        result.ShouldNotBeNull();                           // Check that the result is not null
        result.Name.ShouldNotBeNullOrEmpty();              // Verify file name is returned
        result.DisplayName.ShouldBe(displayName);          // Validate the uploaded file's display name
        progressReported.ShouldBeGreaterThan(0);           // Ensure progress callback was invoked

        Console.WriteLine($"Stream uploaded successfully: {result.Name}, Display Name: {result.DisplayName}");
    }
}