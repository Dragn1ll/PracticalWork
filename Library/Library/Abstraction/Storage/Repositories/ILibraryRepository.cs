using Library.Dto.Output;
using Library.Models;
using Library.SharedKernel.Enums;

namespace Library.Abstraction.Storage.Repositories;

/// <summary>
/// Репозиторий для библиотеки
/// </summary>
public interface ILibraryRepository
{
    /// <summary>
    /// Создать запись о выдаче книги
    /// </summary>
    /// <param name="borrow">Запись о выдаче книги</param>
    /// <returns>Идентификатор выдачи</returns>
    Task<Guid> CreateBorrow(Borrow borrow);
    
    /// <summary>
    /// Получить последнюю запись о выдаче по идентификатору книги 
    /// </summary>
    /// <param name="bookId">Идентификатор книги</param>
    /// <returns>Запись о выдаче</returns>
    Task<Borrow> GetBorrowByBookId(Guid bookId);

    /// <summary>
    /// Получить книгу по идентификатору
    /// </summary>
    /// <param name="bookId">Идентификатор книги</param>
    /// <returns>Книга</returns>
    Task<Book> GetBookById(Guid bookId);
    
    /// <summary>
    /// Получить список книг библиотеки
    /// </summary>
    /// <param name="category">Категория книги</param>
    /// <param name="author">Актор книги</param>
    /// <param name="availableOnly">Доступна ли книга выдаче</param>
    /// <param name="page">Номер страницы</param>
    /// <param name="pageSize">Размер страницы</param>
    /// <returns>Список книг</returns>
    Task<PagedListDto<LibraryBookDto>> GetLibraryBooks(BookCategory category, string author, bool availableOnly, int page, 
        int pageSize);
    
    /// <summary>
    /// Получить идентификатор книги по названию
    /// </summary>
    /// <param name="title">Название книги</param>
    /// <returns>Идентификатор книги</returns>
    Task<Guid> GetBookIdByTitle(string title);
    
    /// <summary>
    /// Обновить данные последней выдачи
    /// </summary>
    /// <param name="borrow">Обновлённая запись о выдаче книги</param>
    /// <returns></returns>
    Task UpdateBorrow(Borrow borrow);
    
    Task<IList<BorrowedIssuedBookInfoDto>> GetBorrowedIssuedBooksInfo(DateOnly targetDueDate);
}