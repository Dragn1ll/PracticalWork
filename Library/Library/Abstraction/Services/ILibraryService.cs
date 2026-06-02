using Library.Dto.Input;
using Library.Dto.Output;
using PracticalWork.Library.Dto.Input;

namespace Library.Abstraction.Services;

/// <summary>
/// Сервис по работе библиотеки
/// </summary>
public interface ILibraryService
{
    /// <summary>
    /// Выдача книги
    /// </summary>
    /// <param name="bookId">Идентификатор книги</param>
    /// <param name="readerId">Идентификатор пользователя</param>
    /// <returns>Идентификатор выдачи книги</returns>
    Task<Guid> BorrowBook(Guid bookId, Guid readerId);
    
    /// <summary>
    /// Получение списка книг библиотеки
    /// </summary>
    /// <param name="getLibraryBooksDto">Фильтры + пагинация</param>
    /// <returns>Список книг</returns>
    Task<PagedListDto<LibraryBookDto>> GetLibraryBooks(GetLibraryBooksDto getLibraryBooksDto);
    
    /// <summary>
    /// Возврат книги
    /// </summary>
    /// <param name="bookId">Идентификатор книги</param>
    Task ReturnBook(Guid bookId);
    
    /// <summary>
    /// Получение детальной информации о книге по идентификатору
    /// </summary>
    /// <param name="bookId">Идентификатор книги</param>
    /// <returns>Детали книги</returns>
    Task<BookDetailsDto> GetBookDetailsById(Guid bookId);
    
    /// <summary>
    /// Получение детальной информации о книге по названию
    /// </summary>
    /// <param name="title">Название книги</param>
    /// <returns>Детали книги</returns>
    Task<BookDetailsDto> GetBookDetailsByTitle(string title);
}