using System.Diagnostics;
using Library.Abstraction.Services;
using Library.Abstraction.Storage.Repositories;
using Library.BackgroundServices.Email.Abstractions.Services;
using Library.BackgroundServices.Email.Dto;
using Library.BackgroundServices.Email.Settings;
using Library.Dto.Input;
using Library.Dto.Output;
using Library.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Library.BackgroundServices.Email.Services;

public class NotificationService : INotificationService
{
    private readonly TimeProvider _timeProvider;
    private readonly ILibraryRepository _libraryRepository;
    private readonly EmailTemplateOptions _templateOptions;
    private readonly INotificationLogRepository _notificationLogRepository;
    private readonly IEmailTemplateService _templateService;
    private readonly IEmailService _emailService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(TimeProvider timeProvider, ILibraryRepository libraryRepository, 
        IOptions<EmailTemplateOptions> templateSettings, INotificationLogRepository notificationLogRepository, 
        IEmailTemplateService templateService, IEmailService emailService,
        ILogger<NotificationService> logger)
    {
        _timeProvider = timeProvider;
        _libraryRepository = libraryRepository;
        _templateOptions = templateSettings.Value;
        _notificationLogRepository = notificationLogRepository;
        _templateService = templateService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<ReturnReminderResultDto> NotifyReadersReturnBooks()
    {
        var targetDueDate = DateOnly.FromDateTime(_timeProvider.GetUtcNow().DateTime
                .AddDays(_templateOptions.ReturnReminder.DaysBeforeDueDate));
        
        var stopWatch = Stopwatch.StartNew();
        
        var borrowedBooksInfo = await _libraryRepository.GetBorrowedIssuedBooksInfo(targetDueDate);
        var notifyResult = await NotifyReadersReturnBooks(borrowedBooksInfo);
        
        stopWatch.Stop();
        notifyResult.ExecutionTime = stopWatch.Elapsed;
        
        return notifyResult;
    }

    private async Task<ReturnReminderResultDto> NotifyReadersReturnBooks(
        IList<BorrowedIssuedBookInfoDto> borrowedBooksInfo)
    {
        var returnReminderResult = new ReturnReminderResultDto();
        
        foreach (var borrowedBookInfo in borrowedBooksInfo)
        {
            if (await CanSkipBorrowedBookInfo(borrowedBookInfo))
            {
                returnReminderResult.SkippedCount++;
                continue;
            }

            var subject = _templateOptions.ReturnReminder.SubjectTemplate
                .Replace("{BookTitle}", borrowedBookInfo.BookTitle);
            
            try
            {
                var messageBody = await CreateMessageBody(borrowedBookInfo);
                
                var sendResult = await SendMessage(borrowedBookInfo, subject, messageBody);

                if (sendResult.IsSuccess)
                {
                    returnReminderResult.SentCount++;
                }
                else
                {
                    returnReminderResult.FailedCount++;
                }
            }
            catch (Exception exception)
            {
                returnReminderResult.FailedCount++;
                
                _logger.LogError(exception, "Ошибка обработки напоминания для выдачи {BorrowId}",
                    borrowedBookInfo.BorrowId);

                await _notificationLogRepository.AddNotificationLog(new NotificationLog
                {
                    BorrowId = borrowedBookInfo.BorrowId,
                    NotificationType = "ReturnReminder",
                    RecipientEmail = borrowedBookInfo.ReaderEmail,
                    Subject = subject,
                    IsSent = false,
                    ErrorMessage = exception.Message
                });
            }
        }
        
        return returnReminderResult;
    }

    private async Task<bool> CanSkipBorrowedBookInfo(BorrowedIssuedBookInfoDto borrowedBookInfo)
    {
        if (string.IsNullOrWhiteSpace(borrowedBookInfo.ReaderEmail))
        {
            _logger.LogWarning("Пропуск: у читателя {ReaderName} (ID: {ReaderId}) нет email", 
                borrowedBookInfo.ReaderFullName, borrowedBookInfo.ReaderId);
            
            return true;
        }

        var wasSent = await _notificationLogRepository.WasReminderSentRecently(borrowedBookInfo.BorrowId,
            _templateOptions.ReturnReminder.IntervalInHours);
        
        return wasSent;
    }

    private async Task<string> CreateMessageBody(BorrowedIssuedBookInfoDto borrowedBookInfo)
    {
        var daysRemaining = borrowedBookInfo.BorrowDueDate.DayNumber -
                            DateOnly.FromDateTime(_timeProvider.GetUtcNow().DateTime).DayNumber;
        
        var dto = new ReturnReminderDto
        {
            ReaderName = borrowedBookInfo.ReaderFullName,
            BookTitle = borrowedBookInfo.BookTitle,
            BookAuthors = string.Join(", ", borrowedBookInfo.BookAuthors),
            DueDate = borrowedBookInfo.BorrowDueDate.ToString(_templateOptions.DateFormat),
            DaysRemaining = daysRemaining,
            LibraryName = _templateOptions.LibraryName,
            LibraryAddress = _templateOptions.LibraryAddress,
            LibraryPhone = _templateOptions.LibraryPhone,
            WorkingHours = _templateOptions.WorkingHours
        };

        return await _templateService.RenderReturnReminderAsync(dto);
    }

    private async Task<EmailSendResultDto> SendMessage(BorrowedIssuedBookInfoDto borrowedBookInfo, string subject, string body)
    {
        var sendResult = await _emailService.SendAsync(new EmailMessageDto
        {
            EmailTo = borrowedBookInfo.ReaderEmail,
            Subject = subject,
            Body = body,
            IsHtml = true
        });

        await _notificationLogRepository.AddNotificationLog(new NotificationLog
        {
            BorrowId = borrowedBookInfo.BorrowId,
            NotificationType = "ReturnReminder",
            RecipientEmail = borrowedBookInfo.ReaderEmail,
            Subject = subject,
            IsSent = sendResult.IsSuccess,
            ErrorMessage = sendResult.Message
        });

        return sendResult;
    }
}