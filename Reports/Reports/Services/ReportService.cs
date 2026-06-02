using Reports.Abstractions.Services;
using Reports.Abstractions.Storage;
using Reports.Abstractions.Storage.Repositories;
using Reports.Dto;
using Reports.Exceptions;
using Reports.Models;
using Reports.SharedKernel.Enums;

namespace Reports.Services;

/// <inheritdoc cref="IReportService"/>
public class ReportService : IReportService
{
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly IReportRepository _reportRepository;
    private readonly ICacheStorage _cache;
    private readonly IFileStorage _fileStorage;
    private const string CacheKey = "reports:list";
    private const string BucketName = "reports";

    public ReportService(IActivityLogRepository activityLogRepository, IReportRepository reportRepository, 
        ICacheStorage cache, IFileStorage fileStorage)
    {
        _activityLogRepository = activityLogRepository;
        _reportRepository = reportRepository;
        _cache = cache;
        _fileStorage = fileStorage;
    }

    /// <inheritdoc cref="IReportService.GetAllActivityLogs"/>
    public async Task<IEnumerable<ActivityLog>> GetAllActivityLogs(DateOnly? dateFrom, DateOnly? dateTo, 
        EventType eventType, int page, int pageSize)
    {
        try
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 20 : pageSize;
            
            return await _activityLogRepository.GetAllActivityLogs(dateFrom, dateTo, eventType, page, pageSize);
        }
        catch (Exception ex) when (ex is not ClientErrorException)
        {
            throw new ReportServiceException("Не удалось получить записи логов активности", ex);
        }
    }
    
    /// <inheritdoc cref="IReportService.CreateReport"/>
    public async Task<CreatedReportDto> CreateReport(string name, DateOnly periodFrom, DateOnly periodTo,
        EventType eventType)
    {
        if (periodTo < periodFrom)
        {
            throw new ClientErrorException("Дата начала периода отчёта не может быть позже даты окончания.");
        }

        var report = new Report
        {
            Name = name,
            PeriodFrom = periodFrom,
            PeriodTo = periodTo,
            Status = ReportStatus.InProgress
        };

        try
        {
            var reportId = await _reportRepository.CreateReport(report);

            await _cache.RemoveAsync(CacheKey);

            return new CreatedReportDto
            {
                Id = reportId,
                Name = report.Name,
                PeriodFrom = report.PeriodFrom,
                PeriodTo = report.PeriodTo,
                Status = report.Status
            };
        }
        catch (Exception ex) when (ex is not ClientErrorException)
        {
            throw new ReportServiceException("Не удалось начать генерацию отчёта", ex);
        }
    }

    /// <inheritdoc cref="IReportService.GetGeneratedReports"/>
    public async Task<IEnumerable<Report>> GetGeneratedReports()
    {
        try
        {
            var cache = await _cache.TryGetAsync<IEnumerable<Report>>(CacheKey);

            if (cache == null)
            {
                var reports = (await _reportRepository.GetGeneratedReports()).ToList();
                
                await _cache.SetAsync(CacheKey, reports, new TimeSpan(24, 0, 0));
                
                return reports;
            }
            
            return cache;
        }
        catch (Exception ex) when (ex is not ClientErrorException)
        {
            throw new ReportServiceException("Не удалось получить список готовых отчётов", ex);
        }
    }

    /// <inheritdoc cref="IReportService.GetReportFileUrl"/>
    public async Task<string> GetReportFileUrl(string reportName)
    {
        try
        {
            var report = await _reportRepository.GetReportByName(reportName);
            
            var link = await _fileStorage.GetFileUrlAsync(report.FilePath, bucketName: BucketName);
            
            return link;
        }
        catch (Exception ex) when (ex is not ClientErrorException)
        {
            throw new ReportServiceException("Не удалось получить ссылку на файл отчёта", ex);
        }
    }

    /// <inheritdoc cref="IReportService.GetStatisticByPeriod"/>
    public async Task<ActivityLogStatisticDto> GetStatisticByPeriod(DateTime startDate, DateTime endDate)
    {
        return await _activityLogRepository.GetStatisticByPeriod(startDate, endDate);
    }
}