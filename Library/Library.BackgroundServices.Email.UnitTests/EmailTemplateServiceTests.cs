using Library.BackgroundServices.Email.Dto;
using Library.BackgroundServices.Email.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Library.BackgroundServices.Email.UnitTests;

public class EmailTemplateServiceTests
{
    private readonly Mock<ILogger<EmailTemplateService>> _logger = new();
 
    [Fact]
    public async Task RenderReturnReminderAsync_WhenEngineThrows_LogsAndRethrows()
    {
        var sut = new EmailTemplateService(_logger.Object);
 
        await Assert.ThrowsAnyAsync<Exception>(() =>
            sut.RenderReturnReminderAsync(new ReturnReminderDto
            {
                ReaderName = "Иван", BookTitle = "Книга", BookAuthors = "Автор",
                DueDate = "01.01.2025", DaysRemaining = 3, LibraryName = "Биб",
                LibraryAddress = "Адрес", LibraryPhone = "Тел", WorkingHours = "9-18"
            }));
 
        _logger.Verify(
            l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
 
    [Fact]
    public async Task RenderWeeklyReportAsync_WhenEngineThrows_LogsAndRethrows()
    {
        var sut = new EmailTemplateService(_logger.Object);
 
        await Assert.ThrowsAnyAsync<Exception>(() =>
            sut.RenderWeeklyReportAsync(new WeeklyReportDto
            {
                PeriodStart = "01.01.2025", PeriodEnd = "07.01.2025",
                NewBooksCount = 1, NewReadersCount = 1,
                BorrowedBooksCount = 5, ReturnedBooksCount = 4,
                OverdueCount = 1, ReportDownloadUrl = "http://test",
                GeneratedAt = "01.01.2025 10:00", LibraryName = "Биб"
            }));
 
        _logger.Verify(
            l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
