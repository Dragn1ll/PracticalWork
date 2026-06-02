namespace Reports.SharedKernel.Events.Report;

/// <summary>
/// Событие на просьбу создания отчёта
/// </summary>
/// <param name="ReportId">Идентификатор отчёта</param>
/// <param name="PeriodFrom">Начало периода отчета</param>
/// <param name="PeriodTo">Конец периода отчета</param>
/// <param name="EventTypeId">Идентификатор типа событий</param>
public record CreateReportEvent(
    Guid ReportId,
    DateOnly PeriodFrom, 
    DateOnly PeriodTo, 
    int EventTypeId)
    : BaseEvent(Guid.NewGuid(), "report.create", "reports-service");