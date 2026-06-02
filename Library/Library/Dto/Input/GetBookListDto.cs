using Library.SharedKernel.Enums;

namespace Library.Dto.Input;

/// <summary>
/// Данные для получения списка книг
/// </summary>
public sealed class GetBookListDto
{
    /// <summary>Статус</summary>
    public BookStatus Status { get; }
    
    /// <summary>Категория</summary>
    public BookCategory Category { get; }

    /// <summary>Один из авторов книги</summary>
    public string Author { get; }

    /// <summary>Номер страницы</summary>
    public int Page { get; }
    
    /// <summary>Размер страницы</summary>
    public int PageSize { get; }

    public GetBookListDto(BookStatus status, BookCategory category, string author, int page, int pageSize)
    {
        Status = status;
        Category = category;
        Author = author;
        Page = page;
        PageSize = pageSize;
    }
}