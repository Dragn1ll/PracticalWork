namespace Library.Dto.Output;

/// <summary>
/// Данные о выданной книге
/// </summary>
public sealed class BorrowedBookDto
{
    /// <summary>Идентификатор книги</summary>
    public Guid BookId { get; }
    
    /// <summary>Дата выдачи</summary>
    public DateOnly BorrowDate { get; }
    
    /// <summary>Срок возврата книги</summary>
    public DateOnly DueDate { get; }

    public BorrowedBookDto(Guid bookId, DateOnly borrowDate, DateOnly dueDate)
    {
        BookId = bookId;
        BorrowDate = borrowDate;
        DueDate = dueDate;
    }
}