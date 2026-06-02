using Library.Dto.Output;
using Library.Models;
using Library.SharedKernel.Enums;

namespace Library.Abstraction.Storage.Repositories;

/// <summary>
/// Репозиторий для Книг
/// </summary>
public interface IBookRepository
{
    /// <summary>
    /// Создать книгу
    /// </summary>
    /// <param name="book">Книга</param>
    /// <returns>Идентификатор созданной книги</returns>
    Task<Guid> CreateBook(Book book);

    /// <summary>
    /// Получить книгу по идентификатору
    /// </summary>
    /// <param name="bookId">Идентификатор книги</param>
    /// <returns>Книга</returns>
    Task<Book> GetBookById(Guid bookId);

    /// <summary>
    /// Получить список книг
    /// </summary>
    /// <param name="status">Статус книги</param>
    /// <param name="category">Категория книги</param>
    /// <param name="author">Автор книги</param>
    /// <param name="page">Номер страницы</param>
    /// <param name="pageSize">Размер страниц</param>
    /// <returns>Список книг</returns>
    Task<PagedListDto<BookListDto>> GetBooks(BookStatus status, BookCategory category, string author, int page, int pageSize);
    
    /// <summary>
    /// Обновить данные книги
    /// </summary>
    /// <param name="bookId">Идентификатор книги</param>
    /// <param name="book">Книга</param>
    Task UpdateBook(Guid bookId, Book book);

    /// <summary>
    /// Получить список доступных книг до даты крайнего срока выдачи
    /// </summary>
    /// <param name="cutoffDate">Крайник срок выдачи книги</param>
    /// <param name="page">Номер страницы</param>
    /// <param name="pageSize">Размер страницы</param>
    /// <returns>Список доступных книг по фильтрам</returns>
    Task<IList<AvailableOldBookDto>> GetAvailableOldBooks(DateOnly cutoffDate, int page, int pageSize);
}