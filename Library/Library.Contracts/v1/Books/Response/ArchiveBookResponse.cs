namespace Library.Contracts.v1.Books.Response;

/// <summary>
/// Ответ на перевод книги в архив
/// </summary>
/// <param name="Id">Идентификатор книги</param>
/// <param name="Title">Название книги</param>
/// <param name="ArchivedAt">Дата перевода в архив</param>
public sealed record ArchiveBookResponse(
    Guid Id,
    string Title,
    DateTime ArchivedAt
);