using Microsoft.EntityFrameworkCore;
using Reports.Data.PostgreSql.Entities;
using Reports.Data.PostgreSql.Repositories;
using Reports.Models;
using Reports.SharedKernel.Enums;

namespace Reports.Data.PostgreSql.UnitTests;

public class ActivityLogRepositoryTests
{
    private AppDbContext NewDb() => DbFactory.Create($"actlog_{Guid.NewGuid()}");
 
    [Fact]
    public async Task AddActivityLog_PersistsEntity()
    {
        await using var db = NewDb();
        var repo = new ActivityLogRepository(db);
        var log = new ActivityLog
        {
            EventType = EventType.BookCreated, EventDate = DateTime.UtcNow,
            Metadata = "{}", ExternalBookId = Guid.NewGuid()
        };
 
        await repo.AddActivityLog(log);
 
        Assert.Equal(1, await db.ActivityLogs.CountAsync());
    }
 
    [Fact]
    public async Task AddActivityLog_SetsAllFields()
    {
        await using var db = NewDb();
        var repo = new ActivityLogRepository(db);
        var bookId = Guid.NewGuid();
        var log = new ActivityLog
        {
            EventType = EventType.BookArchived,
            EventDate = new DateTime(2025, 6, 1, 10, 0, 0),
            Metadata = "{\"key\":\"val\"}", ExternalBookId = bookId
        };
 
        await repo.AddActivityLog(log);
 
        var entity = await db.ActivityLogs.FirstAsync();
        Assert.Equal(EventType.BookArchived, entity.EventType);
        Assert.Equal(bookId, entity.ExternalBookId);
    }
 
    [Fact]
    public async Task GetAllActivityLogs_FiltersDateFrom()
    {
        await using var db = NewDb();
        db.ActivityLogs.AddRange(
            new ActivityLogEntity
            {
                EventType = EventType.BookCreated, 
                EventDate = new DateTime(2025, 1, 10), 
                Metadata = ""
            },
            new ActivityLogEntity
            {
                EventType = EventType.BookCreated, 
                EventDate = new DateTime(2025, 1, 20), 
                Metadata = ""
            }
        );
        await db.SaveChangesAsync();
 
        var repo = new ActivityLogRepository(db);
        var result = (await repo.GetAllActivityLogs(
            new DateOnly(2025, 1, 15), null, EventType.Default, 1, 10)).ToList();
 
        Assert.Single(result);
        Assert.Equal(new DateTime(2025, 1, 20), result[0].EventDate);
    }
 
    [Fact]
    public async Task GetAllActivityLogs_FiltersEventType()
    {
        await using var db = NewDb();
        db.ActivityLogs.AddRange(
            new ActivityLogEntity { EventType = EventType.BookCreated, EventDate = DateTime.UtcNow, Metadata = "" },
            new ActivityLogEntity { EventType = EventType.BookBorrowed, EventDate = DateTime.UtcNow, Metadata = "" }
        );
        await db.SaveChangesAsync();
 
        var repo = new ActivityLogRepository(db);
        var result = 
            (await repo.GetAllActivityLogs(null, null, EventType.BookCreated, 1, 10)).ToList();
 
        Assert.Single(result);
        Assert.Equal(EventType.BookCreated, result[0].EventType);
    }
 
    [Fact]
    public async Task GetAllActivityLogs_WhenEventTypeDefault_ReturnsAll()
    {
        await using var db = NewDb();
        db.ActivityLogs.AddRange(
            new ActivityLogEntity { EventType = EventType.BookCreated, EventDate = DateTime.UtcNow, Metadata = "" },
            new ActivityLogEntity { EventType = EventType.ReaderCreated, EventDate = DateTime.UtcNow, Metadata = "" }
        );
        await db.SaveChangesAsync();
 
        var repo = new ActivityLogRepository(db);
        var result = 
            (await repo.GetAllActivityLogs(null, null, EventType.Default, 1, 10)).ToList();
 
        Assert.Equal(2, result.Count);
    }
 
    [Fact]
    public async Task GetAllActivityLogs_Paginates()
    {
        await using var db = NewDb();
        for (var i = 0; i < 5; i++)
            db.ActivityLogs.Add(new ActivityLogEntity
            {
                EventType = EventType.BookCreated,
                EventDate = DateTime.UtcNow.AddMinutes(i), Metadata = ""
            });
        await db.SaveChangesAsync();
 
        var repo = new ActivityLogRepository(db);
        var page1 = 
            (await repo.GetAllActivityLogs(null, null, EventType.Default, 1, 3)).ToList();
        var page2 = 
            (await repo.GetAllActivityLogs(null, null, EventType.Default, 2, 3)).ToList();
 
        Assert.Equal(3, page1.Count);
        Assert.Equal(2, page2.Count);
    }
 
    [Fact]
    public async Task GetStatisticByPeriod_CountsNewBooksCorrectly()
    {
        await using var db = NewDb();
        var start = new DateTime(2025, 1, 1);
        var end   = new DateTime(2025, 1, 31);
 
        db.ActivityLogs.AddRange(
            new ActivityLogEntity
            {
                EventType = EventType.BookCreated, 
                EventDate = new DateTime(2025, 1, 10), 
                Metadata = ""
            },
            new ActivityLogEntity
            {
                EventType = EventType.BookCreated, 
                EventDate = new DateTime(2025, 1, 20), 
                Metadata = ""
            },
            new ActivityLogEntity
            {
                EventType = EventType.BookCreated, 
                EventDate = new DateTime(2025, 2, 1), 
                Metadata = ""
            }
        );
        await db.SaveChangesAsync();
 
        var repo = new ActivityLogRepository(db);
        var stat = await repo.GetStatisticByPeriod(start, end);
 
        Assert.Equal(2, stat.NewBooksCount);
    }
 
    [Fact]
    public async Task GetStatisticByPeriod_CountsNewReadersCorrectly()
    {
        await using var db = NewDb();
        db.ActivityLogs.AddRange(
            new ActivityLogEntity
            {
                EventType = EventType.ReaderCreated, 
                EventDate = new DateTime(2025, 1, 5), 
                Metadata = ""
            },
            new ActivityLogEntity
            {
                EventType = EventType.ReaderCreated, 
                EventDate = new DateTime(2025, 1, 15), 
                Metadata = ""
            }
        );
        await db.SaveChangesAsync();
 
        var repo = new ActivityLogRepository(db);
        var stat = await repo.GetStatisticByPeriod(new DateTime(2025, 1, 1), new DateTime(2025, 1, 31));
 
        Assert.Equal(2, stat.NewReadersCount);
    }
 
    [Fact]
    public async Task GetStatisticByPeriod_CountsBorrowedAndReturnedBooks()
    {
        await using var db = NewDb();
        db.ActivityLogs.AddRange(
            new ActivityLogEntity
            {
                EventType = EventType.BookBorrowed, 
                EventDate = new DateTime(2025, 1, 5), 
                Metadata = ""
            },
            new ActivityLogEntity
            {
                EventType = EventType.BookBorrowed, 
                EventDate = new DateTime(2025, 1, 10), 
                Metadata = ""
            },
            new ActivityLogEntity
            {
                EventType = EventType.BookReturned, 
                EventDate = new DateTime(2025, 1, 12), 
                Metadata = ""
            }
        );
        await db.SaveChangesAsync();
 
        var repo = new ActivityLogRepository(db);
        var stat = 
            await repo.GetStatisticByPeriod(new DateTime(2025, 1, 1), new DateTime(2025, 1, 31));
 
        Assert.Equal(2, stat.BorrowedBooksCount);
        Assert.Equal(1, stat.ReturnedBooksCount);
    }
 
    [Fact]
    public async Task GetStatisticByPeriod_WhenNoLogs_AllCountsAreZero()
    {
        await using var db = NewDb();
        var repo = new ActivityLogRepository(db);
 
        var stat = 
            await repo.GetStatisticByPeriod(new DateTime(2025, 1, 1), new DateTime(2025, 1, 31));
 
        Assert.Equal(0, stat.NewBooksCount);
        Assert.Equal(0, stat.NewReadersCount);
        Assert.Equal(0, stat.BorrowedBooksCount);
        Assert.Equal(0, stat.ReturnedBooksCount);
    }
}