using Reports.Abstractions.Storage.Entity;
using Reports.SharedKernel.Enums;

namespace Reports.Data.PostgreSql.Entities;

/// <summary>
/// Отчет
/// </summary>
public class ReportEntity : EntityBase
{
    /// <summary>Название отчета</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Путь к файлу в MinIO</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>Дата генерации</summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>Начало периода отчета</summary>
    public DateOnly PeriodFrom { get; set; }
    
    /// <summary>Конец периода отчета</summary>
    public DateOnly PeriodTo { get; set; }

    /// <summary>Статус</summary>
    public ReportStatus Status { get; set; }
}