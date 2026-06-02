using Library.SharedKernel.Enums;

namespace Library.Contracts.v1.Library.Requests;

/// <summary>
/// Запрос на получение книг библиотеки по фильтру
/// </summary>
/// <param name="Category">Категория книг</param>
/// <param name="Author">Один из авторов книг</param>
/// <param name="AvailableOnly">Только доступные к выдаче</param>
/// <param name="Page">Номер страницы</param>
/// <param name="PageSize">Размер страницы</param>
public record GetLibraryBooksRequest(
    BookCategory Category, 
    string Author, 
    bool AvailableOnly, 
    int Page, 
    int PageSize
    );