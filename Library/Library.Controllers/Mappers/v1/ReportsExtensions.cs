using Library.Contracts.v1.Reports.Request;
using Library.Dto.Input;

namespace Library.Controllers.Mappers.v1;

public static class ReportsExtensions
{
    public static GetActivityLogsDto ToGetActivityLogsDto(this GetActivityLogsRequest request)
        => new()
        {
            DateFrom = request.DateFrom,
            DateTo = request.DateTo,
            EventType = request.EventType,
            Page = request.Page,
            PageSize = request.PageSize
        };

    public static CreateReportDto ToCreateReportDto(this CreateReportRequest request)
        => new()
        {
            EventType = request.EventType,
            Name = request.Name,
            PeriodFrom = request.PeriodFrom,
            PeriodTo = request.PeriodTo
        };
}