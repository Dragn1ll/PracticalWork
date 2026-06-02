using Library.SharedKernel.Enums;

namespace Library.Contracts.v1.Books.Request;

/// <summary>
/// Запрос на получение книг по фильтру и пагинации
/// </summary>
/// <param name="Status">Статус книги</param>
/// <param name="Category">Категория книги</param>
/// <param name="Author">Один из авторов книги</param>
/// <param name="Page">Номер страницы</param>
/// <param name="PageSize">Размер страницы</param>
public record GetBookListRequest(
    BookStatus Status,
    BookCategory Category,
    string Author,
    int Page,
    int PageSize
    );