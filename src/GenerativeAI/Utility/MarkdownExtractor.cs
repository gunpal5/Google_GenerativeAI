using System.Text.Json;
using System.Text.RegularExpressions;
using GenerativeAI.Core;

namespace GenerativeAI.Utility;

/// <summary>
/// The <c>MarkdownExtractor</c> class provides functionality to extract code blocks
/// from markdown content. The extracted code blocks can include fenced code blocks
/// with specified programming languages and indented code blocks. It also supports
/// filtering by programming language.
/// </summary>
public class MarkdownExtractor
{
    /// <summary>
    /// Extracts code blocks from a given markdown text, optionally filtering by programming language.
    /// Code blocks can include fenced code blocks with a specific language tag as well as indented code blocks.
    /// </summary>
    /// <param name="markdown">The markdown content from which code blocks will be extracted.</param>
    /// <param name="languageFilter">
    /// An optional filter to extract only code blocks of a specific programming language.
    /// Use "*" to extract code blocks of any language.
    /// </param>
    /// <returns>
    /// A list of <see cref="CodeBlock">CodeBlock</see> objects, each representing a code segment with details
    /// about its programming language and line number.
    /// </returns>
    public static List<CodeBlock> ExtractCodeBlocks(string markdown, string languageFilter = "*")
    {
        List<CodeBlock> extractedCodeBlocks = new List<CodeBlock>();
        string[] lines = markdown.Split('\n');
        bool insideFencedBlock = false; // Track if we are inside a fenced block

        // Match all fenced code blocks and keep track of line numbers
        MatchCollection codeMatches =
            Regex.Matches(markdown, @"```([a-zA-Z0-9+#-]*)\n(.*?)\n```", RegexOptions.Singleline);
        foreach (Match codeMatch in codeMatches)
        {
            string language = codeMatch.Groups[1].Value.Trim().ToLower(); // Group 1: Language
            string code = codeMatch.Groups[2].Value.Trim(); // Group 2: Code

            if (LanguageMatches(language, languageFilter))
            {
                int lineNumber = markdown.Substring(0, codeMatch.Index).Split('\n').Length;
                extractedCodeBlocks.Add(new CodeBlock(language, code, lineNumber));
            }
        }

        // Extract indented code blocks (assumed language is unknown) while maintaining order
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            // Detect fenced code block start
            if (line.StartsWith("```"))
            {
                insideFencedBlock = !insideFencedBlock;
                continue;
            }

            // Ignore indented code while inside a fenced block
            if (insideFencedBlock)
            {
                continue;
            }

            // Detect indented code blocks
            if (Regex.IsMatch(line, @"^ {4,}")) // Line starts with 4+ spaces
            {
                List<string> codeLines = new List<string>();
                while (i < lines.Length && Regex.IsMatch(lines[i], @"^ {4,}"))
                {
                    codeLines.Add(lines[i].Substring(4)); // Remove leading spaces
                    i++;
                }

                i--; // Adjust for loop increment

                string code = string.Join("\n", codeLines).Trim();
                if (!string.IsNullOrEmpty(code))
                {
                    int lineNumber = i + 1; // Line number of the first line of the indented block
                    extractedCodeBlocks.Add(new CodeBlock("", code, lineNumber));
                }
            }
        }

        // Sort code blocks by their appearance in the markdown (line number)
        extractedCodeBlocks.Sort((block1, block2) => block1.LineNumber.CompareTo(block2.LineNumber));

        return extractedCodeBlocks;
    }

    private static bool LanguageMatches(string language, string filter)
    {
        return filter == "*" || string.Equals(language, filter, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Extracts JSON blocks (objects or arrays) from a given text. This method identifies JSON objects
    /// and arrays, including those spanning multiple lines, and extracts them along with their metadata.
    /// </summary>
    /// <param name="text">The text content from which JSON blocks will be extracted.</param>
    /// <returns>
    /// A list of <see cref="JsonBlock">JsonBlock</see> objects, each representing a JSON object or array,
    /// including its content, starting line number, and whether it is an array.
    /// </returns>
    public static List<JsonBlock> ExtractJsonBlocks(string text)
    {
        List<JsonBlock> extractedJsonObjectsAndArrays = new List<JsonBlock>();
        string[] lines = text.Split('\n');
        string currentJsonContent = "";
        bool insideJson = false;
        int openBracesCount = 0; // Tracks the number of opening curly braces '{'
        int openBracketsCount = 0; // Tracks the number of opening square brackets '['
        bool inArrayContext = false; // To track if we're inside a JSON array context

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            // Look for the start of a JSON object or array (curly brace or square bracket)
            for (int j = 0; j < line.Length; j++)
            {
                if (line[j] == '{') // Start of JSON object
                {
                    // If we are not inside a JSON array context, it's a new object
                    if (!inArrayContext && !insideJson)
                    {
                        insideJson = true;
                        currentJsonContent = line.Substring(j,1); // Start collecting the JSON content from the '{'
                        openBracesCount = 1; // We have one open brace
                    }
                    else
                    {
                        // Continue accumulating nested JSON content for objects
                        currentJsonContent += line[j];
                        if(!inArrayContext)
                            openBracesCount++; // Increment the open brace counter for nested objects
                    }
                }
                else if (line[j] == '}') // End of JSON object
                {
                    if (insideJson)
                    {
                        currentJsonContent += line[j];
                        openBracesCount--; // Decrement for closing brace

                        if (openBracesCount == 0) // Object is complete
                        {
                            // Validate and add the JSON object to the result if it's valid
                            if (IsValidJson(currentJsonContent.Trim()))
                            {
                                extractedJsonObjectsAndArrays.Add(new JsonBlock(currentJsonContent.Trim(), i + 1, false)); // false for object
                            }
                            insideJson = false;
                            currentJsonContent = "";
                        }
                    }

                    if (inArrayContext)
                    {
                        currentJsonContent += line[j];
                    }
                }
                else if (line[j] == '[') // Start of JSON array
                {
                    // If we are not inside an object context, it's a new array
                    if (!insideJson && !inArrayContext)
                    {
                        inArrayContext = true;
                        currentJsonContent = line.Substring(j,1); // Start collecting the JSON content from the '['
                        openBracketsCount = 1; // We have one open bracket
                    }
                    else
                    {
                        // Continue accumulating nested JSON content for arrays
                        currentJsonContent += line[j];
                        if(!insideJson)
                        openBracketsCount++; // Increment the open bracket counter for nested arrays
                    }
                }
                else if (line[j] == ']') // End of JSON array
                {
                    if (inArrayContext)
                    {
                        currentJsonContent += line[j];
                        openBracketsCount--; // Decrement for closing bracket

                        if (openBracketsCount == 0) // Array is complete
                        {
                            // Validate and add the JSON array to the result if it's valid
                            if (IsValidJson(currentJsonContent.Trim()))
                            {
                                extractedJsonObjectsAndArrays.Add(new JsonBlock(currentJsonContent.Trim(), i + 1, true)); // true for array
                            }
                            inArrayContext = false;
                            currentJsonContent = "";
                        }
                    }

                    if (insideJson)
                    {
                        currentJsonContent += line[j];
                    }
                }
                // Continue accumulating the JSON content for multiline JSON
                else if (insideJson || inArrayContext)
                {
                    currentJsonContent += line[j];
                }
            }
        }

        return extractedJsonObjectsAndArrays;
    } 
    // public static List<JsonBlock> ExtractJsonBlocks(string text)
    // {
    //     List<JsonBlock> extractedJsonObjects = new List<JsonBlock>();
    //     string[] lines = text.Split('\n');
    //     string currentJsonContent = "";
    //     bool insideJson = false;
    //     int openBracesCount = 0; // Tracks the number of opening curly braces '{'
    //     
    //     for (int i = 0; i < lines.Length; i++)
    //     {
    //         string line = lines[i].Trim();
    //
    //         // Look for the start of a JSON object (an opening curly brace)
    //         for (int j = 0; j < line.Length; j++)
    //         {
    //             if (line[j] == '{')
    //             {
    //                 // If we are not already inside a JSON object, start a new one
    //                 if (!insideJson)
    //                 {
    //                     insideJson = true;
    //                     currentJsonContent = line.Substring(j,1); // Start collecting the JSON content from the '{'
    //                     openBracesCount = 1; // We have one open brace
    //                 }
    //                 else
    //                 {
    //                     // Otherwise, continue accumulating nested JSON content
    //                     currentJsonContent += line[j];
    //                     openBracesCount++; // Increment the open brace counter for nested JSON
    //                 }
    //             }
    //             else if (line[j] == '}')
    //             {
    //                 if (insideJson)
    //                 {
    //                     currentJsonContent += line[j]; // Continue adding closing brace
    //                     openBracesCount--; // Decrement for closing brace
    //
    //                     // When the open and close braces are balanced, it's a complete JSON object
    //                     if (openBracesCount == 0)
    //                     {
    //                         // Validate and add the JSON object to the result if it's valid
    //                         if (IsValidJson(currentJsonContent.Trim()))
    //                         {
    //                             extractedJsonObjects.Add(new JsonBlock(currentJsonContent.Trim(), i + 1)); // Add the JSON object with its line number
    //                         }
    //                         // Reset for the next JSON object
    //                         insideJson = false;
    //                         currentJsonContent = "";
    //                     }
    //                 }
    //             }
    //             // Continue accumulating the JSON content for multiline JSON
    //             else if (insideJson)
    //             {
    //                 currentJsonContent += line[j];
    //             }
    //         }
    //     }
    //
    //     return extractedJsonObjects;
    // }

    // Helper method to check if a string is valid JSON
    private static bool IsValidJson(string JsonBlock)
    {
        try
        {
            // Try to parse the string into a valid JSON object
            using (JsonDocument doc = JsonDocument.Parse(JsonBlock))
            {
                return doc != null; // If parsing is successful, it's valid JSON
            }
        }
        catch
        {
            return false; // If an exception is thrown, it's not valid JSON
        }
    }

}