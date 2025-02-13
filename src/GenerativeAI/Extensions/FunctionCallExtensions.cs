using GenerativeAI.Types;

namespace GenerativeAI;

public static class FunctionCallExtensions
{
    public static Content ToFunctionCallContent(this FunctionResponse? responseContent)
    {
        var content = new Content()
        {
            Role = Roles.Function,
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
}