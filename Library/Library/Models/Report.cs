using Library.SharedKernel.Enums;

namespace Library.Models;

/// <summary>
/// Отчет
/// </summary>
public class Report
{
    /// <summary>Название отчета</summary>
    public string Name { get; set; } = null!;

    /// <summary>Путь к файлу в MinIO</summary>
    public string FilePath { get; set; } = null!;

    /// <summary>Дата генерации</summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>Начало периода отчета</summary>
    public DateOnly PeriodFrom { get; set; }
    
    /// <summary>Конец периода отчета</summary>
    public DateOnly PeriodTo { get; set; }

    /// <summary>Статус</summary>
    public ReportStatus Status { get; set; }

    /// <summary>
    /// Пометить отчёт как сгенерированный
    /// </summary>
    /// <param name="filePath">Путь к файлу в хранилище</param>
    public void MarkAsGenerated(string filePath)
    {
        FilePath = filePath;
        GeneratedAt = DateTime.UtcNow;
        Status = ReportStatus.Generated;
    }
}