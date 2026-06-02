namespace Library.Dto.Output;

/// <summary>
/// Данные о книге из списка книг
/// </summary>
public sealed class BookListDto
{
    /// <summary>Идентификатор книги</summary>
    public Guid Id { get; }
    
    /// <summary>Название книги</summary>
    public string Title { get; }

    /// <summary>Авторы</summary>
    public IReadOnlyList<string> Authors { get; }

    /// <summary>Краткое описание книги</summary>
    public string Description { get; }

    /// <summary>Год издания</summary>
    public int Year { get; }
    
    /// <summary>Путь к изображению обложки</summary>
    public string CoverImagePath { get; set; }

    public BookListDto(Guid id, string title, IReadOnlyList<string> authors, string description, int year,
        string coverImagePath)
    {
        Id = id;
        Title = title;
        Authors = authors;
        Description = description;
        Year = year;
        CoverImagePath = coverImagePath;
    }
}