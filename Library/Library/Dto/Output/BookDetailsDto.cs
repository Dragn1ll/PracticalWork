using Library.SharedKernel.Enums;

namespace Library.Dto.Output;

/// <summary>
/// Детальная информация о книге
/// </summary>
public class BookDetailsDto
{
    /// <summary>Идентификатор книги</summary>
    public Guid Id { get; }
    
    /// <summary>Название книги</summary>
    public string Title { get; }

    /// <summary>Категория</summary>
    public BookCategory Category { get; }

    /// <summary>Авторы</summary>
    public IReadOnlyList<string> Authors { get; }

    /// <summary>Краткое описание книги</summary>
    public string Description { get; }

    /// <summary>Год издания</summary>
    public int Year { get; }

    /// <summary>Путь к изображению обложки</summary>
    public string CoverImagePath { get; }

    /// <summary>Статус</summary>
    public BookStatus Status { get; }

    /// <summary>В архиве</summary>
    public bool IsArchived { get; }

    public BookDetailsDto(Guid id, string title, IReadOnlyList<string> authors, string description, int year, 
        BookCategory category, BookStatus status, string coverImagePath, bool isArchived)
    {
        Id = id;
        Title = title;
        Authors = authors;
        Description = description;
        Year = year;
        Category = category;
        Status = status;
        CoverImagePath = coverImagePath;
        IsArchived = isArchived;
    }
}