using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reports.Abstractions.Storage.Repositories;
using Reports.Data.PostgreSql.Repositories;

namespace Reports.Data.PostgreSql;

public static class Entry
{
    private static readonly Action<DbContextOptionsBuilder> DefaultOptionsAction = (_) => { };

    /// <summary>
    /// Добавления зависимостей для работы с БД
    /// </summary>
    public static IServiceCollection AddPostgreSqlStorage(this IServiceCollection serviceCollection, Action<DbContextOptionsBuilder> optionsAction)
    {
        serviceCollection.AddDbContext<AppDbContext>(optionsAction ?? DefaultOptionsAction, optionsLifetime: ServiceLifetime.Singleton);

        serviceCollection.AddScoped<IActivityLogRepository, ActivityLogRepository>();
        serviceCollection.AddScoped<IReportRepository, ReportRepository>();
        
        return serviceCollection;
    }
}