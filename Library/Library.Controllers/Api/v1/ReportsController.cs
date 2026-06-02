using Asp.Versioning;
using Library.Abstraction.Services;
using Library.Contracts.v1.Reports.Request;
using Library.Contracts.v1.Reports.Response;
using Library.Controllers.Mappers.v1;
using Microsoft.AspNetCore.Mvc;
using PracticalWork.Library.MessageBroker.Events.Report;

namespace Library.Controllers.Api.v1;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportsServiceClient _reportsClient;
    private readonly ILibraryEventProducer _producer;

    public ReportsController(IReportsServiceClient reportsClient, ILibraryEventProducer producer)
    {
        _reportsClient = reportsClient;
        _producer = producer;
    }
    
    /// <summary>Получение логов активности</summary>
    [HttpGet("activity")]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetActivityLogs([FromQuery] GetActivityLogsRequest request)
    {
        var result = await _reportsClient.GetAllActivityLogs(request.ToGetActivityLogsDto());

        return Ok(result);
    }
    
    /// <summary>Создание нового отчёта</summary>
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CreateReport([FromBody] CreateReportRequest request)
    {
        var result = await _reportsClient.CreateReport(request.ToCreateReportDto());

        await _producer.PublishEventAsync(new CreateReportEvent(result.Id, result.PeriodFrom, result.PeriodTo,
            (int)request.EventType));

        return Ok(result);
    }
    
    /// <summary>Получение списка завершённых отчётов</summary>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetGeneratedReports()
    {
        var result = await _reportsClient.GetGeneratedReports();

        return Ok(result);
    }

    /// <summary>Получение ссылки на файл отчёта</summary>
    [HttpGet("{reportName}/download")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(DownloadReportResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> DownloadReport([FromRoute] string reportName)
    {
        var result = await _reportsClient.GetReportFileUrl(reportName);
        
        return Ok(result);
    }
}