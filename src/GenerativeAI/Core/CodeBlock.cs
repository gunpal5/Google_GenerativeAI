namespace GenerativeAI.Core;

/// <summary>
/// Represents a segment of source code with details about its programming language and line number.
/// </summary>
public class CodeBlock
{
    /// <summary>
    /// Gets or sets the content of the code block as a string.
    /// </summary>
    /// <remarks>
    /// This property holds the actual code segment stored within the code block.
    /// It is typically paired with the <see cref="Language"/> property to describe the code and its programming language context.
    /// </remarks>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the programming language associated with the code block.
    /// </summary>
    /// <remarks>
    /// This property specifies the language in which the code in the <c>Code</c> property is written.
    /// It helps in identifying and appropriately processing or highlighting the code based on its language.
    /// </remarks>
    public string Language { get; set; }

    /// <summary>
    /// Gets or sets the line number where the code block appears in the source text.
    /// </summary>
    /// <remarks>
    /// This property indicates the starting line number of a code block, providing
    /// contextual information about its location within the original source content.
    /// </remarks>
    public int LineNumber { get; set; }

    /// <summary>
    /// Represents a block of code with its corresponding programming language and line number information.
    /// </summary>
    public CodeBlock(string language, string code, int lineNumber)
    {
        Code = code;
        Language = language;
        LineNumber = lineNumber;
    }
    /// <summary>
    /// Represents a block of code with its corresponding programming language and line number information.
    /// </summary>
    public CodeBlock(string language, string code)
    {
        Code = code;
        Language = language;
    }

    /// <summary>
    /// Returns a string representation of the CodeBlock object, including the programming language
    /// and the corresponding code in a formatted manner.
    /// </summary>
    /// <returns>
    /// A string that contains the language of the code and the formatted code content.
    /// </returns>
    public override string ToString()
    {
        return $"Language: {Language}\nCode:\n{Code}";
    }
}