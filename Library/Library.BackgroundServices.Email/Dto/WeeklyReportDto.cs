namespace Library.BackgroundServices.Email.Dto;

/// <summary>
/// Модель данных для шаблона еженедельного отчета
/// </summary>
public class WeeklyReportDto
{
    public string PeriodStart { get; set; } = null!;
    public string PeriodEnd { get; set; } = null!;
    public int NewBooksCount { get; set; }
    public int NewReadersCount { get; set; }
    public int BorrowedBooksCount { get; set; }
    public int ReturnedBooksCount { get; set; }
    public int OverdueCount { get; set; }
    public string ReportDownloadUrl { get; set; } = null!;
    public string GeneratedAt { get; set; } = null!;
    public string LibraryName { get; set; } = null!;
}