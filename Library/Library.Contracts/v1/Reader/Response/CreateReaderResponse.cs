namespace Library.Contracts.v1.Reader.Response;

/// <summary>
/// Ответ на запрос создания карточки читателя
/// </summary>
/// <param name="Id">Идентификатор карточки читателя</param>
public record CreateReaderResponse(Guid Id);