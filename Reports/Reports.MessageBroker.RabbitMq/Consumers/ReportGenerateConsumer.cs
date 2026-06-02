using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reports.Abstractions.Services;
using Reports.SharedKernel.Enums;
using Reports.SharedKernel.Events.Report;

namespace Reports.MessageBroker.RabbitMq.Consumers;

public class ReportGenerateConsumer : RabbitMqConsumer<CreateReportEvent>
{
    private readonly IServiceScopeFactory _scopeFactory;
    
    public ReportGenerateConsumer(IOptions<RabbitMqOptions> options, ILogger<RabbitMqConsumer<CreateReportEvent>> logger, 
        IServiceScopeFactory scopeFactory) : base(options, logger)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ProcessMessageAsync(CreateReportEvent? messageObject)
    {
        using var scope = _scopeFactory.CreateScope();
        var genService = scope.ServiceProvider.GetRequiredService<IReportGenService>();

        if (messageObject != null)
        {
            await genService.GenerateReport(messageObject.ReportId, messageObject.PeriodFrom, messageObject.PeriodTo, 
                (EventType)messageObject.EventTypeId);
        }
    }
}