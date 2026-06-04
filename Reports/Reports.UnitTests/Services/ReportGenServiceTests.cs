using Moq;
using Reports.Abstractions.Storage;
using Reports.Abstractions.Storage.Repositories;
using Reports.Models;
using Reports.Services;
using Reports.SharedKernel.Enums;

namespace Reports.UnitTests.Services;

public class ReportGenServiceTests
{
    private readonly Mock<IReportRepository> _reportRepo = new();
    private readonly Mock<IActivityLogRepository> _logRepo = new();
    private readonly Mock<IFileStorage> _fileStorage = new();
    private readonly Mock<ICacheStorage> _cache = new();
    private readonly ReportGenService _sut;
 
    public ReportGenServiceTests()
    {
        _sut = new ReportGenService(_reportRepo.Object, _logRepo.Object, _fileStorage.Object, _cache.Object);
    }
 
    private void SetupDefaultMocks(Guid reportId, Report report, IEnumerable<ActivityLog>? logs = null)
    {
        _reportRepo.Setup(r => r.GetReportById(reportId)).ReturnsAsync(report);
        
        _logRepo.Setup(r => r.GetAllActivityLogs(
            It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<EventType>(), 1, 10))
            .ReturnsAsync(logs ?? new List<ActivityLog>());
        
        _fileStorage.Setup(f 
                => f.UploadFileAsync(It.IsAny<string>(), It.IsAny<Stream>(), "text/csv"))
            .Returns(Task.CompletedTask);
        
        _reportRepo.Setup(r 
            => r.UpdateReport(reportId, It.IsAny<Report>())).Returns(Task.CompletedTask);
        
        _cache.Setup(c => c.RemoveAsync("reports:list")).Returns(Task.CompletedTask);
    }
 
    [Fact]
    public async Task GenerateReport_WhenSuccess_MarksReportAsGeneratedAndInvalidatesCache()
    {
        var reportId = Guid.NewGuid();
        var report = new Report { Name = "Тест", Status = ReportStatus.InProgress };
 
        SetupDefaultMocks(reportId, report);
 
        await _sut.GenerateReport(reportId, new DateOnly(2025, 1, 1), 
            new DateOnly(2025, 1, 31), EventType.Default);
 
        Assert.Equal(ReportStatus.Generated, report.Status);
        Assert.False(string.IsNullOrEmpty(report.FilePath));
        _cache.Verify(c => c.RemoveAsync("reports:list"), Times.Once);
    }
 
    [Fact]
    public async Task GenerateReport_WhenUploadFails_SetsStatusToErrorAndInvalidatesCache()
    {
        var reportId = Guid.NewGuid();
        var report = new Report { Name = "Тест", Status = ReportStatus.InProgress };
 
        _reportRepo.Setup(r 
            => r.GetReportById(reportId)).ReturnsAsync(report);
        _logRepo.Setup(r 
                => r.GetAllActivityLogs(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<EventType>(), 1, 10))
            .ReturnsAsync(new List<ActivityLog>());
        _fileStorage.Setup(f 
                => f.UploadFileAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("MinIO error"));
        _reportRepo.Setup(r 
            => r.UpdateReport(reportId, It.IsAny<Report>())).Returns(Task.CompletedTask);
        _cache.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
 
        await Assert.ThrowsAsync<Exception>(() =>
            _sut.GenerateReport(reportId, new DateOnly(2025, 1, 1), 
                new DateOnly(2025, 1, 31), EventType.Default));
 
        Assert.Equal(ReportStatus.Error, report.Status);
        _cache.Verify(c => c.RemoveAsync("reports:list"), Times.Once);
    }
 
    [Fact]
    public async Task GenerateReport_UploadsFileToCsvStorage()
    {
        var reportId = Guid.NewGuid();
        var report = new Report { Name = "Тест", Status = ReportStatus.InProgress };
        SetupDefaultMocks(reportId, report);
 
        await _sut.GenerateReport(reportId, new DateOnly(2025, 1, 1), 
            new DateOnly(2025, 1, 31), EventType.Default);
 
        _fileStorage.Verify(f => f.UploadFileAsync(
            It.Is<string>(n => n.EndsWith(".csv")),
            It.IsAny<Stream>(), "text/csv"), Times.Once);
    }
 
    [Fact]
    public async Task GenerateReport_FileNameContainsReportId()
    {
        var reportId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var report = new Report { Name = "Тест", Status = ReportStatus.InProgress };
        string? capturedFileName = null;
 
        _reportRepo.Setup(r => r.GetReportById(reportId)).ReturnsAsync(report);
        _logRepo.Setup(r 
                => r.GetAllActivityLogs(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<EventType>(), 1, 10))
            .ReturnsAsync(new List<ActivityLog>());
        _fileStorage.Setup(f 
                => f.UploadFileAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()))
            .Callback<string, Stream, string>((name, _, _) => capturedFileName = name)
            .Returns(Task.CompletedTask);
        _reportRepo.Setup(r 
            => r.UpdateReport(It.IsAny<Guid>(), It.IsAny<Report>())).Returns(Task.CompletedTask);
        _cache.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
 
        await _sut.GenerateReport(reportId, new DateOnly(2025, 1, 1), 
            new DateOnly(2025, 1, 31), EventType.Default);
 
        Assert.Contains(reportId.ToString(), capturedFileName);
    }
 
    [Fact]
    public async Task GenerateReport_WithLogs_CreatesCsvWithData()
    {
        var reportId = Guid.NewGuid();
        var report = new Report { Name = "Тест", Status = ReportStatus.InProgress };
        var logs = new List<ActivityLog>
        {
            new() { EventType = EventType.BookCreated, EventDate = DateTime.UtcNow, Metadata = "{}" },
            new() { EventType = EventType.BookBorrowed, EventDate = DateTime.UtcNow, Metadata = "{}" }
        };
 
        Stream? capturedStream = null;
        _reportRepo.Setup(r => r.GetReportById(reportId)).ReturnsAsync(report);
        _logRepo.Setup(r 
                => r.GetAllActivityLogs(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<EventType>(), 1, 10))
            .ReturnsAsync(logs);
        _fileStorage.Setup(f 
                => f.UploadFileAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()))
            .Callback<string, Stream, string>((_, s, _) => capturedStream = s)
            .Returns(Task.CompletedTask);
        _reportRepo.Setup(r 
            => r.UpdateReport(It.IsAny<Guid>(), It.IsAny<Report>())).Returns(Task.CompletedTask);
        _cache.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
 
        await _sut.GenerateReport(reportId, new DateOnly(2025, 1, 1), 
            new DateOnly(2025, 1, 31), EventType.Default);
 
        Assert.NotNull(capturedStream);
        Assert.True(capturedStream!.Length > 0);
    }
 
    [Fact]
    public async Task GenerateReport_UpdatesReportWithFilePath()
    {
        var reportId = Guid.NewGuid();
        var report = new Report { Name = "Тест", Status = ReportStatus.InProgress };
        SetupDefaultMocks(reportId, report);
 
        await _sut.GenerateReport(reportId, new DateOnly(2025, 1, 1), 
            new DateOnly(2025, 1, 31), EventType.Default);
 
        _reportRepo.Verify(r => r.UpdateReport(reportId, It.Is<Report>(rp =>
            rp.Status == ReportStatus.Generated && !string.IsNullOrEmpty(rp.FilePath))), Times.Once);
    }
}