using Library.Contracts.v1.Abstracts;
using Library.SharedKernel.Enums;

namespace Library.Contracts.v1.Library.Response;

/// <summary>
/// Ответ с детальной информацией по книге
/// </summary>
/// <param name="Id">Идентификатор книги</param>
/// <param name="Title">Название книги</param>
/// <param name="Category">Категория книги</param>
/// <param name="Authors">Авторы</param>
/// <param name="Description">Краткое описание книги</param>
/// <param name="Year">Год издания</param>
/// <param name="CoverImagePath">Путь к изображению обложки</param>
/// <param name="Status">Статус</param>
/// <param name="IsArchived">В архиве</param>
public sealed record BookDetailsResponse(
    Guid Id,
    string Title,
    BookCategory Category,
    IReadOnlyList<string> Authors,
    string Description,
    int Year,
    string CoverImagePath,
    BookStatus Status,
    bool IsArchived
) : AbstractBook(Title, Authors, Year);