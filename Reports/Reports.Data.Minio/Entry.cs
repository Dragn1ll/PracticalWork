using Library.Data.Minio;
using Microsoft.Extensions.DependencyInjection;
using Reports.Abstractions.Storage;

namespace Reports.Data.Minio;

public static class Entry
{
    /// <summary>
    /// Регистрация зависимостей для хранилища документов
    /// </summary>
    public static IServiceCollection AddFileStorage(this IServiceCollection serviceCollection)
    {
        // Реализация подключения к Minio и сервисов
        serviceCollection.AddOptions<MinioOptions>().BindConfiguration("App:Minio");

        serviceCollection.AddScoped<IFileStorage, MinioStorage>();

        return serviceCollection;
    }
}
