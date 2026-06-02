using Library.Abstraction.Storage.Repositories;
using Library.Data.PostgreSql.Entities;
using Library.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.Data.PostgreSql.Repositories;

/// <inheritdoc cref="INotificationLogRepository"/>
public class NotificationLogRepository : INotificationLogRepository
{
    private readonly AppDbContext _appDbContext;

    public NotificationLogRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }
    
    /// <inheritdoc cref="INotificationLogRepository.AddNotificationLog"/>
    public async Task AddNotificationLog(NotificationLog notificationLog)
    {
        var entity = new NotificationLogEntity
        {
            BorrowId = notificationLog.BorrowId,
            NotificationType = notificationLog.NotificationType,
            RecipientEmail = notificationLog.RecipientEmail,
            Subject = notificationLog.Subject,
            IsSent = notificationLog.IsSent,
            ErrorMessage = notificationLog.ErrorMessage
        };
        
        _appDbContext.NotificationLogs.Add(entity);
        await _appDbContext.SaveChangesAsync();
    }
    
    /// <inheritdoc cref="INotificationLogRepository.WasReminderSentRecently"/>
    public async Task<bool> WasReminderSentRecently(Guid borrowId, int withinHours)
    {
        var cutoff = DateTime.UtcNow.AddHours(-withinHours);
        
        return await _appDbContext.NotificationLogs
            .AsNoTracking()
            .AnyAsync(n => n.BorrowId == borrowId 
                && n.NotificationType == "ReturnReminder"
                && n.IsSent
                && n.CreatedAt >= cutoff);
    }

    /// <inheritdoc cref="INotificationLogRepository.GetNotificationLogs"/>
    public async Task<IEnumerable<NotificationLog>> GetNotificationLogs(string notificationType = null!, int page = 1,
        int pageSize = 20)
    {
        var query = _appDbContext.NotificationLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(notificationType))
        {
            query = query.Where(n => n.NotificationType == notificationType);
        }

        var entities = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return entities.Select(e => new NotificationLog
        {
            BorrowId = e.BorrowId,
            NotificationType = e.NotificationType,
            RecipientEmail = e.RecipientEmail,
            Subject = e.Subject,
            IsSent = e.IsSent,
            ErrorMessage = e.ErrorMessage,
            SentAt = e.CreatedAt
        });
    }
}