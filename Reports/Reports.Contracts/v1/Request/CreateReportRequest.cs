using Reports.SharedKernel.Enums;

namespace Reports.Contracts.v1.Request;

/// <summary>
/// Запрос на создание отчёта
/// </summary>
/// <param name="Name">Название отчёта</param>
/// <param name="PeriodFrom">Дата начала отчёта</param>
/// <param name="PeriodTo">Дата окончания отчёта</param>
/// <param name="EventType">Тип событий в отчёте</param>
public record CreateReportRequest(
    string Name, 
    DateOnly PeriodFrom, 
    DateOnly PeriodTo, 
    EventType EventType
    );