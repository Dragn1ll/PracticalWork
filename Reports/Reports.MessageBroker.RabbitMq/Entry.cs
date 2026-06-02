using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reports.Abstractions.Services;
using Reports.MessageBroker.RabbitMq.Consumers;
using Reports.MessageBroker.RabbitMq.Workers;
using Reports.SharedKernel.Events.Book;
using Reports.SharedKernel.Events.Reader;

namespace Reports.MessageBroker.RabbitMq;

public static class Entry
{
    /// <summary>
    /// Регистрация зависимостей для брокера сообщений
    /// </summary>
    public static IServiceCollection AddMessageBroker(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddOptions<RabbitMqOptions>().BindConfiguration("RabbitMq");

        var queueNames = configuration.GetSection("QueueNames");
        
        serviceCollection
            .AddKeyedSingleton<ILibraryEventsConsumer, ActivityLogConsumer<BookCreatedEvent>>(queueNames["BookCreated"]);
        serviceCollection
            .AddKeyedSingleton<ILibraryEventsConsumer, ActivityLogConsumer<BookArchivedEvent>>(queueNames["BookArchived"]);
        serviceCollection
            .AddKeyedSingleton<ILibraryEventsConsumer, ActivityLogConsumer<BookBorrowedEvent>>(queueNames["BookBorrowed"]);
        serviceCollection
            .AddKeyedSingleton<ILibraryEventsConsumer, ActivityLogConsumer<BookReturnedEvent>>(queueNames["BookReturned"]);
        serviceCollection
            .AddKeyedSingleton<ILibraryEventsConsumer, ActivityLogConsumer<ReaderCreatedEvent>>(queueNames["ReaderCreated"]);
        serviceCollection
            .AddKeyedSingleton<ILibraryEventsConsumer, ActivityLogConsumer<ReaderClosedEvent>>(queueNames["ReaderClosed"]);
        serviceCollection
            .AddKeyedSingleton<ILibraryEventsConsumer, ReportGenerateConsumer>(queueNames["CreateReport"]);
        
        serviceCollection.AddHostedService<ConsumersBackgroundService>();

        return serviceCollection;
    }
}
