using Reports.Dto;
using Reports.Models;
using Reports.SharedKernel.Enums;

namespace Reports.Abstractions.Services;

/// <summary>
/// Сервис для работы с отчётами
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Получить все логи активности
    /// </summary>
    /// <param name="dateFrom">Дата начала периода активности</param>
    /// <param name="dateTo">Дата конца периода активности</param>
    /// <param name="eventType">Тип события</param>
    /// <param name="page">Номер страницы</param>
    /// <param name="pageSize">Размер страницы</param>
    /// <returns>Список логов активностей</returns>
    Task<IEnumerable<ActivityLog>> GetAllActivityLogs(DateOnly? dateFrom, DateOnly? dateTo, EventType eventType,
        int page, int pageSize);
    
    /// <summary>
    /// Сгенерировать отчёт
    /// </summary>
    /// <param name="name">Название отчёта</param>
    /// <param name="periodFrom">Дата начала периода отчёта</param>
    /// <param name="periodTo">Дата конца периода отчёта</param>
    /// <param name="eventType">Тип события</param>
    /// <returns>Данные о созданном отчёте</returns>
    Task<CreatedReportDto> CreateReport(string name, DateOnly periodFrom, DateOnly periodTo, EventType eventType);
    
    /// <summary>
    /// Получить список сгенерированных отчётов
    /// </summary>
    /// <returns>Список отчётов</returns>
    Task<IEnumerable<Report>> GetGeneratedReports();
    
    /// <summary>
    /// Получить ссылку на файл отчёта
    /// </summary>
    /// <param name="reportName">Название отчёта</param>
    /// <returns>Ссылка на отчёт</returns>
    Task<string> GetReportFileUrl(string reportName);
    
    /// <summary>
    /// Получить статистику по активностям за период
    /// </summary>
    /// <param name="startDate">Дата начала периода</param>
    /// <param name="endDate">Дата конца периода</param>
    /// <returns>Статистика по активностям за период</returns>
    Task<ActivityLogStatisticDto> GetStatisticByPeriod(DateTime startDate, DateTime endDate);
}