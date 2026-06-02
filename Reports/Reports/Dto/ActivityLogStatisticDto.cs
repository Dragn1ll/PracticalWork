namespace Reports.Dto;

/// <summary>
/// Статистика записей активности
/// </summary>
public class ActivityLogStatisticDto
{
    /// <summary>Количество новых книг</summary>
    public int NewBooksCount { get; set; }
    
    /// <summary>Количество новых читателей</summary>
    public int NewReadersCount { get; set; }
    
    /// <summary>Количество взятых книг</summary>
    public int BorrowedBooksCount { get; set; }
    
    /// <summary>Количество возвращённых книг</summary>
    public int ReturnedBooksCount { get; set; }
    
    /// <summary>Количество просроченных возвратов книг</summary>
    public int OverdueBooksCount { get; set; }
}