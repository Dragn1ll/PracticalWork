namespace Reports.Exceptions;

public class ReportServiceException : AppException
{
    public ReportServiceException(string message) : base($"{message}")
    {
    }

    public ReportServiceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}