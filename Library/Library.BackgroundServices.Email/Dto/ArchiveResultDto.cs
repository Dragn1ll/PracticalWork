namespace Library.BackgroundServices.Email.Dto;

/// <summary>
/// Результат выполнения архивации книг
/// </summary>
public class ArchiveResultDto
{
    public int TotalProcessed { get; set; }
    
    public int ArchivedCount { get; set; }
    
    public int SkippedCount { get; set; }
    
    public string SkipReasons { get; set; } = string.Empty;
    
    public TimeSpan ExecutionTime { get; set; }
}