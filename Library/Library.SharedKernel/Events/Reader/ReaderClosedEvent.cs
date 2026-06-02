namespace Library.SharedKernel.Events.Reader;

/// <summary>
/// Событие закрытия карточки читателя
/// </summary>
/// <param name="ReaderId">Уникальный идентификатор читателя</param>
/// <param name="FullName">Полное имя читателя</param>
/// <param name="ClosedAt">Дата и время закрытия карточки</param>
/// <param name="Reason">Причина закрытия карточки</param>
public sealed record ReaderClosedEvent(
    Guid ReaderId,
    string FullName,
    DateTime ClosedAt,
    string Reason
) : BaseLibraryEvent("reader.closed");