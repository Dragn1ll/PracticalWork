using Library.Dto.Output;
using Library.Models;

namespace Library.Abstraction.Services;

/// <summary>
/// Сервис по работе с читателями
/// </summary>
public interface IReaderService
{
    /// <summary>
    /// Создание карточки
    /// </summary>
    /// <param name="reader">Читатель</param>
    /// <returns>Идентификатор читателя</returns>
    Task<Guid> CreateReader(Reader reader);

    /// <summary>
    /// Продление срока действия
    /// </summary>
    /// <param name="readerId">Идентификатор читателя</param>
    /// <param name="newExpiryDate">Новая дата окончания действия карточки</param>
    Task ExtendValidity(Guid readerId, DateOnly newExpiryDate);

    /// <summary>
    /// Закрытие карточки
    /// </summary>
    /// <param name="readerId">Идентификатор читателя</param>
    Task CloseReader(Guid readerId);

    /// <summary>
    /// Получение взятых книг
    /// </summary>
    /// <param name="readerId">Идентификатор читателя</param>
    /// <returns>Список взятых книг</returns>
    Task<IList<BorrowedBookDto>> GetBorrowedBooks(Guid readerId);
}