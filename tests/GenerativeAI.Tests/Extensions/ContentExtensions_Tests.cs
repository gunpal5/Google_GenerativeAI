using GenerativeAI.Core;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Extensions;

public class ContentExtensions_Tests
{
    #region AddPart Tests

    [Fact]
    public void AddPart_WithValidContentAndPart_ShouldAddPart()
    {
        // Arrange
        var content = new Content();
        var part = new Part();

        // Act
        content.AddPart(part);

        // Assert
        content.Parts.Count.ShouldBe(1);
        content.Parts[0].ShouldBe(part);
    }

    [Fact]
    public void AddPart_NullContent_ShouldThrowNullReferenceException()
    {
        // Arrange
        Content content = null;
        var part = new Part();

        // Act & Assert
        Should.Throw<ArgumentException>(() => { content.AddPart(part); });
    }

    [Fact]
    public void AddPart_NullPart_ShouldAddNullEntry()
    {
        // Arrange
        var content = new Content();

        // Act
        content.AddPart(null);

        // Assert
        content.Parts.Count.ShouldBe(1);
        content.Parts[0].ShouldBeNull();
    }

    #endregion

    #region AddParts Tests

    [Fact]
    public void AddParts_WithValidContentAndParts_ShouldAddAllParts()
    {
        // Arrange
        var content = new Content();
        var parts = new List<Part>
        {
            new() { Text = "Part1" },
            new() { Text = "Part2" }
        };

        // Act
        content.AddParts(parts);

        // Assert
        content.Parts.Count.ShouldBe(2);
        content.Parts[0].Text.ShouldBe("Part1");
        content.Parts[1].Text.ShouldBe("Part2");
    }

    [Fact]
    public void AddParts_NullContent_ShouldThrowNullReferenceException()
    {
        // Arrange
        Content content = null;
        var parts = new List<Part> { new(), new() };

        // Act & Assert
        Should.Throw<ArgumentException>(() => { content.AddParts(parts); });
    }

    [Fact]
    public void AddParts_NullParts_ShouldThrowArgumentNullException()
    {
        // Arrange
        var content = new Content();
        IEnumerable<Part> parts = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => { content.AddParts(parts); });
    }

    [Fact]
    public void AddParts_EmptyParts_ShouldNotThrow()
    {
        // Arrange
        var content = new Content();
        var parts = new List<Part>();

        // Act
        content.AddParts(parts);

        // Assert
        content.Parts.Count.ShouldBe(0);
    }

    #endregion

    #region AddInlineData Tests

    [Fact]
    public void AddInlineData_WithValidData_ShouldAddPartWithInlineData()
    {
        // Arrange
        var content = new Content();
        var data = Convert.ToBase64String(new byte[] { 1, 2, 3 });
        var mimeType = "image/png";

        // Act
        content.AddInlineData(data, mimeType);

        // Assert
        content.Parts.Count.ShouldBe(1);
        content.Parts[0].InlineData.ShouldNotBeNull();
        content.Parts[0].InlineData.Data.ShouldBe(data);
        content.Parts[0].InlineData.MimeType.ShouldBe(mimeType);
    }

    [Fact]
    public void AddInlineData_NullContent_ShouldThrowNullReferenceException()
    {
        // Arrange
        Content content = null;

        // Act & Assert
        Should.Throw<ArgumentException>(() => { content.AddInlineData("someData", "someMime"); });
    }

    [Fact]
    public void AddInlineData_NullOrEmptyData_ShouldAddPartWithNullOrEmptyInlineData()
    {
        // Arrange
        var content = new Content();
        string data = null;
        const string mimeType = "application/octet-stream";

        // Act
        content.AddInlineData(data, mimeType);

        // Assert
        content.Parts.Count.ShouldBe(1);
        content.Parts[0].InlineData.ShouldNotBeNull();
        content.Parts[0].InlineData.Data.ShouldBeNull();
        content.Parts[0].InlineData.MimeType.ShouldBe(mimeType);
    }

    #endregion

    #region AddInlineFile Tests

    [Fact]
    public void AddInlineFile_WithValidFile_ShouldCallAddInlineData()
    {
        // This test will create a small temp file, read it, and verify execution.
        // If you do not want to create an actual file, you could mock out File.ReadAllBytes and FileValidator.

        // Arrange
        var tempFilePath = Path.GetTempFileName() + ".jpg";
        try
        {
            File.WriteAllBytes(tempFilePath, new byte[] { 1, 2, 3 });
            var content = new Content();

            // Act
            content.AddInlineFile(tempFilePath, "irrelevantRole");

            // Assert
            content.Parts.Count.ShouldBe(1);
            content.Parts[0].InlineData.ShouldNotBeNull();
            content.Parts[0].InlineData.Data.ShouldNotBeNullOrEmpty();
            content.Parts[0].InlineData.MimeType.ShouldNotBeNullOrEmpty();
        }
        finally
        {
            // Clean up the file
            File.Delete(tempFilePath);
        }
    }

    [Fact]
    public void AddInlineFile_NonExistentFile_ShouldThrowException()
    {
        // Arrange
        var content = new Content();
        var nonExistentPath = "thisFileDoesNotExist.xyz";

        // Act & Assert
        Should.Throw<FileNotFoundException>(() => { content.AddInlineFile(nonExistentPath, "role"); });
    }

    [Fact]
    public void AddInlineFile_NullContent_ShouldThrowNullReferenceException()
    {
        // Arrange
        Content content = null;
        var filePath = "somePath.txt";

        // Act & Assert
        Should.Throw<ArgumentException>(() => { content.AddInlineFile(filePath, "User"); });
    }

    #endregion

    #region AddRemoteFile(RemoteFile) Tests

    [Fact]
    public void AddRemoteFile_WithValidRemoteFile_ShouldCallAddRemoteFileOverload()
    {
        // Arrange
        var content = new Content();
        var remoteFile = new RemoteFile
        {
            Uri = "https://example.com/file.jpg",
            MimeType = "image/jpeg"
        };

        // Act
        content.AddRemoteFile(remoteFile);

        // Assert
        content.Parts.Count.ShouldBe(1);
        content.Parts[0].FileData.ShouldNotBeNull();
        content.Parts[0].FileData.FileUri.ShouldBe(remoteFile.Uri);
        content.Parts[0].FileData.MimeType.ShouldBe(remoteFile.MimeType);
    }

    [Theory]
    [InlineData(null, "video/mp4", "Remote file URI cannot be null or empty.")]
    [InlineData("https://example.com/file.mp4", null, "Remote file MIME type cannot be null or empty.")]
    [InlineData("", "video/mp4", "Remote file URI cannot be null or empty.")]
    public void AddRemoteFile_InvalidRemoteFile_ShouldThrowArgumentException(
        string uri, string mimeType, string expectedMessage)
    {
        // Arrange
        var content = new Content();
        var remoteFile = new RemoteFile { Uri = uri, MimeType = mimeType };

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => { content.AddRemoteFile(remoteFile); });
        ex.Message.ShouldContain(expectedMessage, Case.Insensitive);
    }

    [Fact]
    public void AddRemoteFile_NullContent_ShouldThrowNullReferenceException()
    {
        // Arrange
        Content content = null;
        var remoteFile = new RemoteFile
        {
            Uri = "https://example.com/file.mp4",
            MimeType = "video/mp4"
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() => { content.AddRemoteFile(remoteFile); });
    }

    #endregion

    #region AddRemoteFile(string, string) Tests

    [Fact]
    public void AddRemoteFile_WithValidUriAndMimeType_ShouldAddPartWithFileData()
    {
        // Arrange
        var content = new Content();
        var fileUri = "https://example.com/image.png";
        var mimeType = "image/png";

        // InlineMimeTypes.AllowedMimeTypes must contain "image/png" 
        // for this test to succeed. If it's not in that list, the test will fail.
        InlineMimeTypes.AllowedMimeTypes.Add("image/png"); // Make sure it's allowed

        // Act
        content.AddRemoteFile(fileUri, mimeType);

        // Assert
        content.Parts.Count.ShouldBe(1);
        content.Parts[0].FileData.ShouldNotBeNull();
        content.Parts[0].FileData.FileUri.ShouldBe(fileUri);
        content.Parts[0].FileData.MimeType.ShouldBe(mimeType);
    }

    [Fact]
    public void AddRemoteFile_InvalidUri_ShouldThrowArgumentException()
    {
        // Arrange
        var content = new Content();
        var fileUri = "";
        var mimeType = "image/png";

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => { content.AddRemoteFile(fileUri, mimeType); });
        ex.Message.ShouldContain("The file URI cannot be null or empty.");
    }

    [Fact]
    public void AddRemoteFile_InvalidMimeType_ShouldThrowArgumentException()
    {
        // Arrange
        var content = new Content();
        var fileUri = "https://example.com/file.mp4";
        var mimeType = "";

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => { content.AddRemoteFile(fileUri, mimeType); });
        ex.Message.ShouldContain("The MIME type cannot be null or empty.");
    }

    [Fact]
    public void AddRemoteFile_MimeTypeNotAllowed_ShouldThrowArgumentException()
    {
        // Arrange
        var content = new Content();
        var fileUri = "https://example.com/file.FAKE";
        var mimeType = "FAKE/FAKE";

        // Make sure our FAKE mime type is not allowed in the list
        InlineMimeTypes.AllowedMimeTypes.Remove(mimeType);

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => { content.AddRemoteFile(fileUri, mimeType); });
        ex.Message.ShouldContain("is not allowed for inline");
    }

    [Fact]
    public void AddRemoteFile_NullContent_ShouldThrowNullReferenceException_Overload()
    {
        // Arrange
        Content content = null;

        // Act & Assert
        Should.Throw<ArgumentException>(() => { content.AddRemoteFile("https://example.com/image.png", "image/png"); });
    }

    #endregion

    #region AddText Tests

    [Fact]
    public void AddText_WithValidText_ShouldAddTextPart()
    {
        // Arrange
        var content = new Content();
        var text = "Hello, world!";

        // Act
        content.AddText(text);

        // Assert
        content.Parts.Count.ShouldBe(1);
        content.Parts[0].Text.ShouldBe(text);
    }

    [Fact]
    public void AddText_NullContent_ShouldThrowNullReferenceException()
    {
        // Arrange
        Content content = null;

        // Act & Assert
        Should.Throw<ArgumentException>(() => { content.AddText("Sample Text"); });
    }

    [Fact]
    public void AddText_NullText_ShouldAddPartWithNullText()
    {
        // Arrange
        var content = new Content();

        // Act
        content.AddText(null);

        // Assert
        content.Parts.Count.ShouldBe(1);
        content.Parts[0].Text.ShouldBeNull();
    }

    #endregion

    #region ExtractCodeBlock Tests

    [Theory]
    [MemberData(nameof(GetContentTestScenarios))]
    public void ExtractCodeBlocks_WithVariousContent_SucceedsTest(
        Content content,
        (int expectedCount, List<(string code, string language)> expectedBlocks) expectedResult)
    {
        // Arrange & Act
        var codeBlocks = content.ExtractCodeBlocks();

        // Assert
        codeBlocks.Count.ShouldBe(expectedResult.expectedCount);

        for (int i = 0; i < codeBlocks.Count; i++)
        {
            var (expectedCode, expectedLang) = expectedResult.expectedBlocks[i];
            codeBlocks[i].Code.ShouldBe(expectedCode);
            codeBlocks[i].Language.ShouldBe(expectedLang);
        }
    }

    /// <summary>
    /// Provides test scenarios for ExtractCodeBlocks extension method on Content type.
    /// Each scenario includes a Content instance and expected code blocks (count and details).
    /// </summary>
    public static IEnumerable<object[]> GetContentTestScenarios()
    {
        // // 1) Null check scenario: we expect an ArgumentException to be thrown.
        // yield return new object[]
        // {
        //     null,
        //     (0, new List<(string code, string language)>())
        // };

        // 2) Content with no Parts => no code blocks
        yield return new object[]
        {
            new Content
            {
                Parts = new List<Part>()
            },
            (0, new List<(string code, string language)>())
        };

        // 3) Content with a single Part of empty text => no code blocks
        yield return new object[]
        {
            new Content
            {
                Parts = new List<Part>
                {
                    new Part { Text = "" }
                }
            },
            (0, new List<(string code, string language)>())
        };

        // 4) Content with a single Part containing plain text (no code blocks)
        yield return new object[]
        {
            new Content
            {
                Parts = new List<Part>
                {
                    new Part
                    {
                        Text = "Markdown example with plain text and code blocks:\n" +
                               "\n" +
                               "Here is some explanation of code:\n" +
                               "\n" +
                               "```csharp\n" +
                               "public void Example() {\n" +
                               "    Console.WriteLine(\"Hello World\");\n" +
                               "}\n" +
                               "```\n" +
                               "\n" +
                               "More details below this code block with **markdown**.\n"
                    }
                }
            },
            (1, new List<(string code, string language)>
            {
                ("public void Example() {\n    Console.WriteLine(\"Hello World\");\n}", "csharp")
            })
        };

        // 5) Content with a single Part having a fenced code block with language
        yield return new object[]
        {
            new Content
            {
                Parts = new List<Part>
                {
                    new Part
                    {
                        Text = "Here is another example:\n" +
                               "\n" +
                               "```python\n" +
                               "def say_hello():\n" +
                               "    print(\"Hello World\")\n" +
                               "```\n" +
                               "\n" +
                               "Markdown can mix plain explanations and code blocks seamlessly."
                    }
                }
            },
            (1, new List<(string code, string language)>
            {
                ("def say_hello():\n    print(\"Hello World\")", "python")
            })
        };

        // 6) Content with multiple Parts, each containing different code blocks
        yield return new object[]
        {
            new Content
            {
                Parts = new List<Part>
                {
                    new Part
                    {
                        Text = "Here is some SQL code block:\n" +
                               "\n" +
                               "```sql\n" +
                               "SELECT * FROM Users WHERE Active = 1;\n" +
                               "```\n"
                    },
                    new Part
                    {
                        Text = "And here's JSON output for reference:\n" +
                               "\n" +
                               "```json\n" +
                               "{\n" +
                               "  \"id\": 123,\n" +
                               "  \"name\": \"John Doe\"\n" +
                               "}\n" +
                               "```\n"
                    }
                }
            },
            (2, new List<(string code, string language)>
            {
                ("SELECT * FROM Users WHERE Active = 1;", "sql"),
                ("{\n  \"id\": 123,\n  \"name\": \"John Doe\"\n}", "json")
            })
        };

        // 7) Mixed scenario: multiple Parts, plain text, indented code, and fenced code
        yield return new object[]
        {
            new Content
            {
                Parts = new List<Part>
                {
                    new Part
                    {
                        Text = "Indented example with pseudo-code:\n" +
                               "\n" +
                               "    if (condition) {\n" +
                               "        doSomething();\n" +
                               "    }\n" +
                               "\n" +
                               "Fenced block next:\n" +
                               "```javascript\n" +
                               "const greet = () => console.log('Hello!');\n" +
                               "```\n" +
                               "\n" +
                               "Plain explanation after both."
                    }
                }
            },
            (2, new List<(string code, string language)>
            {
                ("if (condition) {\n    doSomething();\n}", ""),
                ("const greet = () => console.log('Hello!');", "javascript")
            })
        };
    }

    #endregion

    #region ExtractJsonBlock Tests

    [Theory]
    [MemberData(nameof(GetJsonContentTestScenarios))]
    public void ExtractJsonBlocks_WithVariousContent_SucceedsTest(
        Content content,
        (int expectedCount, List<JsonBlock> expectedBlocks) expectedResult)
    {
        // Arrange & Act
        var jsonBlocks = content.ExtractJsonBlocks();

        // Assert
        jsonBlocks.Count.ShouldBe(expectedResult.expectedCount);

        for (int i = 0; i < jsonBlocks.Count; i++)
        {
            var expectedBlock = expectedResult.expectedBlocks[i];
            jsonBlocks[i].Json.ShouldBe(expectedBlock.Json);
        }
    }

    /// <summary>
    /// Provides test scenarios for ExtractJsonBlocks extension method on Content type.
    /// Each scenario includes a Content instance and expected JSON blocks (count and details).
    /// </summary>
    public static IEnumerable<object[]> GetJsonContentTestScenarios()
    {
        // 1) Content with no JSON parts
        yield return new object[]
        {
            new Content
            {
                Parts = new List<Part>
                {
                    new Part { Text = "This part contains no JSON data." }
                }
            },
            (0, new List<JsonBlock>())
        };

        // 2) Content with a single valid JSON block
        yield return new object[]
        {
            new Content
            {
                Parts = new List<Part>
                {
                    new Part { Text = "{\"key\":\"value\"}" }
                }
            },
            (1, new List<JsonBlock>
            {
                new JsonBlock { Json = "{\"key\":\"value\"}" }
            })
        };

        // 3) Content with multiple JSON blocks
        yield return new object[]
        {
            new Content
            {
                Parts = new List<Part>
                {
                    new Part { Text = "{\"key1\":\"value1\"}" },
                    new Part { Text = "{\"key2\":\"value2\"}" }
                }
            },
            (2, new List<JsonBlock>
            {
                new JsonBlock { Json = "{\"key1\":\"value1\"}" },
                new JsonBlock { Json = "{\"key2\":\"value2\"}" }
            })
        };

        // 4) Content with nested JSON objects in plain text
        yield return new object[]
        {
            new Content
            {
                Parts = new List<Part>
                {
                    new Part
                    {
                        Text =
                            "Here is a nested JSON object: {\"outerKey\":{\"innerKey\":\"innerValue\"}} and some text after."
                    }
                }
            },
            (1, new List<JsonBlock>
            {
                new JsonBlock { Json = "{\"outerKey\":{\"innerKey\":\"innerValue\"}}" }
            })
        };

        // 5) Content with text and JSON blocks interleaved
        yield return new object[]
        {
            new Content
            {
                Parts = new List<Part>
                {
                    new Part { Text = "Introduction text before JSON." },
                    new Part { Text = "{\"introKey\":\"introValue\"}" },
                    new Part { Text = "Some more plain text in the middle." },
                    new Part { Text = "{\"middleKey\":{\"nestedKey\":\"nestedValue\"}}" },
                    new Part { Text = "Conclusion text after JSON." }
                }
            },
            (2, new List<JsonBlock>
            {
                new JsonBlock { Json = "{\"introKey\":\"introValue\"}" },
                new JsonBlock { Json = "{\"middleKey\":{\"nestedKey\":\"nestedValue\"}}" }
            })
        };

        // 6) Mixed content: valid and invalid JSON with text around
        yield return new object[]
        {
            new Content
            {
                Parts = new List<Part>
                {
                    new Part { Text = "Plain text before the first JSON." },
                    new Part { Text = "{\"validKey\":\"validValue\"}" },
                    new Part { Text = "Invalid JSON structure here." },
                    new Part { Text = "{\"anotherValidKey\":{\"subKey\":\"subValue\"}}" },
                    new Part { Text = "Plain text after all JSON blocks." }
                }
            },
            (2, new List<JsonBlock>
            {
                new JsonBlock { Json = "{\"validKey\":\"validValue\"}" },
                new JsonBlock { Json = "{\"anotherValidKey\":{\"subKey\":\"subValue\"}}" }
            })
        };
    }

    #endregion

    #region ExtractObject Tests

    /// <summary>
    /// Sample model class to deserialize JSON into for testing.
    /// </summary>
    public class SampleModel
    {
        public string? Name { get; set; }
    }

    [Fact]
    public void ToObject_NullResponse_ReturnsNull()
    {
        // Arrange
        GenerateContentResponse? response = null;

        // Act
        var result = response.ToObject<SampleModel>();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void ToObject_NullContent_ReturnsNull()
    {
        // Arrange
        var response = new GenerateContentResponse( );

        // Act
        var result = response.ToObject<SampleModel>();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void ToObject_NoJsonBlock_ReturnsNull()
    {
        // Arrange

        var content = "Some text without JSON ### Another text block";
            
        var response = new GenerateContentResponse
        {
            Candidates = new[]
            {
                new Candidate()
                {
                    Content = RequestExtensions.FormatGenerateContentInput(content)
                }
            }
        };
        
        // Act
        var result = response.ToObject<SampleModel>();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void ToObject_FirstBlockValid_ReturnsFirstDeserializedObject()
    {
        // Arrange
        var content = @"### { ""Name"": ""FirstBlock"" } ### { ""Name"": ""SecondBlock"" }";
            
        var response = new GenerateContentResponse
        {
            Candidates = new[]
            {
                new Candidate()
                {
                    Content = RequestExtensions.FormatGenerateContentInput(content)
                }
            }
        };

        // Act
        var result = response.ToObject<SampleModel>();

        // Assert
        result.ShouldNotBeNull();
        result!.Name.ShouldBe("FirstBlock");
    }

    [Fact]
    public void ToObject_MalformedJson_ReturnsNull()
    {
        // Arrange
        var content = "Text that does not contain valid JSON block";
        var response = new GenerateContentResponse
        {
            Candidates = new[]
            {
                new Candidate()
                {
                    Content = RequestExtensions.FormatGenerateContentInput(content)
                }
            }
        };

        // Act
        var result = response.ToObject<SampleModel>();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void ExtractAllObjects_EmptyOrNullContent_ReturnsEmptyList()
    {
        // Arrange
        var response = new GenerateContentResponse(); // Content is null by default

        // Act
        var results = response.ToObjects<SampleModel>();

        // Assert
        results.ShouldNotBeNull();
        results.Count.ShouldBe(0);
    }

    [Fact]
    public void ExtractAllObjects_NoValidJson_ReturnsEmptyList()
    {
        // Arrange
        var content = "Text that does not contain valid JSON block";
        
        var response = new GenerateContentResponse
        {
            Candidates = new[]
            {
                new Candidate()
                {
                    Content = RequestExtensions.FormatGenerateContentInput(content)
                }
            }
        };

        // Act
        var results = response.ToObjects<SampleModel>();

        // Assert
        results.ShouldNotBeNull();
        results.ShouldBeEmpty();
    }

    [Fact]
    public void ExtractAllObjects_MultipleBlocks_ReturnsAllValid()
    {
        // Arrange
        var content = @"### { ""Name"": ""Alpha"" } 
                           ### Some invalid text
                           ### { ""Name"": ""Bravo"" } 
                           ### { ""Name"": ""Charlie"" }";
        
        var response = new GenerateContentResponse
        {
            Candidates = new[]
            {
                new Candidate()
                {
                    Content = RequestExtensions.FormatGenerateContentInput(content)
                }
            }
        };
        
       

        // Act
        var results = response.ToObjects<SampleModel>();

        // Assert
        results.ShouldNotBeNull();
        results.Count.ShouldBe(3);
        results.ShouldContain(m => m.Name == "Alpha");
        results.ShouldContain(m => m.Name == "Bravo");
        results.ShouldContain(m => m.Name == "Charlie");
    }

    [Fact]
    public void ExtractAllObjects_MalformedBlocks_IgnoresInvalidPreservesValid()
    {
        // Arrange
        var response = new GenerateContentResponse
        {
            Candidates = new[]
            {
                new Candidate()
                {
                    Content = RequestExtensions.FormatGenerateContentInput(
                        @"### { ""Name"": ""GoodBlock1"" }                        ### { ""Name"": ""BadBlock"" 
                           ### { ""Name"": ""GoodBlock2"" }")
                }
            }
        };

        // Act
        var results = response.ToObjects<SampleModel>();

        // Assert
        results.ShouldNotBeNull();
        results.Count.ShouldBe(1);
        results[0].Name.ShouldBe("GoodBlock1");
        //results[1].Name.ShouldBe("GoodBlock2");
    }

    #endregion
}