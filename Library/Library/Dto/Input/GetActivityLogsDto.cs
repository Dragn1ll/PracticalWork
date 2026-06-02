using Library.SharedKernel.Enums;

namespace Library.Dto.Input;

/// <summary>
/// Запрос на получение логов активности по фильтру
/// </summary>
public class GetActivityLogsDto
{
    /// <summary>Минимальная дата лога</summary>
    public DateOnly? DateFrom { get; init; }

    /// <summary>Максимальная дата лога</summary>
    public DateOnly? DateTo { get; init; }

    /// <summary>Тип события</summary>
    public EventType? EventType { get; init; }

    /// <summary>Номер страницы</summary>
    public int Page { get; init; } = 1;

    /// <summary>Размер страницы</summary>
    public int PageSize { get; init; } = 10;
}