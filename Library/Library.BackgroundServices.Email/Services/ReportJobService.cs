using System.Text;
using Library.Abstraction.Services;
using Library.Abstraction.Storage;
using Library.BackgroundServices.Email.Abstractions.Services;
using Library.BackgroundServices.Email.Dto;
using Library.BackgroundServices.Email.Settings;
using Library.Dto.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Library.BackgroundServices.Email.Services;

/// <inheritdoc cref="IReportJobService"/>
public class ReportJobService : IReportJobService
{
    private readonly IReportsServiceClient _reportsClient;
    private readonly IFileStorage _fileStorage;
    private readonly EmailTemplateOptions _templateOptions;
    private readonly TimeProvider _timeProvider;
    private readonly IEmailTemplateService _templateService;
    private readonly IEmailService _emailService;
    private readonly ILogger<ReportJobService> _logger;
    
    private const int DaysInWeek = 7;
    private const int MondayOffset = 6;

    public ReportJobService(
        IReportsServiceClient reportsClient,
        IFileStorage fileStorage,
        IOptions<EmailTemplateOptions> templateSettings,
        TimeProvider timeProvider,
        IEmailTemplateService templateService,
        IEmailService emailService,
        ILogger<ReportJobService> logger)
    {
        _reportsClient = reportsClient;
        _fileStorage = fileStorage;
        _templateOptions = templateSettings.Value;
        _timeProvider = timeProvider;
        _templateService = templateService;
        _emailService = emailService;
        _logger = logger;
    }
    
    /// <inheritdoc cref="IReportJobService.GenerateWeeklyReport"/>
    public async Task GenerateWeeklyReport()
    {
        var adminEmails = _templateOptions.WeeklyReport.AdminEmails;
        if (adminEmails.Length == 0)
        {
            _logger.LogWarning("Список email администраторов пуст. Отчет не отправлен.");
            return;
        }
        
        var period = GetPeriodWeeklyReport();
        _logger.LogInformation("Генерация отчета за период: {Start} - {End}", period.StartDate, period.EndDate);

        var report = await GenerateWeeklyReport(period.StartDate, period.EndDate);
        
        await SendWeeklyReportToAdmins(period, report);
    }
    
    private async Task<GeneratedReportDto> GenerateWeeklyReport(DateOnly startDate, DateOnly endDate)
    {
        var startDateTime = startDate.ToDateTime(TimeOnly.MinValue);
        var endDateTime = endDate.ToDateTime(TimeOnly.MaxValue);
        
        var activityLogStatistic = await _reportsClient.GetStatisticByPeriod(startDateTime, endDateTime);
        var report = await CreateWeeklyReportFile(startDate, endDate, activityLogStatistic);
        
        return report;
    }

    private string GenerateCsvContent(DateOnly startDate, DateOnly endDate, 
        int newBooks, int newReaders, int borrowed, int returned, int overdue)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Показатель;Значение");
        sb.AppendLine($"Период;{startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}");
        sb.AppendLine($"Новые книги;{newBooks}");
        sb.AppendLine($"Новые читатели;{newReaders}");
        sb.AppendLine($"Выдано книг;{borrowed}");
        sb.AppendLine($"Возвращено книг;{returned}");
        sb.AppendLine($"Просроченные выдачи;{overdue}");
        return sb.ToString();
    }

    private async Task<GeneratedReportDto> CreateWeeklyReportFile(DateOnly startDate, DateOnly endDate,
        ActivityLogStatisticDto activityLogStatistic)
    {
        var fileName = $"report_{endDate:yyyy-MM-dd}.csv";
        var csvContent = GenerateCsvContent(startDate, endDate, 
            activityLogStatistic.NewBooksCount, 
            activityLogStatistic.NewReadersCount, 
            activityLogStatistic.BorrowedBooksCount, 
            activityLogStatistic.ReturnedBooksCount, 
            activityLogStatistic.OverdueBooksCount);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        await _fileStorage.UploadFileAsync(fileName, stream, "text/csv");

        var downloadUrl = await _fileStorage.GetFileUrlAsync(fileName,
            _templateOptions.WeeklyReport.IntervalInMinutes, "library-reports");

        _logger.LogInformation("Еженедельный отчет сгенерирован: {FileName}. " +
                               "Книг: {Books}, " +
                               "Читателей: {Readers}, " +
                               "Выдано: {Borrowed}, " +
                               "Возвращено: {Returned}, " +
                               "Просрочено: {Overdue}",
            fileName, 
            activityLogStatistic.NewBooksCount, 
            activityLogStatistic.NewReadersCount, 
            activityLogStatistic.BorrowedBooksCount, 
            activityLogStatistic.ReturnedBooksCount, 
            activityLogStatistic.OverdueBooksCount);
        
        return new GeneratedReportDto
        {
            FileName = fileName,
            DownloadUrl = downloadUrl,
            PeriodFrom = startDate,
            PeriodTo = endDate,
            TotalNewBooks = activityLogStatistic.NewBooksCount,
            TotalNewReaders = activityLogStatistic.NewReadersCount,
            TotalBorrowed = activityLogStatistic.BorrowedBooksCount,
            TotalReturned = activityLogStatistic.ReturnedBooksCount,
            TotalOverdue = activityLogStatistic.OverdueBooksCount
        };
    }

    private (DateOnly StartDate, DateOnly EndDate) GetPeriodWeeklyReport()
    {
        var today = _timeProvider.GetUtcNow().DateTime;
        var daysSinceLastMonday = ((int)today.DayOfWeek + MondayOffset) % DaysInWeek;
        var previousMonday = today.Date.AddDays(-daysSinceLastMonday - DaysInWeek);
        var previousSunday = previousMonday.AddDays(MondayOffset);
        
        return (DateOnly.FromDateTime(previousMonday), DateOnly.FromDateTime(previousSunday));
    }

    private async Task SendWeeklyReportToAdmins((DateOnly StartDate, DateOnly EndDate) period,
        GeneratedReportDto reportDto)
    {
        var messageBody = await GenerateWeeklyReportMessageBody(period, reportDto);

        var subject = _templateOptions.WeeklyReport.SubjectTemplate
            .Replace("{StartDate}", period.StartDate.ToString(_templateOptions.DateFormat))
            .Replace("{EndDate}", period.EndDate.ToString(_templateOptions.DateFormat));

        foreach (var adminEmail in _templateOptions.WeeklyReport.AdminEmails)
        {
            var result = await _emailService.SendAsync(new EmailMessageDto
            {
                EmailTo = adminEmail,
                Subject = subject,
                Body = messageBody,
                IsHtml = true
            });

            if (!result.IsSuccess)
            {
                _logger.LogError("Ошибка отправки отчета администратору {Email}: {Error}", 
                    adminEmail, result.Message);
            }
        }

        _logger.LogInformation("Еженедельный отчет отправлен {Count} администраторам",
            _templateOptions.WeeklyReport.AdminEmails.Length);
    }

    private async Task<string> GenerateWeeklyReportMessageBody((DateOnly StartDate, DateOnly EndDate) period,
        GeneratedReportDto reportDto)
    {
        var model = new WeeklyReportDto
        {
            PeriodStart = period.StartDate.ToString(_templateOptions.DateFormat),
            PeriodEnd = period.EndDate.ToString(_templateOptions.DateFormat),
            NewBooksCount = reportDto.TotalNewBooks,
            NewReadersCount = reportDto.TotalNewReaders,
            BorrowedBooksCount = reportDto.TotalBorrowed,
            ReturnedBooksCount = reportDto.TotalReturned,
            OverdueCount = reportDto.TotalOverdue,
            ReportDownloadUrl = reportDto.DownloadUrl,
            GeneratedAt = DateTime.UtcNow.ToString(_templateOptions.DateTimeFormat),
            LibraryName = _templateOptions.LibraryName
        };

        return await _templateService.RenderWeeklyReportAsync(model);
    }
}