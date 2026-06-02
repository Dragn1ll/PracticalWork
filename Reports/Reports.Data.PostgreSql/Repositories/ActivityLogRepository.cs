using Microsoft.EntityFrameworkCore;
using Reports.Abstractions.Storage.Repositories;
using Reports.Data.PostgreSql.Entities;
using Reports.Dto;
using Reports.Models;
using Reports.SharedKernel.Enums;

namespace Reports.Data.PostgreSql.Repositories;

public class ActivityLogRepository : IActivityLogRepository
{
    private readonly AppDbContext _context;

    public ActivityLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddActivityLog(ActivityLog activityLog)
    {
        var entity = new ActivityLogEntity
        {
            ExternalBookId = activityLog.ExternalBookId,
            ExternalReaderId = activityLog.ExternalReaderId,
            EventType = activityLog.EventType,
            EventDate = activityLog.EventDate,
            Metadata = activityLog.Metadata
        };
        
        _context.ActivityLogs.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ActivityLog>> GetAllActivityLogs(DateOnly? startDate, DateOnly? endDate, 
        EventType eventType, int page, int pageSize)
    {
        return (await _context.ActivityLogs.AsNoTracking()
            .Where(al => (DateOnly.FromDateTime(al.EventDate) >= startDate || startDate == null)
                         && (DateOnly.FromDateTime(al.EventDate) <= endDate || endDate == null)
                         && (al.EventType == eventType || eventType == EventType.Default))
            .OrderBy(al => al.EventDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync())
            .Select(al => new ActivityLog
            {
                ExternalBookId = al.ExternalBookId,
                ExternalReaderId = al.ExternalReaderId,
                EventType = al.EventType,
                EventDate = al.EventDate,
                Metadata = al.Metadata
            });
    }

    public async Task<ActivityLogStatisticDto> GetStatisticByPeriod(DateTime startDate, DateTime endDate)
    {
        return new ActivityLogStatisticDto
        {
            NewBooksCount = await CountNewBooks(startDate, endDate),
            NewReadersCount = await CountNewReaders(startDate, endDate),
            BorrowedBooksCount = await CountBorrowedBooks(startDate, endDate),
            ReturnedBooksCount = await CountReturnedBooks(startDate, endDate),
            OverdueBooksCount = await CountOverdueBooks(startDate, endDate)
        };
    }

    private async Task<int> CountNewBooks(DateTime startDate, DateTime endDate)
    {
        return await _context.ActivityLogs
            .AsNoTracking()
            .CountAsync(a => a.EventType == EventType.BookCreated 
                             && a.EventDate >= startDate 
                             && a.EventDate <= endDate);
    }

    private async Task<int> CountNewReaders(DateTime startDate, DateTime endDate)
    {
        return await _context.ActivityLogs
            .AsNoTracking()
            .CountAsync(a => a.EventType == EventType.ReaderCreated 
                             && a.EventDate >= startDate 
                             && a.EventDate <= endDate);
    }

    private async Task<int> CountBorrowedBooks(DateTime startDate, DateTime endDate)
    {
        return await _context.ActivityLogs
            .AsNoTracking()
            .CountAsync(a => a.EventType == EventType.BookBorrowed 
                             && a.EventDate >= startDate 
                             && a.EventDate <= endDate);
    }

    private async Task<int> CountReturnedBooks(DateTime startDate, DateTime endDate)
    {
        return await _context.ActivityLogs
            .AsNoTracking()
            .CountAsync(a => a.EventType == EventType.BookReturned 
                             && a.EventDate >= startDate 
                             && a.EventDate <= endDate);
    }
    
    private async Task<int> CountOverdueBooks(DateTime startDate, DateTime endDate)
    {
        return await _context.ActivityLogs
            .AsNoTracking()
            .Where(a => a.EventType == EventType.BookBorrowed 
                        && a.EventDate <= endDate)
            .GroupBy(a => a.ExternalBookId)
            .Select(g => new { BookId = g.Key, BorrowDate = g.Max(x => x.EventDate) })
            .CountAsync(b => !_context.ActivityLogs
                .Any(a => a.EventType == EventType.BookReturned 
                          && a.ExternalBookId == b.BookId 
                          && a.EventDate >= b.BorrowDate));
    }
}