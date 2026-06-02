using System.Text;
using System.Text.Json;
using Reports.Abstractions.Services;
using Reports.Abstractions.Storage;
using Reports.Abstractions.Storage.Repositories;
using Reports.Models;
using Reports.SharedKernel.Enums;

namespace Reports.Services;

/// <inheritdoc cref="IReportGenService"/>
public class ReportGenService : IReportGenService
{
    private readonly IReportRepository _reportRepository;
    private readonly IActivityLogRepository _logRepository;
    private readonly IFileStorage _fileStorage;
    private readonly ICacheStorage _cache;

    public ReportGenService(IReportRepository reportRepository, IActivityLogRepository logRepository,
        IFileStorage fileStorage, ICacheStorage cache)
    {
        _reportRepository = reportRepository;
        _logRepository = logRepository;
        _fileStorage = fileStorage;
        _cache = cache;
    }

    /// <inheritdoc cref="IReportGenService.GenerateReport"/>
    public async Task GenerateReport(Guid reportId, DateOnly periodFrom, DateOnly periodTo, EventType eventType)
    {
        var report = await _reportRepository.GetReportById(reportId);
        var logs = await _logRepository.GetAllActivityLogs(periodFrom, periodTo, eventType);

        try
        {
            var stream = GetStream(logs);
            var fileName = $"{DateTime.UtcNow.Year}/{DateTime.UtcNow.Month}/{reportId}.csv";
            
            await _fileStorage.UploadFileAsync(fileName, stream, "text/csv");
            
            report.MarkAsGenerated(fileName);
            await _reportRepository.UpdateReport(reportId, report);
            
            await _cache.RemoveAsync("reports:list");
        }
        catch (Exception)
        {
            report.Status = ReportStatus.Error;
            await _reportRepository.UpdateReport(reportId,report);
            await _cache.RemoveAsync("reports:list");
            throw;
        }
    }

    private MemoryStream GetStream(IEnumerable<ActivityLog> logs)
    {
        var stringBuilder = new StringBuilder();
        
        stringBuilder.AppendLine("EventType;EventDate;Metadata");
        foreach (var log in logs)
        {
            stringBuilder.AppendLine($"{GetEventTypeName(log.EventType)};{log.EventDate};{JsonSerializer.Serialize(log,
                log.GetType())}");
        }
        
        return new MemoryStream(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
    }

    private string GetEventTypeName(EventType eventType)
    {
        return eventType switch
        {
            EventType.BookCreated => "book.created",
            EventType.BookArchived => "book.archived",
            EventType.BookBorrowed => "book.borrowed",
            EventType.BookReturned => "book.returned",
            EventType.ReaderCreated => "reader.created",
            EventType.ReaderClosed => "reader.closed",
            _ => "???"
        };
    }
}