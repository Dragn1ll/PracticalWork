namespace Library.SharedKernel.Events.Book;

/// <summary>
/// Событие создания новой книги в библиотеке
/// </summary>
/// <param name="BookId">Уникальный идентификатор книги</param>
/// <param name="Title">Название книги</param>
/// <param name="Category">Категория книги</param>
/// <param name="Authors">Массив авторов книги</param>
/// <param name="Year">Год издания</param>
/// <param name="CreatedAt">Дата и время создания книги</param>
public sealed record BookCreatedEvent(
    Guid BookId,
    string Title,
    string Category,
    IReadOnlyList<string> Authors,
    int Year,
    DateTime CreatedAt
) : BaseLibraryEvent("book.created");