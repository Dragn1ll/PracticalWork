using Library.Abstraction.Storage.Entity;
using Library.SharedKernel.Enums;

namespace Library.Data.PostgreSql.Entities;

/// <summary>
/// Выдачи книги на руки читателю
/// </summary>
public sealed class BookBorrowEntity : EntityBase
{
    /// <summary>Идентификатор книги</summary>
    public Guid BookId { get; set; }

    public AbstractBookEntity Book { get; set; } = null!;

    /// <summary>Идентификатор карточки читателя</summary>
    public Guid ReaderId { get; set; }

    public ReaderEntity Reader { get; set; } = null!;

    /// <summary>Дата выдачи книги</summary>
    public DateOnly BorrowDate { get; set; }

    /// <summary>Срок возврата книги</summary>
    public DateOnly DueDate { get; set; }

    /// <summary>Фактическая дата возврата книги</summary>
    public DateOnly ReturnDate { get; set; }

    /// <summary>Статус книги в библиотеке</summary>
    public BookIssueStatus Status { get; set; }
}