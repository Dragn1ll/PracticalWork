using Library.Abstraction.Services;
using Library.Abstraction.Storage;
using Library.BackgroundServices.Email.Abstractions.Services;
using Library.BackgroundServices.Email.Dto;
using Library.BackgroundServices.Email.Services;
using Library.BackgroundServices.Email.Settings;
using Library.Dto.Input;
using Library.Dto.Output;
using Microsoft.Extensions.Logging;
using Moq;

namespace Library.BackgroundServices.Email.UnitTests;

public class ReportJobServiceTests
{
    private readonly Mock<IReportsServiceClient> _reportsClient = new();
    private readonly Mock<IFileStorage> _fileStorage = new();
    private readonly Mock<IEmailTemplateService> _templateService = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<ILogger<ReportJobService>> _logger = new();

    private static readonly DateTimeOffset Monday = 
        new(2025, 6, 16, 10, 0, 0, TimeSpan.Zero);
 
    private static EmailTemplateOptions MakeOptions(params string[] adminEmails) => new()
    {
        DateFormat = "dd.MM.yyyy", DateTimeFormat = "dd.MM.yyyy HH:mm",
        LibraryName = "Тест",
        WeeklyReport = new WeeklyReportTemplate
        {
            AdminEmails = adminEmails,
            SubjectTemplate = "Отчет {StartDate} - {EndDate}",
            IntervalInMinutes = 60
        }
    };
 
    private ReportJobService Build(EmailTemplateOptions opts, DateTimeOffset now) =>
        new(_reportsClient.Object, _fileStorage.Object,
            Microsoft.Extensions.Options.Options.Create(opts),
            new FakeTimeProvider(now), _templateService.Object, _emailService.Object, _logger.Object);
 
    private void SetupDefaultMocks(ActivityLogStatisticDto? stat = null)
    {
        stat ??= new ActivityLogStatisticDto { NewBooksCount = 5, BorrowedBooksCount = 10 };
        
        _reportsClient.Setup(c => c.GetStatisticByPeriod(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(stat);
        
        _fileStorage.Setup(f 
                => f.UploadFileAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        
        _fileStorage.Setup(f 
                => f.GetFileUrlAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync("https://minio.local/report.csv");
        
        _templateService.Setup(t 
                => t.RenderWeeklyReportAsync(It.IsAny<WeeklyReportDto>()))
            .ReturnsAsync("<html>отчет</html>");
        
        _emailService.Setup(e => e.SendAsync(It.IsAny<EmailMessageDto>()))
            .ReturnsAsync(new EmailSendResultDto { IsSuccess = true });
    }
 
    [Fact]
    public async Task GenerateWeeklyReport_WhenNoAdmins_DoesNotSendEmailOrQueryRepo()
    {
        var sut = Build(MakeOptions(), Monday);
 
        await sut.GenerateWeeklyReport();
 
        _reportsClient.Verify(c 
            => c.GetStatisticByPeriod(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
        _emailService.Verify(e => e.SendAsync(It.IsAny<EmailMessageDto>()), Times.Never);
    }
 
    [Fact]
    public async Task GenerateWeeklyReport_SendsEmailToEachAdmin()
    {
        SetupDefaultMocks();
        var sut = Build(MakeOptions("admin1@test.com", "admin2@test.com"), Monday);
 
        await sut.GenerateWeeklyReport();
 
        _emailService.Verify(e => e.SendAsync(It.IsAny<EmailMessageDto>()), Times.Exactly(2));
    }
 
    [Fact]
    public async Task GenerateWeeklyReport_UploadsCsvToStorage()
    {
        SetupDefaultMocks();
        var sut = Build(MakeOptions("admin@test.com"), Monday);
 
        await sut.GenerateWeeklyReport();
 
        _fileStorage.Verify(f => f.UploadFileAsync(
            It.Is<string>(n => n.EndsWith(".csv")),
            It.IsAny<Stream>(), "text/csv"), Times.Once);
    }
 
    [Fact]
    public async Task GenerateWeeklyReport_FileNameMatchesEndDateOfPreviousWeek()
    {
        SetupDefaultMocks();
        string? capturedFileName = null;
        _fileStorage.Setup(f 
                => f.UploadFileAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()))
            .Callback<string, Stream, string>((name, _, _) => capturedFileName = name)
            .Returns(Task.CompletedTask);
 
        var sut = Build(MakeOptions("admin@test.com"), Monday);
        await sut.GenerateWeeklyReport();
 
        Assert.Equal("report_2025-06-15.csv", capturedFileName);
    }
 
    [Fact]
    public async Task GenerateWeeklyReport_SendsHtmlEmailWithBody()
    {
        SetupDefaultMocks();
        EmailMessageDto? msg = null;
        _emailService.Setup(e => e.SendAsync(It.IsAny<EmailMessageDto>()))
            .Callback<EmailMessageDto>(m => msg = m)
            .ReturnsAsync(new EmailSendResultDto { IsSuccess = true });
 
        var sut = Build(MakeOptions("admin@test.com"), Monday);
        await sut.GenerateWeeklyReport();
 
        Assert.True(msg!.IsHtml);
        Assert.Equal("<html>отчет</html>", msg.Body);
    }
 
    [Fact]
    public async Task GenerateWeeklyReport_RendersTemplateWithCorrectStatistics()
    {
        var stat = new ActivityLogStatisticDto
        {
            NewBooksCount = 7, NewReadersCount = 3, BorrowedBooksCount = 15,
            ReturnedBooksCount = 12, OverdueBooksCount = 2
        };
        SetupDefaultMocks(stat);
        WeeklyReportDto? capturedModel = null;
        _templateService.Setup(t => t.RenderWeeklyReportAsync(It.IsAny<WeeklyReportDto>()))
            .Callback<WeeklyReportDto>(m => capturedModel = m)
            .ReturnsAsync("<html></html>");
 
        var sut = Build(MakeOptions("admin@test.com"), Monday);
        await sut.GenerateWeeklyReport();
 
        Assert.Equal(7, capturedModel!.NewBooksCount);
        Assert.Equal(3, capturedModel.NewReadersCount);
        Assert.Equal(15, capturedModel.BorrowedBooksCount);
        Assert.Equal(2, capturedModel.OverdueCount);
    }
 
    [Fact]
    public async Task GenerateWeeklyReport_WhenEmailFails_ContinuesToNextAdmin()
    {
        SetupDefaultMocks();
        var callCount = 0;
        _emailService.Setup(e => e.SendAsync(It.IsAny<EmailMessageDto>()))
            .ReturnsAsync(() => new EmailSendResultDto { IsSuccess = ++callCount != 1 });
 
        var sut = Build(MakeOptions("a1@t.com", "a2@t.com"), Monday);
        await sut.GenerateWeeklyReport();
 
        _emailService.Verify(e => e.SendAsync(It.IsAny<EmailMessageDto>()), Times.Exactly(2));
    }
}