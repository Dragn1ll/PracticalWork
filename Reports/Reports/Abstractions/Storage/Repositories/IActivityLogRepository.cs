using Reports.Dto;
using Reports.Models;
using Reports.SharedKernel.Enums;

namespace Reports.Abstractions.Storage.Repositories;

/// <summary>
/// Репозитория для логов активности
/// </summary>
public interface IActivityLogRepository
{
    /// <summary>
    /// Добавить лог активности
    /// </summary>
    /// <param name="activityLog">Лог активности</param>
    Task AddActivityLog(ActivityLog activityLog);

    /// <summary>
    /// Получение логов активности 
    /// </summary>
    /// <param name="startDate">Начальная дата логов активности</param>
    /// <param name="endDate">Конечная дата логов активности</param>
    /// <param name="eventType">Тип события</param>
    /// <param name="page">Номер страницы</param>
    /// <param name="pageSize">Размер страницы</param>
    Task<IEnumerable<ActivityLog>> GetAllActivityLogs(DateOnly? startDate, DateOnly? endDate, EventType eventType,
        int page = 1, int pageSize = 20);

    /// <summary>
    /// Получить статистику по активностям за период
    /// </summary>
    /// <param name="startDate">Дата начала периода</param>
    /// <param name="endDate">Дата конца периода</param>
    /// <returns>Статистика по активностям за период</returns>
    Task<ActivityLogStatisticDto> GetStatisticByPeriod(DateTime startDate, DateTime endDate);
}