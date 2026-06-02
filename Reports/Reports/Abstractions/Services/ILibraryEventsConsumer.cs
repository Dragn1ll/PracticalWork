namespace Reports.Abstractions.Services;

/// <summary>
/// Потребитель очереди сообщений
/// </summary>
public interface ILibraryEventsConsumer
{
    /// <summary>
    /// Подписаться на очередь, получать сообщения и обрабатывать
    /// </summary>
    /// <param name="queueName">Название очереди</param>
    /// <param name="cancellationToken">Токен прекращения работы</param>
    /// <returns></returns>
    Task StartAsync(string queueName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Остановить потребление
    /// </summary>
    /// <param name="cancellationToken">Токен прекращения работы</param>
    /// <returns></returns>
    Task StopAsync(CancellationToken cancellationToken = default);
}