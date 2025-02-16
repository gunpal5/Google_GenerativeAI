namespace GenerativeAI.Types;

/// <summary>
/// Represents a request to upload a local file in order to create a File resource.
/// </summary>
public class UploadFileRequest
{
    /// <summary>
    /// Represents information about the file to be uploaded.
    /// </summary>
    public UploadFileInformation? File { get; set; }
}

/// <summary>
/// Represents information about a file to be uploaded.
/// </summary>
public class UploadFileInformation
{
    /// <summary>
    /// Specifies the human-readable name to be associated with the file during the upload process.
    /// This name is optional and may be subject to validation or formatting requirements.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Represents the name of the file. This is used to uniquely identify
    /// the file within the context of the request or application.
    /// </summary>
    public string? Name { get; set; }
}