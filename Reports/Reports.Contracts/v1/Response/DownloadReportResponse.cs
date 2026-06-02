namespace Reports.Contracts.v1.Response;

/// <summary>
/// Ответ на запрос выдачи ссылки на файл отчёта
/// </summary>
/// <param name="Url">Ссылка на файл отчёта</param>
public record DownloadReportResponse(string Url);