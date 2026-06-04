using Library.Abstraction.Services;
using Library.Abstraction.Storage.Repositories;
using Library.BackgroundServices.Email.Abstractions.Services;
using Library.BackgroundServices.Email.Dto;
using Library.BackgroundServices.Email.Services;
using Library.BackgroundServices.Email.Settings;
using Library.Dto.Input;
using Library.Dto.Output;
using Library.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace Library.BackgroundServices.Email.UnitTests;

public class NotificationServiceTests
{
    private readonly Mock<ILibraryRepository> _libraryRepo = new();
    private readonly Mock<INotificationLogRepository> _notifLogRepo = new();
    private readonly Mock<IEmailTemplateService> _templateService = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<ILogger<NotificationService>> _logger = new();
 
    private static readonly EmailTemplateOptions Options = new()
    {
        LibraryName = "Тест", LibraryAddress = "Адрес", LibraryPhone = "Тел",
        WorkingHours = "9-18", DateFormat = "dd.MM.yyyy",
        ReturnReminder = new ReturnReminderTemplate
        {
            DaysBeforeDueDate = 3, IntervalInHours = 24,
            SubjectTemplate = "Напоминание: \"{BookTitle}\""
        }
    };
 
    private NotificationService Build(DateTimeOffset now) =>
        new(new FakeTimeProvider(now), _libraryRepo.Object,
            Microsoft.Extensions.Options.Options.Create(Options),
            _notifLogRepo.Object, _templateService.Object, _emailService.Object, _logger.Object);
 
    private BorrowedIssuedBookInfoDto MakeInfo(string email = "test@mail.ru") => new()
    {
        BorrowId = Guid.NewGuid(), ReaderId = Guid.NewGuid(),
        ReaderFullName = "Иванов И.И.", ReaderEmail = email,
        BookTitle = "Книга", BookAuthors = ["Автор"],
        BorrowDueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3))
    };
 
    [Fact]
    public async Task NotifyReadersReturnBooks_WhenNoIssuances_ReturnsZeroCounts()
    {
        _libraryRepo.Setup(r => r.GetBorrowedIssuedBooksInfo(It.IsAny<DateOnly>()))
            .ReturnsAsync(new List<BorrowedIssuedBookInfoDto>());
 
        var result = await Build(DateTimeOffset.UtcNow).NotifyReadersReturnBooks();
 
        Assert.Equal(0, result.SentCount);
        Assert.Equal(0, result.FailedCount);
        Assert.Equal(0, result.SkippedCount);
    }
 
    [Fact]
    public async Task NotifyReadersReturnBooks_WhenEmailEmpty_SkipsReader()
    {
        _libraryRepo.Setup(r => r.GetBorrowedIssuedBooksInfo(It.IsAny<DateOnly>()))
            .ReturnsAsync(new List<BorrowedIssuedBookInfoDto> { MakeInfo(email: "") });
 
        var result = await Build(DateTimeOffset.UtcNow).NotifyReadersReturnBooks();
 
        Assert.Equal(1, result.SkippedCount);
        _emailService.Verify(e => e.SendAsync(It.IsAny<EmailMessageDto>()), Times.Never);
    }
 
    [Fact]
    public async Task NotifyReadersReturnBooks_WhenAlreadySentRecently_SkipsBorrow()
    {
        var info = MakeInfo();
        _libraryRepo.Setup(r => r.GetBorrowedIssuedBooksInfo(It.IsAny<DateOnly>()))
            .ReturnsAsync(new List<BorrowedIssuedBookInfoDto> { info });
        _notifLogRepo.Setup(r => r.WasReminderSentRecently(info.BorrowId, 24)).ReturnsAsync(true);
 
        var result = await Build(DateTimeOffset.UtcNow).NotifyReadersReturnBooks();
 
        Assert.Equal(1, result.SkippedCount);
    }
 
    [Fact]
    public async Task NotifyReadersReturnBooks_WhenSendSuccess_IncrementsSentCount()
    {
        var info = MakeInfo();
        _libraryRepo.Setup(r => r.GetBorrowedIssuedBooksInfo(It.IsAny<DateOnly>()))
            .ReturnsAsync(new List<BorrowedIssuedBookInfoDto> { info });
        _notifLogRepo.Setup(r => r.WasReminderSentRecently(info.BorrowId, 24)).ReturnsAsync(false);
        _templateService.Setup(t => t.RenderReturnReminderAsync(It.IsAny<ReturnReminderDto>()))
            .ReturnsAsync("<html>тело</html>");
        _emailService.Setup(e => e.SendAsync(It.IsAny<EmailMessageDto>()))
            .ReturnsAsync(new EmailSendResultDto { IsSuccess = true });
        _notifLogRepo.Setup(r => r.AddNotificationLog(It.IsAny<NotificationLog>()))
            .Returns(Task.CompletedTask);
 
        var result = await Build(DateTimeOffset.UtcNow).NotifyReadersReturnBooks();
 
        Assert.Equal(1, result.SentCount);
        Assert.Equal(0, result.FailedCount);
    }
 
    [Fact]
    public async Task NotifyReadersReturnBooks_WhenSendFails_IncrementsFailedCount()
    {
        var info = MakeInfo();
        _libraryRepo.Setup(r => r.GetBorrowedIssuedBooksInfo(It.IsAny<DateOnly>()))
            .ReturnsAsync(new List<BorrowedIssuedBookInfoDto> { info });
        _notifLogRepo.Setup(r 
            => r.WasReminderSentRecently(info.BorrowId, 24)).ReturnsAsync(false);
        _templateService.Setup(t => t.RenderReturnReminderAsync(It.IsAny<ReturnReminderDto>()))
            .ReturnsAsync("<html>тело</html>");
        _emailService.Setup(e => e.SendAsync(It.IsAny<EmailMessageDto>()))
            .ReturnsAsync(new EmailSendResultDto { IsSuccess = false, Message = "SMTP error" });
        _notifLogRepo.Setup(r => r.AddNotificationLog(It.IsAny<NotificationLog>()))
            .Returns(Task.CompletedTask);
 
        var result = await Build(DateTimeOffset.UtcNow).NotifyReadersReturnBooks();
 
        Assert.Equal(1, result.FailedCount);
        Assert.Equal(0, result.SentCount);
    }
 
    [Fact]
    public async Task NotifyReadersReturnBooks_SubjectContainsBookTitle()
    {
        var info = MakeInfo();
        info.BookTitle = "Мастер и Маргарита";
        _libraryRepo.Setup(r => r.GetBorrowedIssuedBooksInfo(It.IsAny<DateOnly>()))
            .ReturnsAsync(new List<BorrowedIssuedBookInfoDto> { info });
        _notifLogRepo.Setup(r 
            => r.WasReminderSentRecently(It.IsAny<Guid>(), 24)).ReturnsAsync(false);
        _templateService.Setup(t => t.RenderReturnReminderAsync(It.IsAny<ReturnReminderDto>()))
            .ReturnsAsync("<html></html>");
        EmailMessageDto? captured = null;
        _emailService.Setup(e => e.SendAsync(It.IsAny<EmailMessageDto>()))
            .Callback<EmailMessageDto>(m => captured = m)
            .ReturnsAsync(new EmailSendResultDto { IsSuccess = true });
        _notifLogRepo.Setup(r => r.AddNotificationLog(It.IsAny<NotificationLog>()))
            .Returns(Task.CompletedTask);
 
        await Build(DateTimeOffset.UtcNow).NotifyReadersReturnBooks();
 
        Assert.Contains("Мастер и Маргарита", captured!.Subject);
    }
 
    [Fact]
    public async Task NotifyReadersReturnBooks_TargetDateCalculatedCorrectly()
    {
        var now = new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero);
        var expectedTarget = new DateOnly(2025, 6, 18);
 
        _libraryRepo.Setup(r => r.GetBorrowedIssuedBooksInfo(expectedTarget))
            .ReturnsAsync(new List<BorrowedIssuedBookInfoDto>())
            .Verifiable();
 
        await Build(now).NotifyReadersReturnBooks();
 
        _libraryRepo.Verify(r => r.GetBorrowedIssuedBooksInfo(expectedTarget), Times.Once);
    }
 
    [Fact]
    public async Task NotifyReadersReturnBooks_WhenTemplateThrows_LogsErrorAndIncrementsFailedCount()
    {
        var info = MakeInfo();
        _libraryRepo.Setup(r => r.GetBorrowedIssuedBooksInfo(It.IsAny<DateOnly>()))
            .ReturnsAsync(new List<BorrowedIssuedBookInfoDto> { info });
        _notifLogRepo.Setup(r 
            => r.WasReminderSentRecently(info.BorrowId, 24)).ReturnsAsync(false);
        _templateService.Setup(t => t.RenderReturnReminderAsync(It.IsAny<ReturnReminderDto>()))
            .ThrowsAsync(new Exception("Шаблон недоступен"));
        _notifLogRepo.Setup(r => r.AddNotificationLog(It.IsAny<NotificationLog>()))
            .Returns(Task.CompletedTask);
 
        var result = await Build(DateTimeOffset.UtcNow).NotifyReadersReturnBooks();
 
        Assert.Equal(1, result.FailedCount);
        _notifLogRepo.Verify(r => r.AddNotificationLog(
            It.Is<NotificationLog>(l => !l.IsSent)), Times.Once);
    }
}