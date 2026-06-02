using Reports.Models;

namespace Reports.Abstractions.Storage.Repositories;

/// <summary>
/// Репозиторий для отчётов
/// </summary>
public interface IReportRepository
{
    /// <summary>
    /// Создание отчёта
    /// </summary>
    /// <param name="report">Отчёт</param>
    /// <returns>Идентификатор созданного отчёта</returns>
    Task<Guid> CreateReport(Report report);
    
    /// <summary>
    /// Получение отчёт по идентификатору
    /// </summary>
    /// <param name="reportId">Идентификатор отчёта</param>
    /// <returns>Отчёт</returns>
    Task<Report> GetReportById(Guid reportId);
    
    /// <summary>
    /// Получить отчёт по названию
    /// </summary>
    /// <param name="reportName">Название отчёта</param>
    /// <returns>Отчёт</returns>
    Task<Report> GetReportByName(string reportName);
    
    /// <summary>
    /// Получить список сгенерированных отчётов
    /// </summary>
    /// <returns>Список отчётов</returns>
    Task<IEnumerable<Report>> GetGeneratedReports();
    
    /// <summary>
    /// Обновить данные отчёта
    /// </summary>
    /// <param name="reportId">Идентификатор отчёта</param>
    /// <param name="report">Отчёт с обновлёнными данными</param>
    Task UpdateReport(Guid reportId, Report report);
}