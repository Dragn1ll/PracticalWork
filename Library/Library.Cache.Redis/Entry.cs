using Library.Abstraction.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Library.Cache.Redis;

public static class Entry
{
    /// <summary>
    /// Регистрация зависимостей для распределенного Cache
    /// </summary>
    public static IServiceCollection AddCache(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var connectionString = configuration["App:Redis:RedisCacheConnection"];
        var prefix = configuration["App:Redis:RedisCachePrefix"];

        // Реализация подключения к Redis и сервисов
        serviceCollection.AddStackExchangeRedisCache(option =>
        {
            option.Configuration = connectionString;
            option.InstanceName = prefix;
        });

        serviceCollection.AddSingleton<ICacheStorage, RedisCache>();

        return serviceCollection;
    }
}

