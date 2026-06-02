using Library.Abstraction.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Library.MessageBroker.RabbitMq;

public static class Entry
{
    /// <summary>
    /// Регистрация зависимостей для брокера сообщений
    /// </summary>
    public static IServiceCollection AddMessageBroker(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddOptions<RabbitMqOptions>().BindConfiguration("RabbitMq");

        serviceCollection.AddSingleton<ILibraryEventProducer, RabbitMqProducer>();

        return serviceCollection;
    }
}
