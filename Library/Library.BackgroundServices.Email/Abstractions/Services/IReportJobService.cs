namespace Library.BackgroundServices.Email.Abstractions.Services;

/// <summary>
/// Сервис для работы с еженедельными отчетами
/// </summary>
public interface IReportJobService
{
    /// <summary>
    /// Генерация еженедельного отчета за указанный период
    /// </summary>
    Task GenerateWeeklyReport();
}