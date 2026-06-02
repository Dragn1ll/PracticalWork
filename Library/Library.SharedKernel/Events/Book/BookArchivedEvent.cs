using PracticalWork.Library.MessageBroker.Events;

namespace Library.SharedKernel.Events.Book;

/// <summary>
/// Событие архивации книги в библиотеке
/// </summary>
/// <param name="BookId">Уникальный идентификатор книги</param>
/// <param name="Title">Название книги</param>
/// <param name="Reason">Причина архивации</param>
/// <param name="ArchivedAt">Дата и время архивации</param>
public sealed record BookArchivedEvent(
    Guid BookId,
    string Title,
    string Reason,
    DateTime ArchivedAt
) : BaseLibraryEvent("book.archived");