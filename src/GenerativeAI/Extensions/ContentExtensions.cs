using System.Runtime.CompilerServices;
using System.Text.Json;
using GenerativeAI.Core;
using GenerativeAI.Types;

namespace GenerativeAI;

/// <summary>
/// Provides extension methods for the <see cref="Content"/> class, allowing operations such as
/// adding parts, handling inline and remote files, adding text, and extracting code or JSON blocks.
/// </summary>
public static class ContentExtensions
{
    /// <summary>
    /// Adds a single <see cref="Part"/> to this <see cref="Content"/>.
    /// </summary>
    /// <param name="content">The <see cref="Content"/> to which the <see cref="Part"/> will be added.</param>
    /// <param name="part">The <see cref="Part"/> to add.</param>
    public static void AddPart(this Content content, Part part)
    {
        if (content == null)
            throw new ArgumentException("Content cannot be null.", nameof(content));

        content.Parts.Add(part);
    }

    /// <summary>
    /// Adds multiple <see cref="Part"/> objects to this <see cref="Content"/>.
    /// </summary>
    /// <param name="content">The <see cref="Content"/> to which the <see cref="Part"/> objects will be added.</param>
    /// <param name="parts">The collection of <see cref="Part"/> objects to add.</param>
    public static void AddParts(this Content content, IEnumerable<Part> parts)
    {
        if (content == null)
            throw new ArgumentException("Content cannot be null.", nameof(content));

        content.Parts.AddRange(parts);
    }

    /// <summary>
    /// Adds inline data (for images, audio, etc.) as a new <see cref="Part"/> to this <see cref="Content"/>.
    /// </summary>
    /// <param name="content">The <see cref="Content"/> to which the inline data will be added.</param>
    /// <param name="data">The base64-encoded data to include.</param>
    /// <param name="mimeType">The MIME type of the data.</param>
    public static void AddInlineData(this Content content, string data, string mimeType)
    {
        if (content == null)
            throw new ArgumentException("Content cannot be null.", nameof(content));

        var part = new Part
        {
            InlineData = new Blob
            {
                Data = data,
                MimeType = mimeType
            }
        };
        content.AddPart(part);
    }

    public static void AddInlineFile(this Content content, string filePath, string role)
    {
        if (content == null)
            throw new ArgumentException("Content cannot be null.", nameof(content));
        
        FileValidator.ValidateInlineFile(filePath);
        var bytes = File.ReadAllBytes(filePath);
        var base64 = Convert.ToBase64String(bytes);
        var mimeType = MimeTypeMap.GetMimeType(filePath);

        AddInlineData(content, base64, mimeType);
    }

    /// <summary>
    /// Adds a remote file to the <see cref="GenerateContentRequest"/> using its URL and MIME type.
    /// </summary>
    /// <param name="request">The <see cref="GenerateContentRequest"/> to which the remote file will be added.</param>
    /// <param name="file">The <see cref="RemoteFile"/> to be added.</param>
    public static void AddRemoteFile(
        this Content request,
        RemoteFile file)
    {
        if (file == null)
            throw new ArgumentException("Remote file cannot be null or empty.", nameof(file));

        if (string.IsNullOrEmpty(file.Uri))
            throw new ArgumentException("Remote file URI cannot be null or empty.", nameof(file));
        if (string.IsNullOrEmpty(file.MimeType))
            throw new ArgumentException("Remote file MIME type cannot be null or empty.", nameof(file));

        AddRemoteFile(request, file.Uri, file.MimeType);
    }

    /// <summary>
    /// Adds a remote file reference as a new <see cref="Part"/> to this <see cref="Content"/>.
    /// </summary>
    /// <param name="content">The <see cref="Content"/> to which the remote file reference will be added.</param>
    /// <param name="fileUri">The URL of the remote file.</param>
    /// <param name="mimeType">The MIME type of the remote file.</param>
    public static void AddRemoteFile(this Content content, string fileUri, string mimeType)
    {
        if (content == null)
            throw new ArgumentException("Content cannot be null.", nameof(content));

        if (string.IsNullOrEmpty(fileUri))
            throw new ArgumentException("The file URI cannot be null or empty.", nameof(fileUri));
        if (string.IsNullOrEmpty(mimeType))
            throw new ArgumentException("The MIME type cannot be null or empty.", nameof(mimeType));


        if (!FilesConstants.SupportedMimeTypes.Contains(mimeType))
            throw new ArgumentException($"File type {mimeType} is not allowed for inline", nameof(mimeType));

        var part = new Part
        {
            FileData = new FileData
            {
                FileUri = fileUri,
                MimeType = mimeType
            }
        };
        content.AddPart(part);
    }

    /// <summary>
    /// Adds a single text <see cref="Part"/> to this <see cref="Content"/>.
    /// </summary>
    /// <param name="content">The <see cref="Content"/> instance.</param>
    /// <param name="text">The text to add.</param>
    public static void AddText(this Content content, string text)
    {
        if(content == null)
            throw new ArgumentException("Content cannot be null.", nameof(content));

        content.AddPart(new Part { Text = text });
    }

    /// <summary>
    /// Extracts all <see cref="CodeBlock"/> from the text <see cref="Part">Parts</see> of <see cref="Content"/> object.
    /// </summary>
    /// <param name="content">The <see cref="Content"/> from which to extract code blocks.</param>
    /// <returns>A list of <see cref="CodeBlock"/> instances found within the <see cref="Content"/>.</returns>
    public static List<CodeBlock> ExtractCodeBlocks(this Content content)
    {
        if(content == null)
            throw new ArgumentException("Content cannot be null.", nameof(content));
        List<CodeBlock> codeBlocks = new List<CodeBlock>();
        foreach (var part in content.Parts)
        {
            if (!string.IsNullOrEmpty(part.Text))
            {
                var blocks = part.Text.ExtractCodeBlocks();
                codeBlocks.AddRange(blocks);
            }
        }
        return codeBlocks;
    }
    
    /// <summary>
    /// Extracts all JSON string from the text <see cref="Part">Parts</see> of <see cref="Content"/> object.
    /// </summary>
    /// <param name="content">The <see cref="Content"/> from which to extract code blocks.</param>
    /// <returns>A list of <see cref="CodeBlock"/> instances found within the <see cref="Content"/>.</returns>
    public static List<JsonBlock> ExtractJsonBlocks(this Content content)
    {
        if(content == null)
            throw new ArgumentException("Content cannot be null.", nameof(content));
        List<JsonBlock> jsonBlocks = new List<JsonBlock>();
        foreach (var part in content.Parts)
        {
            if (!string.IsNullOrEmpty(part.Text))
            {
                var blocks = part.Text.ExtractJsonBlocks();
                jsonBlocks.AddRange(blocks);
            }
        }
        return jsonBlocks;
    }

    /// <summary>
    /// Converts the JSON content of a <see cref="Content"/> object into an instance of the specified type.
    /// </summary>
    /// <typeparam name="T">The target type to which the JSON content will be deserialized. Must be a class.</typeparam>
    /// <param name="content">The <see cref="Content"/> object containing JSON data to be converted.</param>
    /// <returns>An instance of type <typeparamref name="T"/> if conversion succeeds, or null if no valid JSON data is found or deserialization fails.</returns>
    public static T? ToObject<T>(this Content content) where T : class
    {
        var jsonBlocks = ExtractJsonBlocks(content);

        if (jsonBlocks.Any())
        {
            foreach (var block in jsonBlocks)
            {
                return block.ToObject<T?>();
            }
        }

        return null;
    }
}