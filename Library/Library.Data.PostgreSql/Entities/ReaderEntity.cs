using Library.Abstraction.Storage.Entity;

namespace Library.Data.PostgreSql.Entities;

/// <summary>
/// Карточка читателя
/// </summary>
public sealed class ReaderEntity : EntityBase
{
    /// <summary>ФИО</summary>
    /// <remarks>Запись идет через пробел</remarks>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Номер телефона</summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>Дата окончания действия карточки</summary>
    public DateOnly ExpiryDate { get; set; }

    /// <summary>Активность карточки</summary>
    public bool IsActive { get; set; }
    
    /// <summary>Электронная почта читателя</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Записи о взятых книгах</summary>
    public ICollection<BookBorrowEntity> BorrowedRecords { get; set; } = [];
}