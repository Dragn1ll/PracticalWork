namespace Library.SharedKernel.Enums;

/// <summary>
/// Статус отчёта
/// </summary>
public enum ReportStatus
{
    /// <summary>Отчёт в процессе создания</summary>
    InProgress = 0,
    
    /// <summary>Отчёт сгенерирован</summary>
    Generated = 1,
    
    /// <summary>Произошла ошибка во время генерации отчёта</summary>
    Error = 2
}