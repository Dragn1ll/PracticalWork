namespace Library.Contracts.v1.Library.Requests;

/// <summary>
/// Запрос на возврат книги
/// </summary>
/// <param name="BookId">Идентификатор книги</param>
public record ReturnBookRequest(Guid BookId);