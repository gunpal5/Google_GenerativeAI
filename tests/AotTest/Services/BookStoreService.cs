namespace AotTest;

public class BookStoreService : IBookStoreService
{
    public Task<List<GetAuthorBook>> GetAuthorBooksAsync2(string authorName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new List<GetAuthorBook>([
            new GetAuthorBook
                { Title = "Five point someone", Description = "This book is about 3 college friends" },
            new GetAuthorBook
                { Title = "Two States", Description = "This book is about intercast marriage in India" }
        ]));
    }

    public Task<string> GetBookPageContentAsync2(string bookName, int bookPageNumber, CancellationToken cancellationToken = default)
    {
        return Task.FromResult("this is a cool weather out there, and I am stuck at home.");
    }
}