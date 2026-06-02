using Library.BackgroundServices.Email.Abstractions.Services;
using Library.BackgroundServices.Email.Dto;
using Microsoft.Extensions.Logging;
using Razor.Templating.Core;

namespace Library.BackgroundServices.Email.Services;

/// <inheritdoc cref="IEmailTemplateService"/>
public class EmailTemplateService : IEmailTemplateService
{
    private readonly ILogger<EmailTemplateService> _logger;

    public EmailTemplateService(ILogger<EmailTemplateService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc cref="IEmailTemplateService.RenderReturnReminderAsync"/>
    public async Task<string> RenderReturnReminderAsync(ReturnReminderDto dto)
    {
        try
        {
            return await RazorTemplateEngine.RenderAsync("/Views/Emails/ReturnReminder.cshtml", dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка рендеринга шаблона ReturnReminder");
            throw;
        }
    }

    /// <inheritdoc cref="IEmailTemplateService.RenderWeeklyReportAsync"/>
    public async Task<string> RenderWeeklyReportAsync(WeeklyReportDto dto)
    {
        try
        {
            return await RazorTemplateEngine.RenderAsync("/Views/Emails/WeeklyReport.cshtml", dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка рендеринга шаблона WeeklyReport");
            throw;
        }
    }
}