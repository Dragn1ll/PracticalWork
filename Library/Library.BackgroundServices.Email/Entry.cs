using Hangfire;
using Hangfire.PostgreSql;
using Library.Abstraction.Services;
using Library.BackgroundServices.Email.Abstractions.Services;
using Library.BackgroundServices.Email.Jobs;
using Library.BackgroundServices.Email.Services;
using Library.BackgroundServices.Email.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Library.BackgroundServices.Email;

public static class Entry
{
    public static IServiceCollection AddEmailBackgroundService(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddOptions<JobOptions>().BindConfiguration("JobSettings");
        serviceCollection.AddOptions<ArchiveOptions>().BindConfiguration("ArchiveSettings");
        serviceCollection.AddOptions<EmailTemplateOptions>().BindConfiguration("EmailTemplateSettings");
        
        serviceCollection.AddSingleton<IEmailTemplateService, EmailTemplateService>();
        serviceCollection.AddScoped<INotificationService, NotificationService>();
        serviceCollection.AddScoped<IReportJobService, ReportJobService>();
        serviceCollection.AddScoped<IArchiveService, ArchiveService>();
        
        serviceCollection.AddScoped<ReturnReminderJob>();
        serviceCollection.AddScoped<WeeklyReportJob>();
        serviceCollection.AddScoped<ArchiveOldBooksJob>();
        
        serviceCollection.AddHangfire(config => config
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options =>
                options.UseNpgsqlConnection(
                    configuration["App:DbConnectionString"])));

        serviceCollection.AddHangfireServer(options =>
        {
            options.WorkerCount = 1;
            options.ServerName = "Library.Email.Server";
        });
        
        return serviceCollection;
    }
}