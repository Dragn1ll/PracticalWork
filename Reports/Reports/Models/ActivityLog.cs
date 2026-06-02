using Reports.SharedKernel.Enums;

namespace Reports.Models;

/// <summary>
/// Лог активности
/// </summary>
public class ActivityLog
{
    /// <summary>Внешний ключ на Book (опционально)</summary>
    public Guid? ExternalBookId { get; set; }
    
    /// <summary>Внешний ключ на Reader (опционально)</summary>
    public Guid? ExternalReaderId { get; set; }

    /// <summary>Тип события</summary>
    public EventType EventType { get; set; }
    
    /// <summary>Дата события</summary>
    public DateTime EventDate { get; set; }
    
    /// <summary>Дополнительная информация</summary>
    public string Metadata { get; set; } = null!;
}