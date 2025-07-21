using System.ComponentModel;
using CSharpToJsonSchema;

namespace GenerativeAI.IntegrationTests;

public class MethodTools
{
    public Task<List<GetAuthorBook>> GetAuthorBooksAsync(string authorName,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new List<GetAuthorBook>([
            new GetAuthorBook
                { Title = "Five point someone", Description = "This book is about 3 college friends" },
            new GetAuthorBook
                { Title = "Two States", Description = "This book is about intercast marriage in India" }
        ]));
    }

    public Task<string> GetBookPageContentAsync(string bookName, int bookPageNumber,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult("this is a cool weather out there, and I am stuck at home.");
    }

    public Task<string> GetBookListAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult("Five point someone, Two States");
    }

    [Description("Get list of books")]
    [FunctionTool(GoogleFunctionTool = true)]
    public string GetBookList2()
    {
        return "Five point someone, Two States";
    }
}