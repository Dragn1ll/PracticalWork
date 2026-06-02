using Reports.SharedKernel.Enums;

namespace Reports.Abstractions.Services;

/// <summary>
/// Сервис для генерации отчётов
/// </summary>
public interface IReportGenService
{
    /// <summary>
    /// Генерация отчёта в формате csv
    /// </summary>
    /// <param name="reportId">Идентификатор отчёта</param>
    /// <param name="periodFrom">Начало периода отчёта</param>
    /// <param name="periodTo">Конец периода отчёта</param>
    /// <param name="eventType">Тип события</param>
    Task GenerateReport(Guid reportId, DateOnly periodFrom, DateOnly periodTo, EventType eventType);
}