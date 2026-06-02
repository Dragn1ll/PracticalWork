using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Reports.Abstractions.Services;
using Reports.Contracts.v1.Request;
using Reports.Contracts.v1.Response;

namespace Reports.Controllers.Api.v1;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }
    
    /// <summary>Получение логов активности</summary>
    [HttpGet("activity")]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetActivityLogs([FromQuery] GetActivityLogsRequest request)
    {
        var result = await _reportService.GetAllActivityLogs(request.DateFrom, request.DateTo, 
            request.EventType, request.Page, request.PageSize);

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
        var result =
            await _reportService.CreateReport(request.Name, request.PeriodFrom, request.PeriodTo, request.EventType);

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
        var result = await _reportService.GetGeneratedReports();

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
        var result = await _reportService.GetReportFileUrl(reportName);
        
        return Ok(result);
    }
    
    /// <summary>Получение статистики за период</summary>
    [HttpGet("activity/statistic")]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetStatisticByPeriod([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await _reportService.GetStatisticByPeriod(startDate, endDate);
        
        return Ok(result);
    }
}