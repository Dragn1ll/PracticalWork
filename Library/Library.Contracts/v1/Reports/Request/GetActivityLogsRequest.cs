using Library.SharedKernel.Enums;

namespace Library.Contracts.v1.Reports.Request;

/// <summary>
/// Запрос на получение логов активности по фильтру
/// </summary>
/// <param name="DateFrom">Минимальная дата лога</param>
/// <param name="DateTo">Максимальная дата лога</param>
/// <param name="EventType">Тип события</param>
/// <param name="Page">Номер страницы</param>
/// <param name="PageSize">Размер страницы</param>
public record GetActivityLogsRequest(
    DateOnly? DateFrom, 
    DateOnly? DateTo, 
    EventType EventType, 
    int Page, 
    int PageSize
    );