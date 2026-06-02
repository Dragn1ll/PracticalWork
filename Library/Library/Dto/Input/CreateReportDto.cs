using Library.SharedKernel.Enums;

namespace Library.Dto.Input;

/// <summary>
/// Данные для создания отчёта
/// </summary>
public class CreateReportDto
{
    /// <summary>Название отчёта</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Дата начала периода отчёта</summary>
    public DateOnly PeriodFrom { get; init; }

    /// <summary>Дата окончания периода отчёта</summary>
    public DateOnly PeriodTo { get; init; }

    /// <summary>Тип событий в отчёте</summary>
    public EventType EventType { get; init; }
}