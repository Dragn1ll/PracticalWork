using Library.BackgroundServices.Email.Abstractions.Jobs;
using Library.BackgroundServices.Email.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace Library.BackgroundServices.Email.Jobs;

/// <summary>
/// Фоновая задача: еженедельный отчет для администрации
/// </summary>
public class WeeklyReportJob : ILibraryJob
{
    public string JobName => "WeeklyReport";
    public string Description => "Еженедельный отчет для администрации";

    private readonly IReportJobService _reportJobService;
    private readonly ILogger<WeeklyReportJob> _logger;

    public WeeklyReportJob(
        IReportJobService reportJobService,
        ILogger<WeeklyReportJob> logger)
    {
        _reportJobService = reportJobService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Начало выполнения задачи: {JobName}", JobName);

        await _reportJobService.GenerateWeeklyReport();
    }
}