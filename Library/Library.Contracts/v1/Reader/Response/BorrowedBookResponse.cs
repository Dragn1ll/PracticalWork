namespace Library.Contracts.v1.Reader.Response;

/// <summary>
/// Ответ на запрос книга, которую взял читатель
/// </summary>
/// <param name="BookId"></param>
/// <param name="BorrowDate"></param>
/// <param name="DueDate"></param>
public record BorrowedBookResponse(
    Guid BookId, 
    DateOnly BorrowDate, 
    DateOnly DueDate
    );