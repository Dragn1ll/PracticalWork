using Library.Abstraction.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Library.Http.ReportsService;

public static class Entry
{
    /// <summary>
    /// Регистрация зависимостей для клиента ReportsService
    /// </summary>
    public static IServiceCollection AddReportsServiceClient(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddOptions<ReportsServiceOptions>().BindConfiguration("ReportsService");

        serviceCollection.AddScoped<IReportsServiceClient, ReportsServiceClient>();
        serviceCollection.AddHttpClient<IReportsServiceClient, ReportsServiceClient>();

        return serviceCollection;
    }
}
