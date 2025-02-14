using GenerativeAI.Types;

namespace GenerativeAI;

/// <summary>
/// A static class providing extension methods for formatting request contents used in generative AI models.
/// </summary>
public static class RequestExtensions
{
    /// <summary>
    /// Creates a <see cref="Content"/> object based on the specified input text and role.
    /// </summary>
    /// <param name="params">The input text to be formatted into the content object.</param>
    /// <param name="role">The role associated with the content. Defaults to "user".</param>
    /// <returns>A new instance of <see cref="Content"/> containing the formatted input and specified role.</returns>
    public static Content FormatGenerateContentInput(string @params, string role = Roles.User)
    {
        var parts = new[]{new Part(){Text = @params}};
        return new Content(parts, role);
    }

    /// <summary>
    /// Creates a <see cref="Content"/> object for system instructions based on the specified input string.
    /// </summary>
    /// <param name="params">The system instruction text to be formatted into the content object. If null or empty, returns null.</param>
    /// <returns>A new instance of <see cref="Content"/> containing the formatted input as a system role, or null if the input is null or empty.</returns>
    public static Content? FormatSystemInstruction(string? @params)
    {
        if (string.IsNullOrEmpty(@params))
            return null;
        var parts = new[] { new Part() { Text = @params } };
        return new Content(parts, Roles.System);
    }

    /// <summary>
    /// Creates a <see cref="Content"/> object using the specified parts of a request and role.
    /// </summary>
    /// <param name="request">A collection of strings representing individual parts of the request.</param>
    /// <param name="role">The role associated with the content. Defaults to "user".</param>
    /// <returns>A new instance of <see cref="Content"/> containing the specified parts and role.</returns>
    public static Content FormatGenerateContentInput( IEnumerable<string> request, string role = Roles.User)
    {
        var parts = request.Select(part => new Part() { Text = part }).ToArray();

        return new Content(parts, role);
    }

    /// <summary>
    /// Creates a <see cref="Content"/> object based on the specified collection of <see cref="Part"/> instances and role.
    /// </summary>
    /// <param name="request">A collection of <see cref="Part"/> objects representing the content components.</param>
    /// <param name="role">The role associated with the content. Defaults to "user".</param>
    /// <returns>A new instance of <see cref="Content"/> containing the provided parts and specified role.</returns>
    public static Content FormatGenerateContentInput(IEnumerable<Part> request, string role = Roles.User)
    {
        return new Content(request.ToArray(), role);
    }
    
    
}