using Moq;
using Reports.Abstractions.Storage;
using Reports.Abstractions.Storage.Repositories;
using Reports.Dto;
using Reports.Exceptions;
using Reports.Models;
using Reports.Services;
using Reports.SharedKernel.Enums;

namespace Reports.UnitTests.Services;

public class ReportServiceTests
{
    private readonly Mock<IActivityLogRepository> _logRepo = new();
    private readonly Mock<IReportRepository> _reportRepo = new();
    private readonly Mock<ICacheStorage> _cache = new();
    private readonly Mock<IFileStorage> _fileStorage = new();
    private readonly ReportService _sut;
 
    public ReportServiceTests()
    {
        _sut = new ReportService(_logRepo.Object, _reportRepo.Object, _cache.Object, _fileStorage.Object);
    }
 
    [Fact]
    public async Task CreateReport_WhenValidPeriod_CreatesAndInvalidatesCache()
    {
        var from = new DateOnly(2025, 1, 1);
        var to   = new DateOnly(2025, 1, 31);
        _reportRepo.Setup(r => r.CreateReport(It.IsAny<Report>())).ReturnsAsync(Guid.NewGuid());
        _cache.Setup(c => c.RemoveAsync("reports:list")).Returns(Task.CompletedTask);
 
        var result = await _sut.CreateReport("Январь", from, to, EventType.Default);
 
        Assert.Equal("Январь", result.Name);
        Assert.Equal(ReportStatus.InProgress, result.Status);
        _reportRepo.Verify(r => r.CreateReport(It.IsAny<Report>()), Times.Once);
        _cache.Verify(c => c.RemoveAsync("reports:list"), Times.Once);
    }
 
    [Fact]
    public async Task CreateReport_WhenPeriodToBeforePeriodFrom_ThrowsClientErrorException()
    {
        await Assert.ThrowsAsync<ClientErrorException>(() =>
            _sut.CreateReport("Bad", new DateOnly(2025, 2, 1), 
                new DateOnly(2025, 1, 1), EventType.Default));
 
        _reportRepo.Verify(r => r.CreateReport(It.IsAny<Report>()), Times.Never);
    }
 
    [Fact]
    public async Task CreateReport_WhenEqualDates_Succeeds()
    {
        var date = new DateOnly(2025, 6, 15);
        _reportRepo.Setup(r => r.CreateReport(It.IsAny<Report>())).ReturnsAsync(Guid.NewGuid());
        _cache.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
 
        var result = await _sut.CreateReport("День", date, date, EventType.Default);
 
        Assert.Equal(date, result.PeriodFrom);
        Assert.Equal(date, result.PeriodTo);
    }
 
    [Fact]
    public async Task CreateReport_WhenRepositoryThrows_WrapsInReportServiceException()
    {
        _reportRepo.Setup(r => r.CreateReport(It.IsAny<Report>()))
            .ThrowsAsync(new Exception("DB error"));
 
        await Assert.ThrowsAsync<ReportServiceException>(() =>
            _sut.CreateReport("X", new DateOnly(2025, 1, 1), 
                new DateOnly(2025, 1, 31), EventType.Default));
    }
 
    [Fact]
    public async Task GetGeneratedReports_WhenCacheHit_ReturnsCachedData_WithoutQueryingRepo()
    {
        var cached = new List<Report> { new() { Name = "Cached", Status = ReportStatus.Generated } };
        _cache.Setup(c => c.TryGetAsync<IEnumerable<Report>>("reports:list")).ReturnsAsync(cached);
 
        var result = (await _sut.GetGeneratedReports()).ToList();
 
        Assert.Single(result);
        _reportRepo.Verify(r => r.GetGeneratedReports(), Times.Never);
    }
 
    [Fact]
    public async Task GetGeneratedReports_WhenCacheMiss_QueriesRepoAndSetsCache()
    {
        var reports = new List<Report>
        {
            new() { Name = "R1", Status = ReportStatus.Generated },
            new() { Name = "R2", Status = ReportStatus.Generated }
        };
        _cache.Setup(c => c.TryGetAsync<IEnumerable<Report>>("reports:list"))
            .ReturnsAsync((IEnumerable<Report>?)null);
        _reportRepo.Setup(r => r.GetGeneratedReports()).ReturnsAsync(reports);
        _cache.Setup(c 
                => c.SetAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Report>>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);
 
        var result = (await _sut.GetGeneratedReports()).ToList();
 
        Assert.Equal(2, result.Count);
        _cache.Verify(
            c => c.SetAsync("reports:list", It.IsAny<IEnumerable<Report>>(), 
                TimeSpan.FromHours(24)),
            Times.Once);
    }

    [Fact]
    public async Task GetAllActivityLogs_NormalizesPageLessThanOneToOne()
    {
        _logRepo.Setup(r => r.GetAllActivityLogs(null, null, EventType.Default, 1, 20))
            .ReturnsAsync(new List<ActivityLog>())
            .Verifiable();
 
        await _sut.GetAllActivityLogs(null, null, EventType.Default, page: 0, pageSize: 20);
 
        _logRepo.Verify(r 
            => r.GetAllActivityLogs(null, null, EventType.Default, 1, 20), Times.Once);
    }
 
    [Fact]
    public async Task GetAllActivityLogs_NormalizesPageSizeLessThanOneToTwenty()
    {
        _logRepo.Setup(r => r.GetAllActivityLogs(null, null, EventType.Default, 1, 20))
            .ReturnsAsync(new List<ActivityLog>())
            .Verifiable();
 
        await _sut.GetAllActivityLogs(null, null, EventType.Default, page: 1, pageSize: 0);
 
        _logRepo.Verify(r 
            => r.GetAllActivityLogs(null, null, EventType.Default, 1, 20), Times.Once);
    }
 
    [Fact]
    public async Task GetAllActivityLogs_PassesFiltersToRepository()
    {
        var from = new DateOnly(2025, 1, 1);
        var to   = new DateOnly(2025, 1, 31);
        _logRepo.Setup(r 
                => r.GetAllActivityLogs(from, to, EventType.BookCreated, 2, 10))
            .ReturnsAsync(new List<ActivityLog>());
 
        await _sut.GetAllActivityLogs(from, to, EventType.BookCreated, 2, 10);
 
        _logRepo.Verify(r 
            => r.GetAllActivityLogs(from, to, EventType.BookCreated, 2, 10), Times.Once);
    }

    [Fact]
    public async Task GetReportFileUrl_WhenReportExists_ReturnsPresignedUrl()
    {
        const string url = "https://minio.local/report.csv?token=abc";
        _reportRepo.Setup(r => r.GetReportByName("Январь"))
            .ReturnsAsync(new Report
            {
                Name = "Январь", 
                FilePath = "2025/1/report.csv", 
                Status = ReportStatus.Generated
            });
        _fileStorage.Setup(f 
            => f.GetFileUrlAsync("2025/1/report.csv", 60, "reports")).ReturnsAsync(url);
 
        var result = await _sut.GetReportFileUrl("Январь");
 
        Assert.Equal(url, result);
    }
 
    [Fact]
    public async Task GetReportFileUrl_WhenReportNotFound_ThrowsReportServiceException()
    {
        _reportRepo.Setup(r => r.GetReportByName(It.IsAny<string>()))
            .ThrowsAsync(new ClientErrorException("Не найден"));
 
        await Assert.ThrowsAsync<ClientErrorException>(() => _sut.GetReportFileUrl("Несуществующий"));
    }
 
    [Fact]
    public async Task GetStatisticByPeriod_DelegatesToRepository()
    {
        var start = new DateTime(2025, 1, 1);
        var end   = new DateTime(2025, 1, 31);
        var stat  = new ActivityLogStatisticDto { NewBooksCount = 5, BorrowedBooksCount = 12 };
        _logRepo.Setup(r => r.GetStatisticByPeriod(start, end)).ReturnsAsync(stat);
 
        var result = await _sut.GetStatisticByPeriod(start, end);
 
        Assert.Equal(5, result.NewBooksCount);
        Assert.Equal(12, result.BorrowedBooksCount);
    }
}