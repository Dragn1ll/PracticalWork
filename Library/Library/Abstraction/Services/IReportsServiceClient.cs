using Library.BackgroundServices.Email.Dto;
using Library.Dto.Input;
using Library.Dto.Output;
using Library.Models;
using Library.SharedKernel.Enums;

namespace Library.Abstraction.Services;

/// <summary>
/// Клиент для отправки запросов на ReportsService
/// </summary>
public interface IReportsServiceClient
{
    /// <summary>
    /// Получить все логи активности
    /// </summary>
    /// <param name="dto">Данные для получения логов</param>
    /// <returns>Список логов активностей</returns>
    Task<IEnumerable<ActivityLog>> GetAllActivityLogs(GetActivityLogsDto dto);
    
    /// <summary>
    /// Сгенерировать отчёт
    /// </summary>
    /// <param name="dto">Данные для создания отчёта</param>
    /// <returns>Данные созданного отчёта</returns>
    Task<CreatedReportDto> CreateReport(CreateReportDto dto);
    
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