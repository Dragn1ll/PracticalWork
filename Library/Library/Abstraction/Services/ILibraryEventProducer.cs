using Library.SharedKernel.Events;

namespace Library.Abstraction.Services;

/// <summary>
/// Producer для отправки события библиотеки в очередь
/// </summary>
public interface ILibraryEventProducer
{
    /// <summary>
    /// Отправить событие в очередь
    /// </summary>
    /// <param name="libraryEvent">Событие</param>
    /// <param name="cancellationToken">Токен прекращения работы</param>
    Task PublishEventAsync<TEvent>(TEvent libraryEvent, CancellationToken cancellationToken = default) 
        where TEvent : BaseEvent;
}