using Library.Contracts.v1.Abstracts;

namespace Library.Contracts.v1.Books.Request;

/// <summary>
/// Запрос на обновление книги
/// </summary>
public sealed record UpdateBookRequest(string Title, IReadOnlyList<string> Authors, int Year)
    : AbstractBook(Title, Authors, Year);