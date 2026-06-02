namespace Library.Contracts.v1.Library.Requests;

/// <summary>
/// Запрос на выдачу книги
/// </summary>
/// <param name="BookId">Идентификатор книги</param>
/// <param name="ReaderId">Идентификатор читателя</param>
public record BorrowBookRequest(Guid BookId, Guid ReaderId);