namespace Library.Dto.Output;

/// <summary>
/// Книга из библиотеки
/// </summary>
public class LibraryBookDto
{
    /// <summary>Название книги</summary>
    public string Title { get; }

    /// <summary>Авторы</summary>
    public IReadOnlyList<string> Authors { get; }

    /// <summary>Краткое описание книги</summary>
    public string Description { get; }
    
    /// <summary>Год издания</summary>
    public int Year { get; }
    
    /// <summary>Идентификатор читателя, у которого находится книга</summary>
    public Guid? ReaderId { get; }
    
    /// <summary>Дата выдачи</summary>
    public DateOnly? BorrowDate { get; }
    
    /// <summary>Срок возврата</summary>
    public DateOnly? DueDate { get; }

    public LibraryBookDto(string title, IReadOnlyList<string> authors, string description, int year, Guid? readerId = null, 
        DateOnly? borrowDate = null, DateOnly? dueDate = null)
    {
        Title = title;
        Authors = authors;
        Description = description;
        Year = year;
        ReaderId = readerId;
        BorrowDate = borrowDate;
        DueDate = dueDate;
    }
}