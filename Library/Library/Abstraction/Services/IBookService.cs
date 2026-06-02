using Library.Dto.Input;
using Library.Dto.Output;
using Library.Models;
using Microsoft.AspNetCore.Http;
using PracticalWork.Library.Dto.Input;

namespace Library.Abstraction.Services;

/// <summary>
/// Сервис по работе с книгами
/// </summary>
public interface IBookService
{
    /// <summary>
    /// Создание книги
    /// </summary>
    /// <param name="book">Книга</param>
    /// <returns>Идентификатор созданной книги</returns>
    Task<Guid> CreateBook(Book book);
    
    /// <summary>
    /// Редактирование книги
    /// </summary>
    /// <param name="bookId">Идентификатор книги</param>
    /// <param name="updateBookDto">Обновлённые данные</param>
    Task UpdateBook(Guid bookId, UpdateBookDto updateBookDto);
    
    /// <summary>
    /// Перевод книги в архив
    /// </summary>
    /// <param name="bookId">Идентификатор книги</param>
    /// <returns>Информацию об архивации</returns>
    Task<ArchiveBookDto> ArchiveBook(Guid bookId);
    
    /// <summary>
    /// Получение списка книг
    /// </summary>
    /// <param name="getBookList">Фильтры + пагинация</param>
    /// <returns>Список книг</returns>
    Task<PagedListDto<BookListDto>> GetBooks(GetBookListDto getBookList);

    /// <summary>
    /// Добавление деталей книги
    /// </summary>
    /// <param name="bookId">Идентификатор книги</param>
    /// <param name="coverImage">Изображение обложки книги</param>
    /// <param name="description">Краткое описание книги</param>
    Task CreateBookDetails(Guid bookId, IFormFile coverImage, string description);
}