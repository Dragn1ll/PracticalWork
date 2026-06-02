namespace Library.BackgroundServices.Email.Dto;

/// <summary>
/// Результат выполнения отправки напоминаний пользователям о возврате книг
/// </summary>
public sealed class ReturnReminderResultDto
{
    public int SentCount { get; set; }
    
    public int FailedCount { get; set; }
    
    public int SkippedCount { get; set; }
    
    public TimeSpan ExecutionTime { get; set; }
}