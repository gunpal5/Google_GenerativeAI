using GenerativeAI.Constants;
using GenerativeAI.Types;

namespace GenerativeAI.Extensions;

public static class RequestExtensions
{
    public static Content FormatGenerateContentInput(string @params, string role = Roles.User)
    {
        var parts = new[]{new Part(){Text = @params}};
        return new Content(parts, role);
    }

    public static Content FormatSystemInstruction(string @params)
    {
        var parts = new[] { new Part() { Text = @params } };
        return new Content(parts, Roles.System);
    }

    public static Content FormatGenerateContentInput( IEnumerable<string> request, string role = Roles.User)
    {
        var parts = request.Select(part => new Part() { Text = part }).ToArray();

        return new Content(parts, role);
    }

    public static Content FormatGenerateContentInput(IEnumerable<Part> request, string role = Roles.User)
    {
        return new Content(request.ToArray(), role);
    }
}