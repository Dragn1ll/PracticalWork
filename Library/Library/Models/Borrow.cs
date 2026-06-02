using Library.SharedKernel.Enums;

namespace Library.Models;

/// <summary>
/// Выдача книги
/// </summary>
public sealed class Borrow
{
    /// <summary>Идентификатор книги</summary>
    public Guid BookId { get; set; }
    
    /// <summary>Идентификатор читателя</summary>
    public Guid ReaderId { get; set; }
    
    /// <summary>Дата выдачи</summary>
    public DateOnly BorrowDate { get; set; }
    
    /// <summary>Срок возврата</summary>
    public DateOnly DueDate { get; set; }
    
    /// <summary>Фактическая дата возврата</summary>
    public DateOnly ReturnDate { get; set; }
    
    /// <summary>Статус</summary>
    public BookIssueStatus Status { get; set; }

    public void ReturnBook()
    {
        if (Status != BookIssueStatus.Issued)
        {
            throw new InvalidOperationException("Книга уже возвращена в библиотеку!");
        }

        ReturnDate = DateOnly.FromDateTime(DateTime.UtcNow);
        Status = ReturnDate < DueDate 
            ? BookIssueStatus.Returned 
            : BookIssueStatus.Overdue;
    }
}