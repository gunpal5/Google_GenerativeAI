using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Extensions
{
    public class GenerateContentRequestExtensions_Tests:TestBase
    {
        public GenerateContentRequestExtensions_Tests(ITestOutputHelper helper) : base(helper)
        {
            
        }
        const string TestFile = "image.png";
        [Fact]
        public void Add_WithText_CreateNewContentFalse_ShouldAppendToNewlyCreatedContent_IfNoContentExists()
        {
            // Arrange
            var request = new GenerateContentRequest();
            var text = "Hello world";

            // Act
            request.AddText(text, appendToLastContent: true, role: Roles.User);

            // Assert
            request.Contents.Count.ShouldBe(1);
            request.Contents[0].Parts.Count.ShouldBe(1);
            request.Contents[0].Parts[0].Text.ShouldBe(text);
            request.Contents[0].Role.ShouldBe(Roles.User);
        }

        [Fact]
        public void Add_WithText_CreateNewContentFalse_ShouldAppendToExistingContent_IfContentExists()
        {
            // Arrange
            var request = new GenerateContentRequest();
            request.Contents.Add(new Content(new List<Part> { new Part { Text = "Existing" } }, Roles.System));
            var existingCount = request.Contents[0].Parts.Count;

            var text = "Hello world";

            // Act
            request.AddText(text, appendToLastContent: true, role: Roles.System);

            // Assert
            request.Contents.Count.ShouldBe(1);
            request.Contents[0].Parts.Count.ShouldBe(existingCount + 1);
            request.Contents[0].Parts.Last().Text.ShouldBe(text);
            // The original content role remains as it was (note: the extension method doesn't change existing role).
            request.Contents[0].Role.ShouldBe(Roles.System);
        }

        [Fact]
        public void Add_WithText_CreateNewContentTrue_ShouldCreateNewContent_RegardlessOfExistingContent()
        {
            // Arrange
            var request = new GenerateContentRequest();
            request.Contents.Add(new Content(new List<Part> { new Part { Text = "Existing" } }, Roles.System));

            var text = "New Content";

            // Act
            request.AddText(text, appendToLastContent: false, role: Roles.User);

            // Assert
            request.Contents.Count.ShouldBe(2);
            request.Contents.Last().Parts.Count.ShouldBe(1);
            request.Contents.Last().Parts[0].Text.ShouldBe(text);
            request.Contents.Last().Role.ShouldBe(Roles.User);
        }

        [Fact]
        public void Add_WithSinglePart_AppendToLastContent_SingleContentCreatedIfNoneExists()
        {
            // Arrange
            var request = new GenerateContentRequest();
            var part = new Part { Text = "SinglePart" };

            // Act
            request.AddPart(part, appendToLastContent: true);

            // Assert
            request.Contents.Count.ShouldBe(1);
            request.Contents[0].Parts.Count.ShouldBe(1);
            request.Contents[0].Parts[0].Text.ShouldBe("SinglePart");
            request.Contents[0].Role.ShouldBe(Roles.User);
        }

        [Fact]
        public void Add_WithSinglePart_AppendToLastContent_AppendsToExistingContent()
        {
            // Arrange
            var request = new GenerateContentRequest();
            var existingContent = new Content(new[] { new Part { Text = "Existing" } }, Roles.Model);
            request.Contents.Add(existingContent);
            var part = new Part { Text = "Appended" };

            // Act
            request.AddPart(part, appendToLastContent: true, role: Roles.Model);

            // Assert
            request.Contents.Count.ShouldBe(1);
            request.Contents[0].Parts.Count.ShouldBe(2);
            request.Contents[0].Parts[1].Text.ShouldBe("Appended");
            // Role of existing content does not change
            request.Contents[0].Role.ShouldBe(Roles.Model);
        }

        [Fact]
        public void Add_WithSinglePart_DoNotAppend_CreatesNewContent()
        {
            // Arrange
            var request = new GenerateContentRequest();
            request.Contents.Add(new Content(new List<Part> { new Part { Text = "Existing" } }, Roles.System));
            var part = new Part { Text = "SeparateContent" };

            // Act
            request.AddPart(part, appendToLastContent: false, role: Roles.System);

            // Assert
            request.Contents.Count.ShouldBe(2);
            request.Contents.Last().Parts.Count.ShouldBe(1);
            request.Contents.Last().Parts[0].Text.ShouldBe("SeparateContent");
            request.Contents.Last().Role.ShouldBe(Roles.System);
        }

        [Fact]
        public void Add_WithManyParts_AppendToLastContent_CreatesContentIfNoneExists()
        {
            // Arrange
            var request = new GenerateContentRequest();
            var parts = new List<Part> { new Part { Text = "1" }, new Part { Text = "2" } };

            // Act
            request.AddParts(parts, appendToLastContent: true, role: Roles.System);

            // Assert
            request.Contents.Count.ShouldBe(1);
            request.Contents[0].Parts.Count.ShouldBe(2);
            request.Contents[0].Parts[0].Text.ShouldBe("1");
            request.Contents[0].Parts[1].Text.ShouldBe("2");
            request.Contents[0].Role.ShouldBe(Roles.System);
        }

        [Fact]
        public void Add_WithManyParts_AppendToLastContent_AppendsIfContentExists()
        {
            // Arrange
            var request = new GenerateContentRequest();
            request.Contents.Add(new Content(new List<Part> { new Part { Text = "Existing" } }, Roles.User));
            var parts = new List<Part> { new Part { Text = "One" }, new Part { Text = "Two" } };

            // Act
            request.AddParts(parts, appendToLastContent: true, role: Roles.System);

            // Assert
            request.Contents.Count.ShouldBe(1);
            request.Contents[0].Parts.Count.ShouldBe(3);
            request.Contents[0].Parts[1].Text.ShouldBe("One");
            request.Contents[0].Parts[2].Text.ShouldBe("Two");
            // Existing content role does not get overridden by the method
            request.Contents[0].Role.ShouldBe(Roles.User);
        }

        [Fact]
        public void Add_WithManyParts_DoNotAppend_CreatesNewContent_RegardlessOfExistingContent()
        {
            // Arrange
            var request = new GenerateContentRequest();
            request.Contents.Add(new Content(new List<Part> { new Part { Text = "Existing" } }, Roles.User));
            var parts = new List<Part> { new Part { Text = "One" }, new Part { Text = "Two" } };

            // Act
            request.AddParts(parts, appendToLastContent: false, role: Roles.System);

            // Assert
            request.Contents.Count.ShouldBe(2);
            request.Contents.Last().Parts.Count.ShouldBe(2);
            request.Contents.Last().Parts[0].Text.ShouldBe("One");
            request.Contents.Last().Parts[1].Text.ShouldBe("Two");
            request.Contents.Last().Role.ShouldBe(Roles.System);
        }

        [Fact]
        public void AddInlineFile_NoContentExists_AppendToLastContentTrue_ShouldCreateAndAdd()
        {
            // Arrange
            var request = new GenerateContentRequest();
            var filePath = TestFile;

            // Act
            request.AddInlineFile(filePath, appendToLastContent: true, role: Roles.User);

            // Assert
            request.Contents.Count.ShouldBe(1);
            request.Contents[0].Role.ShouldBe(Roles.User);
            request.Contents[0].Parts.Count.ShouldBe(1);
            request.Contents[0].Parts[0].InlineData.Data.ShouldNotBeNull();
        }

        [Fact]
        public void AddInlineFile_ExistingContent_AppendToLastContentTrue_ShouldAppend()
        {
            // Arrange
            var request = new GenerateContentRequest();
            var existingContent = new Content();
            existingContent.Parts.Add(new Part { Text = "ExistingPart" });
            request.Contents.Add(existingContent);

            var filePath = TestFile;

            // Act
            request.AddInlineFile(filePath, appendToLastContent: true, role: Roles.User);

            // Assert
            request.Contents.Count.ShouldBe(1);
            request.Contents[0].Parts.Count.ShouldBe(2);
            request.Contents[0].Parts[1].InlineData.Data.ShouldNotBeNull();
            
        }

        [Fact]
        public void AddInlineFile_DoNotAppend_ShouldCreateNewContentButNotAddToRequest()
        {
            // Arrange
            var request = new GenerateContentRequest();
            request.Contents.Add(new Content());
            var filePath = TestFile;

            // Act
            request.AddInlineFile(filePath, appendToLastContent: false, role: Roles.Model);

            // Assert
            // Notice the extension method adds the new content to the request if appendToLastContent == false
            request.Contents.Count.ShouldBe(2); 
            request.Contents[0].Parts.Count.ShouldBe(0);
        }

        [Fact]
        public void AddInlineData_NoContentExists_AppendToLastContentTrue_ShouldCreateAndAdd()
        {
            // Arrange
            var request = new GenerateContentRequest();
            var data = "BASE64DATA";
            var mimeType = "image/png";

            // Act
            request.AddInlineData(data, mimeType, appendToLastContent: true, role: Roles.User);

            // Assert
            request.Contents.Count.ShouldBe(1);
            request.Contents[0].Parts.Count.ShouldBe(1);
            request.Contents[0].Parts[0].InlineData.ShouldNotBeNull();
            request.Contents[0].Parts[0].InlineData.Data.ShouldBe(data);
            request.Contents[0].Parts[0].InlineData.MimeType.ShouldBe(mimeType);
        }

        [Fact]
        public void AddInlineData_ContentExists_AppendToLastContentTrue_ShouldAddToExisting()
        {
            // Arrange
            var request = new GenerateContentRequest();
            var existingContent = new Content();
            existingContent.Parts.Add(new Part { Text = "ExistingPart" });
            request.Contents.Add(existingContent);

            var data = "BASE64DATA";
            var mimeType = "image/png";

            // Act
            request.AddInlineData(data, mimeType, appendToLastContent: true, role: Roles.User);

            // Assert
            request.Contents.Count.ShouldBe(1);
            request.Contents[0].Parts.Count.ShouldBe(2);
            request.Contents[0].Parts[1].InlineData.ShouldNotBeNull();
            request.Contents[0].Parts[1].InlineData.Data.ShouldBe(data);
            request.Contents[0].Parts[1].InlineData.MimeType.ShouldBe(mimeType);
        }

        [Fact]
        public void AddInlineData_DoNotAppend_ShouldCreateNewContentButNotAddToRequest()
        {
            // Arrange
            var request = new GenerateContentRequest();
            var data = "BASE64DATA";
            var mimeType = "image/png";
            request.Contents.Add(new Content());

            // Act
            request.AddInlineData(data, mimeType, appendToLastContent: false, role: Roles.System);

            // Assert
            request.Contents.Count.ShouldBe(2);
            request.Contents[0].Parts.Count.ShouldBe(0);
        }

        [Fact]
        public void AddContent_ShouldAddContentToRequest()
        {
            // Arrange
            var request = new GenerateContentRequest();
            var content = new Content(new[] { new Part { Text = "Sample" } }, Roles.User);

            // Act
            request.AddContent(content);

            // Assert
            request.Contents.Count.ShouldBe(1);
            request.Contents[0].Parts.Count.ShouldBe(1);
            request.Contents[0].Parts[0].Text.ShouldBe("Sample");
            request.Contents[0].Role.ShouldBe(Roles.User);
        }

        [Fact]
        public void AddTool_ShouldAddToolToRequest()
        {
            // Arrange
            var request = new GenerateContentRequest();
            var tool = new Tool { GoogleSearch = new GoogleSearchTool()};
            var toolConfig = new ToolConfig { FunctionCallingConfig = new FunctionCallingConfig()
            {
                AllowedFunctionNames = new List<string> { "function1", "function2" },
            } };

            // Act
            request.AddTool(tool, toolConfig);

            // Assert
            request.Tools.ShouldNotBeNull();
            request.Tools.Count.ShouldBe(1);
            request.Tools[0].GoogleSearch.ShouldNotBeNull();
            request.ToolConfig.ShouldBe(toolConfig);
        }

        [Fact]
        public void AddRemoteFile_Object_DoNotAppendIfContentExists_ShouldAddTheNewContentToRequest()
        {
            // Arrange
            var request = new GenerateContentRequest();
            request.Contents.Add(new Content());
            var remoteFile = new RemoteFile { Uri = "https://example.com/file.jpg", MimeType = "image/jpeg" };

            // Act
            request.AddRemoteFile(remoteFile, appendToLastContent: false, role: Roles.Model);

            // Assert
            // The extension adds the new content if appendToLastContent == false
            request.Contents.Count.ShouldBe(2);
            request.Contents[0].Parts.Count.ShouldBe(0);
        }

        [Fact]
        public void AddRemoteFile_Object_AppendToLastContent_CreatesAndAddsIfNoneExists()
        {
            // Arrange
            var request = new GenerateContentRequest();
            var remoteFile = new RemoteFile { Uri = "https://example.com/file.jpg", MimeType = "image/jpeg" };

            // Act
            request.AddRemoteFile(remoteFile, appendToLastContent: true, role: Roles.User);

            // Assert
            request.Contents.Count.ShouldBe(1);
            request.Contents[0].Parts.Count.ShouldBe(1);
            request.Contents[0].Parts[0].FileData.ShouldNotBeNull();
            request.Contents[0].Parts[0].FileData.FileUri.ShouldBe(remoteFile.Uri);
            request.Contents[0].Parts[0].FileData.MimeType.ShouldBe(remoteFile.MimeType);
        }

        [Fact]
        public void AddRemoteFile_UrlMime_DoNotAppendIfContentExists_ShouldNotAddTheNewContentToRequest()
        {
            // Arrange
            var request = new GenerateContentRequest();
            request.Contents.Add(new Content());
            var url = "https://example.com/file.mp3";
            var mimeType = "audio/mpeg";

            // Act
            request.AddRemoteFile(url, mimeType, appendToLastContent: false, role: Roles.Model);

            // Assert
            request.Contents.Count.ShouldBe(2);
            request.Contents[0].Parts.Count.ShouldBe(0);
        }

        [Fact]
        public void AddRemoteFile_UrlMime_AppendToLastContentTrue_NoExistingContent_CreatesOne()
        {
            // Arrange
            var request = new GenerateContentRequest();
            var url = "https://example.com/file.mp3";
            var mimeType = "audio/mpeg";

            // Act
            request.AddRemoteFile(url, mimeType, appendToLastContent: true, role: Roles.User);

            // Assert
            request.Contents.Count.ShouldBe(1);
            request.Contents[0].Role.ShouldBe(Roles.User);
            request.Contents[0].Parts.Count.ShouldBe(1);
            request.Contents[0].Parts[0].FileData.ShouldNotBeNull();
            request.Contents[0].Parts[0].FileData.FileUri.ShouldBe(url);
            request.Contents[0].Parts[0].FileData.MimeType.ShouldBe(mimeType);
        }

        [Fact]
        public void AddRemoteFile_UrlMime_AppendToLastContentTrue_ExistingContent_AppendsToIt()
        {
            // Arrange
            var request = new GenerateContentRequest();
            var existingContent = new Content();
            existingContent.Parts.Add(new Part { Text = "ExistingPart" });
            request.Contents.Add(existingContent);

            var url = "https://example.com/file.mp3";
            var mimeType = "audio/mpeg";

            // Act
            request.AddRemoteFile(url, mimeType, appendToLastContent: true, role: Roles.User);

            // Assert
            request.Contents.Count.ShouldBe(1);
            request.Contents[0].Parts.Count.ShouldBe(2);
            request.Contents[0].Parts[1].FileData.ShouldNotBeNull();
            request.Contents[0].Parts[1].FileData.FileUri.ShouldBe(url);
            request.Contents[0].Parts[1].FileData.MimeType.ShouldBe(mimeType);
        }
    }
}