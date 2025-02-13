using System.Text.RegularExpressions;
using GenerativeAI.Core;
using GenerativeAI.Utility;

namespace GenerativeAI;

/// <summary>
/// Provides extension methods for extracting structured data, such as code blocks and JSON blocks,
/// from markdown strings.
/// </summary>
public static class MarkdownCodeExtractorExtension

{
    /// <summary>
    /// Extracts code blocks from a given markdown string and returns them as a list of <see cref="CodeBlock"/> objects.
    /// </summary>
    /// <param name="markdown">The markdown string from which to extract code blocks.</param>
    /// <returns>A list of <see cref="CodeBlock"/> objects representing the code blocks found in the markdown.</returns>
    public static List<CodeBlock> ExtractCodeBlocks(this string markdown)
    {
        return MarkdownExtractor.ExtractCodeBlocks(markdown);
    }

    /// <summary>
    /// Extracts JSON objects and arrays from a given markdown string and returns them as a list of <see cref="JsonBlock"/> objects.
    /// </summary>
    /// <param name="markdown">The markdown string from which to extract JSON blocks.</param>
    /// <returns>A list of <see cref="JsonBlock"/> objects representing the JSON blocks (objects or arrays) found in the markdown.</returns>
    public static List<JsonBlock> ExtractJsonBlocks(this string markdown)
    {
        return MarkdownExtractor.ExtractJsonBlocks(markdown);
    }
}