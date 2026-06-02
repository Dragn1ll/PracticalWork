namespace Reports.Exceptions;

/// <summary>
/// Исключение уровня приложения
/// </summary>
public class AppException : Exception
{
    public AppException()
    {
    }

    public AppException(string message) : base(message)
    {
    }

    public AppException(string message, Exception innerException) : base(message, innerException)
    {
    }
}