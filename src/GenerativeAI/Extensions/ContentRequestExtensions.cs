using GenerativeAI.Core;
using GenerativeAI.Types;

namespace GenerativeAI;

/// <summary>
/// Contains extension methods for working with objects implementing <see cref="IContentsRequest"/>.
/// </summary>
public static class ContentRequestExtensions
{
    /// <summary>
    /// Adds a single text <see cref="Part"/> to the <see cref="IContentsRequest"/> with the specified role.
    /// </summary>
    /// <param name="request">The <see cref="IContentsRequest"/> instance to which the text <see cref="Part"/> will be added.</param>
    /// <param name="text">The text content to add as a <see cref="Part"/>.</param>
    /// <param name="appendToLastContent">Indicates whether to append the <see cref="Part"/> to the latest <see cref="Content"/> or Create a new <see cref="Content"/></param>
    /// <param name="role">The role associated with the text <see cref="Part"/>. Defaults to <see cref="Roles.User"/>.</param>
    public static void AddText(
        this IContentsRequest request,
        string text,
        bool appendToLastContent = true,
        string role = Roles.User)
    {
        var part = new Part { Text = text };
        AddPart(request, part, appendToLastContent, role);
    }

    /// <summary>
    /// Adds a specific <see cref="Part"/> to the latest <see cref="Content"/> in the <see cref="IContentsRequest"/>.
    /// If no <see cref="Content"/> exists, a new one is created with the provided <see cref="Part"/>.
    /// </summary>
    /// <param name="request">The <see cref="IContentsRequest"/> instance to which the <see cref="Part"/> is added.</param>
    /// <param name="part">The <see cref="Part"/> to add.</param>
    /// <param name="appendToLastContent">Indicates whether to append the <see cref="Part"/> to the latest <see cref="Content"/> or Create a new <see cref="Content"/></param>
    /// <param name="role">The role associated with the <see cref="Part"/>. Defaults to <see cref="Roles.User"/>.</param>
    public static void AddPart(
        this IContentsRequest request,
        Part part,
        bool appendToLastContent = true,
        string role = Roles.User)
    {
        if (appendToLastContent)
        {
            var lastContent = request.Contents.LastOrDefault();
            if (lastContent == null)
            {
                lastContent = new Content(new[] { part }, role);
                AddContent(request, lastContent);
            }
            else
            {
                lastContent.AddPart(part);
            }
        }
        else
        {
            var content = new Content(new[] { part }, role);
            AddContent(request, content);
        }
    }

    /// <summary>
    /// Adds a collection of <see cref="Part"/> objects to the latest <see cref="Content"/> in the <see cref="IContentsRequest"/> with the specified role.
    /// If no <see cref="Content"/> exists, a new one is created with the provided <see cref="Part"/> collection.
    /// </summary>
    /// <param name="request">The <see cref="IContentsRequest"/> instance to which the <see cref="Part"/> objects will be added.</param>
    /// <param name="parts">The collection of <see cref="Part"/> objects to add.</param>
    /// <param name="appendToLastContent">Indicates whether to append the <see cref="Part"/> to the latest <see cref="Content"/> or Create a new <see cref="Content"/></param>
    /// <param name="role">The role associated with the <see cref="Part"/> objects. Defaults to <see cref="Roles.User"/>.</param>
    public static void AddParts(
        this IContentsRequest request,
        IEnumerable<Part> parts,
        bool appendToLastContent = true,
        string role = Roles.User)
    {
        if (appendToLastContent)
        {
            var lastContent = request.Contents.LastOrDefault();
            if (lastContent == null)
            {
                lastContent = new Content(parts.ToList(), role);
                AddContent(request, lastContent);
            }
            else
            {
                lastContent.AddParts(parts);
            }
        }
        else
        {
            var content = new Content(parts.ToList(), role);
            AddContent(request, content);
        }
    }

    /// <summary>
    /// Adds a new <see cref="Content"/> that includes an inline file as a <see cref="Part"/> to the <see cref="IContentsRequest"/>.
    /// </summary>
    /// <param name="request">The <see cref="IContentsRequest"/> instance to which the inline file will be added.</param>
    /// <param name="filePath">The file path of the inline file to be added.</param>
    /// <param name="appendToLastContent">Indicates whether to append the <see cref="Part"/> to the latest <see cref="Content"/> or Create a new <see cref="Content"/></param>
    /// <param name="role">The role associated with the inline file. Defaults to <see cref="Roles.User"/>.</param>
    public static void AddInlineFile(
        this IContentsRequest request,
        string filePath, 
        bool appendToLastContent = true, 
        string role = Roles.User)
    {
        if (appendToLastContent)
        {
            var lastContent = request.Contents.LastOrDefault();
            if (lastContent == null)
            {
                lastContent = new Content();
                lastContent.Role = role;
                lastContent.AddInlineFile(filePath, role);
                AddContent(request, lastContent);
            }
            else
            {
                lastContent.AddInlineFile(filePath, role);
            }
        }
        else
        {
            var content = new Content();
            content.Role = role;
            content.AddInlineFile(filePath, role);
            AddContent(request, content);
        }
    }

    /// <summary>
    /// Adds inline data such as an image or audio file into the latest <see cref="Content"/> in the <see cref="IContentsRequest"/> and associates it with the specified role.
    /// </summary>
    /// <param name="request">The <see cref="IContentsRequest"/> instance to which the inline data will be added.</param>
    /// <param name="data">The inline data to include, represented as a base64-encoded string.</param>
    /// <param name="mimeType">The MIME type of the inline data.</param>
    /// <param name="appendToLastContent">Indicates whether to append the <see cref="Part"/> to the latest <see cref="Content"/> or Create a new <see cref="Content"/></param>
    /// <param name="role">The role associated with the inline data. Defaults to <see cref="Roles.User"/>.</param>
    public static void AddInlineData(
        this IContentsRequest request,
        string data,
        string mimeType, 
        bool appendToLastContent = true, 
        string role = Roles.User)
    {
        if (appendToLastContent)
        {
            var lastContent = request.Contents.LastOrDefault();
            if (lastContent == null)
            {
                lastContent = new Content();
                lastContent.Role = role;
                lastContent.AddInlineData(data, mimeType);
                AddContent(request, lastContent);
            }
            else
            {
                lastContent.AddInlineData(data, mimeType);
            }
        }
        else
        {
            var content = new Content();
            content.Role = role;
            content.AddInlineData(data, mimeType);
            AddContent(request, content);
        }
    }

    /// <summary>
    /// Adds a new <see cref="Content"/> object to the <see cref="IContentsRequest"/>.
    /// </summary>
    /// <param name="request">The <see cref="IContentsRequest"/> instance to which the <see cref="Content"/> will be added.</param>
    /// <param name="content">The <see cref="Content"/> object to add to the request.</param>
    public static void AddContent(
        this IContentsRequest request,
        Content content)
    {
        request.Contents.Add(content);
    }
 
    
    /// <summary>
    /// Adds a remote file to the <see cref="IContentsRequest"/> using a <see cref="RemoteFile"/> object.
    /// </summary>
    /// <param name="request">The <see cref="IContentsRequest"/> instance to which the remote file will be added.</param>
    /// <param name="file">The <see cref="RemoteFile"/> object to be added.</param>
    /// <param name="appendToLastContent">Indicates whether to append the <see cref="Part"/> to the latest <see cref="Content"/> or Create a new <see cref="Content"/></param>
    /// <param name="role">The role associated with the remote file. Defaults to <see cref="Roles.User"/>.</param>
    public static void AddRemoteFile(
        this GenerateContentRequest request,
        RemoteFile file,
        bool appendToLastContent = true,
        string role = Roles.User)
    {
        if (appendToLastContent)
        {
            var lastContent = request.Contents.LastOrDefault();
            if (lastContent == null)
            {
                lastContent = new Content();
                lastContent.Role = role;
                lastContent.AddRemoteFile(file);
                AddContent(request, lastContent);
            }
            else
            {
                lastContent.AddRemoteFile(file);
            }
        }
        else
        {
            var content = new Content();
            content.Role = role;
            content.AddRemoteFile(file);
            AddContent(request,content);
        }
    }/// <summary>
    /// Adds a remote file to the <see cref="IContentsRequest"/> using a <see cref="RemoteFile"/> object.
    /// </summary>
    /// <param name="request">The <see cref="IContentsRequest"/> instance to which the remote file will be added.</param>
    /// <param name="file">The <see cref="RemoteFile"/> object to be added.</param>
    /// <param name="appendToLastContent">Indicates whether to append the <see cref="Part"/> to the latest <see cref="Content"/> or Create a new <see cref="Content"/></param>
    /// <param name="role">The role associated with the remote file. Defaults to <see cref="Roles.User"/>.</param>
    public static void AddRemoteFile(
        this IContentsRequest request,
        RemoteFile file,
        bool appendToLastContent = true,
        string role = Roles.User)
    {
        if (appendToLastContent)
        {
            var lastContent = request.Contents.LastOrDefault();
            if (lastContent == null)
            {
                lastContent = new Content();
                lastContent.Role = role;
                lastContent.AddRemoteFile(file);
                AddContent(request, lastContent);
            }
            else
            {
                lastContent.AddRemoteFile(file);
            }
        }
        else
        {
            var content = new Content();
            content.Role = role;
            content.AddRemoteFile(file);
            AddContent(request,content);
        }
    }
    
    /// <summary>
    /// Adds a remote file to the <see cref="IContentsRequest"/> using a URL and MIME type.
    /// </summary>
    /// <param name="request">The <see cref="IContentsRequest"/> instance to which the remote file will be added.</param>
    /// <param name="url">The URL of the remote file to be added.</param>
    /// <param name="mimeType">The MIME type of the remote file. Must be one of the supported formats defined in <see cref="FilesConstants.SupportedMimeTypes"/>.</param>
    /// <param name="appendToLastContent">Indicates whether to append the <see cref="Part"/> to the latest <see cref="Content"/> or Create a new <see cref="Content"/></param>
    /// <param name="role">The role associated with the remote file. Defaults to <see cref="Roles.User"/>.</param>
    public static void AddRemoteFile(
        this IContentsRequest request,
        string url,
        string mimeType,
        bool appendToLastContent = true,
        string role = Roles.User)
    {
        if (appendToLastContent)
        {
            var lastContent = request.Contents.LastOrDefault();
            if (lastContent == null)
            {
                lastContent = new Content();
                lastContent.Role = role;
                lastContent.AddRemoteFile(url, mimeType);
               
                AddContent(request, lastContent);
            }
            else
            {
                lastContent.AddRemoteFile(url, mimeType);
            }
        }
        else
        {
            var content = new Content();
            content.Role = role;
            content.AddRemoteFile(url, mimeType);
            request.Contents.Add(content);
        }
    }
}