namespace Library.Contracts.v1.Library.Response;

/// <summary>
/// Ответ на запрос выдачи книги
/// </summary>
/// <param name="Id">Идентификатор выдачи</param>
public record BorrowBookResponse(Guid Id);