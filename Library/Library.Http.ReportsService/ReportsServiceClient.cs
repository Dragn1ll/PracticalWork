using System.Net.Http.Json;
using System.Text.Json;
using Library.Abstraction.Services;
using Library.BackgroundServices.Email.Dto;
using Library.Dto.Input;
using Library.Dto.Output;
using Library.Exceptions;
using Library.Models;
using Microsoft.Extensions.Options;

namespace Library.Http.ReportsService;

/// <inheritdoc cref="IReportsServiceClient"/>
public class ReportsServiceClient : IReportsServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ReportsServiceOptions _options;
    
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public ReportsServiceClient(HttpClient httpClient, IOptions<ReportsServiceOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }
    
    /// <inheritdoc cref="IReportsServiceClient.GetAllActivityLogs"/>
    public async Task<IEnumerable<ActivityLog>> GetAllActivityLogs(GetActivityLogsDto dto)
    {
        var response = await _httpClient.GetAsync($"{_options.BaseUrl}/activities");
        
        return await ReadResultAsync<IEnumerable<ActivityLog>>(response);
    }

    /// <inheritdoc cref="IReportsServiceClient.CreateReport"/>
    public async Task<CreatedReportDto> CreateReport(CreateReportDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync(_options.BaseUrl, dto, JsonOptions);
        
        return await ReadResultAsync<CreatedReportDto>(response);
    }

    /// <inheritdoc cref="IReportsServiceClient.GetGeneratedReports"/>
    public async Task<IEnumerable<Report>> GetGeneratedReports()
    {
        var response = await _httpClient.GetAsync(_options.BaseUrl);
        
        return await ReadResultAsync<IEnumerable<Report>>(response);
    }

    /// <inheritdoc cref="IReportsServiceClient.GetReportFileUrl"/>
    public async Task<string> GetReportFileUrl(string reportName)
    {
        var response = await _httpClient.GetAsync($"{_options.BaseUrl}/{reportName}/download");
        
        return await ReadResultAsync<string>(response);
    }

    
    /// <inheritdoc cref="IReportsServiceClient.GetStatisticByPeriod"/>
    public async Task<ActivityLogStatisticDto> GetStatisticByPeriod(DateTime startDate, DateTime endDate)
    {
        var response = await _httpClient
            .GetAsync($"{_options.BaseUrl}/activity/statistic?startDate={startDate}&endDate={endDate}");
        
        return await ReadResultAsync<ActivityLogStatisticDto>(response);
    }

    private async Task<T> ReadResultAsync<T>(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
            return data!;
        }

        var error = await response.Content.ReadAsStringAsync();
        throw new ReportsServiceClientException(error);
    }
}