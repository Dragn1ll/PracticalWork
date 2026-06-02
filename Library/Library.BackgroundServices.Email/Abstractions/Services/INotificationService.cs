using Library.BackgroundServices.Email.Dto;

namespace Library.BackgroundServices.Email.Abstractions.Services;

/// <summary>
/// Сервис по отправке уведомлений
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Отправить напоминание читателям о возврате книг
    /// </summary>
    /// <returns>Результат отправки напоминаний</returns>
    Task<ReturnReminderResultDto> NotifyReadersReturnBooks();
}