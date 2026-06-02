namespace Reports.SharedKernel.Enums;

/// <summary>
/// Тип события
/// </summary>
public enum EventType
{
    Default = 0,
    
    /// <summary>Книга создана</summary>
    BookCreated = 1,
    
    /// <summary>Книга заархивирована</summary>
    BookArchived = 2,
    
    /// <summary>Книга выдана</summary>
    BookBorrowed = 3,
    
    /// <summary>Книга возвращена</summary>
    BookReturned = 4,
    
    /// <summary>Карта читателя создана</summary>
    ReaderCreated = 5,
    
    /// <summary>Карта читателя закрыта</summary>
    ReaderClosed = 6
}