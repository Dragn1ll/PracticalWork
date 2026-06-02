using Library.Abstraction.Services;
using Library.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Library;

public static class Entry
{
    /// <summary>
    /// Регистрация зависимостей уровня бизнес-логики
    /// </summary>
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<ILibraryService, LibraryService>();
        services.AddScoped<IReaderService, ReaderService>();
        
        services.AddSingleton(TimeProvider.System);

        return services;
    }
}