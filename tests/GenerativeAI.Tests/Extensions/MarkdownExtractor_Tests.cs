using GenerativeAI.Utility;
using Shouldly;

namespace GenerativeAI.Tests.Extensions;

public class MarkdownCodeExtractorTests
{
    #region ExtractCodeBlock Tests

    [Fact]
    public void ExtractCodeBlocks_EmptyMarkdown_ReturnsEmptyList()
    {
        var codeBlocks = MarkdownExtractor.ExtractCodeBlocks("");
        codeBlocks.ShouldBeEmpty();
    }

    [Fact]
    public void ExtractCodeBlocks_NoCodeBlocks_ReturnsEmptyList()
    {
        var codeBlocks = MarkdownExtractor.ExtractCodeBlocks("This is just some text with no code.");
        codeBlocks.ShouldBeEmpty();
    }

    [Fact]
    public void ExtractCodeBlocks_SingleFencedBlock_ReturnsCorrectCodeAndLanguage()
    {
        var markdown = "```csharp\npublic class Test {}\n```";
        var codeBlocks = MarkdownExtractor.ExtractCodeBlocks(markdown);
        codeBlocks.Count.ShouldBe(1);
        codeBlocks[0].Code.ShouldBe("public class Test {}");
        codeBlocks[0].Language.ShouldBe("csharp");
    }

    [Fact]
    public void ExtractCodeBlocks_SingleFencedBlockNoLanguage_ReturnsCorrectCodeAndEmptyLanguage()
    {
        var markdown = "```\npublic class Test {}\n```";
        var codeBlocks = MarkdownExtractor.ExtractCodeBlocks(markdown);
        codeBlocks.Count.ShouldBe(1);
        codeBlocks[0].Code.ShouldBe("public class Test {}");
        codeBlocks[0].Language.ShouldBe("");
    }


    [Fact]
    public void ExtractCodeBlocks_MultipleFencedBlocks_ReturnsAllCodeBlocks()
    {
        var markdown = "```csharp\npublic class Test1 {}\n```\n```python\nprint('hello')\n```";
        var codeBlocks = MarkdownExtractor.ExtractCodeBlocks(markdown);
        codeBlocks.Count.ShouldBe(2);
        codeBlocks[0].Code.ShouldBe("public class Test1 {}");
        codeBlocks[0].Language.ShouldBe("csharp");
        codeBlocks[1].Code.ShouldBe("print('hello')");
        codeBlocks[1].Language.ShouldBe("python");
    }

    [Fact]
    public void ExtractCodeBlocks_IndentedBlock_ReturnsCorrectCodeAndEmptyLanguage()
    {
        var markdown = "    public class Test {}\n";
        var codeBlocks = MarkdownExtractor.ExtractCodeBlocks(markdown);
        codeBlocks.Count.ShouldBe(1);
        codeBlocks[0].Code.ShouldBe("public class Test {}");
        codeBlocks[0].Language.ShouldBe(""); // Indented blocks usually don't have a language
    }

    [Fact]
    public void ExtractCodeBlocks_MultipleIndentedBlocks_ReturnsAllCodeBlocks()
    {
        var markdown = "    public class Test1 {}\n    public class Test2 {}\n";
        var codeBlocks = MarkdownExtractor.ExtractCodeBlocks(markdown);
        codeBlocks.Count.ShouldBe(1);
        codeBlocks[0].Code.ShouldBe("public class Test1 {}\npublic class Test2 {}");
        codeBlocks[0].Language.ShouldBe("");
    }

    [Fact]
    public void ExtractCodeBlocks_MixedBlocks_ReturnsAllCodeBlocks()
    {
        var markdown = "```csharp\npublic class Test1 {}\n```\n    int x = 10;\n```python\nprint('hello')\n```";
        var codeBlocks = MarkdownExtractor.ExtractCodeBlocks(markdown);
        codeBlocks.Count.ShouldBe(3);
        codeBlocks[0].Code.ShouldBe("public class Test1 {}");
        codeBlocks[0].Language.ShouldBe("csharp");
        codeBlocks[1].Code.ShouldBe("int x = 10;");
        codeBlocks[1].Language.ShouldBe("");
        codeBlocks[2].Code.ShouldBe("print('hello')");
        codeBlocks[2].Language.ShouldBe("python");
    }

    [Fact]
    public void ExtractCodeBlocks_BacktickBlock_ReturnsCorrectCodeAndLanguage()
    {
        var markdown = "```javascript\nconsole.log('Hello');\n```";
        var codeBlocks = MarkdownExtractor.ExtractCodeBlocks(markdown);
        codeBlocks.Count.ShouldBe(1);
        codeBlocks[0].Code.ShouldBe("console.log('Hello');");
        codeBlocks[0].Language.ShouldBe("javascript");
    }

    [Fact]
    public void ExtractCodeBlocks_BacktickBlockNoLanguage_ReturnsCorrectCodeAndEmptyLanguage()
    {
        var markdown = "```\nconsole.log('Hello');\n```";
        var codeBlocks = MarkdownExtractor.ExtractCodeBlocks(markdown);
        codeBlocks.Count.ShouldBe(1);
        codeBlocks[0].Code.ShouldBe("console.log('Hello');");
        codeBlocks[0].Language.ShouldBe("");
    }

    [Fact]
    public void ExtractCodeBlocks_MultipleBacktickBlocks_ReturnsAllCodeBlocks()
    {
        var markdown = "```javascript\nconsole.log('Hello');\n```\n```python\nprint('hello')\n```";
        var codeBlocks = MarkdownExtractor.ExtractCodeBlocks(markdown);
        codeBlocks.Count.ShouldBe(2);
        codeBlocks[0].Code.ShouldBe("console.log('Hello');");
        codeBlocks[0].Language.ShouldBe("javascript");
        codeBlocks[1].Code.ShouldBe("print('hello')");
        codeBlocks[1].Language.ShouldBe("python");
    }

    [Fact]
    public void ExtractCodeBlocks_MixedBlocksIncludingBackticks_ReturnsAllCodeBlocks()
    {
        var markdown =
            "```csharp\npublic class Test1 {}\n```\n    int x = 10;\n```python\nprint('hello')\n```\n```javascript\nconsole.log('Hello');\n```";
        var codeBlocks = MarkdownExtractor.ExtractCodeBlocks(markdown);
        codeBlocks.Count.ShouldBe(4);
        codeBlocks[0].Code.ShouldBe("public class Test1 {}");
        codeBlocks[0].Language.ShouldBe("csharp");
        codeBlocks[1].Code.ShouldBe("int x = 10;");
        codeBlocks[1].Language.ShouldBe("");
        codeBlocks[2].Code.ShouldBe("print('hello')");
        codeBlocks[2].Language.ShouldBe("python");
        codeBlocks[3].Code.ShouldBe("console.log('Hello');");
        codeBlocks[3].Language.ShouldBe("javascript");
    }

    [Theory]
    [MemberData(nameof(GetCodeBlockExtractionData))]
    public void ExtractCodeBlocks_Theory(string markdown, (string code, string language)[] expectedBlocks)
    {
        // Arrange & Act
        var codeBlocks = MarkdownExtractor.ExtractCodeBlocks(markdown);

        // Assert
        codeBlocks.Count.ShouldBe(expectedBlocks.Length);

        for (int i = 0; i < expectedBlocks.Length; i++)
        {
            codeBlocks[i].Code.ShouldBe(expectedBlocks[i].code);
            codeBlocks[i].Language.ShouldBe(expectedBlocks[i].language);
        }
    }

    public static IEnumerable<object[]> GetCodeBlockExtractionData()
    {
        // Random Markdown with stuffed code blocks and normal text
        yield return new object[]
        {
            "Here is some text.\n```csharp\nint a = 5;\n```\nAnd more text after the code.",
            new (string, string)[] { ("int a = 5;", "csharp") }
        };

        yield return new object[]
        {
            "Normal text, then a code block:\n```\nstring text = \"hello\";\n```\nAfter the code block.",
            new (string, string)[] { ("string text = \"hello\";", "") }
        };

        yield return new object[]
        {
            "Multiple codes mixed with text:\n```python\nprint(\"Hello\")\n```\nHere is text.\n```java\nSystem.out.println(\"World!\");\n```",
            new (string, string)[] { ("print(\"Hello\")", "python"), ("System.out.println(\"World!\");", "java") }
        };

        yield return new object[]
        {
            "No code here, just some clear markdown text.\nNo code at all!",
            new (string, string)[0]
        };

        yield return new object[]
        {
            "```javascript\nconsole.log('Start');\n```\nSome plain text.\n```ruby\nputs 'Done!'\n```",
            new (string, string)[] { ("console.log('Start');", "javascript"), ("puts 'Done!'", "ruby") }
        };

        yield return new object[]
        {
            "Here are indented blocks:\n    var x = 10;\nPlain text between them.\n    string name = \"Test\";",
            new (string, string)[] { ("var x = 10;", ""), ("string name = \"Test\";", "") }
        };

        yield return new object[]
        {
            "Text at the start.\n```csharp\nint number = 42;\n```\nMore text in-between.\n    class MyClass {}\nFinal bit of text.",
            new (string, string)[] { ("int number = 42;", "csharp"), ("class MyClass {}", "") }
        };

        yield return new object[]
        {
            "Edge case:\nSome text.\n```\n# Just a comment\n```\nIndented but no code:\n    # Another comment",
            new (string, string)[] { ("# Just a comment", ""), ("# Another comment", "") }
        };

        yield return new object[]
        {
            "Markdown with mixed style:\n```python\ndef hello():\n    pass\n```\nNormal text here.\n    SubText();\nAnd more.\n```javascript\nlet data = {};\n```",
            new (string, string)[]
                { ("def hello():\n    pass", "python"), ("SubText();", ""), ("let data = {};", "javascript") }
        };

        yield return new object[]
        {
            "Markdown having empty fences:\n```\n```\nText follows.",
            new (string, string)[0]
        };
    }

    #endregion

    #region JsonBlock Extractor Tests

    [Theory]
    [MemberData(nameof(GetTestData))]
    public void ExtractJsonBlocks_Returns_ExpectedBlocks(
        string markdown,
        int expectedCount,
        string scenarioDescription
    )
    {
        // Act
        var result = MarkdownExtractor.ExtractJsonBlocks(markdown);

        foreach (var res in result)
        {
            Console.WriteLine(res.ToString());
        }

        // Assert
        result.ShouldNotBeNull(scenarioDescription);
        result.Count.ShouldBe(expectedCount, scenarioDescription);
    }

    // You can adapt or rename this method to suit your particular test structure.
    // The test data below provides multiple scenarios: single fenced JSON, inline JSON,
    // multiple/mixed JSON blocks, invalid JSON, multiline JSON, and no JSON.
    public static IEnumerable<object[]> GetTestData()
    {
        yield return new object[]
        {
            "```json\n{\n  \"name\": \"John\",\n  \"age\": 30,\n  \"isStudent\": false,\n  \"subjects\": [\"Math\", \"Science\"],\n  \"address\": {\n    \"city\": \"New York\",\n    \"zip\": \"10001\"\n  }\n}\n```",
            1,
            "Nested JSON object with an array and multiple fields."
        };
        yield return new object[]
        {
            "Here is some inline JSON: {\"name\": \"Alice\", \"scores\": [10, 20, 30], \"meta\": {\"active\": true}}. And some text after that.",
            1,
            "Complex inline JSON with an array and nested object."
        };
        yield return new object[]
        {
            "```json\n{ \"books\": [ {\"title\": \"Book 1\", \"author\": \"Author 1\"}, {\"title\": \"Book 2\", \"author\": \"Author 2\"} ] }\n```\nSome inline JSON {\"count\": 2}. And another one ```json\n{\"key\": \"value\"}\n```",
            3,
            "Fenced JSON with an array of objects, inline JSON, and another fenced JSON."
        };
        yield return new object[]
        {
            "Malformed JSON example:\n```json\n{\n  \"key1\": \"value1\",\n  \"key2\": \n```\nAnother example of malformed inline JSON: {\"incomplete\": \"value\".",
            0,
            "Only malformed JSON blocks are present—none are valid."
        };
        yield return new object[]
        {
            "```json\n{\n  \"metadata\": {\n    \"tags\": [\"Complex\", \"Test\"],\n    \"info\": {\n      \"id\": 123,\n      \"description\": \"Nested structure\"\n    }\n  },\n  \"config\": [\n    {\"env\": \"staging\"},\n    {\"env\": \"production\"}\n  ]\n}\n```",
            1,
            "Deeply nested JSON block with an array and a dictionary field."
        };
        yield return new object[]
        {
            "Edge case test:\n```json\n{\n  \"key\": \"value\" // inline comment\n}\n```",
            0,
            "Invalid JSON block having comments."
        };
        yield return new object[]
        {
            "```json\n[\n  {\"product\": \"Item A\"},\n  {\"product\": \"Item B\", \"details\": {\"stock\": 50, \"price\": 20.5}}\n]\n```",
            1,
            "Complex JSON having mixed structures at various levels."
        };
        yield return new object[]
        {
            "Text follows.\n```json\n[\n  {\"name\": \"Array obj 1\"}, {\"name\": \"Array obj 2\", \"order\": 34},\n  {\"status\": \"Processing\"}\n]\n```",
            1,
            "Root level array objects with contract keys describing relationships."
        };
        yield return new object[]
        {
            "Intro text.\n```json\n{\n  \"config\": {\n    \"setting1\": true,\n    \"setting2\": \"default\",\n    \"options\": [\n      {\"key\": \"value1\"},\n      {\"key\": \"value2\"}\n    ]\n  }\n}\n```",
            1,
            "Deeply nested JSON structure."
        };
        yield return new object[]
        {
            "```json\n{\n  \"list\": [1, 2, 3, 4, 5]\n}\n```\nPlain text.\n```json\n{\n  \"matrix\": [[1, 2], [3, 4]]\n}\n```",
            2,
            "Multiple fenced JSON blocks with arrays and nested arrays."
        };
        yield return new object[]
        {
            "Text between JSONs.\n```json\n{\n  \"status\": \"ok\",\n  \"details\": {\"time\": \"2023-01-01T00:00:00Z\"}\n}\n```\nAnd inline JSON {\"log\": \"success\", \"retry\": false}.",
            2,
            "A valid fenced JSON block and a valid inline JSON block."
        };
        
        
        
    }

    #endregion
}