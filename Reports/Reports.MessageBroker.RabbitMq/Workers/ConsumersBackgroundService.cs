using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Reports.Abstractions.Services;

namespace Reports.MessageBroker.RabbitMq.Workers;

public class ConsumersBackgroundService : BackgroundService
{
    private readonly List<ILibraryEventsConsumer> _consumers;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConsumersBackgroundService> _logger;
    
    public ConsumersBackgroundService(IServiceScopeFactory factory, IConfiguration configuration, 
        ILogger<ConsumersBackgroundService> logger)
    {
        _consumers = new List<ILibraryEventsConsumer>();
        _scopeFactory = factory;
        _configuration = configuration;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;

        var queues = _configuration
            .GetSection("QueueNames")
            .GetChildren()
            .Select(x => x.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        foreach (var queue in queues)
        {
            if (queue == null)
            {
                continue;
            }
            var consumer = sp.GetKeyedService<ILibraryEventsConsumer>(queue);
            if (consumer is null)
            {
                _logger.LogWarning("Не найден consumer для очереди {Queue}", queue);
                continue;
            }

            await consumer.StartAsync(queue, stoppingToken);
            _consumers.Add(consumer);
        }
    }
    
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var consumer in _consumers.OfType<ILibraryEventsConsumer>())
        {
            await consumer.StopAsync(cancellationToken);
        }
        await base.StopAsync(cancellationToken);
    }
}