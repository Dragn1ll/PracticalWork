using Microsoft.Extensions.DependencyInjection;
using Reports.Abstractions.Services;
using Reports.Services;

namespace Reports;

public static class Entry
{
    /// <summary>
    /// Регистрация зависимостей уровня бизнес-логики
    /// </summary>
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IReportGenService, ReportGenService>();

        return services;
    }
}