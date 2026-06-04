using Library.Data.PostgreSql.Entities;
using Library.Data.PostgreSql.Repositories;
using Library.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.Data.PostgreSql.UnitTests;

public class NotificationLogRepositoryTests
{
    private AppDbContext NewDb() => DbFactory.Create($"notif_{Guid.NewGuid()}");
 
    [Fact]
    public async Task AddNotificationLog_PersistsEntity()
    {
        await using var db = NewDb();
        var repo = new NotificationLogRepository(db);
        var log = new NotificationLog
        {
            BorrowId = Guid.NewGuid(), NotificationType = "ReturnReminder",
            RecipientEmail = "r@t.com", Subject = "Напоминание",
            IsSent = true, ErrorMessage = string.Empty
        };
 
        await repo.AddNotificationLog(log);
 
        Assert.Equal(1, await db.NotificationLogs.CountAsync());
    }
 
    [Fact]
    public async Task WasReminderSentRecently_WhenSentWithinWindow_ReturnsTrue()
    {
        await using var db = NewDb();
        var borrowId = Guid.NewGuid();
        db.NotificationLogs.Add(new NotificationLogEntity
        {
            BorrowId = borrowId, NotificationType = "ReturnReminder",
            RecipientEmail = "r@t.com", Subject = "S",
            IsSent = true, ErrorMessage = "",
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        });
        await db.SaveChangesAsync();
 
        var repo = new NotificationLogRepository(db);
        var result = await repo.WasReminderSentRecently(borrowId, withinHours: 24);
 
        Assert.True(result);
    }
 
    [Fact]
    public async Task WasReminderSentRecently_WhenSentOutsideWindow_ReturnsFalse()
    {
        await using var db = NewDb();
        var borrowId = Guid.NewGuid();
        db.NotificationLogs.Add(new NotificationLogEntity
        {
            BorrowId = borrowId, NotificationType = "ReturnReminder",
            RecipientEmail = "r@t.com", Subject = "S",
            IsSent = true, ErrorMessage = "",
            CreatedAt = DateTime.UtcNow.AddHours(-25)
        });
        await db.SaveChangesAsync();
 
        var repo = new NotificationLogRepository(db);
        var result = await repo.WasReminderSentRecently(borrowId, withinHours: 24);
 
        Assert.False(result);
    }
 
    [Fact]
    public async Task WasReminderSentRecently_WhenNotSentSuccessfully_ReturnsFalse()
    {
        await using var db = NewDb();
        var borrowId = Guid.NewGuid();
        db.NotificationLogs.Add(new NotificationLogEntity
        {
            BorrowId = borrowId, NotificationType = "ReturnReminder",
            RecipientEmail = "r@t.com", Subject = "S",
            IsSent = false,
            ErrorMessage = "SMTP error",
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        });
        await db.SaveChangesAsync();
 
        var repo = new NotificationLogRepository(db);
        var result = await repo.WasReminderSentRecently(borrowId, withinHours: 24);
 
        Assert.False(result);
    }
 
    [Fact]
    public async Task GetNotificationLogs_FiltersByType()
    {
        await using var db = NewDb();
        db.NotificationLogs.Add(new NotificationLogEntity
        {
            BorrowId = Guid.NewGuid(), NotificationType = "ReturnReminder",
            RecipientEmail = "a@t.com", Subject = "S", IsSent = true, ErrorMessage = ""
        });
        db.NotificationLogs.Add(new NotificationLogEntity
        {
            BorrowId = Guid.NewGuid(), NotificationType = "WeeklyReport",
            RecipientEmail = "b@t.com", Subject = "S", IsSent = true, ErrorMessage = ""
        });
        await db.SaveChangesAsync();
 
        var repo = new NotificationLogRepository(db);
        var result = (await repo.GetNotificationLogs("ReturnReminder")).ToList();
 
        Assert.Single(result);
        Assert.Equal("ReturnReminder", result[0].NotificationType);
    }
 
    [Fact]
    public async Task GetNotificationLogs_WithoutFilter_ReturnsAll()
    {
        await using var db = NewDb();
        for (var i = 0; i < 3; i++)
            db.NotificationLogs.Add(new NotificationLogEntity
            {
                BorrowId = Guid.NewGuid(), NotificationType = $"Type{i}",
                RecipientEmail = "x@t.com", Subject = "S", IsSent = true, ErrorMessage = ""
            });
        await db.SaveChangesAsync();
 
        var repo = new NotificationLogRepository(db);
        var result = (await repo.GetNotificationLogs()).ToList();
 
        Assert.Equal(3, result.Count);
    }
}