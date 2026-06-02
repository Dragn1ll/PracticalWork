namespace Library.Contracts.v1.Books.Response;

/// <summary>
/// Ответ на запрос получения списка данных с пагинации
/// </summary>
/// <param name="Items">Список данных</param>
/// <param name="Page">Номер страницы</param>
/// <param name="PageSize">Размер страницы</param>
/// <param name="TotalCount">Общее количество данных</param>
/// <param name="TotalPages">Общее количество страниц</param>
/// <typeparam name="T">Данные для отправки</typeparam>
public record PagedListResponse<T>(
    IList<T> Items, 
    int Page, 
    int PageSize, 
    int TotalCount, 
    int TotalPages
    );