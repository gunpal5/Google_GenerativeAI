using GenerativeAI.Types;

namespace GenerativeAI;

/// <summary>
/// Provides extension methods for working with function call-related types.
/// </summary>
public static class FunctionCallExtensions
{
    /// <summary>
    /// Converts a nullable <see cref="FunctionResponse"/> into a <see cref="Content"/> object configured with the role
    /// "user" (as the official SDKs send function results) and containing the response as a single part of the content.
    /// </summary>
    /// <param name="responseContent">A nullable <see cref="FunctionResponse"/> representing the function response to be converted into content.</param>
    /// <returns>A <see cref="Content"/> object with the "user" role and a single part containing the provided function response.</returns>
    public static Content ToFunctionCallContent(this FunctionResponse? responseContent)
    {
        var content = new Content()
        {
            Role = Roles.User,
            Parts = new[]
            {
                new Part()
                {
                    FunctionResponse = responseContent
                }
            }.ToList()
        };
        return content;
    }

    /// <summary>
    /// Converts a nullable <see cref="FunctionResponse"/> into a <see cref="Content"/> object configured with the role
    /// "user" (as the official SDKs send function results) and containing the provided function responses.
    /// </summary>
    /// <param name="responses">A list of <see cref="FunctionResponse"/> objects to be converted into content.</param>
    /// <returns>A <see cref="Content"/> object with the "user" role and parts containing the provided function responses.</returns>
    public static Content ToFunctionCallContent(this List<FunctionResponse> responses)
    {
        var content = new Content()
        {
            Role = Roles.User,
            Parts = responses.Select(r => new Part() { FunctionResponse = r }).ToList()
        };
        return content;
    }
}