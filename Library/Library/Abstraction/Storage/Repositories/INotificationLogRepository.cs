using Library.Models;

namespace Library.Abstraction.Storage.Repositories;

/// <summary>
/// Репозиторий для логов уведомлений
/// </summary>
public interface INotificationLogRepository
{
    /// <summary>
    /// Добавить запись об отправке уведомления
    /// </summary>
    Task AddNotificationLog(NotificationLog notificationLog);

    /// <summary>
    /// Проверить, было ли отправлено напоминание для указанной выдачи за последние указанные часы
    /// </summary>
    Task<bool> WasReminderSentRecently(Guid borrowId, int withinHours);

    /// <summary>
    /// Получить логи уведомлений по типу
    /// </summary>
    Task<IEnumerable<NotificationLog>> GetNotificationLogs(
        string notificationType = null!, 
        int page = 1,
        int pageSize = 20
        );
}