using Library.Contracts.v1.Abstracts;

namespace Library.Contracts.v1.Books.Response;

/// <summary>
/// Информация о книгах по фильтрам
/// </summary>
/// <param name="Id">Идентификатор книги</param>
/// <param name="Title">Название книги</param>
/// <param name="Authors">Авторы книги</param>
/// <param name="Description">Описание книги</param>
/// <param name="Year">Год выпуска книги</param>
/// <param name="CoverImagePath">Ссылка на изображение обложки</param>
public record BookListResponse(
    Guid Id, 
    string Title, 
    IReadOnlyList<string> Authors, 
    string Description, 
    int Year, 
    string CoverImagePath
    ) : AbstractBook(Title, Authors, Year);