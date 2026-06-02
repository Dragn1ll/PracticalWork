using Library.Dto.Output;
using Library.Models;

namespace Library.Abstraction.Storage.Repositories;

/// <summary>
/// Репозиторий для Читателей
/// </summary>
public interface IReaderRepository
{
    /// <summary>
    /// Создать карточку читателя
    /// </summary>
    /// <param name="reader">Читатель</param>
    /// <returns>Идентификатор читателя</returns>
    Task<Guid> CreateReader(Reader reader);
    
    /// <summary>
    /// Получить карточку читателя по идентификатору
    /// </summary>
    /// <param name="readerId">Идентификатор читателя</param>
    /// <returns>Карточка читателя</returns>
    Task<Reader> GetReaderById(Guid readerId);
    
    /// <summary>
    /// Получить взятые читателем книги
    /// </summary>
    /// <param name="readerId">Идентификатор читателя</param>
    /// <returns>Список взятых читателем книг</returns>
    Task<IList<BorrowedBookDto>> GetBorrowedBooks(Guid readerId);
    
    /// <summary>
    /// Обновить данные читателя
    /// </summary>
    /// <param name="readerId">Идентификатор читателя</param>
    /// <param name="reader">Карточка с обновлёнными данными</param>
    Task UpdateReader(Guid readerId,Reader reader);
}