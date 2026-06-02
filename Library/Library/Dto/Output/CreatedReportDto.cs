using Library.SharedKernel.Enums;

namespace Library.Dto.Output;

/// <summary>
/// Данные созданного отчёта
/// </summary>
public class CreatedReportDto
{
    /// <summary>Идентификатор созданного отчёта</summary>
    public Guid Id { get; set; }
    
    /// <summary>Название отчета</summary>
    public string Name { get; set; } = null!;

    /// <summary>Начало периода отчета</summary>
    public DateOnly PeriodFrom { get; set; }
    
    /// <summary>Конец периода отчета</summary>
    public DateOnly PeriodTo { get; set; }

    /// <summary>Статус</summary>
    public ReportStatus Status { get; set; }
}