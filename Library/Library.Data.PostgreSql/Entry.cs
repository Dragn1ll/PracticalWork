using Library.Abstraction.Storage.Repositories;
using Library.Data.PostgreSql.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Library.Data.PostgreSql;

public static class Entry
{
    private static readonly Action<DbContextOptionsBuilder> DefaultOptionsAction = (_) => { };

    /// <summary>
    /// Добавления зависимостей для работы с БД
    /// </summary>
    public static IServiceCollection AddPostgreSqlStorage(this IServiceCollection serviceCollection,
        Action<DbContextOptionsBuilder> optionsAction)
    {
        serviceCollection.AddDbContext<AppDbContext>(optionsAction ?? DefaultOptionsAction,
            optionsLifetime: ServiceLifetime.Singleton);

        serviceCollection.AddScoped<IBookRepository, BookRepository>();
        serviceCollection.AddScoped<ILibraryRepository, LibraryRepository>();
        serviceCollection.AddScoped<IReaderRepository, ReaderRepository>();

        return serviceCollection;
    }
}