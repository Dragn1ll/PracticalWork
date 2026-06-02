namespace Library.Exceptions;

public class ReportsServiceClientException : AppException
{
    public ReportsServiceClientException(string message) : base($"{message}")
    {
    }
}