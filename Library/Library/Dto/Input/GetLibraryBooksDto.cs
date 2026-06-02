using Library.SharedKernel.Enums;

namespace Library.Dto.Input;

/// <summary>
/// Данные для получения книг библиотеки
/// </summary>
public class GetLibraryBooksDto
{
    /// <summary>Категория</summary>
    public BookCategory Category { get; }

    /// <summary>Один из авторов книги</summary>
    public string Author { get; }

    /// <summary>Только доступные к выдаче</summary>
    public bool AvailableOnly { get; }

    /// <summary>Номер страницы</summary>
    public int Page { get; }
    
    /// <summary>Размер страницы</summary>
    public int PageSize { get; }

    public GetLibraryBooksDto(BookCategory category, string author, bool availableOnly, int page, int pageSize)
    {
        Category = category;
        Author = author;
        AvailableOnly = availableOnly;
        Page = page;
        PageSize = pageSize;
    }
}