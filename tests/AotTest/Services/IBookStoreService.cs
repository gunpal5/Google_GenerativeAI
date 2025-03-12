using System.ComponentModel;
using CSharpToJsonSchema;

namespace AotTest;

[GenerateJsonSchema]
public interface IBookStoreService
{
    [Description("Get books written by some author")]
    public Task<List<GetAuthorBook>> GetAuthorBooksAsync2([Description("Author name")] string authorName, CancellationToken cancellationToken = default);

    [Description("Get book page content")]
    public Task<string> GetBookPageContentAsync2([Description("Book Name")] string bookName, [Description("Book Page Number")] int bookPageNumber, CancellationToken cancellationToken = default);

}