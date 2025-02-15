namespace GenerativeAI.GoogleAuth;

internal static class ScopesConstants
{
    internal readonly static List<string> Scopes =
    [
        "https://www.googleapis.com/auth/cloud-platform",
        "https://www.googleapis.com/auth/generative-language.retriever",
        "https://www.googleapis.com/auth/generative-language.tuning"
    ];
}