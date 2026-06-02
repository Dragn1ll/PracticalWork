using Library.BackgroundServices.Email.Dto;

namespace Library.BackgroundServices.Email.Abstractions.Services;

/// <summary>
/// Сервис для рендеринга HTML email шаблонов
/// </summary>
public interface IEmailTemplateService
{
    /// <summary>
    /// Рендеринг шаблона напоминания о возврате книги
    /// </summary>
    Task<string> RenderReturnReminderAsync(ReturnReminderDto dto);

    /// <summary>
    /// Рендеринг шаблона еженедельного отчета
    /// </summary>
    Task<string> RenderWeeklyReportAsync(WeeklyReportDto dto);
}