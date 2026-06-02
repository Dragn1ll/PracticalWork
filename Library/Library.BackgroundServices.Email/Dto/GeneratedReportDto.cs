namespace Library.BackgroundServices.Email.Dto;

/// <summary>
/// Результат генерации еженедельного отчета
/// </summary>
public class GeneratedReportDto
{
    public string FileName { get; set; } = null!;
    public string DownloadUrl { get; set; } = null!;
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }
    public int TotalNewBooks { get; set; }
    public int TotalNewReaders { get; set; }
    public int TotalBorrowed { get; set; }
    public int TotalReturned { get; set; }
    public int TotalOverdue { get; set; }
}