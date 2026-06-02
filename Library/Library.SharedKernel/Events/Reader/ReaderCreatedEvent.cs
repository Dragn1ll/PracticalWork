namespace Library.SharedKernel.Events.Reader;

/// <summary>
/// Событие создания новой карточки читателя
/// </summary>
/// <param name="ReaderId">Уникальный идентификатор читателя</param>
/// <param name="FullName">Полное имя читателя</param>
/// <param name="PhoneNumber">Номер телефона читателя</param>
/// <param name="ExpiryDate">Дата окончания действия карточки</param>
/// <param name="CreatedAt">Дата и время создания карточки</param>
public sealed record ReaderCreatedEvent(
    Guid ReaderId,
    string FullName,
    string PhoneNumber,
    DateTime ExpiryDate,
    DateTime CreatedAt
) : BaseLibraryEvent("reader.created");