using Hangfire;
using Library.BackgroundServices.Email.Abstractions.Jobs;
using Library.BackgroundServices.Email.Jobs;
using Library.BackgroundServices.Email.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Library.BackgroundServices.Email.Hangfire;

public static class HangfireExtensions
{
    /// <summary>
    /// Регистрация всех фоновых задач
    /// </summary>
    public static IApplicationBuilder UseRecurringJobs(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<JobOptions>>().Value;
        var manager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

        RegisterJob<ReturnReminderJob>(manager, "return-reminder", "ReturnReminder", options);
        RegisterJob<WeeklyReportJob>(manager, "weekly-report", "WeeklyReport", options);
        RegisterJob<ArchiveOldBooksJob>(manager, "archive-old-books", "ArchiveOldBooks", options);
        
        return app;
    }

    private static void RegisterJob<T>(
        IRecurringJobManager manager, 
        string jobId, 
        string configKey, 
        JobOptions options) where T : ILibraryJob
    {
        if (options.Jobs.TryGetValue(configKey, out var config))
        {
            manager.AddOrUpdate<T>(
                jobId,
                job => job.ExecuteAsync(CancellationToken.None),
                config.CronExpression,
                new RecurringJobOptions
                {
                    TimeZone = TimeZoneInfo.Utc
                });
        }
    }
}