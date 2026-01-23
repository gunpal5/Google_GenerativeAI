using Google.Cloud.Storage.V1;
using System.Text;

namespace GenerativeAI.Tests.Helpers;

/// <summary>
/// Helper class for managing GCS test data for batch job integration tests.
/// </summary>
public class GcsTestHelper : IDisposable
{
    private readonly StorageClient _storageClient;
    private readonly string _bucketName;
    private readonly string _projectId;
    private readonly List<string> _uploadedFiles = new();

    /// <summary>
    /// Creates a new GCS test helper.
    /// </summary>
    /// <param name="bucketName">GCS bucket name from environment variable GCS_TEST_BUCKET</param>
    /// <param name="projectId">GCP project ID from environment variable GOOGLE_PROJECT_ID</param>
    public GcsTestHelper(string? bucketName = null, string? projectId = null)
    {
        _bucketName = bucketName ?? Environment.GetEnvironmentVariable("GCS_TEST_BUCKET")
            ?? throw new InvalidOperationException("GCS_TEST_BUCKET environment variable not set");

        _projectId = projectId ?? Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID")
            ?? throw new InvalidOperationException("GOOGLE_PROJECT_ID environment variable not set");

        _storageClient = StorageClient.Create();
    }

    /// <summary>
    /// Uploads a JSONL file with batch requests to GCS for testing.
    /// </summary>
    /// <param name="requests">List of request objects to serialize as JSONL</param>
    /// <param name="fileName">Optional file name (auto-generated if not provided)</param>
    /// <returns>The GCS URI (gs://bucket/path)</returns>
    public async Task<string> UploadBatchRequestsAsync(List<object> requests, string? fileName = null)
    {
        fileName ??= $"test-batch-input-{Guid.NewGuid()}.jsonl";

        var sb = new StringBuilder();
        var jsonOptions = new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        foreach (var request in requests)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(request, jsonOptions);
            sb.AppendLine(json);
        }

        var content = sb.ToString();

        // Log the content being uploaded for debugging
        Console.WriteLine($"Uploading JSONL file: {fileName}");
        Console.WriteLine($"Content ({content.Length} bytes):");
        Console.WriteLine(content);
        Console.WriteLine("---");

        var contentBytes = Encoding.UTF8.GetBytes(content);
        using var stream = new MemoryStream(contentBytes);

        await _storageClient.UploadObjectAsync(
            _bucketName,
            fileName,
            "application/json",
            stream);

        _uploadedFiles.Add(fileName);

        return $"gs://{_bucketName}/{fileName}";
    }

    /// <summary>
    /// Uploads sample batch requests for GenerateContent testing.
    /// Each line must be a JSON object with a "request" wrapper containing the GenerateContentRequest.
    /// Format: {"request": {"contents": [{"role": "user", "parts": [{"text": "..."}]}]}}
    /// </summary>
    /// <returns>GCS URI for the uploaded file</returns>
    public async Task<string> UploadSampleContentRequestsAsync()
    {
        var requests = new List<object>
        {
            new
            {
                request = new
                {
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[] { new { text = "What is the capital of France?" } }
                        }
                    }
                }
            },
            new
            {
                request = new
                {
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[] { new { text = "What is 2 + 2?" } }
                        }
                    }
                }
            }
        };

        return await UploadBatchRequestsAsync(requests);
    }

    /// <summary>
    /// Uploads sample batch requests with schema for structured output testing.
    /// Each line includes generationConfig with responseMimeType and responseSchema.
    /// </summary>
    /// <param name="schemaType">The C# type to generate schema from</param>
    /// <returns>GCS URI for the uploaded file</returns>
    public async Task<string> UploadSampleContentRequestsWithSchemaAsync(Type schemaType)
    {
        // Use BatchJsonLGenerator for proper serialization with schema
        var prompts = new[]
        {
            "Create a profile for: John Smith, age 35, software engineer",
            "Create a profile for: Jane Doe, age 28, data scientist"
        };

        // Use the centralized BatchJsonLGenerator
        var jsonl = GenerativeAI.Helpers.BatchJsonLGenerator.GenerateContentJsonL(
            prompts,
            new GenerativeAI.Helpers.BatchOptions { ResponseSchemaType = schemaType });

        var fileName = $"test-batch-input-{Guid.NewGuid()}.jsonl";

        // Log the content being uploaded for debugging
        Console.WriteLine($"Uploading JSONL file with schema: {fileName}");
        Console.WriteLine($"Content ({jsonl.Length} bytes):");
        Console.WriteLine(jsonl);
        Console.WriteLine("---");

        var contentBytes = Encoding.UTF8.GetBytes(jsonl);
        using var stream = new MemoryStream(contentBytes);

        await _storageClient.UploadObjectAsync(
            _bucketName,
            fileName,
            "application/json",
            stream);

        _uploadedFiles.Add(fileName);

        return $"gs://{_bucketName}/{fileName}";
    }

    /// <summary>
    /// Uploads sample batch requests for Embeddings testing.
    /// Each line must be a JSON object with a "request" field containing the EmbedContentRequest.
    /// </summary>
    /// <returns>GCS URI for the uploaded file</returns>
    public async Task<string> UploadSampleEmbeddingRequestsAsync()
    {
        var requests = new List<object>
        {
            new
            {
                request = new
                {
                    content = new
                    {
                        parts = new[] { new { text = "Hello world" } }
                    }
                }
            },
            new
            {
                request = new
                {
                    content = new
                    {
                        parts = new[] { new { text = "How are you?" } }
                    }
                }
            }
        };

        return await UploadBatchRequestsAsync(requests);
    }

    /// <summary>
    /// Cleans up all uploaded test files.
    /// </summary>
    public async Task CleanupAsync()
    {
        foreach (var fileName in _uploadedFiles)
        {
            try
            {
                await _storageClient.DeleteObjectAsync(_bucketName, fileName);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
        _uploadedFiles.Clear();
    }

    /// <summary>
    /// Gets the output URI prefix for test results.
    /// </summary>
    public string GetOutputUriPrefix()
    {
        return $"gs://{_bucketName}/test-output-{Guid.NewGuid()}/";
    }

    /// <summary>
    /// Checks if a file exists in the bucket.
    /// </summary>
    public async Task<bool> FileExistsAsync(string fileName)
    {
        try
        {
            await _storageClient.GetObjectAsync(_bucketName, fileName);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Downloads and reads the content of a file from GCS.
    /// </summary>
    public async Task<string> DownloadFileContentAsync(string fileName)
    {
        using var stream = new MemoryStream();
        await _storageClient.DownloadObjectAsync(_bucketName, fileName, stream);
        stream.Position = 0;
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    /// <summary>
    /// Verifies the uploaded file exists and displays its content.
    /// </summary>
    public async Task<bool> VerifyUploadedFileAsync(string gcsUri)
    {
        // Extract filename from gs://bucket/filename
        var parts = gcsUri.Replace("gs://", "").Split('/');
        if (parts.Length < 2) return false;

        var fileName = string.Join("/", parts.Skip(1));

        if (!await FileExistsAsync(fileName))
        {
            Console.WriteLine($"File does not exist: {fileName}");
            return false;
        }

        var content = await DownloadFileContentAsync(fileName);
        Console.WriteLine($"File verified: {fileName}");
        Console.WriteLine($"Content ({content.Length} bytes):");
        Console.WriteLine(content);
        return true;
    }

    public void Dispose()
    {
        CleanupAsync().GetAwaiter().GetResult();
    }
}
