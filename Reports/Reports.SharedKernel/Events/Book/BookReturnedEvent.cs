namespace Reports.SharedKernel.Events.Book;

/// <summary>
/// Событие возврата книги в библиотеку
/// </summary>
/// <param name="BookId">Уникальный идентификатор книги</param>
/// <param name="ReaderId">Уникальный идентификатор читателя</param>
/// <param name="BookTitle">Название книги</param>
/// <param name="ReaderName">ФИО читателя</param>
/// <param name="ReturnDate">Дата возврата книги</param>
public sealed record BookReturnedEvent(
    Guid BookId,
    Guid ReaderId,
    string BookTitle,
    string ReaderName,
    DateTime ReturnDate
) : BaseLibraryEvent("book.returned");