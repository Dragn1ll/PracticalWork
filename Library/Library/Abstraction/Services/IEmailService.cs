using Library.Dto.Input;
using Library.Dto.Output;

namespace Library.Abstraction.Services;

/// <summary>
/// Сервис для отправки email сообщений
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Отправить сообщение по email
    /// </summary>
    Task<EmailSendResultDto> SendAsync(EmailMessageDto message);
}