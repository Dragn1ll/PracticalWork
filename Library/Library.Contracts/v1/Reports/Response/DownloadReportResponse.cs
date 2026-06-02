namespace Library.Contracts.v1.Reports.Response;

/// <summary>
/// Ответ на запрос выдачи ссылки на файл отчёта
/// </summary>
/// <param name="Url">Ссылка на файл отчёта</param>
public record DownloadReportResponse(string Url);