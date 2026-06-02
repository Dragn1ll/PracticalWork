using Library.Contracts.v1.Abstracts;

namespace Library.Contracts.v1.Library.Response;

/// <summary>
/// Ответ на запрос получение книг библиотеки
/// </summary>
/// <param name="Title">Название книги</param>
/// <param name="Authors">Авторы книги</param>
/// <param name="Description">Описание книги</param>
/// <param name="Year">Год выпуска</param>
/// <param name="ReaderId">Идентификатор читателя кому выдана книга</param>
/// <param name="BorrowDate">Дата выдачи</param>
/// <param name="DueDate">Срок возврата книги</param>
public record LibraryBookResponse(
    string Title, 
    IReadOnlyList<string> Authors, 
    string Description, 
    int Year,
    Guid? ReaderId,
    DateOnly? BorrowDate,
    DateOnly? DueDate
    ) : AbstractBook(Title, Authors, Year);