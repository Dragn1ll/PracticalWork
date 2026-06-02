using Library.BackgroundServices.Email.Abstractions.Jobs;
using Library.BackgroundServices.Email.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace Library.BackgroundServices.Email.Jobs;

/// <summary>
/// Фоновая задача: автоматические напоминания о возврате книг
/// </summary>
public class ReturnReminderJob : ILibraryJob
{
    public string JobName => "ReturnReminder";
    public string Description => "Автоматические напоминания о возврате книг";

    private readonly INotificationService _notificationService;
    private readonly ILogger<ReturnReminderJob> _logger;

    public ReturnReminderJob(
        INotificationService notificationService,
        ILogger<ReturnReminderJob> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Начало выполнения задачи: {JobName}", JobName);

        var notifyResult = await _notificationService.NotifyReadersReturnBooks();

        _logger.LogInformation(
            "Задача {JobName} завершена: отправлено {Sent}, ошибок {Failed}, пропущено {Skipped}, время выполнения {ExecutionTime} мс",
            JobName, notifyResult.SentCount, notifyResult.FailedCount, notifyResult.SkippedCount,
            notifyResult.ExecutionTime);
    }
}