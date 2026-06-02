namespace Library.SharedKernel.Events.Book;

/// <summary>
/// Событие выдачи книги читателю
/// </summary>
/// <param name="BookId">Уникальный идентификатор книги</param>
/// <param name="ReaderId">Уникальный идентификатор читателя</param>
/// <param name="BookTitle">Название книги</param>
/// <param name="ReaderName">ФИО читателя</param>
/// <param name="BorrowDate">Дата выдачи книги</param>
/// <param name="DueDate">Срок возврата книги</param>
public sealed record BookBorrowedEvent(
    Guid BookId,
    Guid ReaderId,
    string BookTitle,
    string ReaderName,
    DateTime BorrowDate,
    DateTime DueDate
) : BaseLibraryEvent("book.borrowed");